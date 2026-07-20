using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;

namespace NpcTrackerMod
{
    /// <summary>
    /// Управление списком NPC, их маршрутами и черным списком.
    /// </summary>
    public class NpcList
    {
        private readonly _modInstance _modInstance;

        /// <summary>
        /// Черный список NPC.
        /// </summary>
        public HashSet<string> BlacklistedNpcs { get; } = new HashSet<string>();

        /// <summary>
        /// Общий список NPC. HashSet вместо List — Contains O(1), вызывается каждый кадр.
        /// </summary>
        public HashSet<string> TotalNpcList { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Текущие NPC в локации.
        /// </summary>
        public List<string> CurrentNpcList { get; set; } = new List<string>();

        /// <summary>
        /// Список всех NPC у которых есть путь.
        /// </summary>
        public List<NPC> GameNpcs { get; set; }

        /// <summary>
        /// Имя текущего выбранного NPC.
        /// </summary>
        public string CurrentNpcName { get; set; }

        /// <summary>
        /// Пути NPC на текущий день.
        /// Внешний ключ — имя NPC, внутренний — название локации, значение — набор тайлов.
        /// Dictionary вместо List: поиск по локации O(1), HashSet даёт дедупликацию тайлов бесплатно.
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<Point>>> NpcTotalToDayPath { get; private set; }
            = new Dictionary<string, Dictionary<string, HashSet<Point>>>();

        /// <summary>
        /// Глобальные пути NPC (по всем записям расписания).
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<Point>>> GlobalNpcPaths { get; private set; }
            = new Dictionary<string, Dictionary<string, HashSet<Point>>>();

        /// <summary>
        /// Маршруты NPC на текущий день, разбитые по временным слотам расписания.
        /// Ключи: имя NPC → игровое время → локация → набор тайлов.
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<string, HashSet<Point>>>> NpcTimedDayPath { get; }
            = new Dictionary<string, Dictionary<int, Dictionary<string, HashSet<Point>>>>();

        private bool _isGlobalListInitialized;

        public NpcList(_modInstance instance)
        {
            _modInstance = instance;
            CurrentNpcName = null;
        }

        /// <summary>
        /// Добавляет путь NPC в указанный словарь путей.
        /// </summary>
        public void AddNpcPath(NPC npc, Dictionary<string, Dictionary<string, HashSet<Point>>> pathDictionary, Dictionary<string, HashSet<Point>> route)
        {
            if (npc?.Name == null) return;

            if (!pathDictionary.TryGetValue(npc.Name, out var npcPaths))
            {
                npcPaths = new Dictionary<string, HashSet<Point>>();
                pathDictionary[npc.Name] = npcPaths;
                _modInstance.Monitor.Log($"Добавлен нпс: {npc.Name}", LogLevel.Trace);
            }

            foreach (var kvp in route)
            {
                if (!npcPaths.TryGetValue(kvp.Key, out var existingPoints))
                {
                    npcPaths[kvp.Key] = new HashSet<Point>(kvp.Value);
                    _modInstance.Monitor.Log($"Добавлена новая локация {kvp.Key} для NPC {npc.Name}", LogLevel.Trace);
                }
                else
                {
                    existingPoints.UnionWith(kvp.Value);
                    _modInstance.Monitor.Log($"Добавлены новые координаты в локацию {kvp.Key} для NPC {npc.Name}", LogLevel.Trace);
                }
            }
        }

        /// <summary>
        /// Обновляет список NPC для текущей локации игрока.
        /// </summary>
        public void RefreshCurrentNpcList()
        {
            var previousNpcName = CurrentNpcName;

            // Строим HashSet из characters один раз — O(m), затем Contains O(1) вместо Any O(m) на каждый элемент TotalNpcList
            var locationNpcNames = new HashSet<string>(
                Game1.currentLocation.characters.Select(npc => npc.Name));

            CurrentNpcList = TotalNpcList
                .Where(npcName => locationNpcNames.Contains(npcName))
                .OrderBy(name => name)
                .ToList();

            CurrentNpcName = CurrentNpcList.Contains(previousNpcName)
                ? previousNpcName
                : CurrentNpcList.FirstOrDefault();
        }

        /// <summary>
        /// Удаляет NPC из общего списка, если они находятся в черном списке.
        /// </summary>
        public void NpcRemoveBlackTotalList()
        {
            TotalNpcList.ExceptWith(BlacklistedNpcs);
        }

        /// <summary>
        /// Добавляет NPC в черный список.
        /// </summary>
        public void AddToBlacklist(string npcName)
        {
            if (!string.IsNullOrEmpty(npcName))
                BlacklistedNpcs.Add(npcName);
        }

        /// <summary>
        /// Удаляет NPC из черного списка.
        /// </summary>
        public void RemoveNpcBlackList(string npc)
        {
            BlacklistedNpcs.Remove(npc);
        }

        /// <summary>
        /// Добавляет NPC в текущий список.
        /// </summary>
        public void NpcAddCurrentList(IEnumerable<NPC> npcList)
        {
            foreach (var npc in npcList)
            {
                if (TotalNpcList.Contains(npc.Name))
                    CurrentNpcList.Add(npc.Name);
            }
        }

        /// <summary>
        /// Создает и обновляет списки NPC и черного списка.
        /// </summary>
        public void CreateTotalAndBlackList()
        {
            GameNpcs = Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null)
                .ToList();

            if (!_isGlobalListInitialized)
                _modInstance.CustomNpcPaths.TransferPath();

            foreach (var npc in GameNpcs)
            {
                try
                {
                    _modInstance.NpcManager.GetNpcRoutePoints(npc);

                    if (!_isGlobalListInitialized)
                        _modInstance.NpcManager.ProcessNpcGlobalRoute(npc, null, null, null);
                }
                catch (Exception ex)
                {
                    _modInstance.Monitor.Log($"Ошибка обработки NPC {npc.Name}: {ex.Message}", LogLevel.Warn);
                }
            }

            _isGlobalListInitialized = true;
        }

        /// <summary>
        /// Возвращает имя текущего NPC из списка.
        /// Защита от выхода за границы: возвращает null если список пуст или индекс некорректен.
        /// </summary>
        public string GetNpcFromList()
        {
            if (CurrentNpcList.Count == 0 || _modInstance.NpcSelected >= CurrentNpcList.Count)
                return null;
            string npcName = CurrentNpcList[_modInstance.NpcSelected];
            return NpcTotalToDayPath.ContainsKey(npcName) ? npcName : null;
        }
    }
}

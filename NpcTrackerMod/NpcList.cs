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
        /// Общий список NPC.
        /// </summary>
        public List<string> TotalNpcList { get; private set; } = new List<string>();

        /// <summary>
        /// Текущие NPC в локации.
        /// </summary>
        public List<string> CurrentNpcList { get; set; } = new List<string>();

        /// <summary>
        /// Список всех нпс у которых есть путь.
        /// </summary>
        public List<NPC> GameNpcs { get; set; }

        /// <summary> 
        /// Имя текущего выбранного NPC. 
        /// </summary>
        public string CurrentNpcName { get; set; }

        /// <summary>
        /// Пути NPC на текущий день.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> NpcTotalToDayPath { get; private set; } = new Dictionary<string, List<(string, List<Point>)>>();

        /// <summary>
        /// Словарь глобальных путей NPC.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> GlobalNpcPaths { get; private set; } = new Dictionary<string, List<(string, List<Point>)>>();

        private bool _isGlobalListInitialized;
        
        /// <summary>
        /// Конструктор класса NpcList.
        /// </summary>
        /// <param name="instance">Экземпляр основного мода.</param
        public NpcList(_modInstance instance)
        {
            _modInstance = instance;           

            CurrentNpcName = null;
        }

        /// <summary>
        /// Добавляет путь NPC в указанный словарь путей.
        /// </summary>
        /// <param name="npc">NPC, для которого добавляется путь.</param>
        /// <param name="pathDictionary">Словарь путей NPC.</param>
        /// <param name="Global">Флаг, указывающий, является ли путь глобальным.</param>
        public void AddNpcPath(NPC npc, Dictionary<string, List<(string, List<Point>)>> pathDictionary, List<(string, List<Point>)> Route)
        {
            if (npc?.Name == null || npc.Schedule == null || !npc.Schedule.Any()) return;

            //Нормально будет использовать для выборки по 1 маршруту из полного текущего списка 
            /*
            if (!NpcTotalPath.TryGetValue(npc.Name, out var paths))
            {
                paths = new List<(string, List<Point>)>();
                NpcTotalPath[npc.Name] = paths;
                modInstance.Monitor.Log($"Добавлен NPC: {npc.Name}", LogLevel.Trace);
            }

            foreach (var (location, points) in Route)
            {
                var existingLocation = paths.FirstOrDefault(p => p.Item1 == location);
                if (existingLocation.Item1 == null)
                {
                    paths.Add((location, new HashSet<Point>(points).ToList()));
                    modInstance.Monitor.Log($"Добавлена новая локация {location} для NPC {npc.Name}", LogLevel.Trace);
                }
                else
                {
                    existingLocation.Item2 = new HashSet<Point>(existingLocation.Item2.Concat(points)).ToList();
                    modInstance.Monitor.Log($"Обновлены координаты в локации {location} для NPC {npc.Name}", LogLevel.Trace);
                }
            }
            */

            //Проверка наличия данных для NPC
            if (!pathDictionary.ContainsKey(npc.Name))
            {
                // Если нет, создаем новую запись для NPC
                pathDictionary[npc.Name] = new List<(string, List<Point>)>();
                _modInstance.Monitor.Log($"Добавлен нпс: {npc.Name}", LogLevel.Trace);
            }

            var LocationAndPoint = Route;

            // Получаем список локаций для данного NPC
            var npcPaths = pathDictionary[npc.Name];


            // Проходим по каждой новой локации и её путям
            foreach (var newLocationAndPoints in LocationAndPoint)
            {
                // Ищем, существует ли уже эта локация для NPC
                var existingLocation = npcPaths.FirstOrDefault(p => p.Item1 == newLocationAndPoints.Item1);

                if (existingLocation.Item1 == null)
                {
                    // Если локация не найдена, добавляем новую локацию и её пути
                    npcPaths.Add((newLocationAndPoints.Item1, new HashSet<Point>(newLocationAndPoints.Item2).ToList()));
                    _modInstance.Monitor.Log($"Добавлена новая локация {newLocationAndPoints.Item1} для NPC {npc.Name}", LogLevel.Trace);
                }
                else
                {
                    // Если локация есть, добавляем уникальные координаты
                    var uniquePoints = new HashSet<Point>(existingLocation.Item2);
                    uniquePoints.UnionWith(newLocationAndPoints.Item2);
                    existingLocation.Item2.Clear();
                    existingLocation.Item2.AddRange(uniquePoints);

                    _modInstance.Monitor.Log($"Добавлены новые координаты в локацию {newLocationAndPoints.Item1} для NPC {npc.Name}", LogLevel.Trace);
                }
            }
        }


        /// <summary>
        /// Обновляет список NPC для текущей локации игрока.
        /// </summary>
        public void RefreshCurrentNpcList()
        {
            var previousNpcName = CurrentNpcName;
            var currentLocationName = Game1.player.currentLocation.Name;

            CurrentNpcList = TotalNpcList
                .Where(npcName => Game1.currentLocation.characters.Any(npc => npc.Name == npcName))
                .OrderBy(name => name)
                .ToList();

            CurrentNpcName = CurrentNpcList.Contains(previousNpcName) ? previousNpcName : CurrentNpcList.FirstOrDefault();
        }

        /// <summary>
        /// Удаляет NPC из общего списка, если они находятся в черном списке.
        /// </summary>
        public void NpcRemoveBlackTotalList()
        {
            TotalNpcList = TotalNpcList.Except(BlacklistedNpcs).ToList();
        }

        /// <summary>
        /// Добавляет NPC в черный список.
        /// </summary>
        /// <param name="npc">Имя NPC для добавления в черный список.</param>
        public void AddToBlacklist(string npcName)
        {
            if (!string.IsNullOrEmpty(npcName))
                BlacklistedNpcs.Add(npcName);
        }

        /// <summary>
        /// Удаляет NPC из черного списка.
        /// </summary>
        /// <param name="npc">Имя NPC для удаления из черного списка.</param>
        public void RemoveNpcBlackList(string npc)
        {
            BlacklistedNpcs.Remove(npc);
        }

        /// <summary>
        /// Добавляет NPC в текущий список.
        /// </summary>
        /// <param name="npcList">Список Npc для добавления в текущий список.</param>
        public void NpcAddCurrentList(IEnumerable<NPC> npcList)
        {
            foreach (var npc in npcList)
            {
                if (TotalNpcList.Contains(npc.Name))
                {
                    CurrentNpcList.Add(npc.Name);
                }
            }   
                      
        }

        /// <summary>
        /// Создает и обновляет списки NPC и черного списка.
        /// </summary>
        public void CreateTotalAndBlackList()
        {

            // Получение всех NPC во всех локациях
            GameNpcs =  Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null) // Отфильтровываем возможные null значения
                .ToList(); // Преобразуем в список

            //if (!FullGlobalList)
            //    modInstance.CustomNpcPaths.TransferPath();

            //foreach (var x in NpcTotalGlobalPath)
            //{
            //    if (x.Key == "Andy")
            //    {
            //        modInstance.Monitor.Log($"NPC: {x.Key}", LogLevel.Debug);
            //        foreach (var y in x.Value)
            //        {
            //            modInstance.Monitor.Log($"Location: {y.Item1}", LogLevel.Debug);
            //        }
            //    }
            //}

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
        /// </summary>
        public string GetNpcFromList()
        {
            return NpcTotalToDayPath.FirstOrDefault(k => k.Key == CurrentNpcList[_modInstance.NpcSelected]).Key;
        } 

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для управления списком NPC, их маршрутов и черным списком.
    /// </summary>
    public class NpcList
    {
        private readonly NpcTrackerMod modInstance;
        /// <summary>
        /// Черный список NPC.
        /// </summary>
        public HashSet<string> NpcBlackList { get; }

        /// <summary>
        /// Общий список NPC.
        /// </summary>
        public List<string> NpcTotalList { get; private set; }

        /// <summary>
        /// Список текущих NPC в локации.
        /// </summary>
        public List<string> NpcCurrentList { get; set; }

        public List<NPC> NPClist { get; set; }

        /// <summary>
        /// Словарь путей NPC на текущий день.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> NpcTotalToDayPath { get; private set; }

        /// <summary>
        /// Словарь глобальных путей NPC.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> NpcTotalGlobalPath { get; private set; }

        private bool FullGlobalList = false;
        /// <summary>
        /// Конструктор класса NpcList.
        /// </summary>
        /// <param name="instance">Экземпляр основного мода.</param
        public NpcList(NpcTrackerMod instance)
        {
            modInstance = instance;           
            NpcTotalList = new List<string>();
            NpcCurrentList = new List<string>();
            NpcBlackList = new HashSet<string>();
            NpcTotalToDayPath = new Dictionary<string, List<(string, List<Point>)>>();
            NpcTotalGlobalPath = new Dictionary<string, List<(string, List<Point>)>>();
        }

        /// <summary>
        /// Добавляет путь NPC в указанный словарь путей.
        /// </summary>
        /// <param name="npc">NPC, для которого добавляется путь.</param>
        /// <param name="NpcTotalPath">Словарь путей NPC.</param>
        /// <param name="Global">Флаг, указывающий, является ли путь глобальным.</param>
        public void AddNpcPath(NPC npc, Dictionary<string, List<(string, List<Point>)>> NpcTotalPath, List<(string, List<Point>)> Route)
        {
            if (npc.Schedule == null || !npc.Schedule.Any()) return;

            // Проверка наличия данных для NPC
            if (!NpcTotalPath.ContainsKey(npc.Name))
            {
                // Если нет, создаем новую запись для NPC
                NpcTotalPath[npc.Name] = new List<(string, List<Point>)>();
                modInstance.Monitor.Log($"Добавлен нпс: {npc.Name}", LogLevel.Trace);
            }
            var LocationAndPoint = Route;

            // Получаем список локаций для данного NPC
            var npcPaths = NpcTotalPath[npc.Name];


            // Проходим по каждой новой локации и её путям
            foreach (var newLocationAndPoints in LocationAndPoint)
            {
                // Ищем, существует ли уже эта локация для NPC
                var existingLocation = npcPaths.FirstOrDefault(p => p.Item1 == newLocationAndPoints.Item1);

                if (existingLocation.Item1 == null)
                {
                    // Если локация не найдена, добавляем новую локацию и её пути
                    npcPaths.Add((newLocationAndPoints.Item1, new HashSet<Point>(newLocationAndPoints.Item2).ToList()));
                    modInstance.Monitor.Log($"Добавлена новая локация {newLocationAndPoints.Item1} для NPC {npc.Name}", LogLevel.Trace);
                }
                else
                {
                    // Если локация есть, добавляем уникальные координаты
                    var uniquePoints = new HashSet<Point>(existingLocation.Item2);
                    uniquePoints.UnionWith(newLocationAndPoints.Item2);
                    existingLocation.Item2.Clear();
                    existingLocation.Item2.AddRange(uniquePoints);

                    modInstance.Monitor.Log($"Добавлены новые координаты в локацию {newLocationAndPoints.Item1} для NPC {npc.Name}", LogLevel.Trace);
                }
            }           
        }



        /// <summary>
        /// Удаляет NPC из общего списка, если они находятся в черном списке.
        /// </summary>
        public void NpcRemoveBlackTotalList()
        {
            NpcTotalList = NpcTotalList.Except(NpcBlackList).ToList();
        }

        /// <summary>
        /// Добавляет NPC в черный список.
        /// </summary>
        /// <param name="npc">Имя NPC для добавления в черный список.</param>
        public void AddNpcBlackList(string npc)
        {
            NpcBlackList.Add(npc);
        }

        /// <summary>
        /// Удаляет NPC из черного списка.
        /// </summary>
        /// <param name="npc">Имя NPC для удаления из черного списка.</param>
        public void RemoveNpcBlackList(string npc)
        {
            NpcBlackList.Remove(npc);
        }

        /// <summary>
        /// Добавляет NPC в текущий список.
        /// </summary>
        /// <param name="npc">Имя NPC для добавления в текущий список.</param>
        public void NpcAddCurrentList(IEnumerable<NPC> npcList)
        {
            foreach (var npc in npcList)
            if (NpcTotalList.Contains(npc.Name))
            {
                NpcCurrentList.Add(npc.Name);
            }          
        }

        /// <summary>
        /// Создает и обновляет списки NPC и черного списка.
        /// </summary>
        public void CreateTotalAndBlackList()
        {

            // Получение всех NPC во всех локациях
            NPClist =  Game1.locations
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

            foreach (var npc in NPClist)
            {               
                try
                {
                    modInstance.NpcManager.GetNpcRoutePoints(npc);

                    if (!FullGlobalList)
                        modInstance.NpcManager.ProcessNpcGlobalRoute(npc, null, null, null);
                }
                catch (Exception ex)
                {
                    modInstance.Monitor.Log($"Ошибка обработки NPC {npc.Name}: {ex.Message}", LogLevel.Warn);
                }
            }         
            FullGlobalList = true;

            //NPClist.Sort();
            //foreach (var x in NPClist)
            //{
            //    modInstance.Monitor.Log($"{x.Name}", LogLevel.Debug);
            //}
            
        }

        public string GetNpcFromList()
        {
            return NpcTotalToDayPath.FirstOrDefault(k => k.Key == NpcCurrentList[modInstance.NpcSelected]).Key;
        } 
    }
}

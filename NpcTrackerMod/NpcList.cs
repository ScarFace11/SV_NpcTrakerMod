using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewValley;

using Microsoft.Xna.Framework;

namespace NpcTrackerMod
{
    public class NpcList
    {
        private NpcTrackerMod modInstance;
        public HashSet<string> NpcBlackList { get; }
        public List<string> NpcTotalList { get; private set; }
        public List<string> NpcCurrentList { get; private set; }

        public Dictionary<string, List<(string, List<Point>)>> NpcTotalToDayPath = new Dictionary<string, List<(string, List<Point>)>>();


        public NpcList(NpcTrackerMod instance)
        {
            this.modInstance = instance;
            NpcBlackList = new HashSet<string>();
            NpcTotalList = new List<string>();
            NpcCurrentList = new List<string>();
        }
        public void AddNpcPath(string npcName, List<(string, List<Point>)> LocationAndPoint)
        {
            // Проверяем, есть ли уже данные для этого NPC
            if (!NpcTotalToDayPath.ContainsKey(npcName))
            {
                // Если нет, создаем новую запись для NPC
                NpcTotalToDayPath[npcName] = new List<(string, List<Point>)>();
                modInstance.Monitor.Log($"Добавлен нпс: {npcName}", LogLevel.Trace);
            }

            // Получаем список локаций для данного NPC
            var npcPaths = NpcTotalToDayPath[npcName];

            // Проходим по каждой новой локации и её путям
            foreach (var newLocationAndPoints in LocationAndPoint)
            {
                // Ищем, существует ли уже эта локация для NPC
                var existingLocation = npcPaths.FirstOrDefault(p => p.Item1 == newLocationAndPoints.Item1);

                if (existingLocation.Item1 == null)
                {
                    // Если локация не найдена, добавляем новую локацию и её пути
                    npcPaths.Add((newLocationAndPoints.Item1, new HashSet<Point>(newLocationAndPoints.Item2).ToList()));
                    modInstance.Monitor.Log($"Добавлена новая локация {newLocationAndPoints.Item1} для NPC {npcName}", LogLevel.Trace);
                }
                else
                {
                    // Если локация есть, добавляем уникальные координаты
                    var uniquePoints = new HashSet<Point>(existingLocation.Item2);
                    uniquePoints.UnionWith(newLocationAndPoints.Item2);
                    existingLocation.Item2.Clear();
                    existingLocation.Item2.AddRange(uniquePoints);

                    modInstance.Monitor.Log($"Добавлены новые координаты в локацию {newLocationAndPoints.Item1} для NPC {npcName}", LogLevel.Trace);
                }
            }
        }

        

        // Удаляем NPC из списка NpcTotalList, если они находятся в черном списке
        public void NpcRemoveBlackTotalList()
        {
            NpcTotalList = NpcTotalList.Except(NpcBlackList).ToList();
        }

        // добавить нпс в черный список
        public void AddNpcBlackList(string npc)
        {
            NpcBlackList.Add(npc);
        }

        // удалить нпс из черного списка
        public void RemoveNpcBlackList(string npc)
        {
            NpcBlackList.Remove(npc);
        }

        // добавить нпс из текущей локации
        public void NpcAddCurrentList(string npc)
        {
            if (NpcTotalList.Contains(npc))
            {
                NpcCurrentList.Add(npc);
            }
        }
        public void CreateTotalAndBlackList()
        {
            // Проверка для получения всех персонажей во всех локациях
            var npcList =  Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null); // Отфильтровываем возможные null значения

            foreach (var npc in npcList)
            {
                try
                {
                    if (npc.Schedule == null || !npc.Schedule.Any())
                    {
                        //Console.WriteLine($"У {npc.Name} нет пути", LogLevel.Warn);
                        if (!NpcBlackList.Contains(npc.Name))
                        {
                            NpcBlackList.Add(npc.Name);
                        }                       
                    }
                    else
                    {
                        //Console.WriteLine($"У {npc.Name} есть путь", LogLevel.Debug);
                        if (!NpcTotalList.Contains(npc.Name))
                        {
                            NpcTotalList.Add(npc.Name);
                        }
                    }                     
                }
                catch (Exception ex)
                {
                    modInstance.Monitor.Log($"ошибка { ex}", LogLevel.Warn);
                }
            }      
        }
        public void AddNpcToList(NPC npc) // Добавление в список нпс
        {
            NpcAddCurrentList(npc.Name);            
        }
    }
}

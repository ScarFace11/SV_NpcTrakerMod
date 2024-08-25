using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewValley;
namespace NpcTrackerMod
{
    public class NpcList
    {
        public HashSet<string> NpcVanillaList { get; }
        public HashSet<string> NpcBlackList { get; }
        public List<string> NpcTotalList { get; private set; }
        public List<string> NpcCurrentList { get; private set; }




        public NpcList()
        {
            NpcVanillaList = new HashSet<string> { "Evelyn", "George", "Alex", "Emily", "Haley", "Jodi", "Sam", "Vincent", "Clint", "Lewis", "Abigail",
                "Caroline", "Pierre", "Gus", "Pam", "Penny", "Harvey", "Elliott", "Demetrius", "Maru", "Robin", "Sebastian", "Linus", "Jas", "Marnie",
                "Shane", "Leah", "Krobus", "Sandy", "Marlon", "Willy", "Dwarf", "Krobus", "Bouncer", "Gunther", "Marlon", "Henchman", "Birdie", "Mister Qi" };
            NpcBlackList = new HashSet<string>();
            // "Dwarf", "Krobus", "Bouncer", "Gunther", "Marlon", "Henchman", "Birdie", "Mister Qi"
            NpcTotalList = new List<string>();
            NpcCurrentList = new List<string>();
        }

        public void NpcAddTotalList()
        {
            NpcTotalList = new List<string>();

            // Добавляем NPC в список NpcTotalList, если они не в черном списке
            NpcTotalList = NpcVanillaList.Except(NpcBlackList).ToList();
            NpcRemoveBlackTotalList();
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
                    Console.WriteLine($"ошибка { ex}", LogLevel.Warn);
                }
            }      
        }
        public void AddNpcToList(NPC npc) // Добавление в список нпс
        {
            NpcAddCurrentList(npc.Name);
            
        }
    }
}

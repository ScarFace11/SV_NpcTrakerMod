using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using System.IO;

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

        /// <summary>
        /// Словарь путей NPC на текущий день.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> NpcTotalToDayPath { get; private set; }

        /// <summary>
        /// Словарь глобальных путей NPC.
        /// </summary>
        public Dictionary<string, List<(string, List<Point>)>> NpcTotalGlobalPath { get; private set; }

        /// <summary>
        /// Конструктор класса NpcList.
        /// </summary>
        /// <param name="instance">Экземпляр основного мода.</param>

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
        public void AddNpcPath(NPC npc, Dictionary<string, List<(string, List<Point>)>> NpcTotalPath, bool Global)
        {
            // Проверка наличия данных для NPC
            if (!NpcTotalPath.ContainsKey(npc.Name))
            {
                // Если нет, создаем новую запись для NPC
                NpcTotalPath[npc.Name] = new List<(string, List<Point>)>();
                modInstance.Monitor.Log($"Добавлен нпс: {npc.Name}", LogLevel.Trace);
            }

            // Получаем список локаций для данного NPC
            var npcPaths = NpcTotalPath[npc.Name];
            var LocationAndPoint = Global
                ? modInstance.NpcManager.GetNpcGlobalRoutePoints(npc)
                : modInstance.NpcManager.GetNpcRoutePoints(npc);
            //modInstance.NpcManager.test(npc);

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
            var npcList =  Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null) // Отфильтровываем возможные null значения
                .ToList(); // Преобразуем в список
            foreach (var npc in npcList)
            {
                try
                {
                    // Проверка, есть ли у NPC расписание
                    if (npc.Schedule == null || !npc.Schedule.Any())
                    {
                        modInstance.Monitor.Log($"У {npc.Name} нет пути", LogLevel.Trace);
                        if (!NpcBlackList.Contains(npc.Name))
                        {
                            NpcBlackList.Add(npc.Name);
                        }                       
                    }
                    else if (!NpcTotalList.Contains(npc.Name))
                    {
                        NpcTotalList.Add(npc.Name);
                        //if (npc.Name == "Gus")
                        AddNpcPath(npc, NpcTotalToDayPath, false);

                        //if (npc.Name == "Lewis") 
                        AddNpcPath(npc, NpcTotalGlobalPath, true);


                    }                     
                }
                catch (Exception ex)
                {
                    modInstance.Monitor.Log($"Ошибка обработки NPC {npc.Name}: {ex.Message}", LogLevel.Warn);
                }
            }           
        }

        public string GetNpcFromList()
        {
            return NpcTotalToDayPath.FirstOrDefault(k => k.Key == NpcCurrentList[modInstance.NpcSelected]).Key;
        }

        public void LoadAllModSchedules()
        {
            // Получаем путь к папке, где установлены все моды
            string modsFolderPath = Path.Combine(modInstance.Helper.DirectoryPath, "..", "..", "Mods");

            // Проверяем, существует ли папка с модами
            if (!Directory.Exists(modsFolderPath))
            {
                modInstance.Monitor.Log("Папка с модами не найдена!", LogLevel.Error);
                return;
            }

            // Ищем папки модов
            var modFolders = Directory.GetDirectories(modsFolderPath);

            foreach (var modFolder in modFolders)
            {
                // Рекурсивный поиск папки "Schedules" в каждой папке мода
                string scheduleFolderPath = FindSchedulesFolder(modFolder);
                //modInstance.Monitor.Log($"{scheduleFolderPath}", LogLevel.Debug);
                if (!string.IsNullOrEmpty(scheduleFolderPath))
                {
                    // Ищем все файлы расписаний в папке "Schedules"
                    var scheduleFiles = Directory.GetFiles(scheduleFolderPath, "*.json", SearchOption.AllDirectories);

                    foreach (var scheduleFile in scheduleFiles)
                    {
                        try
                        {
                            // Загружаем расписание из файла
                            var schedule = modInstance.Helper.ModContent.Load<Dictionary<string, string>>(scheduleFile);

                            if (schedule != null)
                            {
                                //modInstance.Monitor.Log($"Загружено расписание из файла: {scheduleFile}", LogLevel.Info);
                            }
                        }
                        catch
                        {
                            //modInstance.Monitor.Log($"Ошибка при загрузке файла расписания: {ex.Message}", LogLevel.Error);
                            continue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Рекурсивно ищет папку "Schedules" в указанной директории.
        /// </summary>
        /// <param name="startDirectory">Путь к корневой папке мода.</param>
        /// <returns>Путь к найденной папке "Schedules" или пустую строку, если не найдена.</returns>
        private string FindSchedulesFolder(string startDirectory)
        {
            try
            {
                // Ищем все директории в текущей папке
                foreach (var directory in Directory.GetDirectories(startDirectory))
                {
                    // Проверяем, является ли текущая папка "Schedules"
                    if (Path.GetFileName(directory).Equals("Schedules", StringComparison.OrdinalIgnoreCase))
                    {
                        return directory; // Возвращаем путь к папке
                    }

                    // Рекурсивный вызов для вложенных папок
                    var foundFolder = FindSchedulesFolder(directory);
                    if (!string.IsNullOrEmpty(foundFolder))
                    {
                        return foundFolder;
                    }
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Ошибка при поиске папки Schedules: {ex.Message}", LogLevel.Error);
            }

            return string.Empty; // Возвращаем пустую строку, если папка не найдена
        }
    }
}

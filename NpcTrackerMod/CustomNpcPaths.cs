using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для управления пользовательскими маршрутами NPC.
    /// </summary>
    public class CustomNpcPaths
    {
        private readonly NpcTrackerMod modInstance;


        /// <summary>
        /// Конструктор для инициализации класса CustomNpcPaths.
        /// </summary>
        /// /// <param name="instance">Экземпляр мода.</param>
        public CustomNpcPaths(NpcTrackerMod instance)
        {
            modInstance = instance;
        }
        private string NpcName;

        //private Dictionary<string, Dictionary<string, string>> NpcPaths = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, List<string>>> NpcPaths = new Dictionary<string, Dictionary<string, List<string>>>();

        /// <summary>
        /// Загружает все расписания модов.
        /// </summary>
        public void LoadAllModSchedules()
        {
            try
            {
                string modsFolderPath = GetModsFolderPath();
                var modFolders = Directory.GetDirectories(modsFolderPath);

                foreach (var modFolder in modFolders)
                {
                    
                    string scheduleFolderPath = FindSchedulesFolder(modFolder);
                    if (string.IsNullOrEmpty(scheduleFolderPath)) continue;

                    LoadSchedulesFromFolder(scheduleFolderPath);
                }
                modInstance.Monitor.Log($"Все кастомные нпс добавлены!", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Ошибка при загрузке расписаний: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Получает путь к папке модов.
        /// </summary>
        /// <returns>Путь к папке модов.</returns>
        private string GetModsFolderPath()
        {
            return Path.Combine(modInstance.Helper.DirectoryPath, "..", "..", "Mods");
        }

        /// <summary>
        /// Загружает расписания из указанной папки.
        /// </summary>
        /// <param name="scheduleFolderPath">Путь к папке с расписаниями.</param>
        private void LoadSchedulesFromFolder(string scheduleFolderPath)
        {
            var scheduleFiles = Directory.GetFiles(scheduleFolderPath, "*.json", SearchOption.AllDirectories);

            foreach (var scheduleFile in scheduleFiles)
            {
                if (File.Exists(scheduleFile))
                {
                    LoadScheduleFile(scheduleFile);
                }
            }
        }

        /// <summary>
        /// Загружает расписание из файла.
        /// </summary>
        /// <param name="scheduleFile">Путь к файлу расписания.</param>
        private void LoadScheduleFile(string scheduleFile)
        {
            try
            {
                string jsonContent = RemoveJsonComments(File.ReadAllText(scheduleFile));
                var schedule = JsonConvert.DeserializeObject<JObject>(jsonContent);

                // Получаем имя NPC
                NpcName = ExtractNpcNameFromFile(schedule, scheduleFile);

                //modInstance.Monitor.Log($"{NpcName}", LogLevel.Debug);

                if (schedule != null)
                {
                    

                    if (schedule.TryGetValue("Changes", out var changesToken))
                    {
                        ProcessChanges(changesToken, scheduleFile);
                    }
                    else
                    {
                        AddNpcCustomPath(schedule);
                        // Обработка файла без 'Changes'
                        //ProcessEntriesWithoutChanges(scheduleFile, npcName);

                        //ProcessChanges(null, scheduleFile);
                        //foreach (var i in schedule)
                        //{
                        //    //modInstance.Monitor.Log($"{i.Key}: {i.Value}", LogLevel.Debug);
                        //}
                    }

                }
                else
                {
                    modInstance.Monitor.Log($"Файл {scheduleFile} пропущен, так как содержит некорректные данные.", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Ошибка при загрузке файла расписания {scheduleFile}: {ex.Message}", LogLevel.Error);
            }
        }
        /// <summary>
        /// Обрабатывает изменения из расписания.
        /// </summary>
        /// <param name="changesToken">Токен изменений из JSON.</param>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        private void ProcessChanges(JToken changesToken, string scheduleFile)
        {
            var changesArray = changesToken as JArray;

            if (changesArray != null)
            {
                foreach (var change in changesArray)
                {
                    if (change["Entries"] != null)
                    {
                        var entriesToken = change["Entries"];

                        if (ContainsI18nTokens(entriesToken))
                        {
                            //modInstance.Monitor.Log($"Файл {scheduleFile} пропущен, так как содержит i18n записи.", LogLevel.Warn);
                            continue;
                        }
                        ProcessEntries(change["Entries"], scheduleFile);
                    }
                    else
                    {
                        modInstance.Monitor.Log($"Секция 'Entries' не найдена в {scheduleFile}.", LogLevel.Warn);
                    }
                }
            }
            else
            {
                modInstance.Monitor.Log($"Секция 'Changes' в {scheduleFile} содержит невалидные данные.", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Извлекает имя NPC из файла расписания, проверяя как 'Changes', так и имя файла или папки.
        /// </summary>
        /// <param name="schedule">Десериализованный объект расписания.</param>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        /// <returns>Имя NPC.</returns>
        private string ExtractNpcNameFromFile(JObject schedule, string scheduleFile)
        {
            string npcName = null;

            // Если есть 'Changes', проверяем Target
            if (schedule.TryGetValue("Changes", out var changesToken))
            {
                var changesArray = changesToken as JArray;
                if (changesArray != null)
                {
                    foreach (var change in changesArray)
                    {
                        npcName = ExtractNpcName(change, scheduleFile);
                        if (!string.IsNullOrEmpty(npcName))
                            return npcName; // Возвращаем, как только нашли имя
                    }
                }
            }

            // Если 'Changes' нет, или Target не найден, возвращаем имя из файла или папки
            return ExtractNpcNameFromFileNameOrFolder(scheduleFile);
        }

        /// <summary>
        /// Извлекает имя NPC из 'Target' в Change или из имени файла/папки.
        /// </summary>
        /// <param name="change">Текущий Change объект.</param>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        /// <returns>Имя NPC.</returns>
        private string ExtractNpcName(JToken change, string scheduleFile)
        {
            if (change["Target"] != null)
            {
                string target = change["Target"].ToString();
                string npcName = target.Split('/').Last();  // Получаем последнее значение после '/'
                return npcName;
            }
            return null;
        }

        /// <summary>
        /// Извлекает имя NPC из имени файла или папки.
        /// </summary>
        /// <param name="scheduleFile">Путь к файлу расписания.</param>
        /// <returns>Имя NPC.</returns>
        private string ExtractNpcNameFromFileNameOrFolder(string scheduleFile)
        {
            // Извлекаем имя файла без расширения
            string fileName = Path.GetFileNameWithoutExtension(scheduleFile);

            // Если файл называется 'Schedule' или 'schedule', возвращаем имя папки
            if (fileName.Equals("Schedule", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(Path.GetDirectoryName(scheduleFile));
            }

            // Иначе возвращаем имя файла
            return fileName;
        }

        /// <summary>
        /// Обрабатывает расписание, если 'Changes' отсутствуют.
        /// </summary>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        /// <param name="npcName">Имя NPC.</param>
        private void ProcessEntriesWithoutChanges(string scheduleFile, string npcName)
        {
            // Здесь можно обработать файл без 'Changes', используя имя NPC.
            modInstance.Monitor.Log($"Обрабатываю файл {scheduleFile} для NPC {npcName} без 'Changes'.", LogLevel.Info);
        }

        /// <summary>
        /// Проверяет, содержит ли записи i18n-токены.
        /// </summary>
        private bool ContainsI18nTokens(JToken entriesToken)
        {
            if (entriesToken.Type == JTokenType.Object)
            {
                var entries = entriesToken as JObject;
                foreach (var entry in entries)
                {
                    if (entry.Value.ToString().Contains("{{i18n:"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Обрабатывает записи "Entries" в изменении.
        /// </summary>
        /// <param name="entriesToken">Токен записей.</param>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        private void ProcessEntries(JToken entriesToken, string scheduleFile)
        {
            if (entriesToken.Type == JTokenType.Object)
            {                
                var entries = entriesToken as JObject;

                AddNpcCustomPath(entries);
                //foreach (var entry in entries)
                //{
                //    //modInstance.Monitor.Log($"Ключ: {entry.Key}, Значение: {entry.Value}", LogLevel.Debug);
                //}
            }
            else
            {
                modInstance.Monitor.Log($"Секция 'Entries' не является объектом, её тип: {entriesToken.Type}. Пропускаем...", LogLevel.Warn);
            }
        }


        /// <summary>
        /// Удаляет комментарии из JSON.
        /// </summary>
        /// <param name="json">Строка JSON.</param>
        /// <returns>JSON без комментариев.</returns>
        private string RemoveJsonComments(string json)
        {
            // Удаляем однострочные комментарии, включая комментарии внутри JSON-объектов
            json = System.Text.RegularExpressions.Regex.Replace(json, @"//.*?(?=\r?$)", "", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Удаляем многострочные комментарии
            json = System.Text.RegularExpressions.Regex.Replace(json, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);

            return json;
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
                return Directory.EnumerateDirectories(startDirectory, "*", SearchOption.AllDirectories)
                    .FirstOrDefault(directory => Path.GetFileName(directory).Equals("Schedules", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Ошибка при поиске папки Schedules: {ex.Message}", LogLevel.Error);
                return string.Empty; // Возвращаем пустую строку, если папка не найдена
            }
        }

        private void AddNpcCustomPath(JObject Schedules)
        {
            // Проверка наличия данных для NPC
            if (!NpcPaths.ContainsKey(NpcName))
            {
                // Если нет, создаем новую запись для NPC
                NpcPaths[NpcName] = new Dictionary<string, List<string>>();

                modInstance.Monitor.Log($"Добавлен кастомный нпс: {NpcName}", LogLevel.Trace);
            }

            foreach (var entry in Schedules)
            {
                // Проверяем, есть ли ключ для расписания
                if (!NpcPaths[NpcName].ContainsKey(entry.Key))
                {
                    // Если нет, создаем новый список для этого расписания
                    NpcPaths[NpcName][entry.Key] = new List<string>();
                }
                // Теперь можно добавить новое значение в список
                NpcPaths[NpcName][entry.Key].Add(entry.Value.ToString());

            }
        }

        public void TransferPath()
        {
            // Перебираем все NPC в словаре NpcPaths
            foreach (var npcEntry in NpcPaths)
            {
                string npcName = npcEntry.Key; // Имя NPC
                Dictionary<string, List<string>> npcSchedule = npcEntry.Value; // Расписание NPC

                // Перебираем расписания для каждого NPC
                foreach (var scheduleEntry in npcSchedule)
                {
                    string scheduleDay = scheduleEntry.Key; // Ключ расписания 
                    List<string> paths = scheduleEntry.Value; // Список путей для конкретного дня

                    // Перебираем и выводим все пути для этого расписания
                    foreach (var path in paths)
                    {
                        if (npcName == "Sophia") //"Peaches")
                        {
                            //modInstance.Monitor.Log($"name: {npcName}, key: {scheduleDay}, path: {path}", LogLevel.Debug);
                            modInstance.NpcManager.GetNpcGlobalRoutePoints(null, npcName, path, scheduleDay);
                        }

                    }
                }
            }
        }
    }
}

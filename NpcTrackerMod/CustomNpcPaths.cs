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

                if (schedule != null)
                {
                    if (schedule.TryGetValue("Changes", out var changesToken))
                    {
                        ProcessChanges(changesToken, scheduleFile);
                    }
                    //else
                    //{
                    //    //ProcessScheduleFile(scheduleFile);
                    //    foreach(var i in schedule)
                    //    {
                    //        //modInstance.Monitor.Log($"{i.Key}: {i.Value}", LogLevel.Debug);
                    //    }
                    //}

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
                        //ProcessEntries(change["Entries"], scheduleFile);
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
        /// Обрабатывает записи "Entries" в изменении.
        /// </summary>
        /// <param name="entriesToken">Токен записей.</param>
        /// <param name="scheduleFile">Имя файла расписания.</param>
        private void ProcessEntries(JToken entriesToken, string scheduleFile)
        {
            if (entriesToken.Type == JTokenType.Object)
            {
                var entries = entriesToken as JObject;
                foreach (var entry in entries)
                {
                    //modInstance.Monitor.Log($"Ключ: {entry.Key}, Значение: {entry.Value}", LogLevel.Debug);
                }
            }
            else
            {
                modInstance.Monitor.Log($"Секция 'Entries' не является объектом, её тип: {entriesToken.Type}. Пропускаем...", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Загружает все расписания модов.
        /// </summary>
        public void LoadAllModSchedules2()
        {
            try
            {
                // Получаем путь к папке, где установлены все моды
                string modsFolderPath = Path.Combine(modInstance.Helper.DirectoryPath, "..", "..", "Mods");

                // Ищем папки модов
                var modFolders = Directory.GetDirectories(modsFolderPath);

                // Проходим по всем папкам модов
                foreach (var modFolder in modFolders)
                {
                    // Рекурсивный поиск папки "Schedules" в каждой папке мода
                    string scheduleFolderPath = FindSchedulesFolder(modFolder);

                    if (string.IsNullOrEmpty(scheduleFolderPath))
                    {
                        continue; // Пропускаем моды без папки "Schedules"
                    }

                    // Ищем все файлы расписаний в папке "Schedules"
                    var scheduleFiles = Directory.GetFiles(scheduleFolderPath, "*.json", SearchOption.AllDirectories);

                    foreach (var scheduleFile in scheduleFiles)
                    {
                        if (File.Exists(scheduleFile))
                        {
                            try
                            {
                                // Загружаем содержимое файла как строку
                                string jsonContent = File.ReadAllText(scheduleFile);

                                // Удаляем комментарии
                                jsonContent = RemoveJsonComments(jsonContent);

                                // Пробуем десериализовать как Dictionary<string, object>
                                var schedule = JsonConvert.DeserializeObject<JObject>(jsonContent);

                                // Проверяем, содержит ли файл действительные ключи для расписания NPC
                                if (schedule != null)
                                {
                                    // Проверяем, есть ли секция "Changes"
                                    if (schedule.TryGetValue("Changes", out var changesToken))
                                    {
                                        var changesArray = changesToken as JArray;
                                        if (changesArray != null)
                                        {
                                            foreach (var change in changesArray)
                                            {
                                                // Выводим тип данных "Entries"
                                                modInstance.Monitor.Log($"Тип Entries: {change["Entries"]?.Type}", LogLevel.Debug);

                                                // Проверяем, является ли "Entries" объектом
                                                if (change["Entries"] != null)
                                                {
                                                    if (change["Entries"].Type == JTokenType.Object)
                                                    {
                                                        var entries = change["Entries"] as JObject;
                                                        if (entries != null)
                                                        {
                                                            foreach (var entry in entries)
                                                            {
                                                                modInstance.Monitor.Log($"Ключ: {entry.Key}, Значение: {entry.Value}", LogLevel.Debug);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        modInstance.Monitor.Log($"Секция 'Entries' не является объектом, её тип: {change["Entries"].Type}. Пропускаем...", LogLevel.Warn);
                                                    }
                                                }
                                                else
                                                {
                                                    modInstance.Monitor.Log($"Секция 'Entries' отсутствует в {scheduleFile}.", LogLevel.Warn);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            modInstance.Monitor.Log($"Секция 'Changes' в {scheduleFile} содержит невалидные данные.", LogLevel.Warn);
                                        }
                                    }
                                }
                                else
                                {
                                    modInstance.Monitor.Log($"Файл {scheduleFile} пропущен, так как содержит некорректные данные.", LogLevel.Warn);
                                }
                                //var changes = schedule["Changes"] as JArray;
                                //if (changes != null)
                                //{
                                //    foreach (var change in changes)
                                //    {
                                //        // Проверяем, есть ли секция "Entries"
                                //        if (change["Entries"] != null)
                                //        {
                                //            //var entries = change["Entries"] as JObject;
                                //            //if (entries != null)
                                //            //{
                                //            //    //// Обрабатываем данные из "Entries"
                                //            //    //try
                                //            //    //{
                                //            //    //    foreach (var entry in entries)
                                //            //    //    {
                                //            //    //        modInstance.Monitor.Log($"Entry key: {entry.Key}, Value: {entry.Value}", LogLevel.Debug);
                                //            //    //    }
                                //            //    //}
                                //            //    //catch (Exception ex)
                                //            //    //{
                                //            //    //    modInstance.Monitor.Log($"Ошибка при переборе маршрутов: {ex.Message}", LogLevel.Error);
                                //            //    //}
                                //            //}
                                //            //else
                                //            //{
                                //            //    modInstance.Monitor.Log($"Секция 'Entries' в {scheduleFile} отсутствует или имеет неверный формат.", LogLevel.Warn);
                                //            //}
                                //        }
                                //        else
                                //        {
                                //            modInstance.Monitor.Log($"Секция 'Entries' не найдена в {scheduleFile}. Возможно, это другой тип действия.", LogLevel.Warn);
                                //        }
                                //    }
                                //}

                                //else
                                //{
                                //    //foreach (var item in schedule)
                                //    //{
                                //    //    modInstance.Monitor.Log($"Basic key {item.Key}, Value: {item.Value}", LogLevel.Debug);
                                //    //}
                                //    //modInstance.Monitor.Log($"Файл {scheduleFile} содержит обычное расписание или другую структуру данных.", LogLevel.Warn);
                                //}
                            }
                            catch (Exception ex)
                            {
                                modInstance.Monitor.Log($"Ошибка при загрузке файла расписания {scheduleFile}: {ex.Message}", LogLevel.Error);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Ошибка при загрузке расписаний: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Удаляет комментарии из JSON.
        /// </summary>
        /// <param name="json">Строка JSON.</param>
        /// <returns>JSON без комментариев.</returns>
        private string RemoveJsonComments(string json)
        {
            modInstance.Monitor.Log($"до: {json}", LogLevel.Debug);
            // Удаляем однострочные комментарии, включая комментарии внутри JSON-объектов
            json = System.Text.RegularExpressions.Regex.Replace(json, @"//.*?(?=\r?$)", "", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Удаляем многострочные комментарии
            json = System.Text.RegularExpressions.Regex.Replace(json, @"/\*.*?\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);

            modInstance.Monitor.Log($"после: {json}", LogLevel.Debug);
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


        /// <summary>
        /// Обрабатывает файл расписания, проверяя его соответствие NPC или названию Schedule.
        /// </summary>
        private void ProcessScheduleFile(string scheduleFile)
        {
            string filename = Path.GetFileNameWithoutExtension(scheduleFile);
            string lastFolderName = Path.GetFileName(Path.GetDirectoryName(scheduleFile));

            var npcFromFile = modInstance.NpcList.NPClist.FirstOrDefault(npc => npc.Name.Equals(filename, StringComparison.OrdinalIgnoreCase));

            if (npcFromFile != null)
            {
                modInstance.Monitor.Log($"Файл для NPC '{filename}' найден и загружен.", LogLevel.Info);
            }
            else if (filename.ToLower().Contains("schedule"))
            {
                ProcessScheduleFolder(lastFolderName, filename);
            }
            else
            {
                modInstance.Monitor.Log($"Файл '{filename}' не содержит ни 'Schedule', ни имени NPC.", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Обрабатывает расписание, если оно находится в папке с названием NPC или содержащей "Schedule".
        /// </summary>
        private void ProcessScheduleFolder(string folderName, string filename)
        {
            var npcFromFolder = modInstance.NpcList.NPClist.FirstOrDefault(npc => npc.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (npcFromFolder != null)
            {
                modInstance.Monitor.Log($"Расписание для NPC '{folderName}' найдено в папке '{folderName}, из фалйа {filename}'!", LogLevel.Info);
            }
            else
            {
                modInstance.Monitor.Log($"Не удалось сопоставить NPC с файлом '{filename}' в папке '{folderName}'.", LogLevel.Warn);
            }
        }
    }
}

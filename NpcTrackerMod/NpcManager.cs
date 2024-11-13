using System;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Pathfinding;
using System.Text.RegularExpressions;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для управления NPC: отслеживание, фильтрация маршрутов и работа со списками NPC.
    /// </summary>
    public class NpcManager
    {
        private readonly NpcTrackerMod modInstance;

        private string LastLocationName;
        private string EndLocationName;


        /// <summary>
        /// Инициализирует экземпляр <see cref="NpcManager"/> с указанной ссылкой на мод.
        /// </summary>
        /// <param name="instance">Ссылка на экземпляр мода NpcTrackerMod.</param>
        public NpcManager(NpcTrackerMod instance)
        {
            modInstance = instance;
        }

        /// <summary>
        /// Получает список NPC, которые должны отслеживаться.
        /// </summary>
        /// <param name="trackAllLocations">Флаг, указывающий, нужно ли отслеживать NPC во всех локациях.</param>
        /// <param name="NpcList">Список имен NPC для отслеживания.</param>
        /// <returns>Коллекция NPC для отслеживания.</returns>
        public IEnumerable<NPC> GetNpcsToTrack(bool trackAllLocations, List<string> NpcList)
        {
            // Возвращает всех NPC в текущей локации
            if (!trackAllLocations)
                return Game1.currentLocation?.characters
                .Where(npc => NpcList.Contains(npc.Name))
                ?? Enumerable.Empty<NPC>();

            // Возвращает всех NPC во всех локациях
            return Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null && NpcList.Contains(npc.Name)); // Отфильтровываем возможные null значения
        }

        /// <summary>
        /// Получает глобальные маршруты NPC на основе всех их расписаний.
        /// </summary>
        /// <param name="npc">NPC, для которого требуется получить маршруты.</param>
        /// <returns>Список пар, где строка — это название локации, а список точек — это маршрут NPC.</returns>
        public void ProcessNpcGlobalRoute(NPC currentNpc, string npcName, string path, string pathkey)
        {
            // Находим NPC
            NPC npc = currentNpc ?? FindNpcByName(npcName);

            // Проверяем, существует ли NPC
            if (npc == null)
            {
                modInstance.Monitor.Log($"NPC {npcName} не найден.", LogLevel.Warn);
                return;
            }

            // Проверяем наличие расписания у NPC
            if (npc.Schedule?.Any() != true)
            {
                //modInstance.Monitor.Log($"NPC {npc.Name} Не имеет пути.", LogLevel.Warn);
                //modInstance.NpcList.AddNpcBlackList(npc.Name);
                return;
            }

            // Получаем расписание NPC
            Dictionary<string, string> masterSchedule = GetMasterSchedule(npc, path, pathkey);

            // Добавляем NPC в список, если его там еще нет
            if (!modInstance.NpcList.NpcTotalList.Contains(npc.Name))
            {
                modInstance.NpcList.NpcTotalList.Add(npc.Name);
            }

            var totalNpcPath = new List<(string, List<Point>)>();

            try
            {
                foreach (var schedule in masterSchedule)
                {
                    string key = schedule.Key;
                    var rawData = schedule.Value;

                    bool monitor = false;

                    //if (key != "DesertFestival_3") continue;
                    //modInstance.Monitor.Log($"{npc.Name} rawData: {rawData}", LogLevel.Info);
                    
                    // Пропускаем некорректные или нежелательные данные расписания
                    if (!IsValidScheduleEntry(key, rawData))
                    {                       
                        modInstance.Monitor.Log($"У НПС {npc.Name} пропуск неверного или проблемного ключа расписания: {key}", LogLevel.Warn);
                        modInstance.Monitor.Log($"{rawData}", LogLevel.Debug);
                        
                        continue;
                    }

                    if (monitor) modInstance.Monitor.Log($"{npc.Name} Выбрано расписание: {key}:{rawData}", LogLevel.Debug);

                    // Парсинг расписания и обработка маршрутов
                    try
                    {
                        // Разделяем данные расписания по "/"
                        var scheduleEntries = rawData.Split('/');
                        var lastLocation = npc.currentLocation?.Name;
                        var npcLocation = EndLocationName == null ? Game1.locations.FirstOrDefault(loc => loc.Name == npc.currentLocation?.Name) : Game1.locations.FirstOrDefault(loc => loc.Name == EndLocationName);
                        EndLocationName = null;
                        if (npcLocation == null)
                        {
                            modInstance.Monitor.Log($"Не известно где находится {npc.Name}", LogLevel.Warn);
                            return;
                        }

                        int npcX = (int)npcLocation.characters.FirstOrDefault(n => n.Name == npc.Name)?.TilePoint.X;
                        int npcY = (int)npcLocation.characters.FirstOrDefault(n => n.Name == npc.Name)?.TilePoint.Y;

                        LastLocationName = null;

                        bool first = true;
                        
                        //if (key == "DesertFestival_3") first = true;

                        foreach (var entry in scheduleEntries)
                        {
                            if (monitor) modInstance.Monitor.Log($"0) Начат парсинг пути: {entry}", LogLevel.Info);

                            // Разделяем по пробелу: "время локация x y ..."
                            string[] entryParts = entry.Split(' ');

                            if (entry.Contains("MAIL") || entry.Contains("friendship") || entry.Contains("GOTO") || entry.Contains("NO_SCHEDULE"))  continue;
                            // Проверяем минимальное количество полей
                            //if (entryParts.Length < 5) continue;
                            if (entryParts.Length <= 2) continue;

                           

                            ParseNpcScheduleEntry(entryParts, entry, masterSchedule, out string time, out string locationName, out int x, out int y, out int facingDirection, out string endBehavior, out string endMessage);

                            if (monitor)  modInstance.Monitor.Log($"{time} {locationName} {x} {y} {facingDirection} {endBehavior} {endMessage}",LogLevel.Debug);

                            if (monitor)  if (first) MessageRoute("1) Данные для патч финда", Convert.ToInt32(time), new Point(npcX, npcY), locationName, new Point(x, y), facingDirection, endBehavior, endMessage);

                            try
                            {

                                if (monitor)  if (first) modInstance.Monitor.Log($"2) Попытка найти путь от {lastLocation} ({npcX}, {npcY}) к {locationName} ({x}, {y}) для NPC '{npc.Name}'", LogLevel.Debug);



                                SchedulePathDescription pathDescription = npc.pathfindToNextScheduleLocation(
                                                                time,
                                                                lastLocation,
                                                                npcX,
                                                                npcY,
                                                                locationName,
                                                                x,
                                                                y,
                                                                facingDirection,
                                                                endBehavior,
                                                                endMessage
                                                                ) ?? null;


                                if (pathDescription != null)
                                {
                                    

                                    if (monitor) if (first)
                                        {
                                            MessageRoute("3) Pathfinding", pathDescription.time, new Point(npcX, npcY), pathDescription.targetLocationName, pathDescription.targetTile, pathDescription.facingDirection, pathDescription.endOfRouteBehavior, pathDescription.endOfRouteMessage);

                                            modInstance.Monitor.Log($"path: {pathDescription.time} {pathDescription.targetLocationName} {pathDescription.targetTile} {pathDescription.facingDirection} {pathDescription.endOfRouteBehavior} {pathDescription.endOfRouteMessage}", LogLevel.Debug);

                                        }

                                    //MessageRoute("3) Pathfinding", pathDescription.time, new Point(npcX, npcY), pathDescription.targetLocationName, pathDescription.targetTile, pathDescription.facingDirection, pathDescription.endOfRouteBehavior, pathDescription.endOfRouteMessage);

                                    // Логирование успешного поиска пути


                                    //modInstance.Monitor.Log($"4) Pathfinding успешно завершен для {npc.Name}: {pathDescription.targetLocationName} -> {pathDescription.targetTile}", LogLevel.Debug);


                                    // Добавляем отфильтрованный маршрут в общий список
                                    if (pathDescription.route != null)
                                    {
                                        totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation?.Name, pathDescription.route));
                                    }
                                    else
                                    {
                                        continue;
                                        //modInstance.Monitor.Log($"{npc.Name}, {key} Путь {entry} пуст", LogLevel.Warn);
                                    }
                                    if (monitor) modInstance.Monitor.Log($"----------------------------------------------------------", LogLevel.Debug);
                                    // Обновляем текущие координаты NPC и локацию
                                    lastLocation = locationName;
                                    npcX = x;
                                    npcY = y;
                                }
                            }
                            catch (Exception ex)
                            {
                                modInstance.Monitor.Log($"Ошибка при поиске маршурта от {lastLocation} XY {npcX} {npcY} до '{time}':{locationName} XY:{x} {y} для '{npc.Name}': {ex.Message}", LogLevel.Error);


                                // В случае ошибки, обновляем lastLocation на текущую locationName
                                lastLocation = locationName;

                                // Обновляем координаты NPC
                                npcX = x;
                                npcY = y;

                                continue; // Переход к следующему маршруту
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        modInstance.Monitor.Log($"Error parsing schedule '{key}' for NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
                        continue; // Переход к следующему расписанию
                    }
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Unexpected error while processing NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
            }

            LastLocationName = null; // Сброс последней локации
            modInstance.NpcList.AddNpcPath(npc, modInstance.NpcList.NpcTotalGlobalPath, totalNpcPath);
        }

        private NPC FindNpcByName(string npcName)
        {
            return modInstance.NpcList.NPClist.FirstOrDefault(npc => npc.Name.IndexOf(npcName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private Dictionary<string, string> GetMasterSchedule(NPC npc, string path, string pathKey)
        {
            if (npc.Schedule != null && npc.Schedule.Any())
            {
                return npc.getMasterScheduleRawData();
            } // оказывается модовые npc отображаются и вызывается этот метод из кастомнпспатча 
            else
            {
                var schedule = new Dictionary<string, string>();
                schedule[pathKey] = path;

                return schedule;
            }
        }

        // Функция для извлечения информации о времени, локации, координатах и действиях NPC
        private void ParseNpcScheduleEntry(
            string[] scheduleParts,
            string scheduleEntry,
            Dictionary<string, string> npcScheduleData,
            out string time,
            out string locationName,
            out int x,
            out int y,
            out int facingDirection,
            out string endBehavior,
            out string endMessage)
        {
            // Устанавливаем значения по умолчанию
            time = "0";
            locationName = string.Empty;
            x = y = facingDirection = 0;
            endBehavior = null;
            endMessage = null;

            //string PastLocatinon = string.Empty;

            int currentIndex = 0; // Индекс для прохода по элементам scheduleParts


            // Обработка расписания в стандартном формате
            //time = Regex.Match(scheduleParts[0], @"\d+").Value;

            // 1 key
            // Проверка первого элемента — это время или название локации?
            if (int.TryParse(Regex.Match(scheduleParts[currentIndex], @"\d+").Value, out _))
            {
                // Если первый элемент — это время
                time = Regex.Match(scheduleParts[currentIndex], @"\d+").Value;
                currentIndex++;
                //modInstance.Monitor.Log($"1.1 {time}", LogLevel.Debug);
            }
            else
            {
                // Если это не время, значит, это название локации
                //locationName = scheduleParts[currentIndex];
                bool NotLocation = int.TryParse(scheduleParts[currentIndex], out _);
                if (NotLocation)
                {
                    locationName = LastLocationName;
                }
                else
                {
                    locationName = scheduleParts[currentIndex];
                    //PastLocatinon = locationName;
                }

                currentIndex++;
                //modInstance.Monitor.Log($"1.2 {locationName}", LogLevel.Debug);
            }

            // 2 location
            // Если название локации еще не установлено, то второй элемент — это локация
            if (string.IsNullOrEmpty(locationName))
            {
                //locationName = scheduleParts[currentIndex];
                
                bool NotLocation = int.TryParse(scheduleParts[currentIndex], out _);
                if (NotLocation)
                {
                    locationName = LastLocationName;
                    
                }
                else
                {
                    locationName = scheduleParts[currentIndex];
                    //PastLocatinon = locationName;
                    currentIndex++;
                }
                
                //modInstance.Monitor.Log($"2 {locationName}", LogLevel.Debug);
            }

            // 3,4 X Y
            // Дальше идут координаты X и Y
            if (scheduleParts.Length > currentIndex + 1)
            {
                x = int.Parse(scheduleParts[currentIndex]);
                y = int.Parse(scheduleParts[currentIndex + 1]);
                currentIndex += 2;
                //modInstance.Monitor.Log($"3 {x} {y}", LogLevel.Debug);
            }

            // 4 FacingDiretcion
            // Устанавливаем направление взгляда (по умолчанию 2)
            if (scheduleParts.Length > currentIndex && int.TryParse(scheduleParts[currentIndex], out var direction))
            {
                facingDirection = direction;
                currentIndex++;
                //modInstance.Monitor.Log($"4 {facingDirection}", LogLevel.Debug);
            }
            else
            {
                facingDirection = 2; // Значение по умолчанию
                //modInstance.Monitor.Log($"4.2 {facingDirection}", LogLevel.Debug);
            }

            // 5 Animation or Message           
            // Извлекаем поведение или сообщение, если есть
            if (scheduleParts.Length > currentIndex)
            {
                string potentialValue = scheduleParts[currentIndex];

                // Проверка на наличие строки с кавычками для endMessage
                if (potentialValue.StartsWith("\"Strings\\"))
                {
                    endMessage = potentialValue;
                    //modInstance.Monitor.Log($"5.1 {endMessage}", LogLevel.Debug);
                }
                else
                {
                    endBehavior = potentialValue;
                    //modInstance.Monitor.Log($"5.2 {endBehavior}", LogLevel.Debug);
                }
                currentIndex++;
            }

            // 6 Message
            // Проверка на наличие endMessage, если он идет следом за endBehavior
            if (scheduleParts.Length > currentIndex)
            {
                string potentialMessage = scheduleParts[currentIndex];
                if (potentialMessage.StartsWith("\"Strings\\"))
                {
                    endMessage = potentialMessage;
                    //modInstance.Monitor.Log($"6 {endMessage}", LogLevel.Debug);
                }
            }

            //// Извлекаем поведение, если есть
            //if (scheduleParts.Length > currentIndex)
            //{
            //    endBehavior = scheduleParts[currentIndex];
            //    currentIndex++;
            //}

            //// Проверяем наличие сообщения с кавычками (endMessage)
            //if (scheduleParts.Length > currentIndex)
            //{
            //    endMessage = GetEndMessageFromEntry(scheduleParts, currentIndex);
            //}


            //// Проверяем, является ли первый элемент временем
            //if (!int.TryParse(time, out _))
            //{
            //    //modInstance.Monitor.Log($"1 ", LogLevel.Debug);
            //    time = "0"; // Если время не указано, используем "0" по умолчанию
            //    locationName = scheduleParts[0];
            //    EndLocationName = locationName;
            //    x = int.Parse(scheduleParts[1]);
            //    y = int.Parse(scheduleParts[2]);

            //    // Извлекаем направление взгляда или используем 2 по умолчанию
            //    facingDirection = scheduleParts.Length > 3 && int.TryParse(scheduleParts[3], out var direction) ? direction : 2;

            //    // Извлекаем поведение и сообщение, если они есть
            //    endBehavior = partsCount > 4 ? scheduleParts[4] : null;

            //    // Проверяем наличие строки с кавычками для endMessage
            //    endMessage = partsCount > 5 ? GetEndMessageFromEntry(scheduleParts, 5) : null;
            //}
            //else
            //{
            //    //modInstance.Monitor.Log($"2 ", LogLevel.Debug);

            //    string locationEntry = scheduleParts[1];

            //    modInstance.Monitor.Log($"{locationEntry}", LogLevel.Debug);
            //    // Проверяем, является ли локация числом (например, идентификатором)
            //    if (int.TryParse(locationEntry, out _))
            //    {
            //        // Если это число, пробуем найти правильную локацию
            //        locationName = ResolveLocationName(locationEntry, scheduleEntry, npcScheduleData);
            //        modInstance.Monitor.Log($"{locationName}", LogLevel.Debug);
            //        shift = 1;
            //    }
            //    else
            //    {
            //        locationName = scheduleParts[1];
            //    }

            //    // Извлекаем координаты и направление
            //    x = int.Parse(scheduleParts[2 - shift]);
            //    y = int.Parse(scheduleParts[3 - shift]);

            //    modInstance.Monitor.Log($"{x} {y}", LogLevel.Debug);

            //    facingDirection = int.TryParse(scheduleParts[4 - shift], out var direction) ? direction : 2;

            //    modInstance.Monitor.Log($"{facingDirection}", LogLevel.Debug);
            //    // Извлекаем поведение и сообщение, если они есть

            //    endBehavior = partsCount > 5 - shift ? scheduleParts[5 - shift] : null;

            //    modInstance.Monitor.Log($"{endBehavior}", LogLevel.Debug);
            //    endMessage = partsCount > 6 - shift ? GetEndMessageFromEntry(scheduleParts, 6 - shift) : null;
            //    modInstance.Monitor.Log($"{endMessage}", LogLevel.Debug);
            //}          
        }

        

        private void MessageRoute(string info, int time, Point NpcTile, string targetLocationName, Point targetTile, int facingDirection, string endOfRouteBehavior, string endOfRouteMessage)
        {
            modInstance.Monitor.Log($"{info}", LogLevel.Info);
            modInstance.Monitor.Log($"Время(key): {time}", LogLevel.Info);
            if(NpcTile != null || NpcTile.X != 0) modInstance.Monitor.Log($"корды нпс: {NpcTile.X} {NpcTile.Y}", LogLevel.Info);
            modInstance.Monitor.Log($"локация куда идти: {targetLocationName}", LogLevel.Info);
            modInstance.Monitor.Log($"конечный XY: {targetTile}", LogLevel.Info);
            modInstance.Monitor.Log($"Направление взгляда: {facingDirection}", LogLevel.Info);
            modInstance.Monitor.Log($"endBehavior: {endOfRouteBehavior}", LogLevel.Info);
            modInstance.Monitor.Log($"endMessage: {endOfRouteMessage}", LogLevel.Info);
        }

        /// <summary>
        /// Проверяет корректность записи расписания NPC.
        /// </summary>
        /// <param name="key">Ключ записи расписания.</param>
        /// <param name="rawData">Сырые данные расписания.</param>
        /// <returns>Возвращает true, если запись валидна, иначе false.</returns>
        private bool IsValidScheduleEntry(string key, string rawData)
        {
            return !string.IsNullOrWhiteSpace(key) && rawData.Contains(" ");

            //return !string.IsNullOrWhiteSpace(key) && rawData.Contains(" ") &&
            //       !rawData.Contains("MAIL") && !rawData.Contains("GOTO") && !rawData.Contains("NO_SCHEDULE") &&
            //       key != "CommunityCenter_Replacement" && key != "JojaMart_Replacement" && key != "DesertFestival_3";
        }

        public void test(NPC npc)
        {
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                modInstance.Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
            }

            var totalNpcPath = new List<(string, List<Point>)>();
            // Парсинг расписания и обработка маршрутов
            try
            {
                var rawData = "1400 CommunityCenter 47 5 0";
                // Разделяем данные расписания по "/"
                var scheduleEntries = rawData.Split('/');
                var lastLocation = "CommunityCenter";

                int npcX = 47;
                int npcY = 7;

                LastLocationName = null;


                foreach (var entry in scheduleEntries)
                {
                    modInstance.Monitor.Log($"Начат парсинг пути: {entry}", LogLevel.Info);
                    // Разделяем по пробелу: "время локация x y ..."
                    var entryParts = entry.Split(' ');

                    // Проверяем минимальное количество полей
                    if (entryParts.Length < 5)
                        continue;

                    // Извлекаем время, направление и поведение в конце маршрута
                    string time = Regex.Match(entryParts[0], @"\d+").Value;
                    string locationName = entryParts[1];
                    int x = int.Parse(entryParts[2]);
                    int y = int.Parse(entryParts[3]);

                    int facingDirection = 0;
                    string endBehavior = null;

                    if (!Int32.TryParse(entryParts[4], out facingDirection))
                    {
                        endBehavior = entryParts[4];
                    }
                    else if (entryParts.Length > 5)
                    {
                        endBehavior = entryParts[5];
                    }
                    //int facingDirection = int.Parse(entryParts[4]);

                    // Добавляем конечное поведение, если оно есть                          
                    string endMessage = entryParts.Length > 6 ? entryParts[6] : null;


                    modInstance.Monitor.Log($"Данные для патч финда", LogLevel.Info);
                    modInstance.Monitor.Log($"Время(key): {time}", LogLevel.Info);
                    modInstance.Monitor.Log($"текущая лока: {lastLocation}", LogLevel.Info);
                    modInstance.Monitor.Log($"XY npc: {npcX} {npcY}", LogLevel.Info);
                    modInstance.Monitor.Log($"локация куда идти: {locationName}", LogLevel.Info);
                    modInstance.Monitor.Log($"конечный XY: {x} {y}", LogLevel.Info);
                    modInstance.Monitor.Log($"Направление взгляда: {facingDirection}", LogLevel.Info);
                    modInstance.Monitor.Log($"endBehavior: {endBehavior}", LogLevel.Info);
                    modInstance.Monitor.Log($"endMessage: {endMessage}", LogLevel.Info);


                    var pathDescription = npc.pathfindToNextScheduleLocation(
                        time,
                        lastLocation,
                        npcX,
                        npcY,
                        locationName,
                        x,
                        y,
                        facingDirection,
                        endBehavior,
                        endMessage
                        );

                    modInstance.Monitor.Log($"Pathfind", LogLevel.Info);
                    modInstance.Monitor.Log($"Время(key): {pathDescription.time}", LogLevel.Info);
                    modInstance.Monitor.Log($"локация куда идти: {pathDescription.targetLocationName}", LogLevel.Info);
                    modInstance.Monitor.Log($"конечный XY: {pathDescription.targetTile}", LogLevel.Info);
                    modInstance.Monitor.Log($"Направление взгляда: {pathDescription.facingDirection}", LogLevel.Info);
                    modInstance.Monitor.Log($"endBehavior: {pathDescription.endOfRouteBehavior}", LogLevel.Info);
                    modInstance.Monitor.Log($"endMessage: {pathDescription.endOfRouteMessage}", LogLevel.Info);

                    foreach (var item in pathDescription.route)
                    {
                        modInstance.Monitor.Log($"{item}", LogLevel.Debug);
                    }
                    // Добавляем отфильтрованный маршрут в общий список
                    totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation?.Name, pathDescription.route));

                    // Обновляем текущие координаты NPC и локацию
                    lastLocation = locationName;
                    npcX = x;
                    npcY = y;
                }
            }
            catch (Exception ex)
            {
                //modInstance.Monitor.Log($"Error parsing schedule '{key}' for NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
                modInstance.Monitor.Log($"Error parsing schedule  for NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
                //continue; // Переход к следующему расписанию
            }
            // Сброс последней локации
            LastLocationName = null;

        }

        /// <summary>
        /// Получает маршруты NPC на текущий день.
        /// </summary>
        /// <param name="npc">NPC, для которого требуется получить маршруты на день.</param>
        /// <returns>Список пар, где строка — это название локации, а список точек — это маршрут NPC на день.</returns>
        public void GetNpcRoutePoints(NPC npc)
        {
            // Проверяем наличие расписания у NPC
            if (npc.Schedule?.Any() != true)
            {
                //modInstance.Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                modInstance.NpcList.AddNpcBlackList(npc.Name);
                return;
            }

            var totalNpcPath = new List<(string, List<Point>)>();
            
            totalNpcPath.AddRange(npc.Schedule
                .SelectMany(scheduleEntry => NpcPathFilter(npc.currentLocation.Name, scheduleEntry.Value.route))
            );
            modInstance.NpcList.NpcTotalList.Add(npc.Name);

            // Сброс последней локации
            LastLocationName = null;
            modInstance.NpcList.AddNpcPath(npc, modInstance.NpcList.NpcTotalToDayPath, totalNpcPath);
        }

        /// <summary>
        /// Фильтрует маршрут NPC, отделяя сегменты до и после телепортации.
        /// </summary>
        /// <param name="LocationName">Название текущей локации.</param>
        /// <param name="ListPoints">Маршрут, который нужно отфильтровать.</param>
        /// <returns>Список пар, где строка — это название локации, а список точек — это сегмент пути NPC.</returns>    
        public List<(string, List<Point>)> NpcPathFilter(string LocationName, Stack<Point> ListPoints) // Отделение пути передвижение, от пути после телепорта
        {

            if (string.IsNullOrEmpty(LocationName) || ListPoints == null || !ListPoints.Any())
            {
                modInstance.Monitor.Log($"Invalid input to NpcPathFilter. {LocationName} {ListPoints} {ListPoints.Count()}", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }
            if (LastLocationName == null)
            {
                LastLocationName = LocationName;
            }

            var CurrentCoordinatePath = Point.Zero; // текущая координата
            var NpcCurrentPath = new List<Point>(); // Текущий сегмент пути
            var TotalNpcPath = new List<(string, List<Point>)>(); // Итоговый путь

            foreach (var point in ListPoints)
            {
                if (CurrentCoordinatePath == Point.Zero)
                {
                    // Инициализация первой точки
                    CurrentCoordinatePath = point;
                    NpcCurrentPath.Add(CurrentCoordinatePath);
                    continue;
                }

                // Проверяем, находится ли следующая точка рядом
                bool isAdjacent = Math.Abs(point.X - CurrentCoordinatePath.X) <= 1 &&
                                  Math.Abs(point.Y - CurrentCoordinatePath.Y) <= 1;

                //smodInstance.Monitor.Log($"location: {LastLocationName}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Debug);

                if (isAdjacent)
                {
                    // Добавляем точку в текущий маршрут
                    NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }
                else
                {
                    //modInstance.LocationsList.LocationsTeleportCord[LastLocationName].FirstOrDefault(x => x.);

                    // Если точка не рядом, заканчиваем текущий сегмент и начинаем новый
                    TotalNpcPath.Add((LastLocationName, NpcCurrentPath));
                    NpcCurrentPath = new List<Point>(); // очищаем текущий маршрут

                    // Обновляем локацию для телепортации
                    LastLocationName = modInstance.LocationsList.GetTeleportLocation(LastLocationName, CurrentCoordinatePath);

                    // Начинаем новый сегмент маршрута
                    CurrentCoordinatePath = point;
                }
            }
            // Добавляем последний сегмент маршрута
            if (NpcCurrentPath.Count > 0)
            {
                TotalNpcPath.Add((LastLocationName, NpcCurrentPath));
            }
            return TotalNpcPath;
        }
    }
}

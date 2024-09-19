using System;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Pathfinding;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для управления NPC: отслеживание, фильтрация маршрутов и работа со списками NPC.
    /// </summary>
    public class NpcManager
    {
        private readonly NpcTrackerMod modInstance;

        private string LastLocationName;

        
        // <summary>
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
        public List<(string, List<Point>)> GetNpcGlobalRoutePoints(NPC npc)
        {
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                modInstance.Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var totalNpcPath = new List<(string, List<Point>)>();
            var masterSchedule = npc.getMasterScheduleRawData();

           
            try
            {

                foreach (var schedule in masterSchedule)
                {
                    string key = schedule.Key;
                    var rawData = schedule.Value;               

                    //modInstance.Monitor.Log($"Processing schedule key: {key} with data: {rawData}", LogLevel.Debug);

                    // Простая проверка на валидность данных
                    if (string.IsNullOrWhiteSpace(key) || !rawData.Contains(" ") ||
                        rawData.Contains("MAIL") || rawData.Contains("GOTO") || rawData.Contains("NO_SCHEDULE") ||
                        key == "CommunityCenter_Replacement" || key == "JojaMart_Replacement" || key == "DesertFestival_3")
                    {
                        //modInstance.Monitor.Log($"Skipping invalid or problematic schedule key: {key}", LogLevel.Warn);
                        continue;
                    }

                    //modInstance.Monitor.Log($"Выбрано расписание: {key}:{rawData}", LogLevel.Info);
                    // Парсинг расписания и обработка маршрутов
                    try
                    {
                        //var a = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + npc.Name)[key].Split('/');
                        //modInstance.Monitor.Log($"0: {a}", LogLevel.Debug);
                        //KeyValuePair<int, StardewValley.Pathfinding.SchedulePathDescription> tempshedule = new KeyValuePair<int, SchedulePathDescription>();
                        //tempshedule = rawData;



                        // Разделяем данные расписания по "/"
                        var scheduleEntries = rawData.Split('/');

                        var lastloc = npc.currentLocation.Name;
                        var npcX = ((int)Game1.locations.FirstOrDefault(name => name.Name == npc.currentLocation.Name).characters.FirstOrDefault(name => name.Name == npc.Name).TilePoint.X);
                        var npcY = ((int)Game1.locations.FirstOrDefault(name => name.Name == npc.currentLocation.Name).characters.FirstOrDefault(name => name.Name == npc.Name).TilePoint.Y);
                        LastLocationName = null;

                        foreach (var entry in scheduleEntries)
                        {
                            //modInstance.Monitor.Log($"Начат парсинг пути: {entry}", LogLevel.Info);
                            // Разделяем по пробелу: "время локация x y ..."
                            var entryParts = entry.Split(' ');

                            if (entryParts.Length >= 5)
                            {
                                // Извлекаем время, направление и поведение в конце маршрута
                                string time = int.Parse(entryParts[0]).ToString();
                                string locationName = entryParts[1];
                                int x = int.Parse(entryParts[2]);
                                int y = int.Parse(entryParts[3]);
                                int facingDirection = int.Parse(entryParts[4]);

                                // Добавляем конечное поведение, если оно есть
                                string endBehavior = entryParts.Length > 5 ? entryParts[5] : null;
                                string endMessage = entryParts.Length > 6 ? entryParts[6] : null;

                                // Создаём маршрут для NPC
                                var route = new Stack<Point>();
                                route.Push(new Point(x, y));


                                //var loc = lastloc == " " ? locationName : npc.currentLocation.Name;

                                /*
                                modInstance.Monitor.Log($"Данные для патч финда", LogLevel.Info);
                                modInstance.Monitor.Log($"Время(key): {time}", LogLevel.Info);
                                modInstance.Monitor.Log($"текущая лока: {lastloc}", LogLevel.Info);
                                modInstance.Monitor.Log($"XY npc: {npcX} {npcY}", LogLevel.Info);
                                modInstance.Monitor.Log($"локация куда идти: {locationName}", LogLevel.Info);
                                modInstance.Monitor.Log($"конечный XY: {x} {y}", LogLevel.Info);
                                modInstance.Monitor.Log($"Направление взгляда: {facingDirection}", LogLevel.Info);
                                modInstance.Monitor.Log($"endBehavior: {endBehavior}", LogLevel.Info);
                                modInstance.Monitor.Log($"endMessage: {endMessage}", LogLevel.Info);
                                */

                                var a = npc.pathfindToNextScheduleLocation(time,
                                    lastloc,
                                    npcX,
                                    npcY,
                                    locationName,
                                    x,
                                    y,
                                    facingDirection,
                                    endBehavior,
                                    endMessage);
                                //Создаём SchedulePathDescription
                                //var schedulePath = new SchedulePathDescription(route, facingDirection, endBehavior, endMessage, locationName, new Point(x, y));
                                //var a = PathFindController.findPathForNPCSchedules(npc.TilePoint, new Point(x, y), location, 1);

                                //modInstance.Monitor.Log($"Pathfind", LogLevel.Info);
                                //modInstance.Monitor.Log($"Время(key): {a.time}", LogLevel.Info);
                                //modInstance.Monitor.Log($"локация куда идти: {a.targetLocationName}", LogLevel.Info);
                                //modInstance.Monitor.Log($"конечный XY: {a.targetTile}", LogLevel.Info);
                                //modInstance.Monitor.Log($"Направление взгляда: {a.facingDirection}", LogLevel.Info);
                                //modInstance.Monitor.Log($"endBehavior: {a.endOfRouteBehavior}", LogLevel.Info);
                                //modInstance.Monitor.Log($"endMessage: {a.endOfRouteMessage}", LogLevel.Info);


                                totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, a.route));


                                lastloc = locationName;
                                npcX = x;
                                npcY = y;
                                
                            }
                        }
                        //foreach (var i in schedulePath)

                        //var parsedSchedule = npc.parseMasterSchedule(key, rawData).Values;
                        
                        //foreach(var item in npc.Schedule)
                        //{
                        //    tempshedule.Value.add(item);
                        //    npc.Schedule.Clear();
                        //    var a = npc.parseMasterSchedule(key, rawData).Values;
                            
                        //}
                        ////foreach (var s in currentnpc.Schedule)
                        ////{

                        ////    totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation?.Name, s.Value.route));
                        ////}
                        //foreach (var s in parsedSchedule)
                        //{

                        //    totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation?.Name, s.route));
                        //}
                        //npc.Schedule.Add()

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
            
            // Сброс последней локации
            LastLocationName = null;
            return totalNpcPath;
        }
        
        /// <summary>
        /// Получает маршруты NPC на текущий день.
        /// </summary>
        /// <param name="npc">NPC, для которого требуется получить маршруты на день.</param>
        /// <returns>Список пар, где строка — это название локации, а список точек — это маршрут NPC на день.</returns>
        public List<(string, List<Point>)> GetNpcRoutePoints(NPC npc)
        {
            // Проверяем наличие расписания у NPC
            if (npc.Schedule?.Any() != true)
            {
                modInstance.Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var totalNpcPath = new List<(string, List<Point>)>();

            totalNpcPath.AddRange(npc.Schedule
                .SelectMany(scheduleEntry => NpcPathFilter(npc.currentLocation.Name, scheduleEntry.Value.route))
            );

            // Сброс последней локации
            LastLocationName = null;
            return totalNpcPath;
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
                //modInstance.Monitor.Log($"location: {LastLocationName}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Debug);
                if (isAdjacent)
                {
                    // Добавляем точку в текущий маршрут
                    NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }
                else
                {
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

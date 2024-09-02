using System;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    // Этот класс будет отвечать за работу с NPC: отслеживание, фильтрацию, получение маршрутов и добавление в списки.
    class NpcManager
    {
        private NpcTrackerMod modInstance;

        
        public NpcManager(NpcTrackerMod instance)
        {
            this.modInstance = instance;
        }
        public IEnumerable<NPC> GetNpcsToTrack(bool trackAllLocations, List<string> NpcList)
        {
            // Проверка для получения всех персонажей в текущей локации
            if (!trackAllLocations)
                return Game1.currentLocation?.characters ?? Enumerable.Empty<NPC>().Where(npc => NpcList.Contains(npc.Name));

            // Проверка для получения всех персонажей во всех локациях
            return Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null && NpcList.Contains(npc.Name)); // Отфильтровываем возможные null значения
        }
        public List<List<(string, List<Point>)>> GetNpcRoutePoints(NPC npc, bool AllRoutes) // Сбор данных об пути от нпс
        {
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                Console.WriteLine($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<List<(string, List<Point>)>>();
            }

            var routePoints = new List<Point>();  // Получаем точки маршрута

            var PathList = new List<List<(string, List<Point>)>>();
            var totalNpcPath = new List<(string, List<Point>)>();
            
            if (AllRoutes)
            {
                /*
                modInstance.Monitor.Log($"Общий путь", LogLevel.Debug);
                // Получаем все возможные маршруты из всех расписаний
                foreach (var schedule in npc.getMasterScheduleRawData())
                {
                    //Console.WriteLine($"1/ Key: {la.Key} | Value: {la.Value}", LogLevel.Debug);
                    try
                    {
                        
                        foreach (var path in npc.parseMasterSchedule(schedule.Key, schedule.Value))
                        {
                            routePoints.AddRange(path.Value.route);
                            totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, routePoints));
                            //PathList.Add(totalNpcPath);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex}");
                    }

                    //Console.WriteLine($"{npc.parseMasterSchedule(npc.ScheduleKey, la)}");
                }
                */
                try
                {
                    foreach (var schedule in npc.getMasterScheduleRawData())
                    {
                        var rawData = schedule.Value;

                        // Простая проверка на валидность данных
                        if (schedule.Key == "CommunityCenter_Replacement")
                        {
                            modInstance.Monitor.Log($"Skipping known problematic schedule '{schedule.Key}' for NPC '{npc.Name}'", LogLevel.Warn);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(rawData) || !rawData.Contains(" "))
                        {
                            modInstance.Monitor.Log($"Skipping invalid schedule entry for '{schedule.Key}' with data '{rawData}'", LogLevel.Warn);
                            continue;
                        }

                        //modInstance.Monitor.Log($"master: {npc.getMasterScheduleEntry(schedule.Key)}", LogLevel.Debug);
                        try
                        {
                            foreach (var path in npc.parseMasterSchedule(schedule.Key, rawData))
                            {
                                //modInstance.Monitor.Log($"key: {path.Key}", LogLevel.Debug);
                                foreach (var abc in path.Value.route)
                                {
                                    //modInstance.Monitor.Log($"route: {abc}", LogLevel.Debug);
                                    routePoints.Add(abc);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            modInstance.Monitor.Log($"Error parsing schedule '{schedule.Key}' with raw data '{schedule.Value}': {ex.Message}", LogLevel.Error);
                            // Продолжить выполнение цикла для других расписаний
                            continue;
                        }
                        totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, routePoints));
                        PathList.Add(totalNpcPath);
                    }
                    

                 }
                catch (Exception ex)
                {
                    modInstance.Monitor.Log($"Unexpected error: {ex.Message}", LogLevel.Error);
                }
            }           
            else
            {
                foreach (var scheduleEntry in npc.Schedule)
                {
                    routePoints.AddRange(scheduleEntry.Value.route);
                    if (!modInstance.Switchnpcpath)
                    {
                        //this.Monitor.Log($"NPC: {npc.Name} | KEY: {scheduleEntry.Key} | Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                        //foreach (var point in routePoints) this.Monitor.Log($"Route: {point}", LogLevel.Info);
                        
                    }
                }
                totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, routePoints));
                PathList.Add(totalNpcPath);
            }
            
            modInstance.Monitor.Log($"Totalcount: {totalNpcPath.Count()}", LogLevel.Debug);
            modInstance.Monitor.Log($"PathListcount: {PathList.Count()}", LogLevel.Debug);

            foreach (var item in routePoints)
            {
                //modInstance.Monitor.Log($"route {item}", LogLevel.Debug);
            }

            foreach (var path in PathList)
            {

                //Console.WriteLine($"Path1. {path[0].Item1}", LogLevel.Debug);
                foreach (var point in path)
                {
                    //modInstance.Monitor.Log($"Path2. {point.Item1}", LogLevel.Debug);
                    foreach (var da in point.Item2)
                    {
                        //modInstance.Monitor.Log($"Path3. {point.Item1} {da}", LogLevel.Debug);
                    }
                }
            }
            foreach (var path in totalNpcPath)
            {
                
                foreach (var point in path.Item2)
                {
                    //modInstance.Monitor.Log($"Total. {path.Item1} {point}", LogLevel.Debug);
                }
                
            }       

            modInstance.Monitor.Log($"-------------", LogLevel.Debug);
            return PathList;
        }
        
        public List<(string, List<Point>)> NpcPathFilter(string LocationName, List<Point> ListPoints) // Отделение пути передвижение, от пути после телепорта
        {
            if (string.IsNullOrEmpty(LocationName) || ListPoints == null || !ListPoints.Any())
            {
                Console.WriteLine("Invalid input to NpcPathFilter.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var CurrentLocationPath = LocationName; // текущая локация

            var CurrentCoordinatePath = new Point(); // текущая координата, где находится NPC

            var NpcCurrentPath = new List<Point>(); // часть маршрута
            var TotalNpcPath = new List<(string, List<Point>)>(); // общий маршрут

            foreach (var point in ListPoints)
            {
                if (CurrentCoordinatePath == Point.Zero)
                {
                    CurrentCoordinatePath = point; // устанавливаем стартовую координату
                    NpcCurrentPath.Add(CurrentCoordinatePath);
                    continue;
                }

                bool isAdjacent = Math.Abs(point.X - CurrentCoordinatePath.X) <= 1 &&
                                  Math.Abs(point.Y - CurrentCoordinatePath.Y) <= 1; // true - если нпс идёт / fakse - если тпхнулся
                //modInstance.Monitor.Log($"location: {CurrentLocationPath}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Info);
                if (isAdjacent)
                {
                    // Добавляем точку в текущий маршрут
                    NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }
                else
                {
                    // Сегмент пути завершён, добавляем его в общий маршрут

                    TotalNpcPath.Add((CurrentLocationPath, NpcCurrentPath));

                    NpcCurrentPath = new List<Point>(); // очищаем текущий маршрут

                    // Обновляем текущую локацию

                    CurrentLocationPath = modInstance.LocationsList.GetTeleportLocation(CurrentLocationPath, CurrentCoordinatePath);

                    // Начинаем новый сегмент маршрута
                    //NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }

            }
            // Добавляем последний сегмент маршрута
            if (NpcCurrentPath.Count > 0)
            {
                TotalNpcPath.Add((CurrentLocationPath, NpcCurrentPath));
            }
            return TotalNpcPath;

        }
    }
}

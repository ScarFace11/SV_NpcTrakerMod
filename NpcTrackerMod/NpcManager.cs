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

        private string LastLocationName; 
        public NpcManager(NpcTrackerMod instance)
        {
            this.modInstance = instance;
        }
        public IEnumerable<NPC> GetNpcsToTrack(bool trackAllLocations, List<string> NpcList)
        {
            // Проверка для получения всех персонажей в текущей локации
            if (!trackAllLocations)
                return Game1.currentLocation?.characters
                .Where(npc => NpcList.Contains(npc.Name))
                ?? Enumerable.Empty<NPC>();

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
                modInstance.Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<List<(string, List<Point>)>>();
            }


            var totalNpcPath = new List<(string, List<Point>)>();
            var PathList = new List<List<(string, List<Point>)>>();           
            
            if (AllRoutes)
            {
                try
                {
                    foreach (var schedule in npc.getMasterScheduleRawData())
                    {
                        var rawData = schedule.Value;

                        //modInstance.Monitor.Log($"Processing schedule key: {schedule.Key} with data: {rawData}", LogLevel.Debug);


                        // Простая проверка на валидность данных
                        if (schedule.Key == null || !schedule.Key.Any())
                        {
                            modInstance.Monitor.Log("ListPoints is null or empty", LogLevel.Warn);
                            return new List<List<(string, List<Point>)>>();
                        }
                        if (rawData.Contains("MAIL") || rawData.Contains("GOTO") || rawData.Contains("NO_SCHEDULE"))
                        {
                            modInstance.Monitor.Log($"Skipping problematic schedule key: {schedule.Key}", LogLevel.Warn);
                            continue;
                        }
                        if (schedule.Key == "CommunityCenter_Replacement" || schedule.Key == "JojaMart_Replacement")
                        {
                            modInstance.Monitor.Log($"Skipping known problematic schedule '{schedule.Key}' for NPC '{npc.Name}'", LogLevel.Warn);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(rawData) || !rawData.Contains(" "))
                        {
                            modInstance.Monitor.Log($"Skipping invalid schedule entry for '{schedule.Key}' with data '{rawData}'", LogLevel.Warn);
                            continue;
                        }

                        try
                        {
                            foreach (var path in npc.parseMasterSchedule(schedule.Key, rawData))
                            {
                                totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, path.Value.route));
                            }
                        }
                        catch (Exception ex)
                        {
                            modInstance.Monitor.Log($"Error parsing schedule '{schedule.Key}' with raw data '{schedule.Value}': {ex.Message}", LogLevel.Error);
                            // Продолжить выполнение цикла для других расписаний
                            continue;
                        }
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
                    totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation.Name, scheduleEntry.Value.route));
                    //modInstance.TotalNpcList.AddNpcPath(npc.Name, npc.currentLocation.Name, totalNpcPath.SelectMany(p => p.Item2).ToList());

                    
                    // Добавляем полученные пути в NpcTotalToDayPath для определенного NPC
                }
                modInstance.TotalNpcList.AddNpcPath(npc.Name, totalNpcPath);                
            }
            //PathList.Add(totalNpcPath); 
            foreach (var PathsItems in modInstance.TotalNpcList.NpcTotalToDayPath)
            {
                PathList.Add(PathsItems.Value);
            }
            /*
            modInstance.Monitor.Log($"Totalcount: {totalNpcPath.Count()}", LogLevel.Debug);
            modInstance.Monitor.Log($"PathListcount: {PathList.Count()}", LogLevel.Debug);


            modInstance.Monitor.Log($"TotalPathCount: {modInstance.TotalNpcList.NpcTotalToDayPath.Count()}", LogLevel.Debug);

            
            modInstance.Monitor.Log($"-----Некст--------", LogLevel.Debug);

            foreach (var lopat in modInstance.TotalNpcList.NpcTotalToDayPath)
            {
                modInstance.Monitor.Log($"ToDay НПС: {lopat.Key}", LogLevel.Debug);
                foreach (var lopata in lopat.Value)
                {
                    modInstance.Monitor.Log($"ToDay Локация: {lopata.Item1}", LogLevel.Debug);
                }
            }

            modInstance.Monitor.Log($"-----Некст2--------", LogLevel.Debug);

            foreach (var lopat in PathList)
            {
                foreach (var lopat2 in lopat)
                {

                    modInstance.Monitor.Log($"Total Локация: {lopat2.Item1}", LogLevel.Debug);
                }

            }
            modInstance.Monitor.Log($"-----Некст3--------", LogLevel.Debug);

            foreach (var lopat in totalNpcPath)
            {
                modInstance.Monitor.Log($"Total Локация: {lopat.Item1}", LogLevel.Debug);
                
            }
            modInstance.Monitor.Log($"-------------", LogLevel.Debug);
            */

            LastLocationName = null;
            return PathList;
        }
        
        public List<(string, List<Point>)> NpcPathFilter(string LocationName, Stack<Point> ListPoints) // Отделение пути передвижение, от пути после телепорта
        {

            if (string.IsNullOrEmpty(LocationName) || ListPoints == null || !ListPoints.Any())
            {
                modInstance.Monitor.Log("Invalid input to NpcPathFilter.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }
            if (LastLocationName == null)
            {
                LastLocationName = LocationName;
            }
            //var LastLocationName = LocationName; // текущая локация
            var CurrentCoordinatePath = Point.Zero; // текущая координата, где находится NPC
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
                //modInstance.Monitor.Log($"location: {LastLocationName}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Info);
                if (isAdjacent)
                {
                    // Добавляем точку в текущий маршрут
                    NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }
                else
                {
                    // Сегмент пути завершён, добавляем его в общий маршрут

                    TotalNpcPath.Add((LastLocationName, NpcCurrentPath));

                    NpcCurrentPath = new List<Point>(); // очищаем текущий маршрут

                    // Обновляем текущую локацию

                    LastLocationName = modInstance.LocationsList.GetTeleportLocation(LastLocationName, CurrentCoordinatePath);

                    // Начинаем новый сегмент маршрута
                    //NpcCurrentPath.Add(point);
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

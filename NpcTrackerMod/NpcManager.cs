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
    public class NpcManager
    {
        private readonly NpcTrackerMod modInstance;

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
        public List<(string, List<Point>)> GetNpcGlobalRoutePoints(NPC npc) // Сбор данных об пути от нпс из всех рассписаний
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
                    var rawData = schedule.Value;
                    string key = schedule.Key;
                    modInstance.Monitor.Log($"Processing schedule key: {schedule.Key} with data: {rawData}", LogLevel.Debug);

                    // Простая проверка на валидность данных
                    if (string.IsNullOrWhiteSpace(key) || !rawData.Contains(" ") ||
                        rawData.Contains("MAIL") || rawData.Contains("GOTO") || rawData.Contains("NO_SCHEDULE") ||
                        key == "CommunityCenter_Replacement" || key == "JojaMart_Replacement")
                    {
                        modInstance.Monitor.Log($"Skipping invalid or problematic schedule key: {key}", LogLevel.Warn);
                        continue;
                    }
                    // Парсинг расписания и добавление маршрутов
                    try
                    {
                        var parsedSchedule = npc.parseMasterSchedule(key, rawData);
                        foreach (var path in parsedSchedule)
                        {
                            totalNpcPath.AddRange(NpcPathFilter(npc.currentLocation?.Name, path.Value.route));
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

            // Сброс последней локации
            LastLocationName = null;
            return totalNpcPath;
        }
        public List<(string, List<Point>)> GetNpcRoutePoints(NPC npc) // Сбор данных об пути от нпс на текущий день
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
                //modInstance.Monitor.Log($"location: {LastLocationName}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Trace);
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

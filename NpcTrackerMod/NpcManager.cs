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
        private readonly _modInstance modInstance;

        private string LastLocationName;
        private string EndLocationName;

        public NpcManager(_modInstance instance)
        {
            modInstance = instance;
        }

        /// <summary>
        /// Получает список NPC, которые должны отслеживаться.
        /// </summary>
        public IEnumerable<NPC> GetNpcsToTrack(bool trackAllLocations, HashSet<string> NpcList)
        {
            if (!trackAllLocations)
                return Game1.currentLocation?.characters
                    .Where(npc => NpcList.Contains(npc.Name))
                    ?? Enumerable.Empty<NPC>();

            return Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null && NpcList.Contains(npc.Name));
        }

        /// <summary>
        /// Строит глобальные маршруты NPC по всем записям расписания.
        /// </summary>
        public void ProcessNpcGlobalRoute(NPC currentNpc, string npcName, string path, string pathkey)
        {
            NPC npc = currentNpc ?? FindNpcByName(npcName);

            if (npc == null)
            {
                modInstance.Monitor.Log($"NPC {npcName} не найден.", LogLevel.Warn);
                return;
            }

            if (path == null && npc.Schedule?.Any() != true)
                return;

            Dictionary<string, string> masterSchedule = GetMasterSchedule(npc, path, pathkey);

            // HashSet.Add игнорирует дубликаты — отдельная проверка Contains не нужна
            modInstance.NpcList.TotalNpcList.Add(npc.Name);

            var totalNpcPath = new Dictionary<string, HashSet<Point>>();

            try
            {
                foreach (var schedule in masterSchedule)
                {
                    string key = schedule.Key;
                    var rawData = schedule.Value;

                    if (!IsValidScheduleEntry(key, rawData))
                    {
                        modInstance.Monitor.Log($"У НПС {npc.Name} пропуск неверного или проблемного ключа расписания: {key}", LogLevel.Warn);
                        modInstance.Monitor.Log($"{rawData}", LogLevel.Debug);
                        continue;
                    }

                    try
                    {
                        var scheduleEntries = rawData.Split('/');
                        var lastLocation = npc.currentLocation?.Name;

                        var npcLocation = EndLocationName == null
                            ? Game1.locations.FirstOrDefault(loc => loc.Name == npc.currentLocation?.Name)
                            : Game1.locations.FirstOrDefault(loc => loc.Name == EndLocationName);
                        EndLocationName = null;

                        if (npcLocation == null)
                        {
                            modInstance.Monitor.Log($"Не известно где находится {npc.Name}", LogLevel.Warn);
                            return;
                        }

                        // Один поиск вместо двух
                        var npcInLocation = npcLocation.characters.FirstOrDefault(n => n.Name == npc.Name);
                        int npcX = (int)(npcInLocation?.TilePoint.X ?? 0);
                        int npcY = (int)(npcInLocation?.TilePoint.Y ?? 0);

                        LastLocationName = null;

                        foreach (var entry in scheduleEntries)
                        {
                            string[] entryParts = entry.Split(' ');

                            if (entry.Contains("MAIL") || entry.Contains("friendship") || entry.Contains("GOTO") || entry.Contains("NO_SCHEDULE")) continue;
                            if (entryParts.Length <= 2) continue;

                            ParseNpcScheduleEntry(entryParts, entry, masterSchedule,
                                out string time, out string locationName, out int x, out int y,
                                out int facingDirection, out string endBehavior, out string endMessage);

                            try
                            {
                                SchedulePathDescription pathDescription = npc.pathfindToNextScheduleLocation(
                                    time, lastLocation, npcX, npcY,
                                    locationName, x, y,
                                    facingDirection, endBehavior, endMessage);

                                if (pathDescription?.route != null)
                                {
                                    MergeSegments(totalNpcPath,
                                        NpcPathFilter(npc.currentLocation?.Name, pathDescription.route));
                                    lastLocation = locationName;
                                    npcX = x;
                                    npcY = y;
                                }
                            }
                            catch (Exception ex)
                            {
                                modInstance.Monitor.Log($"Ошибка при поиске маршрута от {lastLocation} XY {npcX} {npcY} до '{time}':{locationName} XY:{x} {y} для '{npc.Name}': {ex.Message}", LogLevel.Error);
                                lastLocation = locationName;
                                npcX = x;
                                npcY = y;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        modInstance.Monitor.Log($"Error parsing schedule '{key}' for NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Unexpected error while processing NPC '{npc.Name}': {ex.Message}", LogLevel.Error);
            }

            LastLocationName = null;
            modInstance.NpcList.AddNpcPath(npc, modInstance.NpcList.GlobalNpcPaths, totalNpcPath);
        }

        /// <summary>
        /// Точный поиск NPC по имени (без подстроки).
        /// </summary>
        private NPC FindNpcByName(string npcName)
        {
            return modInstance.NpcList.GameNpcs.FirstOrDefault(
                npc => string.Equals(npc.Name, npcName, StringComparison.OrdinalIgnoreCase));
        }

        private Dictionary<string, string> GetMasterSchedule(NPC npc, string path, string pathKey)
        {
            if (npc.Schedule != null && npc.Schedule.Any())
                return npc.getMasterScheduleRawData();

            return new Dictionary<string, string> { [pathKey] = path };
        }

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
            time = "0";
            locationName = string.Empty;
            x = y = facingDirection = 0;
            endBehavior = null;
            endMessage = null;

            int currentIndex = 0;

            // 1: время или локация?
            if (int.TryParse(Regex.Match(scheduleParts[currentIndex], @"\d+").Value, out _))
            {
                time = Regex.Match(scheduleParts[currentIndex], @"\d+").Value;
                currentIndex++;
            }
            else
            {
                if (int.TryParse(scheduleParts[currentIndex], out _))
                    locationName = LastLocationName;
                else
                    locationName = scheduleParts[currentIndex];
                currentIndex++;
            }

            // 2: локация (если ещё не определена)
            if (string.IsNullOrEmpty(locationName))
            {
                if (int.TryParse(scheduleParts[currentIndex], out _))
                    locationName = LastLocationName;
                else
                {
                    locationName = scheduleParts[currentIndex];
                    currentIndex++;
                }
            }

            // 3,4: X Y
            if (scheduleParts.Length > currentIndex + 1)
            {
                x = int.Parse(scheduleParts[currentIndex]);
                y = int.Parse(scheduleParts[currentIndex + 1]);
                currentIndex += 2;
            }

            // 5: направление взгляда
            if (scheduleParts.Length > currentIndex && int.TryParse(scheduleParts[currentIndex], out var direction))
            {
                facingDirection = direction;
                currentIndex++;
            }
            else
            {
                facingDirection = 2;
            }

            // 6: анимация или сообщение
            if (scheduleParts.Length > currentIndex)
            {
                string potentialValue = scheduleParts[currentIndex];
                if (potentialValue.StartsWith("\"Strings\\"))
                    endMessage = potentialValue;
                else
                    endBehavior = potentialValue;
                currentIndex++;
            }

            // 7: сообщение после анимации
            if (scheduleParts.Length > currentIndex)
            {
                string potentialMessage = scheduleParts[currentIndex];
                if (potentialMessage.StartsWith("\"Strings\\"))
                    endMessage = potentialMessage;
            }
        }

        private bool IsValidScheduleEntry(string key, string rawData)
        {
            return !string.IsNullOrWhiteSpace(key) && rawData.Contains(" ");
        }

        /// <summary>
        /// Получает маршруты NPC на текущий день из предвычисленного расписания.
        /// </summary>
        public void GetNpcRoutePoints(NPC npc)
        {
            if (npc.Schedule?.Any() != true)
                return;

            var totalNpcPath = new Dictionary<string, HashSet<Point>>();
            var timedPath = new Dictionary<int, Dictionary<string, HashSet<Point>>>();

            // Один проход вместо трёх — NpcPathFilter вызывается ровно один раз на запись
            foreach (var scheduleEntry in npc.Schedule)
            {
                var segments = NpcPathFilter(npc.currentLocation.Name, scheduleEntry.Value.route);
                MergeSegments(totalNpcPath, segments);
                if (segments.Count > 0)
                    timedPath[scheduleEntry.Key] = segments;
            }

            if (totalNpcPath.Count == 0)
            {
                ProcessNpcGlobalRoute(npc, null, null, null);
                return;
            }

            modInstance.NpcList.TotalNpcList.Add(npc.Name);
            modInstance.NpcList.NpcTimedDayPath[npc.Name] = timedPath;

            LastLocationName = null;
            modInstance.NpcList.AddNpcPath(npc, modInstance.NpcList.NpcTotalToDayPath, totalNpcPath);
        }

        /// <summary>
        /// Объединяет сегменты маршрута source в target (без дубликатов тайлов).
        /// </summary>
        private static void MergeSegments(
            Dictionary<string, HashSet<Point>> target,
            Dictionary<string, HashSet<Point>> source)
        {
            foreach (var kvp in source)
            {
                if (!target.TryGetValue(kvp.Key, out var pts))
                    target[kvp.Key] = new HashSet<Point>(kvp.Value);
                else
                    pts.UnionWith(kvp.Value);
            }
        }

        /// <summary>
        /// Фильтрует маршрут NPC, разбивая на сегменты по локациям.
        /// Возвращает Dictionary: локация → набор тайлов (HashSet для дедупликации).
        /// </summary>
        public Dictionary<string, HashSet<Point>> NpcPathFilter(string LocationName, Stack<Point> ListPoints)
        {
            var result = new Dictionary<string, HashSet<Point>>();

            if (string.IsNullOrEmpty(LocationName) || ListPoints == null || !ListPoints.Any())
            {
                modInstance.Monitor.Log($"NpcPathFilter: пустой маршрут для локации '{LocationName}' (точек: {ListPoints?.Count ?? 0})", LogLevel.Debug);
                return result;
            }

            if (LastLocationName == null)
                LastLocationName = LocationName;

            var currentCoord = Point.Zero;
            var currentSegment = new HashSet<Point>();

            foreach (var point in ListPoints)
            {
                if (currentCoord == Point.Zero)
                {
                    currentCoord = point;
                    currentSegment.Add(point);
                    continue;
                }

                bool isAdjacent = Math.Abs(point.X - currentCoord.X) <= 1 &&
                                  Math.Abs(point.Y - currentCoord.Y) <= 1;

                if (isAdjacent)
                {
                    currentSegment.Add(point);
                    currentCoord = point;
                }
                else
                {
                    // Завершаем текущий сегмент, переходим в новую локацию
                    AddSegmentToResult(result, LastLocationName, currentSegment);
                    currentSegment = new HashSet<Point>();
                    LastLocationName = modInstance.LocationsList.GetTeleportLocation(LastLocationName, currentCoord);
                    currentCoord = point;
                }
            }

            if (currentSegment.Count > 0)
                AddSegmentToResult(result, LastLocationName, currentSegment);

            return result;
        }

        private static void AddSegmentToResult(
            Dictionary<string, HashSet<Point>> result,
            string location,
            HashSet<Point> segment)
        {
            if (!result.TryGetValue(location, out var existing))
                result[location] = new HashSet<Point>(segment);
            else
                existing.UnionWith(segment);
        }
    }
}

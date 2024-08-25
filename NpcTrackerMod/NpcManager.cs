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

        private LocationsList Locations_List;

        public NpcManager(NpcTrackerMod instance)
        {
            this.modInstance = instance;
            Locations_List = new LocationsList();
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
        public List<(string, List<Point>)> GetNpcRoutePoints(NPC npc) // Сбор данных об пути от нпс
        {
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                Console.WriteLine($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var routePoints = new List<Point>();

            foreach (var scheduleEntry in npc.Schedule)
            {
                routePoints.AddRange(scheduleEntry.Value.route);

                if (!modInstance.Switchnpcpath)
                {
                    //this.Monitor.Log($"NPC: {npc.Name} | KEY: {scheduleEntry.Key} | Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                    //foreach (var point in routePoints) this.Monitor.Log($"Route: {point}", LogLevel.Info);
                }
            }
            return NpcPathFilter(npc.currentLocation.Name, routePoints);
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
                //Monitor.Log($"location: {CurrentLocationPath}, Предыдущая точка: {CurrentCoordinatePath} Некст точка: {point}", LogLevel.Info);
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

                    CurrentLocationPath = Locations_List.GetTeleportLocation(CurrentLocationPath, CurrentCoordinatePath);

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

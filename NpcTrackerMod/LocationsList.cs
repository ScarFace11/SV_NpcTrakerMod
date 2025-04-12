using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    public class LocationsList
    {
        private readonly _modInstance modInstance;

        // Словарь с локациями, их координатами и локациями для телепортации
        public Dictionary<string, List<(Point, string)>> LocationsTeleportCord { get; } = new Dictionary<string, List<(Point, string)>>();

        public LocationsList(_modInstance instance)
        {
            this.modInstance = instance;
        }

        public List<(string, List<Point>)> LocationsPaths = new List<(string, List<Point>)>();

        /// <summary>
        /// Возвращает локацию, куда телепортируется NPC.
        /// </summary>
        /// <param name="currentLocation">Текущая локация, где находится NPC</param>
        /// <param name="npcPosition">Координаты NPC</param>
        /// <returns>Название локации назначения или "Null" если телепортация невозможна</returns>
        public string GetTeleportLocation(string currentLocation, Point npcPosition)
        {
            if (LocationsTeleportCord.TryGetValue(currentLocation, out var teleportData))
            {
                var teleportPoint = teleportData.Find(t => t.Item1 == npcPosition);
                if (teleportPoint != default)
                {
                    return teleportPoint.Item2;
                }
                else
                {
                    //LocationsTeleportCord[currentLocation]
                    modInstance.Monitor.Log($"что-то не так. {currentLocation} {npcPosition}", LogLevel.Warn);
                }
            }
            

            //Console.WriteLine($"Teleportation not possible for location: {currentLocation}, position: {npcPosition}");
            return "Null"; // Телепортация невозможна
        }

        /// <summary>
        /// Собирает и сохраняет данные обо всех игровых локациях.
        /// </summary>
        public void SetLocations()
        {
            LocationsTeleportCord.Clear();


            foreach (var location in Game1.locations)
            {
                var teleportData = new List<(Point, string)>();

                // Добавляем данные о всех warp'ах
                teleportData.AddRange(location.warps.Select(warp => (new Point(warp.X, warp.Y), warp.TargetName)));

                // Добавляем данные о дверях
                teleportData.AddRange(location.doors.Pairs.Select(door => (door.Key, door.Value)));

                
                // Добавляем данные в словарь
                LocationsTeleportCord[location.Name] = teleportData;               
            }

            modInstance.LocationSet = true;
        }
    }
}


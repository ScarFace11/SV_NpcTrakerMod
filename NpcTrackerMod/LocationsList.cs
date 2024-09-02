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
    public class LocationsList
    {
        private NpcTrackerMod modInstance;

        //Назва локи, (Тп корды, название локации куда тп)
        public LocationsList(NpcTrackerMod instance)
        {
            this.modInstance = instance;
            LocationsTeleportCord = new Dictionary<string, List<(Point, string)>>(); // Инициализируем словарь
        }
        public Dictionary<string, List<(Point, string)>> LocationsTeleportCord { get; }
        List<(Point, string)> TeleportedCord = new List<(Point, string)>();

        public string GetTeleportLocation(string currentLocation, Point npcPosition)
        {
            if (LocationsTeleportCord.TryGetValue(currentLocation, out var teleportData))
            {
                foreach (var (point, destination) in teleportData)
                {
                    if (point == npcPosition)
                    {
                        return destination;
                    }
                }
            }

            // Логируем, если телепортация невозможна
            //Console.WriteLine($"Teleportation not possible for location: {currentLocation}, position: {npcPosition}");
            return "Null";
        }
        public void Locations()
        {
            var locations = Game1.locations;
            foreach (var loccord in locations)
            {
                TeleportedCord.Clear(); // Очищаем список перед добавлением новых данных
                foreach (var warp in loccord.warps)
                {
                    TeleportedCord.Add((new Point(warp.X, warp.Y), warp.TargetName));
                }
                foreach (var door in loccord.doors.Pairs)
                {
                    TeleportedCord.Add((door.Key, door.Value));
                }
                LocationsTeleportCord.Add(loccord.Name, new List<(Point, string)>(TeleportedCord)); // Создаем новый список, чтобы избежать ссылок на один и тот же объект
            }
            foreach (var location in LocationsTeleportCord)
            {
                modInstance.Monitor.Log($"Все локи: {location.Key}", LogLevel.Debug); // Выводим только ключ, так как словарь содержит пары имя локации и список точек
                foreach (var lc in location.Value)
                {
                    modInstance.Monitor.Log($"{lc.Item1} {lc.Item2}", LogLevel.Debug); // Выводим только ключ, так как словарь содержит пары имя локации и список точек
                }
            }
        }

    }
}


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
        

        public Dictionary<string, (List<Point>, List<string>)> LocationsTeleportCord_Vanilla = new Dictionary<string, (List<Point>, List<string>)>()
        {
            { "AbandonedJojaMart", (CreatePoints(), new List<string>()) },
            { "AdventureGuild", (CreatePoints(), new List<string>()) },
            { "AnimalShop", (CreatePoints( (13, 20)), 
                new List<string>{ "Forest" }) },
            { "ArchaeologyHouse", (CreatePoints( (3,15) ), 
                new List<string>{ "Town"}) },
            { "Backwoods", (CreatePoints((50, 11), (50, 12), (50, 13), (50, 14), (50, 15), (50, 16), (50, 16), (13, 40), (14, 40), (15, 40), (50, 28), (50, 29), (50, 30), (50, 31), (50, 32), (22, 29), (22, 30), (22, 31), (22, 32)), new List<string>()) },
            { "BathHouse_Entry", (CreatePoints(), new List<string>()) },
            { "BathHouse_MensLocker", (CreatePoints(), new List<string>()) },
            { "BathHouse_Pool", (CreatePoints(), new List<string>()) },
            { "BathHouse_WomensLocker", (CreatePoints(), new List<string>()) },
            { "Beach", (CreatePoints(), new List<string>()) },
            { "BeachNightMarket", (CreatePoints(), new List<string>()) },
            { "Blacksmith", (CreatePoints( (5,20) ), 
                new List<string>{ "Town" }) },
            { "BoatTunnel", (CreatePoints(), new List<string>()) },
            { "BugLand", (CreatePoints(), new List<string>()) },
            { "BusStop", (CreatePoints((22, 8), (44, 22), (44, 23), (44, 24), (44, 25), (65, 22), (65, 23), (65, 24), (65, 25), (9, 22), (9, 23), (9, 24), (9, 25), (-1, 22), (-1, 23), (-1, 24), (-1, 25), (-1, 26), (11, 6), (11, 7), (11, 8), (11, 9)), new List<string>()) },
            { "Caldera", (CreatePoints(), new List<string>()) },
            { "CaptainRoom", (CreatePoints(), new List<string>()) },
            { "Cellar", (CreatePoints(), new List<string>()) },
            { "Cellar2", (CreatePoints(), new List<string>()) },
            { "Cellar3", (CreatePoints(), new List<string>()) },
            { "Cellar4", (CreatePoints(), new List<string>()) },
            { "Cellar5", (CreatePoints(), new List<string>()) },
            { "Cellar6", (CreatePoints(), new List<string>()) },
            { "Cellar7", (CreatePoints(), new List<string>()) },
            { "Cellar8", (CreatePoints(), new List<string>()) },
            { "Club", (CreatePoints(), new List<string>()) },
            { "CommunityCenter", (CreatePoints(), new List<string>()) },
            { "Desert", (CreatePoints(), new List<string>()) },
            { "DesertFestival", (CreatePoints(), new List<string>()) },
            { "ElliottHouse", (CreatePoints(), new List<string>()) },
            { "Farm", (CreatePoints(), new List<string>()) },
            { "FarmCave", (CreatePoints(), new List<string>()) },
            { "FarmHouse", (CreatePoints(), new List<string>()) },
            { "FishShop", (CreatePoints(), new List<string>()) },
            { "Forest", (CreatePoints(), new List<string>()) },
            { "Greenhouse", (CreatePoints(), new List<string>()) },
            { "HaleyHouse", (CreatePoints((2,25), (4,3)),
                new List<string>{"Town" }) },
            { "HarveyRoom", (CreatePoints(), new List<string>()) },
            { "Hospital", (CreatePoints( (10,20), (10,11) ), 
                new List<string>{ "Town", "HarveyRoom" }) },
            { "IslandEast", (CreatePoints(), new List<string>()) },
            { "IslandFarmCave", (CreatePoints(), new List<string>()) },
            { "IslandFarmHouse", (CreatePoints(), new List<string>()) },
            { "IslandFieldOffice", (CreatePoints(), new List<string>()) },
            { "IslandHut", (CreatePoints(), new List<string>()) },
            { "IslandNorth", (CreatePoints(), new List<string>()) },
            { "IslandNorthCave1", (CreatePoints(), new List<string>()) },
            { "IslandShrine", (CreatePoints(), new List<string>()) },
            { "IslandSouth", (CreatePoints(), new List<string>()) },
            { "IslandSouthEast", (CreatePoints(), new List<string>()) },
            { "IslandSouthEastCave", (CreatePoints(), new List<string>()) },
            { "IslandWest", (CreatePoints(), new List<string>()) },
            { "IslandWestCave1", (CreatePoints(), new List<string>()) },
            { "JojaMart", (CreatePoints( (13,30), (14,30) ), 
                new List<string>{ "Town", "Town"}) },
            { "JoshHouse", (CreatePoints( (9,25) ),
                new List<string>{ "Town" }) },
            { "LeahHouse", (CreatePoints(), new List<string>()) },
            { "LeoTreeHouse", (CreatePoints(), new List<string>()) },
            { "LewisBasement", (CreatePoints(), new List<string>()) },
            { "ManorHouse", (CreatePoints( (4,12), (4,13) ),
                new List<string>{ "Town", "Town"}) },
            { "MasteryCave", (CreatePoints(), new List<string>()) },
            { "MermaidHouse", (CreatePoints(), new List<string>()) },
            { "Mine", (CreatePoints(), new List<string>()) },
            { "Mountain", (CreatePoints(), new List<string>()) },
            { "MovieTheater", (CreatePoints(), new List<string>()) },
            { "QiNutRoom", (CreatePoints(), new List<string>()) },
            { "Railroad", (CreatePoints(), new List<string>()) },
            { "Saloon", (CreatePoints( (14,25) ), 
                new List<string>{ "Town" }) },
            { "SamHouse", (CreatePoints( (4,24) ), 
                new List<string>{ "Town" }) },
            { "SandyHouse", (CreatePoints(), new List<string>()) },
            { "ScienceHouse", (CreatePoints(), new List<string>()) },
            { "SebastianRoom", (CreatePoints(), new List<string>()) },
            { "SeedShop", (CreatePoints((6, 30), (32,3)), 
                new List<string>{ "Town", "Sunroom"}) },
            { "Sewer", (CreatePoints(), new List<string>()) },
            { "SkullCave", (CreatePoints(), new List<string>()) },
            { "Submarine", (CreatePoints(), new List<string>()) },
            { "Summit", (CreatePoints(), new List<string>()) },
            { "Sunroom", (CreatePoints(), new List<string>()) },
            { "Tent", (CreatePoints(), new List<string>()) },
            { "Town", (CreatePoints(( -1, 53), (-1, 54), (-1, 55), (53, 110), (54, 110), (55, 110), (79, -1), (80, -1), (81, -1), (82, -1), (83, -1), (-1, 89), (-1, 90), (-1, 91), (-1, 92), (98, -1), (90, -1), (94, 110),    (10,85), (20,88), (36,55), (43,56), (44,56), (45,70), (57,63), (58,85), (59,85), (72,68), (95,50), (96,50), (94,81), (101,89),   (52,19), (53,19)), 
                new List<string>{ "BusStop", "BusStop", "BusStop", "Beach", "Beach", "Beach", "Mountain", "Mountain", "Mountain", "Mountain", "Mountain", "Forest", "Forest", "Forest", "Forest", "Mountain", "Mountain", "Beach", "SamHouse", "HaleyHouse", "Hospital", "SeedShop", "SeedShop", "Saloon", "JoshHouse", "ManorHouse", "ManorHouse", "Trailer", "JojaMart", "JojaMart", "Blacksmith", "ArchaeologyHouse","CommunityCenter", "CommunityCenter", }) },
            { "Trailer", (CreatePoints( (12,10) ), 
                new List<string> {"Town" }) },
            { "Trailer_Big", (CreatePoints(), new List<string>()) },
            { "Tunnel", (CreatePoints(), new List<string>()) },
            { "WitchHut", (CreatePoints(), new List<string>()) },
            { "WitchSwamp", (CreatePoints(), new List<string>()) },
            { "WitchWarpCave", (CreatePoints(), new List<string>()) },
            { "WizardHouse", (CreatePoints(), new List<string>()) },
            { "WizardHouseBasement", (CreatePoints(), new List<string>()) },
            { "Woods", (CreatePoints(), new List<string>()) }
        };

        


        private static List<Point> CreatePoints(params (int x, int y)[] points)
        {
            
            return points.Select(p => new Point(p.x, p.y)).ToList();
        }

        public string GetTeleportLocation(string currentLocation, Point npcPosition)
        {
            if (LocationsTeleportCord_Vanilla.TryGetValue(currentLocation, out var teleportData))
            {
                List<Point> points = teleportData.Item1;
                List<string> destinations = teleportData.Item2;

                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i] == npcPosition)
                    {
                        return destinations[i];
                    }
                }
            }

            // Логируем, если телепортация невозможна
            Console.WriteLine($"Teleportation not possible for location: {currentLocation}, position: {npcPosition}");
            return "Null"; // Или верните пустую строку, если так удобнее обрабатывать
        }


    }
}

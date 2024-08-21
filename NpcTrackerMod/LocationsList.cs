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
            { "AdventureGuild", (CreatePoints( (6,20) ),
                new List<string>{ "Mountain" }) },
            { "AnimalShop", (CreatePoints( (13, 20) ),
                new List<string>{ "Forest" }) },
            { "ArchaeologyHouse", (CreatePoints( (3,15) ),
                new List<string>{ "Town"}) },
            { "Backwoods", (CreatePoints((50, 11), (50, 12), (50, 13), (50, 14), (50, 15), (50, 16), (13, 40), (14, 40), (15, 40), (50, 28), (50, 29), (50, 30), (50, 31), (50, 32), (22, 29), (22, 30), (22, 31), (22, 32)),
                new List<string>{ "Mountain", "Mountain", "Mountain", "Mountain", "Mountain", "Mountain",  "Farm", "Farm", "Farm", "BusStop", "BusStop", "BusStop", "BusStop", "BusStop", "Tunnel", "Tunnel", "Tunnel", "Tunnel",}) },
            { "BathHouse_Entry", (CreatePoints( (5,10), (2,3), (7,3) ),
                new List<string>{ "RailRoad", "BathHouse_WomensLocker", "BathHouse_MensLocker" }) }, //Добавить корды в женской раздевалки
            { "BathHouse_MensLocker", (CreatePoints( (3,28), (15,28) ),
                new List<string>{ "BathHouse_Entry", "BathHouse_Pool" }) },
            { "BathHouse_Pool", (CreatePoints( (6, -1), (21, -1) ),
                new List<string>{ "BathHouse_WomensLocker", "BathHouse_MensLocker" }) },
            { "BathHouse_WomensLocker", (CreatePoints(), new List<string>()) },
            { "Beach", (CreatePoints( (38, -1), (39, -1), (40, -1), (67, -1), (30, 33), (49,10) ),
                new List<string>{ "Town", "Town", "Town", "Town", "FishShop", "ElliottHouse"}) },
            { "BeachNightMarket", (CreatePoints(), new List<string>()) },
            { "Blacksmith", (CreatePoints( (5,20) ),
                new List<string>{ "Town" }) },
            { "BoatTunnel", (CreatePoints(), new List<string>()) },
            { "BugLand", (CreatePoints(), new List<string>()) },
            { "BusStop", (CreatePoints((22, 8), (44, 22), (44, 23), (44, 24), (44, 25),  (9, 22), (9, 23), (9, 24), (9, 25), (-1, 22), (-1, 23), (-1, 24), (-1, 25), (-1, 26), (11, 6), (11, 7), (11, 8), (11, 9), (65, 22), (65, 23), (65, 24), (65, 25)),
                new List<string>{ "Desert", "Town", "Town", "Town", "Town", "Farm", "Farm", "Farm", "Farm", "Farm", "Backwoods", "Backwoods", "Backwoods", "Backwoods", "Null", "Null", "Null", "Null", }) },
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
            { "Club", (CreatePoints( (8,14) ),
                new List<string>{ "SandyHouse" }) },
            { "CommunityCenter", (CreatePoints(), new List<string>()) },
            { "Desert", (CreatePoints( (18, 26), (8, 5), (6,51) ),
                new List<string>{ "BusStop", "SkullCave", "SandyHouse" }) },
            { "DesertFestival", (CreatePoints(), new List<string>()) },
            { "ElliottHouse", (CreatePoints( (3,10) ),
                new List<string>{ "Beach" }) },
            { "Farm", (CreatePoints(), new List<string>()) },
            { "FarmCave", (CreatePoints(), new List<string>()) },
            { "FarmHouse", (CreatePoints(), new List<string>()) },
            { "FishShop", (CreatePoints( (5,10), (4,3) ), 
                new List<string>{ "Beach", "Submarine" }) },
            { "Forest", (CreatePoints( (67, -1), (68, -1), (69, -1), (70, -1), (71, -1), (72, -1), (90,15), (120, 25), (120, 26), (120, 24), (120, 27), (-1, 6), (-1, 7), (5,26),  (104,32) ), 
                new List<string>{ "Farm", "Farm", "Farm", "Farm", "Farm", "Farm", "AmimalShop", "Town", "Town", "Town", "Town", "Woods", "Woods", "WizardHouse", "LeahHouse"}) },
            { "Greenhouse", (CreatePoints(), new List<string>()) },
            { "HaleyHouse", (CreatePoints((2,25), (4,3)),
                new List<string>{"Town" }) },
            { "HarveyRoom", (CreatePoints( (6,13) ),
                new List<string>{ "Hospital" }) },
            { "Hospital", (CreatePoints( (10, 20), (10, 1), (9, 1) ), 
                new List<string>{ "Town", "HarveyRoom", "HarveyRoom" }) },
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
            { "LeahHouse", (CreatePoints( (7,10) ),
                new List<string>{ "Forest" }) },
            { "LeoTreeHouse", (CreatePoints(), new List<string>()) },
            { "LewisBasement", (CreatePoints(), new List<string>()) },
            { "ManorHouse", (CreatePoints( (4,12), (4,13) ),
                new List<string>{ "Town", "Town"}) },
            { "MasteryCave", (CreatePoints(), new List<string>()) },
            { "MermaidHouse", (CreatePoints(), new List<string>()) },
            { "Mine", (CreatePoints( (18, 14), (67, 18) ), 
                new List<string>{ "Mountain", "Mountain" }) },
            { "Mountain", (CreatePoints( (14, 41), (15, 41), (16, 41), (29, 6), (-1, 12), (-1, 13), (-1, 14), (9, -1), (10, -1), (54, 4), (57, 41), (85, 41), (103, 15),    (12,25), (8,20), (76,8)  ),
                new List<string>{ "Town", "Town", "Town", "Tent", "Backwoods", "Backwoods", "Backwoods", "Railroad", "Railroad", "Mine", "Town", "Town", "Mine", "ScienceHouse", "ScienceHouse", "AdventureGuild"}) },
            { "MovieTheater", (CreatePoints(), new List<string>()) },
            { "QiNutRoom", (CreatePoints(), new List<string>()) },
            { "Railroad", (CreatePoints( (27, 62), (28, 62), (29, 62), (30, 62), (32, -1), (33, -1), (34, -1), (54, 33), (10,56) ), 
                new List<string>{ "Mountain", "Mountain", "Mountain", "Mountain", "Summit", "Summit", "Summit", "WitchWarpCave", "BathHouse_Entry" }) },
            { "Saloon", (CreatePoints( (14,25) ), 
                new List<string>{ "Town" }) },
            { "SamHouse", (CreatePoints( (4,24) ), 
                new List<string>{ "Town" }) },
            { "SandyHouse", (CreatePoints(  (4, 10), (17, 1) ), 
                new List<string>{ "Desert", "Club"}) },
            { "ScienceHouse", (CreatePoints( (6, 25), (3, 9), (12, 23), (13, 23) ), 
                new List<string>{ "Mountain", "Mountain", "SebastianRoom", "SebastianRoom"}) },
            { "SebastianRoom", (CreatePoints( (1,0) ), 
                new List<string>{ "ScienceHouse" }) },
            { "SeedShop", (CreatePoints((6, 30), (32,3)), 
                new List<string>{ "Town", "Sunroom"}) },
            { "Sewer", (CreatePoints(), new List<string>()) },
            { "SkullCave", (CreatePoints(), new List<string>()) },
            { "Submarine", (CreatePoints(), new List<string>()) },
            { "Summit", (CreatePoints( (2, 30), (3, 30), (4, 30), (5, 30), (6, 30), (7, 30), (8, 30), (9, 30), (10, 30), (11, 30), (12, 30), (13, 30), (14, 30), (15, 30), (16, 30), (17, 30) ),
                new List<string>{ "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad", "Railroad",}) },
            { "Sunroom", (CreatePoints( (5,14) ),
                new List<string>{ "SeedShop"}) },
            { "Tent", (CreatePoints( (2,6) ), 
                new List<string>{ "Mountain" }) },
            { "Town", (CreatePoints(( -1, 53), (-1, 54), (-1, 55), (53, 110), (54, 110), (55, 110), (79, -1), (80, -1), (81, -1), (82, -1), (83, -1), (-1, 89), (-1, 90), (-1, 91), (-1, 92), (98, -1), (90, -1), (94, 110),    (10,85), (20,88), (36,55), (43,56), (44,56), (45,70), (57,63), (58,85), (59,85), (72,68), (95,50), (96,50), (94,81), (101,89),   (52,19), (53,19)), 
                new List<string>{ "BusStop", "BusStop", "BusStop", "Beach", "Beach", "Beach", "Mountain", "Mountain", "Mountain", "Mountain", "Mountain", "Forest", "Forest", "Forest", "Forest", "Mountain", "Mountain", "Beach", "SamHouse", "HaleyHouse", "Hospital", "SeedShop", "SeedShop", "Saloon", "JoshHouse", "ManorHouse", "ManorHouse", "Trailer", "JojaMart", "JojaMart", "Blacksmith", "ArchaeologyHouse","CommunityCenter", "CommunityCenter", }) },
            { "Trailer", (CreatePoints( (12,10) ), 
                new List<string> {"Town" }) },
            { "Trailer_Big", (CreatePoints(), new List<string>()) },
            { "Tunnel", (CreatePoints( (40, 7), (40, 8), (40, 9), (40, 10), (40, 11), (40, 12) ), 
                new List<string>{"Backwoods", "Backwoods", "Backwoods", "Backwoods", "Backwoods", "Backwoods",}) },
            { "WitchHut", (CreatePoints(), new List<string>()) },
            { "WitchSwamp", (CreatePoints(), new List<string>()) },
            { "WitchWarpCave", (CreatePoints( (4,10) ), 
                new List<string>{ "Railroad" }) },
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

/*
[NpcTrackerMod] NPC Dwarf has no schedule.
[NpcTrackerMod] NPC Krobus has no schedule.
[NpcTrackerMod] NPC Mister Qi has no schedule.
[NpcTrackerMod] NPC Bouncer has no schedule.
[NpcTrackerMod] NPC Gunther has no schedule.
[NpcTrackerMod] NPC Marlon has no schedule.
[NpcTrackerMod] NPC Henchman has no schedule.
[NpcTrackerMod] NPC Birdie has no schedule.
[NpcTrackerMod] NPC Mister Qi has no schedule.
*/
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
    /// <summary>The mod entry point.</summary>
    public class NpcTrackerMod : Mod // разобраться в коде, ещё слишком много и лишнего отображается в патче
    {
        // Для меню
        public static NpcTrackerMod Instance { get; private set; }
        public bool DisplayGrid { get; set; }
        public bool SwitchTargetLocations { get; set; } // true - Все локации / false - Локация с игроком     
        public bool SwitchTargetNPC { get; set; } // true - выбор отедльного нпс / false - всех нпс

        public bool SwitchDrawContinuePath = false;

        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        private Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        private Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();



        private List<(String, HashSet<Point>)> NpcNewPathRoute = new List<(String, HashSet<Point>)>();

        // Список путей и локаций

        public List<string> NpcList = new List<string>(); // Список нпс в игре


        private string previousLocationName; // локация где находился игрок

        public bool Switchnpcpath = false;


        private Texture2D lineTexture;

        private int tileSize;

        public int NpcSelected = 0;

        private Draw_Tiles DrawTiles;

        private LocationsList Locations_List;

        public override void Entry(IModHelper helper)
        {

            tileSize = Game1.tileSize;
            lineTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            lineTexture.SetData(new[] { Color.White });

            Instance = this; // Инициализация экземпляра
            DrawTiles = new Draw_Tiles(tileSize, lineTexture); // Инициализация экземпляра Draw_Tiles
            Locations_List = new LocationsList();
            // Подписка на события
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.G) // Вызов меню
            {
                if (Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new TrackingMenu();
                    
                }
            }
            if (e.Button == SButton.Z)
            {
                Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
                var warpCoordinates = new List<string>();
                
                foreach (var warps in Game1.currentLocation.warps)
                {
                    warpCoordinates.Add($"({warps.X}, {warps.Y})");
                }

                Monitor.Log(string.Join(", ", warpCoordinates), LogLevel.Info);

            }
            

        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            tileStates.Clear();
            npcPreviousPositions.Clear();
            npcTemporaryColors.Clear();
            NpcList.Clear();
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e) // отрисовка в мире
        {
            if (!DisplayGrid) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);
                
                DrawTiles.DrawGrid(spriteBatch, cameraOffset);
                DrawNpcPaths(spriteBatch, cameraOffset);               
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error in {nameof(OnRenderedWorld)}: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }



        private void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset) // сбор инфы от нпс
        {
            foreach (var npc in GetNpcsToTrack())
            {
                if (npc == null || string.IsNullOrWhiteSpace(npc.Name))
                {
                    Monitor.Log("Encountered an NPC with a null reference or without a name. Skipping this NPC.", LogLevel.Warn);
                    continue;
                }

                if (SwitchTargetNPC)
                {
                    if (!NpcList.Any())
                    {
                        AddNpcToList();
                    }

                    if (npc.Name == NpcList[NpcSelected])
                    {
                        NpcCreatePath(npc);
                    }
                }
                else
                {
                    NpcCreatePath(npc);
                }
                if (npc.Name == "Abigail")
                {

                    
                    //this.Monitor.Log($"Location: {npc.currentLocation.Name}", LogLevel.Info);
                }
            }
            
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.ContainsKey(tile.Key) ? npcTemporaryColors[tile.Key] : tile.Value.currentColor;
                DrawTiles.DrawTileHighlight(spriteBatch, tilePosition, color);
            }
        }
        private IEnumerable<NPC> GetNpcsToTrack()
        {
            // Проверка для получения всех персонажей в текущей локации
            if (!SwitchTargetLocations)
                return Game1.currentLocation?.characters ?? Enumerable.Empty<NPC>();

            // Проверка для получения всех персонажей во всех локациях
            var allNpcs = Game1.locations
                .Where(location => location != null && location.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null); // Отфильтровываем возможные null значения

            return allNpcs;
        }

        private void AddNpcToList() // Добавление в список нпс
        {
            int NpcCount = 0;
            
            if (SwitchTargetLocations)
            {
                foreach (var locateOnNpc in Game1.locations)
                {
                    foreach (var CurentNPC in locateOnNpc.characters)
                    {
                        NpcList.Add(CurentNPC.Name);                        
                        NpcCount++;
                    }
                }
            }
            else
            {
                foreach (var CurentNPC in Game1.currentLocation.characters)
                {
                    NpcList.Add(CurentNPC.Name);
                    NpcCount++;
                }
            }
            this.Monitor.Log($"Количество НПС: {NpcCount}", LogLevel.Debug);
            foreach (var CurentNpcForList in NpcList)
            {
                this.Monitor.Log($"{CurentNpcForList}", LogLevel.Debug);
            }
            Monitor.Log($"Номер нпс: {NpcSelected} Имя: {NpcList[NpcSelected]}", LogLevel.Info);


        }
       
        private void NpcCreatePath(NPC npc)
        {
            var path = GetNpcRoutePoints(npc);
            
            PathCreate(npc);
            //AddEndPositionTile(npc); // создание тайла в конце расписания (В разработке)
            AddNpcPositionTile(npc); // Создание тайла под нпс
        }
        private void AddNpcPositionTile(NPC npc) // Создание тайла под нпс
        {
            if (Game1.currentLocation == npc.currentLocation)
            {
                // Определение текущей позиции НПС
                Vector2 npcPosition = npc.Position;
                Vector2 npcBeforePosition = npc.positionBeforeEvent;
                int tileX = (int)Math.Floor((npcPosition.X + tileSize / 2) / tileSize);
                int tileY = (int)Math.Floor((npcPosition.Y + tileSize / 2) / tileSize);
                Point currentTile = new Point(tileX, tileY);

                // Обновление предыдущей позиции НПС
                if (npcPreviousPositions.TryGetValue(npc, out var prevPos) && prevPos != currentTile)
                {
                    RestoreTileColor(prevPos);
                }
                npcPreviousPositions[npc] = currentTile;

                // Отрисовка движения НПС
                DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            }
        }
        
        private void AddEndPositionTile(NPC npc) // Создание тайла в конце расписание (В разработке)
        {
            if (Game1.currentLocation == npc.currentLocation)
            {
                // Определение конечной точки для НПС
                Point end = npc.previousEndPoint;
                Vector2 endVector = new Vector2(end.X, end.Y);
                int EndTileX = (int)Math.Floor((endVector.X + tileSize / 2) / tileSize);
                int EndTileY = (int)Math.Floor((endVector.Y + tileSize / 2) / tileSize);

                Point endTile = new Point(EndTileX, EndTileY);

                //DrawTileWithPriority(endTile, Color.Blue, 1);
            }

        }
        
        private void PathCreate(NPC npc) // Пред-установка пути нпс
        {
            // Получение и обработка пути НПС
            var path = GetNpcRoutePoints(npc);
            
            if (path != null)
            {
                bool isCurrentLocation = Game1.player.currentLocation == npc.currentLocation;
                
                if (!SwitchTargetLocations) 
                { 
                    foreach (var point in path)
                    {
                        foreach (var Cord in point.Item2)
                        {
                            if (Game1.player.currentLocation.Name == point.Item1)
                            {

                                DrawTileWithPriority(Cord, Color.Green, 2);
                            }
                        }
                    }
                }

                
                else
                {
                    foreach (var GlobalPoints in path.Where(np => Game1.player.currentLocation.Name == np.Item1))
                    {
                        foreach (var pp in GlobalPoints.Item2)
                        {
                            //if (!Switchnpcpath) this.Monitor.Log($"Loc: {GlobalPoints.Item1} Route: {pp}", LogLevel.Info);
                            
                            DrawTileWithPriority(pp, Color.Green, 2);
 
                        }
                    }
                }
                // возможно стоит сделать список с локацияими и местами координат куда идёт телепорт. Например локация автобусная остановка имеет3 места телепорта и вбить корды телепортных краев
                /*
                if (SwitchDrawContinuePath)
                {
                    foreach (var NewPoints in path.Where(np => Game1.player.currentLocation.Name == np.Item1))
                    {

                        foreach (var pp in NewPoints.Item2)
                        {
                            NpcPathFilter(pp);
                            if (NpcNewPathRoute.Contains(pp))
                            {
                                DrawTileWithPriority(pp, Color.Green, 2);
                            }
                        }
                    }
                }
                */
                if (Game1.player.currentLocation.Name != previousLocationName)
                {
                    tileStates.Clear();
                    previousLocationName = Game1.player.currentLocation.Name;
                }
            }

            if (!Switchnpcpath)
            {
                
                foreach (var i in path)
                {
                    Monitor.Log($"Location: {i.Item1}, ", LogLevel.Info);
                    /*
                    foreach (var b in i.Item2)
                    {
                        Monitor.Log($"Coordination: {b}, ", LogLevel.Info);
                    }
                    */
                }
                
                Monitor.Log($"Закончилось---------------------------------------", LogLevel.Info);              
                NpcNewPathRoute.Clear();
            }

            Switchnpcpath = true;
            

        }
        
        private void DrawTileWithPriority(Point tile, Color color, int priority) // выставление приоритета на отображение тайлов
        {
            if (!tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
            {
                tileStates[tile] = (currentState.originalColor, color, priority);
            }
        }

        private void DrawTileForNpcMovement(Point tile, Color color, int priority) // Рисовка тайла под нпс
        {
            if (!tileStates.TryGetValue(tile, out var currentState))
            {
                currentState = (Color.Transparent, Color.Transparent, 0);
            }

            npcTemporaryColors[tile] = color;

            if (currentState.priority < priority)
            {
                tileStates[tile] = (currentState.originalColor, color, priority);
            }
        }

        private void RestoreTileColor(Point tile) // Востановление цвета (хз)
        {
            if (tileStates.TryGetValue(tile, out var tileState))
            {
                npcTemporaryColors.Remove(tile);
            }
        }
        private List<(string, List<Point>)> GetNpcRoutePoints(NPC npc) // Сбор данных об пути от нпс
        {
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var routePoints = new List<Point>();
            (String Location, List<Point> Coordinates) TupleLocationsPoint;
            List<(String, List<Point>)> ListTupleLocAndPoint = new List<(String, List<Point>)>();


            foreach (var scheduleEntry in npc.Schedule)
            {
                routePoints.AddRange(scheduleEntry.Value.route);
                TupleLocationsPoint = (scheduleEntry.Value.targetLocationName, new List<Point>(routePoints));
                ListTupleLocAndPoint.Add(TupleLocationsPoint);

                if (!Switchnpcpath)
                {                  
                    //this.Monitor.Log($"NPC: {npc.Name} | KEY: {scheduleEntry.Key} | Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                    //foreach (var point in routePoints) this.Monitor.Log($"Route: {point}", LogLevel.Info);
                }

            }

            return NpcPathFilter(npc.currentLocation.Name, routePoints);
            //return ListTupleLocAndPoint;
            //return routePoints;
        }
        
        public List<(string, List<Point>)> NpcPathFilter(string LocationName, List<Point> ListPoints) // Отделение пути передвижение, от пути после телепорта
        {
            if (string.IsNullOrEmpty(LocationName) || ListPoints == null || !ListPoints.Any())
            {
                Monitor.Log("Invalid input to NpcPathFilter.", LogLevel.Warn);
                return null;
            }

            string NpcLocation = LocationName; // стартовая локация где находится нпс
            string CurrentLocationPath = NpcLocation; // текущая локация

            Point CurrentCoordinatePath = new Point(); // текущая координата, где находится NPC

            List < Point > NpcCurrentPath = new List<Point>(); // часть маршрута
            List<(string, List<Point>)> TotalNpcPath = new List<(string, List<Point>)>(); // общий маршрут
            
            foreach (var point in ListPoints)
            {
                if (CurrentCoordinatePath == Point.Zero)
                {
                    CurrentCoordinatePath = point; // устанавливаем стартовую координату
                }

                bool isAdjacent = Math.Abs(point.X - CurrentCoordinatePath.X) <= 1 && 
                                  Math.Abs(point.Y - CurrentCoordinatePath.Y) <= 1; // true - если нпс идёт / fakse - если тпхнулся

                if (isAdjacent)
                {
                    // Добавляем точку в текущий маршрут
                    NpcCurrentPath.Add(point);
                    CurrentCoordinatePath = point;
                }
                else
                {
                    // Сегмент пути завершён, добавляем его в общий маршрут
                    if (NpcCurrentPath.Count > 0)
                    {
                        TotalNpcPath.Add((CurrentLocationPath, NpcCurrentPath));
                        NpcCurrentPath = new List<Point>(); // очищаем текущий маршрут
                    }
                    // Обновляем текущую локацию
                    CurrentLocationPath = Locations_List.GetTeleportLocation(CurrentLocationPath, CurrentCoordinatePath);
                    
                    // Начинаем новый сегмент маршрута
                    NpcCurrentPath.Add(point);
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
        
        private List<Point> GetPathThroughLocations(string fromLocation, string toLocation)
        {
            // Здесь вы должны реализовать логику для получения полного пути от одной локации до другой.
            // Например, используя методы поиска пути, доступные в игровом API.
            // Возвращаем полный путь между локациями, включая точки всех промежуточных локаций.

            // Примерный код для получения пути:
            // return Game1.currentLocation.GetPathToLocation(toLocation);

            return null; // Замените это на фактический метод получения пути
        }

        private IEnumerable<(String, List<Point>)> SplitPathByLocation(List<Point> fullPath, string fromLocation, string toLocation)
        {
            // Здесь реализуем разбиение полного пути на сегменты, каждый из которых соответствует отдельной локации.
            // Например, путем анализа переходных точек или известных границ между локациями.

            // Примерный код для разбиения пути:
            // var segments = new List<(String, List<Point>)>();
            // var currentSegment = new List<Point>();

            // foreach (var point in fullPath)
            // {
            //     if (IsBoundaryBetweenLocations(point))
            //     {
            //         segments.Add((fromLocation, new List<Point>(currentSegment)));
            //         currentSegment.Clear();
            //         fromLocation = GetNextLocationName(point); // Получить имя следующей локации
            //     }
            //     currentSegment.Add(point);
            // }

            // segments.Add((fromLocation, currentSegment)); // Добавляем последний сегмент

            // return segments;

            return null; // Замените это на фактическую реализацию разбиения пути
        }
    }
}
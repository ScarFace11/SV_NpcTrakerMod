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

        
        private Dictionary<NPC, Dictionary<int, Stack<Point>>> npcRoutesCache = new Dictionary<NPC, Dictionary<int, Stack<Point>>>();
        private Dictionary<NPC, List<Point>> npcPaths = new Dictionary<NPC, List<Point>>();
        private Dictionary<Point, (int priority, Color color)> tilePriorities = new Dictionary<Point, (int, Color)>();
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        private Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        private Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        //private bool DisplayGrid = false;
        //private bool SwitchTargetLocations = false; // true - Все локации / false - Локация с игроком
        //private bool SwitchTargetNPC = false; // true - выбор отедльного нпс / false - всех нпс

        private bool Switchnpcpath = false; // 

        private Texture2D lineTexture;

        private int tileSize;
        private int NpcSelected = 0;

        
        private List<string> NpcList = new List<string>(); // Список нпс в игре

        // Создаем словарь для хранения маршрутов
        // Для меню
        public static NpcTrackerMod Instance { get; private set; }
        public bool DisplayGrid { get; set; }
        public bool SwitchTargetLocations { get; set; }     
        public bool SwitchTargetNPC { get; set; }
        
        public override void Entry(IModHelper helper)
        {

            tileSize = Game1.tileSize;
            lineTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            lineTexture.SetData(new[] { Color.White });

            Instance = this; // Инициализация экземпляра
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            // Подписка на события
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.G) // Переключение отображение сетки
            {
                if (Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new TrackingMenu();
                }
            }

            if (e.Button == SButton.H) // Переключение отображение сетки
            {
                DisplayGrid = !DisplayGrid;
            }           

            if (e.Button == SButton.J) // Меняет видимость всех локаций или локация где находится игрок
            {
                tileStates.Clear();
                SwitchTargetLocations = !SwitchTargetLocations;
                Monitor.Log($"Global locations: {SwitchTargetLocations}", LogLevel.Info);
            }

            if (e.Button == SButton.K) // Переключает отображение пути у 1 или у всех нпс
            {
                tileStates.Clear();
                NpcList.Clear();
                NpcSelected = 0;
                SwitchTargetNPC = !SwitchTargetNPC;
                Monitor.Log($"Конкретика нпс: {SwitchTargetNPC}", LogLevel.Info);
            }

            if (e.Button == SButton.B) // Уменьшает показной номер выбранного нпс
            {
                if (NpcList.Any())
                {
                    tileStates.Clear();
                    if (NpcSelected > 0)
                    {
                        NpcSelected--;
                    }
                    Monitor.Log($"Номер нпс: {NpcSelected} Конкретика нпс: {NpcList[NpcSelected]}", LogLevel.Info);
                    Switchnpcpath = false;
                }
            }
            if (e.Button == SButton.N) // Увеличивает показной номер выбранного нпс
            {
                if (NpcList.Any())
                {
                    tileStates.Clear();
                    if (NpcSelected < NpcList.Count() - 1)
                    {
                        NpcSelected++;
                    }
                    Monitor.Log($"Номер нпс: {NpcSelected} Конкретика нпс: {NpcList[NpcSelected]}", LogLevel.Info);
                    Switchnpcpath = false;
                }             
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            //npcPaths.Clear();
            //tilePriorities.Clear();
            

            tileStates.Clear();
            npcPreviousPositions.Clear();
            npcTemporaryColors.Clear();
            NpcList.Clear();
            //AddNpcToListDebug();
            
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            //Monitor.Log($"1: {DisplayGrid}\n2: {SwitchTargetLocations}\n3: {SwitchTargetNPC}", LogLevel.Info);
            if (!DisplayGrid) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);
                
                DrawGrid(spriteBatch, cameraOffset);
                DrawNpcPaths(spriteBatch, cameraOffset);
                //Monitor.Log($"{DisplayGrid}", LogLevel.Info);
                

            }
            catch (Exception ex)
            {
                Monitor.Log($"Error in OnRenderedWorld: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }

        private void DrawGrid(SpriteBatch spriteBatch, Vector2 cameraOffset) // исправить изменение разрешения при изменении размера интерфейса
        {
            var location = Game1.currentLocation;
            
            for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
                {
                    Vector2 tilePosition = new Vector2(x * tileSize, y * tileSize) - cameraOffset;
                    DrawTileHighlight(spriteBatch, tilePosition, Color.Black);
                }
            }
        }

        private void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            if (!SwitchTargetLocations) // лоакция где находится игрок с нпс
            {
                
                foreach (NPC npc in Game1.currentLocation.characters)
                {
                    if (npc == null)
                    {
                        Monitor.Log("NPC is null. Skipping this NPC.", LogLevel.Warn);
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
                            PathCreateForNpc(npc);
                        }
                        
                        
                    }
                    else
                    {
                        PathCreate(npc);
                    }
                    
                }
            }
            
            else // все локации
            {
                foreach (var locate in Game1.locations)
                {
                    foreach (var npc in locate.characters)
                    {

                        if (SwitchTargetNPC)
                        {

                            if (!NpcList.Any())
                            {
                                AddNpcToList();
                            }
                            if (npc.Name == NpcList[NpcSelected])
                            {
                                PathCreateForNpc(npc);
                                
                            }
                        }
                        else
                        {
                            PathCreate(npc);
                        }

                        /*
                        if (npc.Name == "Abigail")
                        {
                            if (npcPreviousPositions.TryGetValue(npc, out var prevPos))
                            {
                                if (prevPos != currentTile)
                                {
                                    RestoreTileColor(prevPos);
                                }
                            }

                            npcPreviousPositions[npc] = currentTile;

                            DrawTileWithPriority(end, Color.Blue, 1);

                            var path = GetNpcRoutePoints(npc);
                            if (path != null)
                            {
                                foreach (var point in path)
                                {
                                    DrawTileWithPriority(point, Color.Green, 2);
                                }
                            }

                            DrawTileForNpcMovement(currentTile, Color.Blue, 1);

                            //this.Monitor.Log($"{npc.Name} Двигается от {start} в {end} ", LogLevel.Debug); // X Y конечные корды
                            //this.Monitor.Log($"{npc.isMovingOnPathFindPath} ", LogLevel.Debug); // Булевый вывод

                            /*
                            if (!npcRoutesCache.TryGetValue(npc, out var routes))
                            {
                                routes = GetNpcRoutes(npc);
                                npcRoutesCache[npc] = routes;
                            }

                            if (routes == null) continue;

                            foreach (var routeEntry in routes)
                            {
                                foreach (var point in routeEntry.Value)
                                {
                                    Vector2 tilePosition = new Vector2(point.X * tileSize, point.Y * tileSize);
                                    DrawTileHighlight(spriteBatch, tilePosition - cameraOffset, Color.Green);
                                }
                            }
                        До переноса в функцию
                            */

                        /* Прошлая
                        if (!npcPaths.TryGetValue(npc, out var npcPath))
                        {
                            npcPath = GetNpcRoutePoints(npc);
                            if (npcPath != null)
                            {
                                npcPaths[npc] = npcPath;
                            }
                        }
                        */

                        /* прошлая версия
                        if (NPCpath != null)
                        {
                            foreach (var point in NPCpath)
                            {
                                Vector2 tilePosition = new Vector2(point.X * tileSize, point.Y * tileSize) - cameraOffset;
                                DrawTileHighlight(spriteBatch, tilePosition, Color.Green);
                            }
                        }
                        */

                        // Закрашиваем клетки первого приоритета (начало и конец пути)

                        //DrawTileWithPriority(spriteBatch, start, cameraOffset, Color.Blue, 1);

                        //DrawTileWithPriority(spriteBatch, end, cameraOffset, new Color(0, 255, 0, 100), 1);

                        /*
                        if (npcPath != null)
                        {
                            foreach (var point in npcPath)
                            {
                                    DrawTileWithPriority(spriteBatch, point, cameraOffset, Color.Green, 2);
                                    //this.Monitor.Log($"{point}", LogLevel.Debug);
                                    //continue;
                            }
                        }

                        // Закрашиваем все клетки с учётом приоритета
                        foreach (var tile in tilePriorities)
                        {
                                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                                //this.Monitor.Log($"{tilePosition}", LogLevel.Debug);
                                DrawTileHighlight(spriteBatch, tilePosition, tile.Value.color);
                        }
                            /*
                        if (!keepTilesHighlighted)
                        {
                            // Для случая, когда клетки не остаются закрашенными
                            var npcpath = NPCpath;
                            if (npcpath != null)
                            {
                                foreach (var point in npcpath)
                                {
                                    Vector2 tilePosition = new Vector2(point.X * tileSize, point.Y * tileSize) - cameraOffset;
                                    DrawTileHighlight(spriteBatch, tilePosition, Color.Green);
                                }
                            }
                        }
                        */



                        /*
                        // Находим путь от текущего положения до цели
                        if (!npcPaths.TryGetValue(npc, out var path))
                        {
                            path = new List<Point>();
                            npcPaths[npc] = path;
                        }
                        */
                        /*
                        var newPath = pathfinding.FindPath(start, end);

                        if (newPath != null)
                        {
                            path.AddRange(newPath);
                        }

                        if (path != null)
                        {
                            // Отображаем путь для текущего НПС
                            foreach (var point in path)
                            {
                                Vector2 pathPosition = GetTilePosition(point);
                                DrawTileHighlight(spriteBatch, pathPosition - cameraOffset, Color.Blue);
                            }

                            
                        }

                    }
                    */
                        //Vector2 destinationPosition = GetTilePosition(end);
                        //DrawTileHighlight(spriteBatch, destinationPosition - cameraOffset, new Color(0, 255, 0, 100));


                    }
                    
                }
            }
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.ContainsKey(tile.Key) ? npcTemporaryColors[tile.Key] : tile.Value.currentColor;
                DrawTileHighlight(spriteBatch, tilePosition, color);
            }

        }
        private void AddNpcToListDegub()
        {
            int NpcCount = 0;
            foreach (var locate in Game1.locations)
            {
                foreach (var CurentNPC in locate.characters)
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
            
        }
        private void AddNpcToList() // добавляет в список
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
            Monitor.Log($"Номер нпс: {NpcSelected} Конкретика нпс: {NpcList[NpcSelected]}", LogLevel.Info);

        }
        private void PathCreateForNpc(NPC npc)
        {
            
            Vector2 npcPosition = npc.Position;
            Vector2 npcBeforePosition = npc.positionBeforeEvent;
            int tileX = (int)Math.Floor((npcPosition.X + tileSize / 2) / tileSize);
            int tileY = (int)Math.Floor((npcPosition.Y + tileSize / 2) / tileSize);


            Point currentTile = new Point(tileX, tileY);


            // Получаем точку назначения для текущего НПС
            Point end = npc.previousEndPoint;
            Vector2 endVector = new Vector2(end.X, end.Y);

            int EndTileX = (int)Math.Floor((endVector.X + tileSize / 2) / tileSize);
            int EndTileY = (int)Math.Floor((endVector.Y + tileSize / 2) / tileSize);

            Point endTile = new Point(EndTileX, EndTileY);

            if (npcPreviousPositions.TryGetValue(npc, out var prevPos))
            {
                if (prevPos != currentTile)
                {
                    RestoreTileColor(prevPos);
                }
            }

            npcPreviousPositions[npc] = currentTile;


            var path = GetNpcRoutePoints(npc);
            
            if (path != null)
            {
                foreach (var point in path)
                {
                    DrawTileWithPriority(point, Color.Green, 2);
                    if (Switchnpcpath == false) {
                        //Monitor.Log($"{point}", LogLevel.Info);
                    }
                    
                }
            }
            Switchnpcpath = true;
            DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            //DrawTileWithPriority(endTile, Color.Blue, 1);

        }

        private void PathCreate(NPC npc)
        {
            Vector2 npcPosition = npc.Position;
            Vector2 npcBeforePosition = npc.positionBeforeEvent;
            int tileX = (int)Math.Floor((npcPosition.X + tileSize / 2) / tileSize);
            int tileY = (int)Math.Floor((npcPosition.Y + tileSize / 2) / tileSize);


            Point currentTile = new Point(tileX, tileY);


            // Получаем точку назначения для текущего НПС
            Point end = npc.previousEndPoint;

            Vector2 endVector = new Vector2(end.X, end.Y);

            int EndTileX = (int)Math.Floor((endVector.X + tileSize / 2) / tileSize);
            int EndTileY = (int)Math.Floor((endVector.Y + tileSize / 2) / tileSize);

            Point endTile = new Point(EndTileX, EndTileY);
            
            if (npcPreviousPositions.TryGetValue(npc, out var prevPos))
            {
                if (prevPos != currentTile)
                {
                    RestoreTileColor(prevPos);
                }
            }

            npcPreviousPositions[npc] = currentTile;
                    

            var path = GetNpcRoutePoints(npc);
            if (path != null)
            {
                foreach (var point in path)
                {
                    DrawTileWithPriority(point, Color.Green, 2);
                }
            }

            DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            DrawTileWithPriority(endTile, Color.Blue, 1);

        }

        private void DrawTileHighlight(SpriteBatch spriteBatch, Vector2 tilePosition, Color color) // Рисует тайлы
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);

            // Отрисовка полупрозрачного зелёного квадрата
            spriteBatch.Draw(
                lineTexture, // Белая текстура 1x1 пиксель
                tileRect,
                new Color(color, 0.05f) // Полупрозрачный цвет
            );

            // Отрисовка границ квадрата

            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color); // Верхняя граница
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color); // Нижняя граница
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color); // Левая граница
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color); // Правая граница
        }

        private void DrawTileWithPriority(Point tile, Color color, int priority)
        {
            if (!tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
            {
                tileStates[tile] = (currentState.originalColor, color, priority);
            }
        }

        private void DrawTileForNpcMovement(Point tile, Color color, int priority)
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

        private void RestoreTileColor(Point tile)
        {
            if (tileStates.TryGetValue(tile, out var tileState))
            {
                npcTemporaryColors.Remove(tile);
            }
        }

        Vector2 GetTilePosition(Point tile)
        {
            return new Vector2(tile.X * tileSize, tile.Y * tileSize);
        }



        private List<Point> GetNpcRoutePoints(NPC npc)
        {
            // В данной функции спрайты не удаляются
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null)
            {
                return null;
            }
            
            var routePoints = new List<Point>();
            //if (npc.currentLocation.mapPath)

            foreach (var scheduleEntry in npc.Schedule)
            {
                
                
                //if (scheduleEntry.Value.targetLocationName == npc.currentLocation.Name.ToString()) // сравнить название локации из пути нпс и название локации где находится сам нпс
                //if (scheduleEntry.Value.targetLocationName == "Sunroom")
                //{ 
                routePoints.AddRange(scheduleEntry.Value.route); // НИХУЯ НЕ РАБОТАЕТ
                //if (Switchnpcpath == false)  this.Monitor.Log($"KEY: {scheduleEntry.Key} Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Debug);


                //}
                //if (Switchnpcpath == false)
                //{
                    //this.Monitor.Log($"KEY: {scheduleEntry.Key} Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                    //this.Monitor.Log($"Route: {routePoints}", LogLevel.Info);
                    //this.Monitor.Log($"Behavior: {scheduleEntry.Value.endOfRouteBehavior}", LogLevel.Info);
                    //this.Monitor.Log($"Tile: {scheduleEntry.Value.targetTile}", LogLevel.Info);
                    
                    
                //}

            }
            //Switchnpcpath = true;
            return routePoints;

        }
        private Dictionary<int, Stack<Point>> GetNpcRouteDictionary(NPC npc)
        {
            // Спрайты удаляются
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null)
            {
                return null;
            }
            
            var routes = new Dictionary<int, Stack<Point>>();
            foreach (var scheduleEntry in npc.Schedule)
            {
                if (scheduleEntry.Value.targetLocationName == npc.currentLocation.Name.ToString()) // сравнить название локации из пути нпс и название локации где находится сам нпс
                {
                    routes[scheduleEntry.Key] = scheduleEntry.Value.route;
                    //this.Monitor.Log($"KEY: {scheduleEntry.Key} Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                }
                
            }
            
            return routes;

        }

    }
}
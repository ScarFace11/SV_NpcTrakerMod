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

        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        private Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        private Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        private Point NpcPathException = new Point();
        private HashSet<Point> NpcNewPathRoute = new HashSet<Point>();
        private Dictionary<string, HashSet<Point>> NpcNewPathRoute2 = new Dictionary<string, HashSet<Point>>();


        public bool Switchnpcpath = false;

        private Texture2D lineTexture;

        private int tileSize;
        public int NpcSelected = 0;

        
        public List<string> NpcList = new List<string>(); // Список нпс в игре

        // Создаем словарь для хранения маршрутов
        // Для меню
        public static NpcTrackerMod Instance { get; private set; }
        public bool DisplayGrid { get; set; }
        public bool SwitchTargetLocations { get; set; } // true - Все локации / false - Локация с игроком     
        public bool SwitchTargetNPC { get; set; } // true - выбор отедльного нпс / false - всех нпс

        public override void Entry(IModHelper helper)
        {

            tileSize = Game1.tileSize;
            lineTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            lineTexture.SetData(new[] { Color.White });

            Instance = this; // Инициализация экземпляра

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

        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {

            tileStates.Clear();
            npcPreviousPositions.Clear();
            npcTemporaryColors.Clear();
            NpcList.Clear();
            //AddNpcToListDebug();
            
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e) // отрисовка в мире
        {
            if (!DisplayGrid) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);
                
                DrawGrid(spriteBatch, cameraOffset);
                DrawNpcPaths(spriteBatch, cameraOffset);               
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error in {nameof(OnRenderedWorld)}: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }

        private void DrawGrid(SpriteBatch spriteBatch, Vector2 cameraOffset) // отрисовка сетки. исправить изменение разрешения при изменении размера интерфейса
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

        private void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset) // сбор инфы от нпс
        {
            foreach (var npc in GetNpcsToTrack())
            {
                if (SwitchTargetNPC)
                {
                    if (string.IsNullOrWhiteSpace(npc.Name))
                    {
                        Monitor.Log("NPC is null. Skipping this NPC.", LogLevel.Warn);
                        continue;
                    }
                    if (!NpcList.Any())
                    {
                        AddNpcToList();
                    }
                    if (npc.Name == NpcList[NpcSelected])
                    {
                        PathCreate(npc);

                    }
                }
                else
                {
                    PathCreate(npc);
                }
            }
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.ContainsKey(tile.Key) ? npcTemporaryColors[tile.Key] : tile.Value.currentColor;
                DrawTileHighlight(spriteBatch, tilePosition, color);
            }
        }
        private IEnumerable<NPC> GetNpcsToTrack() // проверка где находится нпс, на локации с игроком или просто в мире
        {
            if (!SwitchTargetLocations) return Game1.currentLocation.characters;

            else return Game1.locations.SelectMany(location => location.characters);
        }
        private void AddNpcToListDegub() // ДЕБАГ. Добавление в список нпс
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
            Monitor.Log($"Номер нпс: {NpcSelected} Конкретика нпс: {NpcList[NpcSelected]}", LogLevel.Info);

        }

        private void PathCreate(NPC npc) // Пред-установка пути нпс
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

            if (npcPreviousPositions.TryGetValue(npc, out var prevPos) && prevPos != currentTile)
            {
                RestoreTileColor(prevPos);
            }
            npcPreviousPositions[npc] = currentTile;



            var path = GetNpcRoutePoints(npc);

            if (path != null)
            {
                foreach (var point in path)
                {
                    if (!Switchnpcpath)
                    {
                        //if (!NpcPathFilter(point)) DrawTileWithPriority(point, Color.Green, 2);
                        //Monitor.Log($"{point} SWITCH: {Switchnpcpath}", LogLevel.Info);
                    }

                    //NpcPathFilter(point);
                    //DrawTileWithPriority(point, Color.Green, 2);

                    if (Game1.player.currentLocation == npc.currentLocation && !NpcPathFilter(point)) // сравнить название локации из пути нпс и название локации где находится сам нпс
                    {
                        DrawTileWithPriority(point, Color.Green, 2);
                    }
                    else
                    {
                        tileStates.Clear();
                    }

                    //if (!NpcPathFilter(point)) DrawTileWithPriority(point, Color.Green, 2);
                    //DrawTileWithPriority(point, Color.Green, 2);                 

                }
                if (NpcNewPathRoute != null)
                {
                    foreach (var Newpoint in NpcNewPathRoute)
                    {
                        //if (Game1.player.currentLocation.Name == path.)
                            //DrawTileWithPriority(Newpoint, Color.Green, 2);
                    }
                }
            }

            if (Switchnpcpath == false)
            {
                Monitor.Log($"Закончилось---------------------------------------", LogLevel.Info);
                if (NpcNewPathRoute.Any())
                {
                    /*
                    foreach (var i in NpcNewPathRoute)
                    {
                        Monitor.Log($"Новый путь: {i}", LogLevel.Info);
                    }
                    */
                }
                NpcNewPathRoute.Clear();
                

            }
            NpcPathException = Point.Zero;
            Switchnpcpath = true;
            DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            //DrawTileWithPriority(endTile, Color.Blue, 1);

        }

        private void DrawTileHighlight(SpriteBatch spriteBatch, Vector2 tilePosition, Color color) // отрисовка тайлов
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);

            // Отрисовка полупрозрачного квадрата с границами
            spriteBatch.Draw(lineTexture, tileRect, new Color(color, 0.05f));

            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color);
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




        private List<Point> GetNpcRoutePoints(NPC npc) // Сбор данных об пути от нпс
        {
            // В данной функции спрайты не удаляются
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null)
            {
                return null;
            }
            
            var routePoints = new List<Point>();

            //Dictionary<string, List<Point>> routePoints2 = new Dictionary<string, List<Point>>();
            // Пробую добавить название локи и её корды

            foreach (var scheduleEntry in npc.Schedule)
            {
                
                
                //if (scheduleEntry.Value.targetLocationName == npc.currentLocation.Name.ToString()) // сравнить название локации из пути нпс и название локации где находится сам нпс
                //if (scheduleEntry.Value.targetLocationName == "Sunroom")
                //{ 
                routePoints.AddRange(scheduleEntry.Value.route); // НИХУЯ НЕ РАБОТАЕТ
                //routePoints2[scheduleEntry.Value.targetLocationName].AddRange(scheduleEntry.Value.route);

                //}
                if (Switchnpcpath == false)
                {
                    this.Monitor.Log($"NPC: {npc.Name} ---------------------------------", LogLevel.Info);
                    this.Monitor.Log($"KEY: {scheduleEntry.Key} Value: {scheduleEntry.Value.targetLocationName}", LogLevel.Info);
                    foreach (var point in routePoints)
                        this.Monitor.Log($"Route: {point}", LogLevel.Info);
                    this.Monitor.Log($"Behavior: {scheduleEntry.Value.endOfRouteBehavior}", LogLevel.Info);
                    this.Monitor.Log($"Tile: {scheduleEntry.Value.targetTile}", LogLevel.Info);
                    //this.Monitor.Log($"Schedule: {npc.getMasterScheduleRawData()}", LogLevel.Info); 

                }

            }
            //Switchnpcpath = true;
            return routePoints;
            //return routePoints2[npc.currentLocation.Name];

        }
        
        public bool NpcPathFilter(Point point) // Отделение пути передвижение, от пути после телепорта
            // либо както доработать
            // либо сделать чтобы все пути закинулись в какой нибудь список где будет надпись локации где должно отрисовывать путь и сами координаты
        {                                      
            if (NpcPathException == Point.Zero) // ставим стартовую клетку нпс
            {
                NpcPathException = point;
                return false;
            }

            //if (!Switchnpcpath) Monitor.Log($"NpcPathException: {NpcPathException} point: {point}", LogLevel.Info);

            bool isAdjacent = Math.Abs(point.X - NpcPathException.X) <= 1 && Math.Abs(point.Y - NpcPathException.Y) <= 1; 

            if (isAdjacent) // проверка на телепорт
            {
                //Monitor.Log($"Одинаковы! Ex: {NpcPathException} point: {point}", LogLevel.Info);
                NpcPathException = point;
                return false;
            }
            else // создаем новый путь после телепорта
            {
                //Monitor.Log($"Разные! Ex: {NpcPathException} point: {point}, Switch: {NpcPathTeleported}", LogLevel.Info);
                NpcNewPathRoute.Add(point);
                return true;
            }
        } 
    }
}
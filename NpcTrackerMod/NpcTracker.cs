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
        public bool SwitchGetNpcPath { get; set; } = true;

        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        private Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        private Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();


        // Список путей и локаций

        public List<string> NpcList = new List<string>(); // Список нпс в игре

        private List<(string, List<Point>)> path = new List<(string, List<Point>)>();

        private string previousLocationName; // локация где находился игрок

        public bool Switchnpcpath = false;


        private Texture2D lineTexture;

        private int tileSize;

        public int NpcSelected { get; set; }

        private Draw_Tiles DrawTiles;

        private LocationsList Locations_List;

        public override void Entry(IModHelper helper)
        {

            tileSize = Game1.tileSize;
            lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);

            Instance = this; // Инициализация экземпляра
            DrawTiles = new Draw_Tiles(tileSize, lineTexture); // Инициализация экземпляра Draw_Tiles
            Locations_List = new LocationsList();

            // Подписка на события
            helper.Events.Input.ButtonPressed += OnButtonPressed;            
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            
        }
        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.G && Game1.activeClickableMenu == null) // Вызов меню
            {
                Game1.activeClickableMenu = new TrackingMenu();
            }
            if (e.Button == SButton.Z)
            {
                LogCurrentLocationWarps();
            }
        }
        private void LogCurrentLocationWarps()
        {
            Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
            var warpCoordinates = Game1.currentLocation.warps
                .Select(warp => $"({warp.X}, {warp.Y})")
                .ToList();
            Monitor.Log(string.Join(", ", warpCoordinates), LogLevel.Info);
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            tileStates.Clear();
            npcPreviousPositions.Clear();
            npcTemporaryColors.Clear();
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
                        AddNpcToList(npc);
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

            }
            
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.ContainsKey(tile.Key) ? npcTemporaryColors[tile.Key] : tile.Value.currentColor;
                DrawTiles.DrawTileHighlight(spriteBatch, tilePosition, color);
            }

            SwitchGetNpcPath = false;
            DetectPlayerLocation();
        }
        private IEnumerable<NPC> GetNpcsToTrack()
        {
            // Проверка для получения всех персонажей в текущей локации
            if (!SwitchTargetLocations)
                return Game1.currentLocation?.characters ?? Enumerable.Empty<NPC>();

            // Проверка для получения всех персонажей во всех локациях
            return Game1.locations
                .Where(location => location?.characters != null)
                .SelectMany(location => location.characters)
                .Where(npc => npc != null); // Отфильтровываем возможные null значения
        }



        private void AddNpcToList(NPC npc) // Добавление в список нпс
        {
            int NpcCount = 0;
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                Monitor.Log($"У {npc.Name} Нет пути.", LogLevel.Debug);
            }
            else
            {
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


        }
       
        private void NpcCreatePath(NPC npc)
        {
            DrawNpcRoute(npc);
            AddNpcPositionTile(npc); // Создание тайла под нпс
        }
        private void AddNpcPositionTile(NPC npc) // Создание тайла под нпс
        {
            if (Game1.currentLocation != npc.currentLocation) return;

            // Определение текущей позиции НПС
            Vector2 npcPosition = npc.Position;
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

        private void DrawNpcRoute(List<(string, List<Point>)> path)
        {
            foreach (var segment in path)
            {
                foreach (var point in segment.Item2)
                {
                    if (!SwitchTargetLocations && Game1.player.currentLocation.Name != segment.Item1)
                        continue;

                    DrawTileWithPriority(point, Color.Green, 2);
                }
            }
        }

        
        private void DrawNpcRoute(NPC npc) // Пред-установка пути нпс
        {
            // Получение и обработка пути НПС
            if (SwitchGetNpcPath)
            {
                path = GetNpcRoutePoints(npc);
            }
            


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
            }
            Switchnpcpath = true;
        }
        
        private void DetectPlayerLocation()
        {
            if (Game1.player.currentLocation.Name != previousLocationName)
            {
                tileStates.Clear();
                previousLocationName = Game1.player.currentLocation.Name;
                SwitchGetNpcPath = true;
            }
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
            npcTemporaryColors[tile] = color;
            DrawTileWithPriority(tile, color, priority);
        }

        private void RestoreTileColor(Point tile) // Востановление цвета (хз)
        {
            npcTemporaryColors.Remove(tile);
        }
        private List<(string, List<Point>)> GetNpcRoutePoints(NPC npc) // Сбор данных об пути от нпс
        {
            Monitor.Log("Создание пути!", LogLevel.Debug);
            // Проверяем, есть ли у NPC расписание
            if (npc.Schedule == null || !npc.Schedule.Any())
            {
                //Monitor.Log($"NPC {npc.Name} has no schedule.", LogLevel.Warn);
                return new List<(string, List<Point>)>();
            }

            var routePoints = new List<Point>();

            foreach (var scheduleEntry in npc.Schedule)
            {
                routePoints.AddRange(scheduleEntry.Value.route);

                if (!Switchnpcpath)
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
                Monitor.Log("Invalid input to NpcPathFilter.", LogLevel.Warn);
                return null;
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
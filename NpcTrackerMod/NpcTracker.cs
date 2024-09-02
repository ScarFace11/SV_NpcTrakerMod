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
        public static NpcTrackerMod Instance;
        public bool DisplayGrid { get; set; }
        public bool SwitchTargetLocations { get; set; } // true - Все локации / false - Локация с игроком     
        public bool SwitchTargetNPC { get; set; } // true - выбор отедльного нпс / false - всех нпс
        public bool SwitchGetNpcPath { get; set; } = true; //
        public bool SwitchListFull { get; set; } = false;
        
        
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        public Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        public bool showAllRoutes { get; set; } = false;
        // Список путей и локаций

        private List<List<(string, List<Point>)>> path = new List<List<(string, List<Point>)>>();
        private string previousLocationName; // локация где находился игрок
        public bool Switchnpcpath = false;
        private Texture2D lineTexture;
        private int tileSize;
        public int NpcCount = 0;
        public int NpcSelected { get; set; }
        public Draw_Tiles DrawTiles;
        public NpcList TotalNpcList;
        private NpcManager Npc_Manager;
        public ModEntry ModEntry;
        public LocationsList LocationsList;
        public NpcTrackerMod()
        {
            TotalNpcList = new NpcList();          
        }

        public override void Entry(IModHelper helper)
        {
            tileSize = Game1.tileSize;
            lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);

            Instance = this; // Инициализация экземпляра
            DrawTiles = new Draw_Tiles(Instance, tileSize, lineTexture);
            Npc_Manager = new NpcManager(Instance);
            ModEntry = new ModEntry(Instance);
            LocationsList = new LocationsList(Instance);

            // Подписка на события
            helper.Events.Input.ButtonPressed += ModEntry.OnButtonPressed;            
            helper.Events.Display.RenderedWorld += ModEntry.OnRenderedWorld;
            helper.Events.GameLoop.DayStarted += ModEntry.OnDayStarted;
            helper.Events.GameLoop.DayEnding += ModEntry.OnDayEnding;
            //helper.Events.GameLoop.UpdateTicked += ModEntry.OnUpdateTicked;
            // Пример использования функции для проверки мода
            if (ModEntry.IsModInstalled("FlashShifter.StardewValleyExpandedCP"))
            {
                Monitor.Log("Мод установлен!", LogLevel.Info);
                // Действия, если мод установлен
            }
            else
            {
                Monitor.Log("Нет мода.", LogLevel.Info);
                // Действия, если мод не установлен
            }
            

        }

        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        public void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset) // сбор инфы от нпс
        {

            foreach (var npc in Npc_Manager.GetNpcsToTrack(SwitchTargetLocations, TotalNpcList.NpcTotalList))
            {
                if (npc == null || string.IsNullOrWhiteSpace(npc.Name))
                {
                    Monitor.Log("Encountered an NPC with a null reference or without a name. Skipping this NPC.", LogLevel.Warn);
                    continue;
                }

                if (SwitchTargetNPC)
                {
                    if (!SwitchListFull)
                    {
                        TotalNpcList.AddNpcToList(npc);
                    }

                    if (npc.Name == TotalNpcList.NpcCurrentList[NpcSelected])
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
            SwitchListFull = true;
            SwitchGetNpcPath = false;
            TotalNpcList.NpcCurrentList.Sort();
            DetectPlayerLocation();
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
                DrawTiles.RestoreTileColor(prevPos);
            }

            npcPreviousPositions[npc] = currentTile;
            // Отрисовка движения НПС
            DrawTiles.DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            
        }

        private void DrawNpcRoute(NPC npc) // Пред-установка пути нпс
        {
            // Получение и обработка пути НПС
            if (SwitchGetNpcPath)
            {
                path = Npc_Manager.GetNpcRoutePoints(npc, showAllRoutes);
            }
            if (path != null)
            {
                bool isCurrentLocation = Game1.player.currentLocation == npc.currentLocation;

                if (!SwitchTargetLocations)
                {
                    foreach (var point in path)
                    {
                        foreach (var GlobalPoints in point.Where(np => npc.currentLocation.Name == np.Item1)) //.Where(np => npc.currentLocation.Name == np.Item1)
                        {
                            foreach (var Cord in GlobalPoints.Item2)
                            {


                                DrawTiles.DrawTileWithPriority(Cord, Color.Green, 2);
                                
                            }
                        }
                            
                    }
                }
                else
                {
                    foreach (var PathList in path)
                    {
                            // пофиксить двойное отправление и ощбий путь
                        foreach (var GlobalPoints in PathList.Where(np => Game1.player.currentLocation.Name == np.Item1))
                        {
                            if (SwitchGetNpcPath)
                            {
                                try
                                {
                                    Monitor.Log($"{PathList[0]}", LogLevel.Debug);
                                }
                                catch (Exception ex)
                                {
                                    Monitor.Log($" warning : {ex}", LogLevel.Debug);
                                }
                            }
                            foreach (var pp in GlobalPoints.Item2)
                            {
                                //if (!Switchnpcpath) this.Monitor.Log($"Loc: {GlobalPoints.Item1} Route: {pp}", LogLevel.Info);

                                DrawTiles.DrawTileWithPriority(pp, Color.Green, 2);

                            }
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
        

    }
}
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
    /// <summary>
    /// Основной класс мода для отслеживания NPC.
    /// </summary>
    public class NpcTrackerMod : Mod // разобраться в коде, ещё слишком много и лишнего отображается в патче
    {
        public static NpcTrackerMod Instance;

        public Draw_Tiles DrawTiles;
        public NpcList NpcList;
        public NpcManager NpcManager;
        public ModEntry ModEntry;
        public LocationsList LocationsList;

        /// <summary> Флаг отображения сетки </summary>
        public bool DisplayGrid;

        /// <summary> Переключает отслеживание всех локаций или только текущей локации игрока </summary>
        public bool SwitchTargetLocations; // true - Все локации / false - Локация с игроком  

        /// <summary> Переключает между отслеживанием всех NPC или конкретного NPC </summary>
        public bool SwitchTargetNPC; // true - выбор отедльного нпс / false - всех нпс

        /// <summary> Флаг получения пути NPC </summary>
        public bool SwitchGetNpcPath { get; set; } = true;

        /// <summary> Флаг отображения глобального пути NPC </summary>
        public bool SwitchGlobalNpcPath = false;

        /// <summary>Флаг полного списка NPC </summary>
        public bool SwitchListFull { get; set; } = false;

        /// <summary> Флаг установки локаций </summary>
        public bool LocationSet = false;

        /// <summary> Флаг для показа всех маршрутов </summary>
        public bool showAllRoutes { get; set; } = false;

        /// <summary> Флаг переключения пути NPC </summary>
        public bool Switchnpcpath = false;

        /// <summary> Количество NPC </summary>
        public int NpcCount = 0;

        /// <summary> Выбранный NPC </summary>
        public int NpcSelected { get; set; }

        /// <summary> Словарь для хранения состояния плиток </summary>
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();

        /// <summary> Словарь для хранения предыдущих позиций NPC </summary>
        public Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();

        /// <summary> Словарь для временных цветов NPC </summary>
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        /// <summary> Список путей и локаций </summary>
        public List<List<(string, List<Point>)>> path = new List<List<(string, List<Point>)>>();


        /// <summary> Предыдущая локация игрока </summary>
        private string previousLocationName;

        /// <summary> Текстура линии для отрисовки </summary>
        private Texture2D lineTexture;

        /// <summary> Размер плитки </summary>
        private int tileSize;


        /// <summary>
        /// Инициализация мода.
        /// </summary>
        /// <param name="helper">Справочник помощника мода.</param>
        public override void Entry(IModHelper helper)
        {
            tileSize = Game1.tileSize;
            lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);

            Instance = this; // Инициализация экземпляра мода

            NpcList =       new NpcList(Instance);
            ModEntry =      new ModEntry(Instance);           
            DrawTiles =     new Draw_Tiles(Instance, tileSize, lineTexture);
            NpcManager =    new NpcManager(Instance);
            LocationsList = new LocationsList(Instance);
            
            

            // Подписка на события
            helper.Events.GameLoop.DayStarted += ModEntry.OnDayStarted;
            helper.Events.Input.ButtonPressed += ModEntry.OnButtonPressed;            
            helper.Events.Display.RenderedWorld += ModEntry.OnRenderedWorld;            
            helper.Events.GameLoop.DayEnding += ModEntry.OnDayEnding;
            //helper.Events.GameLoop.UpdateTicked += ModEntry.OnUpdateTicked;

            // Проверка наличия мода
            if (ModEntry.IsModInstalled("FlashShifter.StardewValleyExpandedCP"))
            {
                Monitor.Log("Мод установлен!", LogLevel.Info);
            }
            else
            {
                Monitor.Log("Нет мода.", LogLevel.Info);
            }
        }

        /// <summary>
        /// Создает текстуру линии для отрисовки.
        /// </summary>
        /// <param name="graphicsDevice">Графическое устройство.</param>
        /// <returns>Созданная текстура.</returns>
        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        /// <summary>
        /// Отрисовывает пути NPC.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch для отрисовки.</param>
        /// <param name="cameraOffset">Смещение камеры.</param>
        public void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            foreach (var npc in NpcManager.GetNpcsToTrack(SwitchTargetLocations, NpcList.NpcTotalList))
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
                        NpcList.AddNpcToList(npc);
                    }

                    if (npc.Name == NpcList.NpcCurrentList[NpcSelected])
                    {
                        NpcCreatePath(npc);
                    }
                }
                else
                {
                    NpcCreatePath(npc);
                }
            }

            // Отрисовка плиток
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.ContainsKey(tile.Key) ? npcTemporaryColors[tile.Key] : tile.Value.currentColor;
                
                DrawTiles.DrawTileHighlight(spriteBatch, tilePosition, color);
            }

            // Сброс флагов
            SwitchListFull = true;
            SwitchGetNpcPath = false;
            NpcList.NpcCurrentList.Sort();
            DetectPlayerLocation();
        }

        /// <summary>
        /// Создает путь для NPC.
        /// </summary>
        /// <param name="npc">NPC для создания пути.</param>
        private void NpcCreatePath(NPC npc)
        {
            DrawNpcRoute(npc);
            AddNpcPositionTile(npc);
        }

        /// <summary>
        /// Добавляет плитку для текущего положения NPC.
        /// </summary>
        /// <param name="npc">NPC для добавления плитки.</param>
        private void AddNpcPositionTile(NPC npc)
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

        /// <summary>
        /// Отрисовывает маршрут NPC.
        /// </summary>
        /// <param name="npc">NPC для отрисовки маршрута.</param>
        private void DrawNpcRoute(NPC npc) // Пред-установка пути нпс
        {
            string TargetLocation;

            // Получение и обработка пути НПС
            if (SwitchGetNpcPath)
            {
                path = new List<List<(string, List<Point>)>>();

                var TotalPath = SwitchGlobalNpcPath
                    ? NpcList.NpcTotalGlobalPath.FirstOrDefault(i => i.Key == npc.Name)
                    : NpcList.NpcTotalToDayPath.FirstOrDefault(i => i.Key == npc.Name);

                path.Add(TotalPath.Value);         
            }
            
            if (path != null)
            {
                TargetLocation = SwitchTargetLocations ? Game1.player.currentLocation.Name : npc.currentLocation.Name;

                foreach (var point in path)
                {
                    foreach (var globalPoints in point.Where(np => TargetLocation == np.Item1))
                    {
                        foreach (var coord in globalPoints.Item2)
                        {
                            DrawTiles.DrawTileWithPriority(coord, Color.Green, 2);
                        }
                    }
                }
            }          
            Switchnpcpath = true;
        }

        /// <summary>
        /// Проверка игрока на переход между локациями
        /// </summary>
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
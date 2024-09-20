using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace NpcTrackerMod
{
    /// <summary>
    /// Основной класс мода для отслеживания NPC.
    /// </summary>
    public class NpcTrackerMod : Mod
    {
        public static NpcTrackerMod Instance;

        public Draw_Tiles DrawTiles;
        public NpcList NpcList;
        public NpcManager NpcManager;
        public ModEntry ModEntry;
        public LocationsList LocationsList;

        /// <summary> Флаг отображения сетки. </summary>
        public bool DisplayGrid;

        /// <summary> Флаг установки локаций. </summary>
        public bool LocationSet = false;

        /// <summary>Флаг полного списка NPC. </summary>
        public bool SwitchListFull;

        /// <summary> Переключает между отслеживанием всех NPC или конкретного NPC. </summary>
        public bool SwitchTargetNPC = false;

        /// <summary> Флаг получения пути NPC. </summary>
        public bool SwitchGetNpcPath { get; set; } = true;

        /// <summary> Флаг отображения глобального пути NPC. </summary>
        public bool SwitchGlobalNpcPath = false;

        /// <summary> Переключает отслеживание всех локаций или только текущей локации игрока. </summary>
        public bool SwitchTargetLocations;

        /// <summary> Количество NPC. </summary>
        public int NpcCount = 0;

        /// <summary> Выбранный NPC. </summary>
        public int NpcSelected { get; set; } = 0;

        /// <summary> Словарь для хранения состояния плиток. </summary>
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();

        /// <summary> Словарь для хранения предыдущих позиций NPC. </summary>
        public Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();

        /// <summary> Словарь для временных цветов NPC. </summary>
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        /// <summary> Список путей и локаций. </summary>
        public List<List<(string, List<Point>)>> path = new List<List<(string, List<Point>)>>();


        /// <summary> Предыдущая локация игрока. </summary>
        private string previousLocationName;

        /// <summary> Размер плитки. </summary>
        private int tileSize;


        /// <summary>
        /// Инициализация мода.
        /// </summary>
        /// <param name="helper">Справочник помощника мода.</param>
        public override void Entry(IModHelper helper)
        {
            tileSize = Game1.tileSize;
            Instance = this; // Инициализация экземпляра мода

            NpcList =       new NpcList(Instance);
            ModEntry =      new ModEntry(Instance);           
            DrawTiles =     new Draw_Tiles(Instance, tileSize);
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
        /// Отрисовывает пути NPC.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch для отрисовки.</param>
        /// <param name="cameraOffset">Смещение камеры.</param>
        public void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            // Получаем текущий список NPC для отслеживания
            var npcList = NpcManager.GetNpcsToTrack(SwitchTargetLocations, NpcList.NpcTotalList);

            // Если нужно отслеживать конкретного NPC
            if (SwitchTargetNPC && !SwitchListFull)
            {
                NpcList.NpcAddCurrentList(npcList);
                NpcList.NpcCurrentList.Sort();
                SwitchListFull = true;
            }

            foreach (var npc in npcList.Where(n => n != null && !string.IsNullOrWhiteSpace(n.Name)))
            {
                if (!SwitchTargetNPC || npc.Name == NpcList.GetNpcFromList())
                {
                    NpcCreatePath(npc);
                }
            }

            // Отрисовка всех плиток
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.TryGetValue(tile.Key, out var tempColor) ? tempColor : tile.Value.currentColor;

                DrawTiles.DrawTileHighlight(spriteBatch, tilePosition, color);
            }

            SwitchGetNpcPath = false;
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
            // Проверка на то, находится ли NPC в текущей локации
            if (Game1.currentLocation != npc.currentLocation) return;

            // Определение текущей позиции NPC
            Point currentTile = new Point(
                (int)Math.Floor((npc.Position.X + tileSize / 2) / tileSize),
                (int)Math.Floor((npc.Position.Y + tileSize / 2) / tileSize)
            );

            // Обновление предыдущей позиции NPC и восстановление цвета плитки, если NPC переместился
            if (npcPreviousPositions.TryGetValue(npc, out var prevPos) && prevPos != currentTile)
            {
                DrawTiles.RestoreTileColor(prevPos);
            }

            // Обновление позиции NPC в словаре
            npcPreviousPositions[npc] = currentTile;

            // Отрисовка плитки для движения NPC
            DrawTiles.DrawTileForNpcMovement(currentTile, Color.Blue, 1);
        }

        /// <summary>
        /// Отрисовывает маршрут NPC.
        /// </summary>
        /// <param name="npc">NPC для отрисовки маршрута.</param>
        private void DrawNpcRoute(NPC npc)
        {
            // Проверка на наличие расписания
            if (!SwitchGetNpcPath) return;

            var totalPath = SwitchGlobalNpcPath
                ? NpcList.NpcTotalGlobalPath.FirstOrDefault(i => i.Key == npc.Name)
                : NpcList.NpcTotalToDayPath.FirstOrDefault(i => i.Key == npc.Name);

            var path = new List<List<(string, List<Point>)>> { totalPath.Value };

            if (path == null || !path.Any())
            {
                Monitor.Log($"NPC {npc.Name} has no valid path data.", LogLevel.Warn);
                return;
            }

            string TargetLocation = SwitchTargetLocations ? Game1.player.currentLocation.Name : npc.currentLocation.Name;

            foreach (var globalPoints in path.SelectMany(p => p.Where(np => np.Item1 == TargetLocation)))
            {
                foreach (var coord in globalPoints.Item2)
                {
                    DrawTiles.DrawTileWithPriority(coord, Color.Green, 2);
                }
            }
        }

        /// <summary>
        /// Проверка игрока на переход между локациями.
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
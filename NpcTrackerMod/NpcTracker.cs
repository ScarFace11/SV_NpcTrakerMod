using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    /// <summary>
    /// Основной класс мода для отслеживания NPC.
    /// </summary>
    public class _modInstance : Mod
    {
        public static _modInstance Instance { get; private set; }

        public Draw_Tiles DrawTiles { get; private set; }
        public NpcList NpcList { get; private set; }
        public NpcManager NpcManager { get; private set; }
        public ModEntry ModEntry { get; private set; }
        public LocationsList LocationsList { get; private set; }
        public CustomNpcPaths CustomNpcPaths { get; private set; }
  
        /// <summary> Флаг включения видимости путей. </summary>
        public bool EnableDisplay;

        /// <summary> Флаг отображения сетки. </summary>
        public bool DisplayGrid = false;

        /// <summary> Флаг установки локаций. </summary>
        public bool LocationSet { get; set; } = false;

        /// <summary>Флаг полного списка NPC. </summary>
        public bool SwitchListFull = false;

        /// <summary> Переключает между отслеживанием всех NPC или конкретного NPC. </summary>
        public bool SwitchTargetNPC  = false;

        /// <summary> Флаг получения пути NPC. </summary>
        public bool SwitchGetNpcPath = true;

        /// <summary> Флаг отображения глобального пути NPC. </summary>
        public bool SwitchGlobalNpcPath = false;

        /// <summary> Переключает отслеживание всех локаций или только текущей локации игрока. </summary>
        public bool SwitchTargetLocations = false;

        /// <summary> Количество NPC. </summary>
        public int NpcCount { get; set; } = 0;

        /// <summary> Выбранный NPC. </summary>
        public int NpcSelected { get; set; } = 0;

        
        /// <summary> Словарь для хранения предыдущих позиций NPC. </summary>
        public Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();


        public IEnumerable<NPC> npcList;

        /// <summary>
        /// Инициализация мода.
        /// </summary>
        /// <param name="helper">Справочник помощника мода.</param>
        public override void Entry(IModHelper helper)
        {

            Instance = this; // Инициализация экземпляра мода

            NpcList = new NpcList(Instance);
            ModEntry = new ModEntry(Instance);
            DrawTiles = new Draw_Tiles(Instance);
            NpcManager = new NpcManager(Instance);
            LocationsList = new LocationsList(Instance);
            CustomNpcPaths = new CustomNpcPaths(Instance);

            // Подписка на события
            helper.Events.GameLoop.DayStarted += ModEntry.OnDayStarted;
            helper.Events.Input.ButtonPressed += ModEntry.OnButtonPressed;
            helper.Events.Display.RenderedWorld += ModEntry.OnRenderedWorld;
            helper.Events.GameLoop.DayEnding += ModEntry.OnDayEnding;
            helper.Events.GameLoop.UpdateTicked += ModEntry.OnUpdateTicked;
            
            helper.Events.Player.Warped += ModEntry.OnPlayerWarped;
            //CustomNpcPaths.LoadAllModSchedules();
            
        }



        /// <summary>
        /// Отрисовывает пути NPC.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch для отрисовки.</param>
        /// <param name="cameraOffset">Смещение камеры.</param>
        public void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            // Получаем текущий список NPC для отслеживания
            npcList = NpcManager.GetNpcsToTrack(SwitchTargetLocations, NpcList.TotalNpcList);

            if (!npcList.Any()) return;

            // Если нужно отслеживать конкретного NPC
            if (SwitchTargetNPC && !SwitchListFull)
            {
                NpcList.NpcAddCurrentList(npcList);
                NpcList.CurrentNpcList.Sort();
                SwitchListFull = true;
            }

            foreach (var npc in npcList.Where(n => n != null && !string.IsNullOrWhiteSpace(n.Name)))
            {
                if (!SwitchTargetNPC || npc.Name == NpcList.GetNpcFromList())
                {
                    NpcCreatePath(npc);
                }
            }

            DrawTiles.DrawTiles(spriteBatch, cameraOffset);

            SwitchGetNpcPath = false;
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
                (int)Math.Floor((npc.Position.X + DrawTiles.tileSize / 2) / DrawTiles.tileSize),
                (int)Math.Floor((npc.Position.Y + DrawTiles.tileSize / 2) / DrawTiles.tileSize)
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

            var pathData = SwitchGlobalNpcPath
                ? NpcList.GlobalNpcPaths.TryGetValue(npc.Name, out var globalPath) ? globalPath : null
                : NpcList.NpcTotalToDayPath.TryGetValue(npc.Name, out var dailyPath) ? dailyPath : null;

            if (pathData == null)
            {
                Monitor.Log($"NPC {npc.Name} не имеет действительных данных о пути.", LogLevel.Warn);
                return;
            }

            string targetLocation = SwitchTargetLocations ? Game1.player.currentLocation.Name : npc.currentLocation.Name;
            foreach (var globalPoints in pathData.Where(np => np.Item1 == targetLocation))
            {
                foreach (var coord in globalPoints.Item2)
                {
                    DrawTiles.DrawTileWithPriority(coord, Color.Green, 2);
                }
            }
        }
    }
}
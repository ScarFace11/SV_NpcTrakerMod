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

        /// <summary> Конфигурация мода (горячие клавиши). </summary>
        public ModConfig Config { get; private set; }

        /// <summary> Флаг включения видимости путей. </summary>
        public bool EnableDisplay;

        /// <summary> Флаг отображения сетки. </summary>
        public bool DisplayGrid = false;

        /// <summary> Флаг установки локаций. </summary>
        public bool LocationSet { get; set; } = false;

        /// <summary> Флаг полного списка NPC. </summary>
        public bool SwitchListFull = false;

        /// <summary> Переключает между отслеживанием всех NPC или конкретного NPC. </summary>
        public bool SwitchTargetNPC = false;

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

        /// <summary>
        /// Фильтр по времени дня. -1 = показывать полный дневной маршрут.
        /// Иначе показываются только отрезки пути до указанного игрового времени (например, 900, 1200).
        /// </summary>
        public int TimeFilter { get; set; } = -1;

        /// <summary> Словарь для хранения предыдущих позиций NPC. </summary>
        public Dictionary<string, Point> npcPreviousPositions = new Dictionary<string, Point>();

        public IEnumerable<NPC> npcList;

        /// <summary>
        /// Инициализация мода.
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            Instance = this;

            Config = helper.ReadConfig<ModConfig>();

            NpcList = new NpcList(Instance);
            ModEntry = new ModEntry(Instance);
            DrawTiles = new Draw_Tiles(Instance);
            NpcManager = new NpcManager(Instance);
            LocationsList = new LocationsList(Instance);
            CustomNpcPaths = new CustomNpcPaths(Instance);

            helper.Events.GameLoop.DayStarted += ModEntry.OnDayStarted;
            helper.Events.Input.ButtonPressed += ModEntry.OnButtonPressed;
            helper.Events.Display.RenderedWorld += ModEntry.OnRenderedWorld;
            helper.Events.GameLoop.DayEnding += ModEntry.OnDayEnding;
            helper.Events.GameLoop.UpdateTicked += ModEntry.OnUpdateTicked;
            helper.Events.Player.Warped += ModEntry.OnPlayerWarped;
        }

        /// <summary> Сохраняет конфигурацию в config.json. </summary>
        public void SaveConfig() => Helper.WriteConfig(Config);

        /// <summary> Форматирует игровое время (например, 930 → "09:30"). </summary>
        public static string FormatGameTime(int gameTime)
        {
            int hours = gameTime / 100;
            int minutes = gameTime % 100;
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// Отрисовывает пути NPC.
        /// </summary>
        public void DrawNpcPaths(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            try
            {
                npcList = NpcManager.GetNpcsToTrack(SwitchTargetLocations, NpcList.TotalNpcList)
                    .Where(n => n != null && !string.IsNullOrWhiteSpace(n.Name));

                if (!npcList.Any()) return;

                if (SwitchTargetNPC && !SwitchListFull)
                {
                    NpcList.NpcAddCurrentList(npcList);
                    NpcList.CurrentNpcList.Sort();
                    SwitchListFull = true;
                }

                foreach (var npc in npcList)
                {
                    if (ShouldTrackNpc(npc))
                        NpcCreatePath(npc);
                }

                DrawTiles?.DrawTiles(spriteBatch, cameraOffset);
                SwitchGetNpcPath = false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Ошибка в DrawNpcPaths: {ex.Message}", LogLevel.Error);
            }
        }

        private bool ShouldTrackNpc(NPC npc)
        {
            if (!SwitchTargetNPC) return true;
            var targetNpcName = NpcList.GetNpcFromList();
            return !string.IsNullOrEmpty(targetNpcName) && npc.Name == targetNpcName;
        }

        private void NpcCreatePath(NPC npc)
        {
            DrawNpcRoute(npc);
            AddNpcPositionTile(npc);
        }

        /// <summary>
        /// Добавляет синюю плитку на текущую позицию NPC и регистрирует владельца тайла.
        /// </summary>
        private void AddNpcPositionTile(NPC npc)
        {
            if (Game1.currentLocation != npc.currentLocation) return;

            Point currentTile = new Point(
                (int)Math.Floor((npc.Position.X + DrawTiles.tileSize / 2) / DrawTiles.tileSize),
                (int)Math.Floor((npc.Position.Y + DrawTiles.tileSize / 2) / DrawTiles.tileSize)
            );

            string npcName = npc.Name ?? "unknown";

            if (npcPreviousPositions.TryGetValue(npcName, out var prevPos) && prevPos != currentTile)
                DrawTiles.RestoreTileColor(prevPos);

            npcPreviousPositions[npcName] = currentTile;
            DrawTiles.DrawTileForNpcMovement(currentTile, Color.Blue, 1);
            DrawTiles.RegisterTileOwner(currentTile, npcName, "Сейчас здесь");
        }

        /// <summary>
        /// Отрисовывает маршрут NPC (зелёные тайлы).
        /// Поддерживает фильтр по времени: если TimeFilter >= 0, показывает только
        /// отрезки пути с временем расписания ≤ TimeFilter.
        /// </summary>
        private void DrawNpcRoute(NPC npc)
        {
            try
            {
                if (!SwitchGetNpcPath || npc == null) return;

                List<(string, List<Point>)> pathData;
                string timeLabel = null;

                if (SwitchGlobalNpcPath)
                {
                    if (!NpcList.GlobalNpcPaths.TryGetValue(npc.Name, out var globalPath) || globalPath == null)
                    {
                        Monitor.Log($"NPC {npc.Name} не имеет глобального пути.", LogLevel.Warn);
                        return;
                    }
                    pathData = globalPath;
                }
                else if (TimeFilter >= 0 &&
                         NpcList.NpcTimedDayPath.TryGetValue(npc.Name, out var timedPath) &&
                         timedPath.Any())
                {
                    // Собираем только отрезки с временем ≤ TimeFilter
                    pathData = new List<(string, List<Point>)>();
                    int lastTime = -1;
                    foreach (var kvp in timedPath.Where(t => t.Key <= TimeFilter))
                    {
                        pathData.AddRange(kvp.Value);
                        lastTime = kvp.Key;
                    }
                    if (!pathData.Any()) return;
                    timeLabel = $"До {FormatGameTime(lastTime)}";
                }
                else
                {
                    if (!NpcList.NpcTotalToDayPath.TryGetValue(npc.Name, out var dailyPath) || dailyPath == null)
                    {
                        Monitor.Log($"NPC {npc.Name} не имеет дневного пути.", LogLevel.Warn);
                        return;
                    }
                    pathData = dailyPath;
                }

                string targetLocation = SwitchTargetLocations
                    ? Game1.player.currentLocation.Name
                    : npc.currentLocation.Name;

                foreach (var segment in pathData.Where(np => np.Item1 == targetLocation))
                {
                    foreach (var coord in segment.Item2)
                    {
                        DrawTiles.DrawTileWithPriority(coord, Color.Green, 2);
                        DrawTiles.RegisterTileOwner(coord, npc.Name, timeLabel ?? "Маршрут");
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Ошибка при отрисовке пути NPC {npc?.Name}: {ex.Message}", LogLevel.Error);
            }
        }
    }
}

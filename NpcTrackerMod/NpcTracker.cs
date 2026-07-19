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

            // Загружаем расписания модовых NPC из ContentPatcher JSON-файлов при старте,
            // чтобы данные были готовы к первому DayStarted.
            CustomNpcPaths.LoadAllModSchedules();
            

            // Загружаем расписания модовых NPC из ContentPatcher JSON-файлов при старте,
            // чтобы данные были готовы к первому DayStarted.
            CustomNpcPaths.LoadAllModSchedules();
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

            string npcName = npc.Name ?? "unknown";

            // Обновление предыдущей позиции NPC и восстановление цвета плитки, если NPC переместился
            if (npcPreviousPositions.TryGetValue(npcName, out var prevPos) && prevPos != currentTile)
            {
                DrawTiles.RestoreTileColor(prevPos);
            }

            // Обновление позиции NPC в словаре
            npcPreviousPositions[npcName] = currentTile;

            // Отрисовка плитки для движения NPC
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
            // Проверка на наличие расписания
            if (!SwitchGetNpcPath) return;

            List<(string, List<Microsoft.Xna.Framework.Point>)> pathData = null;

            if (SwitchGlobalNpcPath)
            {
                NpcList.GlobalNpcPaths.TryGetValue(npc.Name, out pathData);
            }
            else
            {
                // Сначала пробуем дневной путь. Если его нет (модовые NPC часто попадают
                // только в GlobalNpcPaths через CustomNpcPaths.TransferPath) — берём глобальный.
                if (!NpcList.NpcTotalToDayPath.TryGetValue(npc.Name, out pathData) || pathData == null)
                {
                    NpcList.GlobalNpcPaths.TryGetValue(npc.Name, out pathData);
                }
            }

            if (pathData == null)
            try
            {
                if (!SwitchGetNpcPath || npc == null) return;
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
                    if (!NpcList.NpcTotalToDayPath.TryGetValue(npc.Name, out pathData) || pathData == null)
                    {
                        NpcList.GlobalNpcPaths.TryGetValue(npc.Name, out pathData);
                    }

                }

                if (pathData == null)
                {
                    Monitor.Log($"NPC {npc.Name} не имеет действительных данных о пути.", LogLevel.Warn);
                    return;
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
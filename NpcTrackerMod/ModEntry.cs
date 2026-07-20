using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NpcTrackerMod.Core;
using NpcTrackerMod.Rendering;
using NpcTrackerMod.Scheduling;
using NpcTrackerMod.Tracking;
using NpcTrackerMod.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace NpcTrackerMod
{
    /// <summary>
    /// Точка входа мода. Создаёт все сервисы, подписывается на события SMAPI.
    /// Не хранит публичного состояния — всё через ModState.
    /// </summary>
    public class ModEntry : Mod
    {
        // ── Сервисы (создаются в Entry) ───────────────────────────────────────────
        private ModState _state;
        private NpcPathStore _pathStore;
        private LocationMapper _locationMapper;
        private NpcRegistry _registry;
        private ScheduleProcessor _scheduleProcessor;
        private CustomScheduleLoader _scheduleLoader;
        private TileRenderer _tileRenderer;
        private RouteRenderer _routeRenderer;
        private NpcTracker _tracker;
        private ModConfig _config;

        // ── Служебное состояние ───────────────────────────────────────────────────
        private bool _globalRoutesBuilt;
        private bool _dayActive;
        private string _previousLocationName;

        // ── Entry ─────────────────────────────────────────────────────────────────

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();

            // Core
            _state = new ModState();
            _pathStore = new NpcPathStore(Monitor);

            // Scheduling
            _locationMapper = new LocationMapper(Monitor);
            _registry = new NpcRegistry(Monitor, _pathStore);
            _scheduleProcessor = new ScheduleProcessor(Monitor, _pathStore, _registry, _locationMapper);
            _scheduleLoader = new CustomScheduleLoader(Monitor, helper, _scheduleProcessor, _registry);

            // Rendering
            _tileRenderer = new TileRenderer(Game1.graphics.GraphicsDevice);
            _routeRenderer = new RouteRenderer(Monitor, _state, _pathStore, _tileRenderer);

            // Tracking
            _tracker = new NpcTracker(_state, _registry, _scheduleProcessor, _routeRenderer, _tileRenderer);

            // Подписки на события
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Player.Warped += OnPlayerWarped;

            // Загружаем JSON-расписания модов заранее, чтобы они были готовы к DayStarted
            _scheduleLoader.LoadAll();
        }

        // ── SMAPI Events ──────────────────────────────────────────────────────────

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            ClearDay();
            _dayActive = true;

            if (!_state.LocationSet)
            {
                _locationMapper.BuildFromGame();
                _state.LocationSet = true;
            }

            // Собираем GameNpcs из всех локаций
            _registry.RefreshGameNpcs();

            // Глобальные маршруты строятся только один раз за сессию
            if (!_globalRoutesBuilt)
            {
                _scheduleLoader.TransferToProcessor();
                foreach (var npc in _registry.GameNpcs)
                {
                    try { _scheduleProcessor.BuildGlobalRoute(npc, null, null, null); }
                    catch (Exception ex)
                    { Monitor.Log($"Ошибка глобального маршрута {npc.Name}: {ex.Message}", LogLevel.Warn); }
                }
                _globalRoutesBuilt = true;
            }

            // Дневные маршруты строятся каждый день
            foreach (var npc in _registry.GameNpcs)
            {
                try { _scheduleProcessor.BuildDayRoutes(npc); }
                catch (Exception ex)
                { Monitor.Log($"Ошибка дневного маршрута {npc.Name}: {ex.Message}", LogLevel.Warn); }
            }

            PopulateModSources();
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e) => _dayActive = false;

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (Game1.activeClickableMenu != null || !Context.IsPlayerFree) return;

            if (e.Button == _config.MenuKey)
                OpenMenu();
            else if (e.Button == _config.DebugKey)
                LogCurrentLocationWarps();
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!_state.EnableDisplay) return;

            try
            {
                var batch = e.SpriteBatch;
                var camera = new Vector2(Game1.viewport.X, Game1.viewport.Y);

                _tracker.DrawPaths(batch, camera);

                if (_state.DisplayGrid)
                    _tileRenderer.DrawGrid(batch, camera);

                // Тултип при наведении на тайл маршрута
                int tx = (int)((Game1.viewport.X + Game1.getMouseX()) / Game1.tileSize);
                int ty = (int)((Game1.viewport.Y + Game1.getMouseY()) / Game1.tileSize);
                var hovered = new Point(tx, ty);

                if (_tileRenderer.TileOwners.TryGetValue(hovered, out var owners) && owners.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var o in owners)
                    {
                        if (sb.Length > 0) sb.Append('\n');
                        sb.Append(string.IsNullOrEmpty(o.TimeInfo)
                            ? o.NpcName
                            : $"{o.NpcName} ({o.TimeInfo})");
                    }
                    IClickableMenu.drawHoverText(batch, sb.ToString(), Game1.smallFont);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Ошибка в OnRenderedWorld: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Game1.player.currentLocation.Name == _previousLocationName) return;

            _tileRenderer.Clear();
            _previousLocationName = Game1.player.currentLocation.Name;
            _state.SwitchGetNpcPath = true;
            _state.NpcCount = Game1.player.currentLocation.characters.Count();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_dayActive || !e.IsMultipleOf(60)) return;
            if (!_state.EnableDisplay || _state.SwitchGlobalNpcPath || _state.SwitchTargetLocations) return;

            int npcCount = Game1.player.currentLocation.characters.Count();
            if (npcCount == _state.NpcCount) return;

            _tileRenderer.Clear();
            _state.SwitchGetNpcPath = true;
            _state.NpcCount = npcCount;

            _registry.RefreshCurrentNpcList();
        }

        // ── Утилиты ───────────────────────────────────────────────────────────────

        private void PopulateModSources()
        {
            _registry.NpcModSource.Clear();
            foreach (string name in _registry.TotalNpcList)
            {
                _registry.NpcModSource[name] = _scheduleLoader.NpcModNames.TryGetValue(name, out string mod)
                    ? mod
                    : "Жители деревни";
            }
        }

        private void OpenMenu()
        {
            Game1.activeClickableMenu = new TrackingMenu(
                Monitor, _state, _registry, _tileRenderer, _config,
                () => Helper.WriteConfig(_config));
        }

        private void LogCurrentLocationWarps()
        {
            Monitor.Log(Game1.currentLocation.Name, LogLevel.Info);
            foreach (var w in Game1.currentLocation.warps)
                Monitor.Log($" warp: X={w.X} Y={w.Y} → {w.TargetName}", LogLevel.Debug);
            foreach (var d in Game1.currentLocation.doors.Pairs)
                Monitor.Log($" door: {d}", LogLevel.Debug);
        }

        private void ClearDay()
        {
            _tileRenderer.Clear();
            _tileRenderer.NpcPositionColors.Clear();
            _state.NpcPreviousPositions.Clear();
            _state.SwitchGetNpcPath = false;
            _state.NpcCount = 0;

            _registry.ClearDay();
            _pathStore.ClearDay();
        }
    }
}
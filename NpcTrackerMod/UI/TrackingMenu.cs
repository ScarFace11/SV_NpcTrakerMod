using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NpcTrackerMod.Core;
using NpcTrackerMod.Rendering;
using NpcTrackerMod.Tracking;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace NpcTrackerMod.UI
{
    /// <summary>
    /// Главное меню мода. Чистый UI — только отображение и обработка ввода.
    /// Взаимодействует с ModState, NpcRegistry и TileRenderer через конструктор.
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        // ── Константы ────────────────────────────────────────────────────────────
        private const int MENU_WIDTH       = 600;
        private const int MENU_HEIGHT      = 600;
        private const int TAB_HEIGHT       = 40;
        private const int CHECKBOX_SPACING = 50;
        private const int BUTTON_SIZE      = 30;

        private static readonly int[] TimeSteps =
        {
            -1,
            600, 700, 800, 900, 1000, 1100, 1200,
            1300, 1400, 1500, 1600, 1700, 1800,
            1900, 2000, 2100, 2200, 2300, 2400, 2500, 2600
        };

        // ── Зависимости (без singleton) ──────────────────────────────────────────
        private readonly IMonitor    _monitor;
        private readonly ModState    _state;
        private readonly NpcRegistry _registry;
        private readonly TileRenderer _tiles;
        private readonly ModConfig   _config;
        private readonly Action      _saveConfig;

        // ── UI-компоненты ────────────────────────────────────────────────────────
        private readonly List<UIComponent>       _components = new List<UIComponent>();
        private readonly List<ClickableCheckbox> _checkboxes = new List<ClickableCheckbox>();
        private readonly List<TabInfo>           _tabs       = new List<TabInfo>();

        private int    _activeTabIndex;
        private string _rebindTarget;   // "menu" / "debug" / null
        private int    _timeFilterIndex;

        private ClickableTextureComponent _closeBtn;
        private ClickableTextureComponent _prevTabBtn;
        private ClickableTextureComponent _nextTabBtn;

        // ── Конструктор ──────────────────────────────────────────────────────────
        public TrackingMenu(
            IMonitor monitor,
            ModState state,
            NpcRegistry registry,
            TileRenderer tiles,
            ModConfig config,
            Action saveConfig)
            : base(0, 0, MENU_WIDTH, MENU_HEIGHT)
        {
            _monitor    = monitor    ?? throw new ArgumentNullException(nameof(monitor));
            _state      = state      ?? throw new ArgumentNullException(nameof(state));
            _registry   = registry   ?? throw new ArgumentNullException(nameof(registry));
            _tiles      = tiles      ?? throw new ArgumentNullException(nameof(tiles));
            _config     = config     ?? throw new ArgumentNullException(nameof(config));
            _saveConfig = saveConfig ?? throw new ArgumentNullException(nameof(saveConfig));

            int idx = Array.IndexOf(TimeSteps, _state.TimeFilter);
            _timeFilterIndex = idx >= 0 ? idx : 0;

            InitPosition();
            InitTabs();
            RebuildComponents();
        }

        // ── Инициализация ────────────────────────────────────────────────────────

        private void InitPosition()
        {
            xPositionOnScreen = Game1.viewport.Width  / 2 - MENU_WIDTH  / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - MENU_HEIGHT / 2;
        }

        private void InitTabs()
        {
            _tabs.Add(new TabInfo("Main",     BuildMainTab));
            _tabs.Add(new TabInfo("Settings", BuildSettingsTab));
            _tabs.Add(new TabInfo("Info",     BuildInfoTab));
        }

        private void RebuildComponents()
        {
            _components.Clear();
            _checkboxes.Clear();

            _closeBtn = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 48, yPositionOnScreen - 8, 48, 48),
                Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            _prevTabBtn = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 2f);

            _nextTabBtn = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 50, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 2f);

            _tabs[_activeTabIndex].InitializeAction?.Invoke();
        }

        // ── Вкладки ───────────────────────────────────────────────────────────────

        private void BuildMainTab()
        {
            int y = yPositionOnScreen + 80;

            Add(y, "Включение",                  () => _state.EnableDisplay,      v => _state.EnableDisplay = v);
            y += CHECKBOX_SPACING;
            Add(y, "Отображение сетки",           () => _state.DisplayGrid,        v => _state.DisplayGrid = v);
            y += CHECKBOX_SPACING;
            Add(y, "Выбор NPC",                   () => _state.SwitchTargetNPC,    v =>
            {
                _state.SwitchTargetNPC = v;
                if (!v) _registry.CurrentNpcName = null;
                else    _state.NpcSelected = 0;
                _tiles.Clear();
                _registry.CurrentNpcList.Clear();
                _state.SwitchGetNpcPath = true;
                _state.SwitchListFull   = false;
            });
            y += CHECKBOX_SPACING * 2;
            Add(y, "Отображение всех локаций",    () => _state.SwitchTargetLocations, v =>
            {
                _state.SwitchTargetLocations = v;
                _tiles.Clear();
                _state.SwitchGetNpcPath = true;
                _registry.CurrentNpcList.Clear();
                _state.SwitchListFull = false;
                if (v && _registry.CurrentNpcName != null)
                    _state.NpcSelected = _registry.CurrentNpcList.IndexOf(_registry.CurrentNpcName);
                else if (!v)
                    ClampNpcSelection();
            });
            y += CHECKBOX_SPACING;
            Add(y, "Отображение всех маршрутов",  () => _state.SwitchGlobalNpcPath, v =>
            {
                _state.SwitchGlobalNpcPath = v;
                _tiles.Clear();
                _state.SwitchGetNpcPath = true;
            });

            // Кнопки ◄ ► выбора NPC
            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f),
                () => MoveNpcSelection(-1)));

            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + 350, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f),
                () => MoveNpcSelection(1)));
        }

        private void BuildSettingsTab()
        {
            int y = yPositionOnScreen + 110;

            string menuLabel  = _rebindTarget == "menu"  ? "Нажмите клавишу" : "Изменить";
            string debugLabel = _rebindTarget == "debug" ? "Нажмите клавишу" : "Изменить";

            _components.Add(new UITextButton(
                new Rectangle(xPositionOnScreen + width - 180, y, 150, 36),
                menuLabel, () => StartRebind("menu")));
            y += 50;

            _components.Add(new UITextButton(
                new Rectangle(xPositionOnScreen + width - 180, y, 150, 36),
                debugLabel, () => StartRebind("debug")));
            y += 90;

            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + 30, y + 5, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 2f),
                () => ChangeTimeFilter(-1)));

            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + width - 60, y + 5, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 2f),
                () => ChangeTimeFilter(1)));
        }

        private void BuildInfoTab() { /* только отрисовка */ }

        // ── Отрисовка ────────────────────────────────────────────────────────────

        public override void draw(SpriteBatch b)
        {
            try
            {
                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);

                DrawCenteredText(b, $"NPC Tracker — {_tabs[_activeTabIndex].Name}",
                    Game1.dialogueFont, yPositionOnScreen + 20, Game1.textColor);

                DrawCenteredText(b, _tabs[_activeTabIndex].Name,
                    Game1.smallFont, yPositionOnScreen - TAB_HEIGHT + 10, Color.Gold);

                _prevTabBtn.draw(b);
                _nextTabBtn.draw(b);

                switch (_activeTabIndex)
                {
                    case 0: DrawMainTab(b);     break;
                    case 1: DrawSettingsTab(b); break;
                    case 2: DrawInfoTab(b);     break;
                }

                _closeBtn.draw(b);
                drawMouse(b);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка отрисовки меню: {ex.Message}", LogLevel.Error);
                base.draw(b);
                drawMouse(b);
            }
        }

        private void DrawMainTab(SpriteBatch b)
        {
            foreach (var cb in _checkboxes)  cb.Draw(b);
            foreach (var c  in _components)  c.Draw(b);

            if (_state.SwitchTargetNPC && _registry.CurrentNpcList.Any())
            {
                string npc = _registry.GetSelectedNpcName(_state.NpcSelected);
                if (!string.IsNullOrEmpty(npc))
                    Utility.drawTextWithShadow(b, $"Выбран: {npc}", Game1.dialogueFont,
                        new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 230), Color.Black);
            }
        }

        private void DrawSettingsTab(SpriteBatch b)
        {
            int y = yPositionOnScreen + 60;

            DrawLabel(b, "Горячие клавиши", Game1.dialogueFont, y); y += 45;

            string menuText = _rebindTarget == "menu"
                ? "Открыть меню: [Нажмите клавишу...]"
                : $"Открыть меню: {_config.MenuKey}";
            DrawLabel(b, menuText, Game1.smallFont, y + 12); y += 50;

            string debugText = _rebindTarget == "debug"
                ? "Отладка: [Нажмите клавишу...]"
                : $"Отладка: {_config.DebugKey}";
            DrawLabel(b, debugText, Game1.smallFont, y + 12); y += 90;

            DrawLabel(b, "Фильтр по времени", Game1.dialogueFont, y); y += 45;

            string timeText = _state.TimeFilter < 0
                ? "Всё время"
                : RouteRenderer.FormatTime(_state.TimeFilter);
            DrawCenteredText(b, timeText, Game1.dialogueFont, y + 5, Color.Gold);

            foreach (var c in _components) c.Draw(b);
        }

        private void DrawInfoTab(SpriteBatch b)
        {
            string text =
                $"NPC Tracker\n" +
                $"Отслеживается NPC: {_registry.TotalNpcList?.Count ?? 0}\n" +
                $"Текущая локация: {Game1.currentLocation?.Name ?? "Неизвестно"}";

            Utility.drawTextWithShadow(b, text, Game1.dialogueFont,
                new Vector2(xPositionOnScreen + 50, yPositionOnScreen + 80), Game1.textColor);
        }

        // ── Обработка ввода ───────────────────────────────────────────────────────

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            try
            {
                base.receiveLeftClick(x, y, playSound);

                if (_closeBtn.containsPoint(x, y))
                { exitThisMenu(); if (playSound) Game1.playSound("bigDeSelect"); return; }

                if (_prevTabBtn.containsPoint(x, y))
                { SwitchTab(-1); if (playSound) Game1.playSound("shwip"); return; }

                if (_nextTabBtn.containsPoint(x, y))
                { SwitchTab(1); if (playSound) Game1.playSound("shwip"); return; }

                foreach (var cb in _checkboxes)
                    if (cb.ContainsPoint(x, y)) { cb.Toggle(); if (playSound) Game1.playSound("drumkit6"); return; }

                foreach (var c in _components)
                    if (c.ContainsPoint(x, y)) { c.OnClick?.Invoke(); if (playSound) Game1.playSound("smallSelect"); return; }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка клика в меню: {ex.Message}", LogLevel.Error);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (_rebindTarget != null)
            {
                if (key != Keys.Escape)
                {
                    var btn = (StardewModdingAPI.SButton)(int)key;
                    if (_rebindTarget == "menu")  _config.MenuKey  = btn;
                    else                          _config.DebugKey = btn;
                    _saveConfig();
                }
                _rebindTarget = null;
                RebuildComponents();
                return;
            }
            base.receiveKeyPress(key);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            InitPosition();
            RebuildComponents();
        }

        // ── Вспомогательные ──────────────────────────────────────────────────────

        private void Add(int y, string label, Func<bool> getter, Action<bool> setter)
        {
            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, y, 300, 40),
                label, getter(), setter));
        }

        private void SwitchTab(int delta)
        {
            int next = MathHelper.Clamp(_activeTabIndex + delta, 0, _tabs.Count - 1);
            if (next != _activeTabIndex) { _activeTabIndex = next; RebuildComponents(); }
        }

        private void StartRebind(string target) { _rebindTarget = target; RebuildComponents(); }

        private void ChangeTimeFilter(int delta)
        {
            _timeFilterIndex = MathHelper.Clamp(_timeFilterIndex + delta, 0, TimeSteps.Length - 1);
            _state.TimeFilter = TimeSteps[_timeFilterIndex];
            _tiles.Clear();
            _state.SwitchGetNpcPath = true;
        }

        private void MoveNpcSelection(int delta)
        {
            if (!_state.SwitchTargetNPC || !_registry.CurrentNpcList.Any()) return;
            _state.NpcSelected = MathHelper.Clamp(
                _state.NpcSelected + delta, 0, _registry.CurrentNpcList.Count - 1);
            _registry.CurrentNpcName = _registry.GetSelectedNpcName(_state.NpcSelected);
            _tiles.Clear();
            _state.SwitchGetNpcPath = true;
        }

        private void ClampNpcSelection()
        {
            if (!_registry.CurrentNpcList.Any() ||
                _state.NpcSelected >= _registry.CurrentNpcList.Count)
                _state.NpcSelected = 0;
        }

        private void DrawLabel(SpriteBatch b, string text, SpriteFont font, int y)
            => Utility.drawTextWithShadow(b, text, font,
                new Vector2(xPositionOnScreen + 30, y), Game1.textColor);

        private void DrawCenteredText(SpriteBatch b, string text, SpriteFont font, int y, Color color)
        {
            var sz = font.MeasureString(text);
            Utility.drawTextWithShadow(b, text, font,
                new Vector2(xPositionOnScreen + (width - sz.X) / 2f, y), color);
        }
    }
}

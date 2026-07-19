using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    /// <summary>
    /// Главное меню мода для отслеживания NPC.
    /// Вкладки: Main (основные переключатели), Settings (клавиши + фильтр времени), Info.
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        // ── Константы ────────────────────────────────────────────────────────────
        private const int MENU_WIDTH     = 600;
        private const int MENU_HEIGHT    = 600;
        private const int TAB_HEIGHT     = 40;
        private const int CHECKBOX_SPACING = 50;
        private const int BUTTON_SIZE    = 30;

        // ── Компоненты ───────────────────────────────────────────────────────────
        private readonly List<UIComponent>       _components = new List<UIComponent>();
        private readonly List<ClickableCheckbox> _checkboxes = new List<ClickableCheckbox>();
        private readonly List<TabInfo>           _tabs       = new List<TabInfo>();
        private int _activeTabIndex = 0;

        // ── Ссылки ───────────────────────────────────────────────────────────────
        private readonly _modInstance _modInstance;
        private ClickableTextureComponent _closeButton;
        private ClickableTextureComponent _prevTabButton;
        private ClickableTextureComponent _nextTabButton;

        // ── Состояние вкладки Settings ───────────────────────────────────────────
        /// <summary> "menu" или "debug" — ожидаем клавишу для переназначения. null — не ждём. </summary>
        private string _rebindTarget = null;

        private static readonly int[] _timeSteps =
        {
            -1,
            600, 700, 800, 900, 1000, 1100, 1200,
            1300, 1400, 1500, 1600, 1700, 1800,
            1900, 2000, 2100, 2200, 2300, 2400, 2500, 2600
        };
        private int _timeFilterIndex = 0;

        // ── Конструктор ──────────────────────────────────────────────────────────
        public TrackingMenu(_modInstance modInstance)
            : base(0, 0, MENU_WIDTH, MENU_HEIGHT)
        {
            _modInstance = modInstance ?? throw new ArgumentNullException(nameof(modInstance));

            // Синхронизируем индекс слайдера времени с текущим состоянием мода
            int idx = Array.IndexOf(_timeSteps, _modInstance.TimeFilter);
            _timeFilterIndex = idx >= 0 ? idx : 0;

            InitializePosition();
            InitializeTabs();
            InitializeComponents();
        }

        // ── Инициализация ────────────────────────────────────────────────────────

        private void InitializePosition()
        {
            Rectangle viewport = new Rectangle(
                Game1.viewport.X, Game1.viewport.Y,
                Game1.viewport.Width, Game1.viewport.Height);

            xPositionOnScreen = viewport.Width  / 2 - MENU_WIDTH  / 2;
            yPositionOnScreen = viewport.Height / 2 - MENU_HEIGHT / 2;
        }

        private void InitializeTabs()
        {
            _tabs.Add(new TabInfo("Main",     InitializeMainTab));
            _tabs.Add(new TabInfo("Settings", InitializeSettingsTab));
            _tabs.Add(new TabInfo("Info",     InitializeInfoTab));
        }

        private void InitializeComponents()
        {
            _components.Clear();
            _checkboxes.Clear();

            _closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 48, yPositionOnScreen - 8, 48, 48),
                Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            _prevTabButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 2f);

            _nextTabButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 50, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 2f);

            InitializeActiveTab();
        }

        private void InitializeActiveTab()
        {
            if (_activeTabIndex >= 0 && _activeTabIndex < _tabs.Count)
                _tabs[_activeTabIndex].InitializeAction?.Invoke();
        }

        /// <summary>
        /// Главная вкладка: включение, сетка, выбор NPC, локации, маршруты.
        /// </summary>
        private void InitializeMainTab()
        {
            int yOffset = yPositionOnScreen + 80;

            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Включение", _modInstance.EnableDisplay,
                v => _modInstance.EnableDisplay = v));
            yOffset += CHECKBOX_SPACING;

            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение сетки", _modInstance.DisplayGrid,
                v => _modInstance.DisplayGrid = v));
            yOffset += CHECKBOX_SPACING;

            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Выбор NPC", _modInstance.SwitchTargetNPC,
                v =>
                {
                    _modInstance.SwitchTargetNPC = v;
                    if (!v) _modInstance.NpcList.CurrentNpcName = null;
                    else    _modInstance.NpcSelected = 0;
                    _modInstance.DrawTiles.ClearTiles();
                    _modInstance.NpcList.CurrentNpcList.Clear();
                    _modInstance.SwitchGetNpcPath = true;
                    _modInstance.SwitchListFull   = false;
                }));
            yOffset += CHECKBOX_SPACING * 2;

            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение всех локаций", _modInstance.SwitchTargetLocations,
                v =>
                {
                    _modInstance.SwitchTargetLocations = v;
                    _modInstance.DrawTiles.ClearTiles();
                    _modInstance.SwitchGetNpcPath = true;
                    _modInstance.NpcList.CurrentNpcList.Clear();
                    _modInstance.SwitchListFull = false;

                    if (v && _modInstance.NpcList.CurrentNpcName != null)
                        _modInstance.NpcSelected = _modInstance.NpcList.CurrentNpcList.IndexOf(_modInstance.NpcList.CurrentNpcName);
                    else if (!v)
                        EnsureValidNpcSelection();
                }));
            yOffset += CHECKBOX_SPACING;

            _checkboxes.Add(new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение всех маршрутов", _modInstance.SwitchGlobalNpcPath,
                v =>
                {
                    _modInstance.SwitchGlobalNpcPath = v;
                    _modInstance.DrawTiles.ClearTiles();
                    _modInstance.SwitchGetNpcPath = true;
                }));

            // Кнопки выбора NPC
            var leftArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            var rightArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 350, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);

            _components.Add(new UIButton(leftArrow,  () => HandleArrowClick(-1)));
            _components.Add(new UIButton(rightArrow, () => HandleArrowClick(1)));
        }

        /// <summary>
        /// Вкладка настроек: переназначение клавиш + фильтр по времени.
        /// </summary>
        private void InitializeSettingsTab()
        {
            int yOffset = yPositionOnScreen + 110;

            // ── Горячие клавиши ──────────────────────────────────────────────────
            // Кнопка переназначения клавиши меню
            string menuBtnLabel  = _rebindTarget == "menu"  ? "Нажмите клавишу" : "Изменить";
            string debugBtnLabel = _rebindTarget == "debug" ? "Нажмите клавишу" : "Изменить";

            _components.Add(new UITextButton(
                new Rectangle(xPositionOnScreen + width - 180, yOffset, 150, 36),
                menuBtnLabel,
                () => StartRebind("menu")));
            yOffset += 50;

            _components.Add(new UITextButton(
                new Rectangle(xPositionOnScreen + width - 180, yOffset, 150, 36),
                debugBtnLabel,
                () => StartRebind("debug")));
            yOffset += 90;

            // ── Фильтр по времени ────────────────────────────────────────────────
            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + 30, yOffset + 5, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 2f),
                () => ChangeTimeFilter(-1)));

            _components.Add(new UIButton(
                new ClickableTextureComponent(
                    new Rectangle(xPositionOnScreen + width - 60, yOffset + 5, BUTTON_SIZE, BUTTON_SIZE),
                    Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 2f),
                () => ChangeTimeFilter(1)));
        }

        private void InitializeInfoTab()
        {
            // Статистика — только отрисовка, без кликабельных элементов
        }

        // ── Вспомогательные методы ───────────────────────────────────────────────

        private void HandleArrowClick(int direction)
        {
            if (!_modInstance.SwitchTargetNPC || !_modInstance.NpcList.CurrentNpcList.Any())
                return;

            var npcList = _modInstance.NpcList.CurrentNpcList;
            _modInstance.NpcSelected = MathHelper.Clamp(
                _modInstance.NpcSelected + direction, 0, npcList.Count - 1);
            _modInstance.NpcList.CurrentNpcName = _modInstance.NpcList.GetNpcFromList();
            _modInstance.DrawTiles.ClearTiles();
            _modInstance.SwitchGetNpcPath = true;
        }

        private void EnsureValidNpcSelection()
        {
            if (!_modInstance.NpcList.CurrentNpcList.Any() ||
                _modInstance.NpcSelected >= _modInstance.NpcList.CurrentNpcList.Count)
                _modInstance.NpcSelected = 0;
        }

        /// <summary>
        /// Начинает режим ожидания нажатия клавиши для переназначения.
        /// </summary>
        private void StartRebind(string target)
        {
            _rebindTarget = target;
            InitializeComponents(); // перерисовываем кнопки с надписью "Нажмите клавишу"
        }

        /// <summary>
        /// Сдвигает фильтр времени на шаг (direction: -1 / +1).
        /// </summary>
        private void ChangeTimeFilter(int direction)
        {
            _timeFilterIndex = MathHelper.Clamp(
                _timeFilterIndex + direction, 0, _timeSteps.Length - 1);
            _modInstance.TimeFilter = _timeSteps[_timeFilterIndex];
            _modInstance.DrawTiles.ClearTiles();
            _modInstance.SwitchGetNpcPath = true;
        }

        private static string FormatGameTime(int gameTime)
        {
            return $"{gameTime / 100:D2}:{gameTime % 100:D2}";
        }

        // ── Отрисовка ────────────────────────────────────────────────────────────

        public override void draw(SpriteBatch b)
        {
            try
            {
                // Фон
                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);

                // Заголовок
                string title = $"NPC Tracker - {_tabs[_activeTabIndex].Name}";
                Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
                Utility.drawTextWithShadow(b, title, Game1.dialogueFont,
                    new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + 20),
                    Game1.textColor);

                // Имя вкладки
                string tabName = _tabs[_activeTabIndex].Name;
                Vector2 tabSize = Game1.smallFont.MeasureString(tabName);
                Utility.drawTextWithShadow(b, tabName, Game1.smallFont,
                    new Vector2(xPositionOnScreen + (width - tabSize.X) / 2, yPositionOnScreen - TAB_HEIGHT + 10),
                    Color.Gold);

                _prevTabButton.draw(b);
                _nextTabButton.draw(b);

                DrawActiveTabContent(b);

                _closeButton.draw(b);
                drawMouse(b);
            }
            catch (Exception ex)
            {
                _modInstance.Monitor.Log($"Ошибка отрисовки меню: {ex.Message}", LogLevel.Error);
                base.draw(b);
                drawMouse(b);
            }
        }

        private void DrawActiveTabContent(SpriteBatch b)
        {
            switch (_activeTabIndex)
            {
                case 0: DrawMainTab(b);     break;
                case 1: DrawSettingsTab(b); break;
                case 2: DrawInfoTab(b);     break;
            }
        }

        private void DrawMainTab(SpriteBatch b)
        {
            foreach (var checkbox in _checkboxes)
                checkbox.Draw(b);
            foreach (var component in _components)
                component.Draw(b);

            if (_modInstance.SwitchTargetNPC && _modInstance.NpcList.CurrentNpcList.Any())
            {
                string npcName = _modInstance.NpcList.GetNpcFromList();
                if (!string.IsNullOrEmpty(npcName))
                    Utility.drawTextWithShadow(b, $"Выбран: {npcName}", Game1.dialogueFont,
                        new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 230), Color.Black);
            }
        }

        /// <summary>
        /// Отрисовка вкладки настроек: горячие клавиши + фильтр по времени.
        /// </summary>
        private void DrawSettingsTab(SpriteBatch b)
        {
            int yOffset = yPositionOnScreen + 60;

            // ── Горячие клавиши ──────────────────────────────────────────────────
            Utility.drawTextWithShadow(b, "Горячие клавиши",
                Game1.dialogueFont, new Vector2(xPositionOnScreen + 30, yOffset), Game1.textColor);
            yOffset += 45;

            // Клавиша меню
            string menuKeyLabel = _rebindTarget == "menu"
                ? "Открыть меню: [Нажмите клавишу...]"
                : $"Открыть меню: {_modInstance.Config.MenuKey}";
            Utility.drawTextWithShadow(b, menuKeyLabel, Game1.smallFont,
                new Vector2(xPositionOnScreen + 30, yOffset + 12), Game1.textColor);
            yOffset += 50;

            // Клавиша отладки
            string debugKeyLabel = _rebindTarget == "debug"
                ? "Отладка: [Нажмите клавишу...]"
                : $"Отладка: {_modInstance.Config.DebugKey}";
            Utility.drawTextWithShadow(b, debugKeyLabel, Game1.smallFont,
                new Vector2(xPositionOnScreen + 30, yOffset + 12), Game1.textColor);
            yOffset += 90;

            // ── Фильтр по времени ────────────────────────────────────────────────
            Utility.drawTextWithShadow(b, "Фильтр по времени",
                Game1.dialogueFont, new Vector2(xPositionOnScreen + 30, yOffset), Game1.textColor);
            yOffset += 45;

            string timeText = _modInstance.TimeFilter < 0
                ? "Всё время"
                : FormatGameTime(_modInstance.TimeFilter);

            Vector2 timeSize = Game1.dialogueFont.MeasureString(timeText);
            Utility.drawTextWithShadow(b, timeText, Game1.dialogueFont,
                new Vector2(xPositionOnScreen + (width - timeSize.X) / 2, yOffset + 5),
                Color.Gold);

            // Кнопки (стрелки и кнопки переназначения)
            foreach (var component in _components)
                component.Draw(b);
        }

        private void DrawInfoTab(SpriteBatch b)
        {
            string infoText =
                $"NPC Tracker Mod v1.0\n" +
                $"Отслеживаемых NPC: {_modInstance.NpcList.TotalNpcList?.Count ?? 0}\n" +
                $"Текущая локация: {Game1.currentLocation?.Name ?? "Неизвестно"}";

            Utility.drawTextWithShadow(b, infoText, Game1.dialogueFont,
                new Vector2(xPositionOnScreen + 50, yPositionOnScreen + 80), Game1.textColor);
        }

        // ── Обработка ввода ──────────────────────────────────────────────────────

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            try
            {
                base.receiveLeftClick(x, y, playSound);

                if (_closeButton.containsPoint(x, y))
                {
                    exitThisMenu();
                    if (playSound) Game1.playSound("bigDeSelect");
                    return;
                }

                if (_prevTabButton.containsPoint(x, y))
                {
                    SwitchTab(-1);
                    if (playSound) Game1.playSound("shwip");
                    return;
                }

                if (_nextTabButton.containsPoint(x, y))
                {
                    SwitchTab(1);
                    if (playSound) Game1.playSound("shwip");
                    return;
                }

                foreach (var checkbox in _checkboxes)
                {
                    if (checkbox.ContainsPoint(x, y))
                    {
                        checkbox.Toggle();
                        if (playSound) Game1.playSound("drumkit6");
                        return;
                    }
                }

                foreach (var component in _components)
                {
                    if (component.ContainsPoint(x, y))
                    {
                        component.OnClick?.Invoke();
                        if (playSound) Game1.playSound("smallSelect");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _modInstance.Monitor.Log($"Ошибка обработки клика: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Перехватывает нажатие клавиши при активном режиме переназначения.
        /// </summary>
        public override void receiveKeyPress(Keys key)
        {
            if (_rebindTarget != null)
            {
                if (key != Keys.Escape)
                {
                    var pressed = (StardewModdingAPI.SButton)(int)key;
                    if (_rebindTarget == "menu")
                        _modInstance.Config.MenuKey = pressed;
                    else if (_rebindTarget == "debug")
                        _modInstance.Config.DebugKey = pressed;
                    _modInstance.SaveConfig();
                }
                _rebindTarget = null;
                InitializeComponents();
                return;
            }
            base.receiveKeyPress(key);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
        }

        private void SwitchTab(int direction)
        {
            int newIndex = MathHelper.Clamp(_activeTabIndex + direction, 0, _tabs.Count - 1);
            if (newIndex != _activeTabIndex)
            {
                _activeTabIndex = newIndex;
                InitializeComponents();
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            InitializePosition();
            InitializeComponents();
        }

        // ── Внутренние классы ────────────────────────────────────────────────────

        private class TabInfo
        {
            public string Name { get; }
            public Action InitializeAction { get; }
            public TabInfo(string name, Action initializeAction)
            {
                Name = name;
                InitializeAction = initializeAction;
            }
        }

        private class UIComponent
        {
            public ClickableTextureComponent TextureComponent { get; }
            public Action OnClick { get; set; }

            public UIComponent(ClickableTextureComponent textureComponent, Action onClick = null)
            {
                TextureComponent = textureComponent;
                OnClick = onClick;
            }

            public virtual bool ContainsPoint(int x, int y) =>
                TextureComponent?.containsPoint(x, y) ?? false;

            public virtual void Draw(SpriteBatch batch)
            {
                TextureComponent?.draw(batch);
            }
        }

        private class UIButton : UIComponent
        {
            public UIButton(ClickableTextureComponent textureComponent, Action onClick)
                : base(textureComponent, onClick) { }
        }

        /// <summary>
        /// Кнопка с текстовой подписью. Используется для кнопок переназначения клавиш.
        /// </summary>
        private class UITextButton : UIComponent
        {
            private readonly Rectangle _bounds;
            private readonly string    _label;

            public UITextButton(Rectangle bounds, string label, Action onClick)
                : base(new ClickableTextureComponent(bounds, null, Rectangle.Empty, 1f), onClick)
            {
                _bounds = bounds;
                _label  = label;
            }

            public override bool ContainsPoint(int x, int y) => _bounds.Contains(x, y);

            public override void Draw(SpriteBatch batch)
            {
                IClickableMenu.drawTextureBox(batch, Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60),
                    _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height,
                    Color.White, 0.5f, false);

                Vector2 sz = Game1.smallFont.MeasureString(_label);
                Utility.drawTextWithShadow(batch, _label, Game1.smallFont,
                    new Vector2(
                        _bounds.X + (_bounds.Width  - sz.X) / 2,
                        _bounds.Y + (_bounds.Height - sz.Y) / 2),
                    Game1.textColor);
            }
        }

        private class ClickableCheckbox
        {
            public Rectangle Bounds   { get; }
            public string    Label    { get; }
            public bool      IsChecked { get; private set; }

            private readonly Action<bool> _onToggle;

            public ClickableCheckbox(Rectangle bounds, string label, bool initialState, Action<bool> onToggle)
            {
                Bounds    = bounds;
                Label     = label;
                IsChecked = initialState;
                _onToggle = onToggle;
            }

            public bool ContainsPoint(int x, int y) => Bounds.Contains(x, y);

            public void Toggle()
            {
                IsChecked = !IsChecked;
                _onToggle?.Invoke(IsChecked);
            }

            public void Draw(SpriteBatch batch)
            {
                try
                {
                    var texture    = Game1.mouseCursors_1_6;
                    var sourceRect = IsChecked
                        ? new Rectangle(291, 253, 9, 9)
                        : new Rectangle(273, 253, 9, 9);

                    batch.Draw(texture, new Vector2(Bounds.X, Bounds.Y), sourceRect,
                        Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.4f);

                    Vector2 textPos = new Vector2(
                        Bounds.X + 70,
                        Bounds.Y + Bounds.Height / 2 - Game1.dialogueFont.MeasureString(Label).Y / 2);

                    Utility.drawTextWithShadow(batch, Label, Game1.dialogueFont, textPos, Game1.textColor);
                }
                catch { /* Игнорируем ошибки отрисовки */ }
            }
        }
    }
}

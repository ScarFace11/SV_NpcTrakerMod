using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    /// <summary>
    /// Главное меню мода для отслеживания NPC
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        // Константы
        private const int MENU_WIDTH = 600;
        private const int MENU_HEIGHT = 600;
        private const int TAB_HEIGHT = 40;
        private const int CHECKBOX_SPACING = 50;
        private const int BUTTON_SIZE = 30;

        // Компоненты
        private readonly List<UIComponent> _components = new List<UIComponent>();
        private readonly List<ClickableCheckbox> _checkboxes = new List<ClickableCheckbox>();
        private readonly List<TabInfo> _tabs = new List<TabInfo>();
        private int _activeTabIndex = 0;

        // Ссылки
        private readonly _modInstance _modInstance;
        private ClickableTextureComponent _closeButton;
        private ClickableTextureComponent _prevTabButton;
        private ClickableTextureComponent _nextTabButton;

        /// <summary>
        /// Новый экземпляр класса <see cref="TrackingMenu"/>.
        /// </summary>
        /// <param name="modInstance">Экземпляр основного мода.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если modInstance равен null.</exception>
        public TrackingMenu(_modInstance modInstance)
            : base(0, 0, MENU_WIDTH, MENU_HEIGHT)
        {
            _modInstance = modInstance ?? throw new ArgumentNullException(nameof(modInstance));
            InitializePosition();
            InitializeTabs();
            InitializeComponents();
        }

        /// <summary>
        /// Инициализирует позицию меню по центру экрана.
        /// </summary>
        private void InitializePosition()
        {
            // Преобразуем xTile.Rectangle в Microsoft.Xna.Framework.Rectangle
            Rectangle viewport = new Rectangle(
                Game1.viewport.X,
                Game1.viewport.Y,
                Game1.viewport.Width,
                Game1.viewport.Height
            );

            // Обновите позиции всех элементов
            xPositionOnScreen = viewport.Width / 2 - MENU_WIDTH / 2;
            yPositionOnScreen = viewport.Height / 2 - MENU_HEIGHT / 2;
        }

        /// <summary>
        /// Инициализирует доступные вкладки меню.
        /// </summary>
        private void InitializeTabs()
        {
            _tabs.Add(new TabInfo("Main", InitializeMainTab));
            _tabs.Add(new TabInfo("Settings", InitializeSettingsTab));
            _tabs.Add(new TabInfo("Info", InitializeInfoTab));
        }

        /// <summary>
        /// Инициализирует все компоненты меню.
        /// </summary>
        private void InitializeComponents()
        {
            // Очищаем предыдущие компоненты
            _components.Clear();
            _checkboxes.Clear();

            // Кнопка закрытия
            _closeButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 48, yPositionOnScreen - 8, 48, 48),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );

            // Кнопки переключения вкладок
            _prevTabButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(352, 495, 12, 11),
                2f
            );

            _nextTabButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 50, yPositionOnScreen - TAB_HEIGHT, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(365, 495, 12, 11),
                2f
            );

            // Инициализируем активную вкладку
            InitializeActiveTab();
        }

        /// <summary>
        /// Инициализирует компоненты активной вкладки.
        /// </summary>
        private void InitializeActiveTab()
        {
            if (_activeTabIndex >= 0 && _activeTabIndex < _tabs.Count)
            {
                _tabs[_activeTabIndex].InitializeAction?.Invoke();
            }
        }

        /// <summary>
        /// Инициализирует компоненты главной вкладки меню.
        /// Содержит основные настройки отслеживания NPC.
        /// </summary>
        private void InitializeMainTab()
        {
            int yOffset = yPositionOnScreen + 80;

            // Чекбокс "Включение"
            var enableCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Включение",
                _modInstance.EnableDisplay,
                (checkedState) => _modInstance.EnableDisplay = checkedState
            );
            _checkboxes.Add(enableCheckbox);
            yOffset += CHECKBOX_SPACING;

            // Чекбокс "Отображение сетки"
            var gridCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение сетки",
                _modInstance.DisplayGrid,
                (checkedState) => _modInstance.DisplayGrid = checkedState
            );
            _checkboxes.Add(gridCheckbox);
            yOffset += CHECKBOX_SPACING;

            // Чекбокс "Выбор NPC"
            var npcCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Выбор NPC",
                _modInstance.SwitchTargetNPC,
                (checkedState) =>
                {
                    _modInstance.SwitchTargetNPC = checkedState;
                    if (!checkedState)
                    {
                        _modInstance.NpcList.CurrentNpcName = null;
                    }
                    else
                    {
                        _modInstance.NpcSelected = 0;
                    }
                    _modInstance.DrawTiles.tileStates.Clear();
                    _modInstance.NpcList.CurrentNpcList.Clear();
                    _modInstance.SwitchGetNpcPath = true;
                    _modInstance.SwitchListFull = false;
                }
            );
            _checkboxes.Add(npcCheckbox);
            yOffset += CHECKBOX_SPACING * 2;

            // Чекбокс "Отображение всех локаций"
            var locationsCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение всех локаций",
                _modInstance.SwitchTargetLocations,
                (checkedState) =>
                {
                    _modInstance.SwitchTargetLocations = checkedState;
                    _modInstance.DrawTiles.tileStates.Clear();
                    _modInstance.SwitchGetNpcPath = true;
                    _modInstance.NpcList.CurrentNpcList.Clear();
                    _modInstance.SwitchListFull = false;

                    if (checkedState && _modInstance.NpcList.CurrentNpcName != null)
                    {
                        _modInstance.NpcSelected = _modInstance.NpcList.CurrentNpcList.IndexOf(_modInstance.NpcList.CurrentNpcName);
                    }
                    else if (!checkedState)
                    {
                        EnsureValidNpcSelection();
                    }
                }
            );
            _checkboxes.Add(locationsCheckbox);
            yOffset += CHECKBOX_SPACING;

            // Чекбокс "Отображение всех маршрутов"
            var routesCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yOffset, 300, 40),
                "Отображение всех маршрутов",
                _modInstance.SwitchGlobalNpcPath,
                (checkedState) =>
                {
                    _modInstance.SwitchGlobalNpcPath = checkedState;
                    _modInstance.DrawTiles.tileStates.Clear();
                    _modInstance.SwitchGetNpcPath = true;
                }
            );
            _checkboxes.Add(routesCheckbox);

            // Кнопки выбора NPC
            var leftArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(352, 495, 12, 11),
                4f
            );
            var rightArrow = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 350, yPositionOnScreen + 230, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(365, 495, 12, 11),
                4f
            );

            _components.Add(new UIButton(leftArrow, () => HandleArrowClick(-1)));
            _components.Add(new UIButton(rightArrow, () => HandleArrowClick(1)));
        }

        /// <summary>
        /// Инициализирует компоненты вкладки настроек.
        /// </summary>
        /// <remarks>
        /// В будущем здесь можно добавить дополнительные настройки:
        /// - Настройки горячих клавиш
        /// - Настройки цветов отображения
        /// - Настройки прозрачности
        /// - Экспорт/импорт настроек
        /// </remarks>
        private void InitializeSettingsTab()
        {
            // Здесь можно добавить дополнительные настройки
            // Например: прозрачность, цвета, горячие клавиши
        }

        /// <summary>
        /// Инициализирует компоненты информационной вкладки.
        /// </summary>
        private void InitializeInfoTab()
        {
            // Информация о моде, статистика и т.д.
        }

        /// <summary>
        /// Обрабатывает клик по кнопкам-стрелкам для выбора NPC.
        /// </summary>
        /// <param name="direction">Направление изменения выбора: -1 для предыдущего NPC, 1 для следующего.</param>
        private void HandleArrowClick(int direction)
        {
            if (!_modInstance.SwitchTargetNPC || !_modInstance.NpcList.CurrentNpcList.Any())
                return;

            var npcList = _modInstance.NpcList.CurrentNpcList;
            _modInstance.NpcSelected = MathHelper.Clamp(_modInstance.NpcSelected + direction, 0, npcList.Count - 1);
            _modInstance.NpcList.CurrentNpcName = _modInstance.NpcList.GetNpcFromList();
            _modInstance.DrawTiles.tileStates.Clear();
            _modInstance.SwitchGetNpcPath = true;
        }

        /// <summary>
        /// Проверяет и корректирует выбор NPC, если текущий выбор некорректен.
        /// </summary>
        /// <remarks>
        /// Вызывается при отключении режима отображения всех локаций,
        /// чтобы убедиться, что выбранный NPC существует в обновленном списке.
        /// </remarks>
        private void EnsureValidNpcSelection()
        {
            if (!_modInstance.NpcList.CurrentNpcList.Any() ||
                _modInstance.NpcSelected >= _modInstance.NpcList.CurrentNpcList.Count)
            {
                _modInstance.NpcSelected = 0;
            }
        }

        /// <summary>
        /// Отрисовывает меню и все его компоненты.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки.</param>
        /// <remarks>
        /// Метод переопределяет базовый метод отрисовки и добавляет обработку ошибок.
        /// </remarks>
        public override void draw(SpriteBatch b)
        {
            try
            {
                // Фон меню
                drawTextureBox(b,
                    Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60),
                    xPositionOnScreen,
                    yPositionOnScreen,
                    width,
                    height,
                    Color.White,
                    1f,
                    true);

                // Заголовок
                string title = $"NPC Tracker - {_tabs[_activeTabIndex].Name}";
                Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
                Vector2 titlePosition = new Vector2(
                    xPositionOnScreen + (width - titleSize.X) / 2,
                    yPositionOnScreen + 20);

                Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePosition, Game1.textColor);

                // Вкладки
                string tabName = _tabs[_activeTabIndex].Name;
                Vector2 tabSize = Game1.smallFont.MeasureString(tabName);
                Vector2 tabPosition = new Vector2(
                    xPositionOnScreen + (width - tabSize.X) / 2,
                    yPositionOnScreen - TAB_HEIGHT + 10);

                Utility.drawTextWithShadow(b, tabName, Game1.smallFont, tabPosition, Color.Gold);

                // Кнопки вкладок
                _prevTabButton.draw(b);
                _nextTabButton.draw(b);

                // Содержимое активной вкладки
                DrawActiveTabContent(b);

                // Кнопка закрытия
                _closeButton.draw(b);

                // Курсор
                drawMouse(b);
            }
            catch (Exception ex)
            {
                _modInstance.Monitor.Log($"Ошибка отрисовки меню: {ex.Message}", LogLevel.Error);
                base.draw(b);
                drawMouse(b);
            }
        }

        /// <summary>
        /// Отрисовывает содержимое активной вкладки.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки.</param>
        /// <remarks>
        /// В зависимости от активной вкладки вызывает соответствующий метод отрисовки.
        /// </remarks>
        private void DrawActiveTabContent(SpriteBatch b)
        {
            switch (_activeTabIndex)
            {
                case 0: // Главная
                    DrawMainTab(b);
                    break;
                case 1: // Настройки
                    DrawSettingsTab(b);
                    break;
                case 2: // Информация
                    DrawInfoTab(b);
                    break;
            }
        }

        /// <summary>
        /// Отрисовывает главную вкладку меню.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки.</param>
        /// <remarks>
        /// Отображает все чекбоксы настроек, кнопки выбора NPC и информацию о выбранном NPC.
        /// </remarks>
        private void DrawMainTab(SpriteBatch b)
        {
            // Чекбоксы
            foreach (var checkbox in _checkboxes)
            {
                checkbox.Draw(b);
            }

            // Кнопки
            foreach (var component in _components)
            {
                component.Draw(b);
            }

            // Выбранный NPC
            if (_modInstance.SwitchTargetNPC && _modInstance.NpcList.CurrentNpcList.Any())
            {
                string npcName = _modInstance.NpcList.GetNpcFromList();
                if (!string.IsNullOrEmpty(npcName))
                {
                    Vector2 position = new Vector2(
                        xPositionOnScreen + 100,
                        yPositionOnScreen + 230);

                    Utility.drawTextWithShadow(b,
                        $"Выбран: {npcName}",
                        Game1.dialogueFont,
                        position,
                        Color.Black);
                }
            }
        }

        /// <summary>
        /// Отрисовывает вкладку настроек.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки.</param>
        /// <remarks>
        /// В текущей реализации отображает заглушку.
        /// В будущем здесь будут настройки мода.
        /// </remarks>
        private void DrawSettingsTab(SpriteBatch b)
        {
            string text = "Настройки будут здесь";
            Vector2 textSize = Game1.dialogueFont.MeasureString(text);
            Vector2 textPosition = new Vector2(
                xPositionOnScreen + (width - textSize.X) / 2,
                yPositionOnScreen + 100);

            Utility.drawTextWithShadow(b, text, Game1.dialogueFont, textPosition, Color.Gray);
        }

        /// <summary>
        /// Отрисовывает информационную вкладку.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки.</param>
        /// <remarks>
        /// Отображает информацию о моде, статистику отслеживания и текущее состояние.
        /// </remarks>
        private void DrawInfoTab(SpriteBatch b)
        {
            string infoText = $"NPC Tracker Mod v1.0\n" +
                            $"Отслеживаемых NPC: {_modInstance.NpcList.TotalNpcList?.Count ?? 0}\n" +
                            $"Текущая локация: {Game1.currentLocation?.Name ?? "Неизвестно"}";

            Vector2 position = new Vector2(xPositionOnScreen + 50, yPositionOnScreen + 80);
            Utility.drawTextWithShadow(b, infoText, Game1.dialogueFont, position, Game1.textColor);
        }

        /// <summary>
        /// Обрабатывает левый клик мыши по элементам меню.
        /// </summary>
        /// <param name="x">X-координата клика в пикселях.</param>
        /// <param name="y">Y-координата клика в пикселях.</param>
        /// <param name="playSound">Если true, проигрывает звук при взаимодействии.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            try
            {
                base.receiveLeftClick(x, y, playSound);

                // Кнопка закрытия
                if (_closeButton.containsPoint(x, y))
                {
                    exitThisMenu();
                    if (playSound) Game1.playSound("bigDeSelect");
                    return;
                }

                // Переключение вкладок
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

                // Чекбоксы
                foreach (var checkbox in _checkboxes)
                {
                    if (checkbox.ContainsPoint(x, y))
                    {
                        checkbox.Toggle();
                        if (playSound) Game1.playSound("drumkit6");
                        return;
                    }
                }

                // Компоненты UI
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
        /// Выполняет действие при наведении курсора мыши.
        /// </summary>
        /// <param name="x">X-координата курсора в пикселях.</param>
        /// <param name="y">Y-координата курсора в пикселях.</param>
        /// <remarks>
        /// Обновляет состояние наведения для всех интерактивных элементов.
        /// </remarks>
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
        }

        /// <summary>
        /// Переключает активную вкладку меню.
        /// </summary>
        /// <param name="direction">Направление переключения: -1 для предыдущей вкладки, 1 для следующей.</param>
        /// <remarks>
        /// Ограничивает индекс вкладки допустимыми границами и переинициализирует компоненты.
        /// </remarks>
        private void SwitchTab(int direction)
        {
            int newIndex = MathHelper.Clamp(_activeTabIndex + direction, 0, _tabs.Count - 1);
            if (newIndex != _activeTabIndex)
            {
                _activeTabIndex = newIndex;
                InitializeComponents();
            }
        }

        /// <summary>
        /// Обрабатывает изменение размера игрового окна.
        /// </summary>
        /// <param name="oldBounds">Предыдущие границы окна.</param>
        /// <param name="newBounds">Новые границы окна.</param>
        /// <remarks>
        /// Пересчитывает позицию меню и переинициализирует компоненты для нового размера.
        /// </remarks>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            InitializePosition();
            InitializeComponents();
        }

        /// <summary>
        /// Представляет информацию о вкладке меню.
        /// </summary>
        /// <remarks>
        /// Хранит название вкладки и метод для инициализации её компонентов.
        /// </remarks>
        private class TabInfo
        {
            /// <summary>
            /// Получает название вкладки.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Получает действие для инициализации компонентов вкладки.
            /// </summary>
            public Action InitializeAction { get; }

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="TabInfo"/>.
            /// </summary>
            /// <param name="name">Название вкладки.</param>
            /// <param name="initializeAction">Действие для инициализации компонентов вкладки.</param>
            public TabInfo(string name, Action initializeAction)
            {
                Name = name;
                InitializeAction = initializeAction;
            }
        }

        /// <summary>
        /// Базовый класс для UI компонентов.
        /// </summary>
        /// <remarks>
        /// Предоставляет базовую функциональность для всех интерактивных элементов UI.
        /// </remarks>
        private class UIComponent
        {
            /// <summary>
            /// Получает текстуру компонента.
            /// </summary>
            public ClickableTextureComponent TextureComponent { get; }

            /// <summary>
            /// Получает или задает действие, выполняемое при клике.
            /// </summary>
            public Action OnClick { get; set; }

            /// <summary>
            /// Получает или задает значение, указывающее, находится ли курсор над компонентом.
            /// </summary>
            public bool IsHovered { get; private set; }

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="UIComponent"/>.
            /// </summary>
            /// <param name="textureComponent">Текстура компонента.</param>
            /// <param name="onClick">Действие при клике (опционально).</param>
            public UIComponent(ClickableTextureComponent textureComponent, Action onClick = null)
            {
                TextureComponent = textureComponent;
                OnClick = onClick;
            }

            /// <summary>
            /// Проверяет, содержит ли компонент указанную точку.
            /// </summary>
            /// <param name="x">X-координата точки.</param>
            /// <param name="y">Y-координата точки.</param>
            /// <returns>true, если точка находится внутри границ компонента; иначе false.</returns>
            public bool ContainsPoint(int x, int y)
            {
                return TextureComponent.containsPoint(x, y);
            }

            /// <summary>
            /// Обновляет состояние наведения компонента.
            /// </summary>
            /// <param name="x">X-координата курсора.</param>
            /// <param name="y">Y-координата курсора.</param>
            public void UpdateHover(int x, int y)
            {
                IsHovered = ContainsPoint(x, y);
            }

            /// <summary>
            /// Отрисовывает компонент.
            /// </summary>
            /// <param name="batch">SpriteBatch для отрисовки.</param>
            /// <remarks>
            /// Изменяет цвет компонента при наведении курсора.
            /// </remarks>
            public virtual void Draw(SpriteBatch batch)
            {
                TextureComponent.draw(batch);
            }
        }

        /// <summary>
        /// Представляет кнопку в пользовательском интерфейсе.
        /// </summary>
        /// <remarks>
        /// Специализированный класс для кнопок, наследует базовую функциональность UIComponent.
        /// </remarks>
        private class UIButton : UIComponent
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="UIButton"/>.
            /// </summary>
            /// <param name="textureComponent">Текстура кнопки.</param>
            /// <param name="onClick">Действие при клике на кнопку.</param>
            public UIButton(ClickableTextureComponent textureComponent, Action onClick)
                : base(textureComponent, onClick)
            {
            }
        }

        /// <summary>
        /// Представляет настраиваемый чекбокс в пользовательском интерфейсе.
        /// </summary>
        /// <remarks>
        /// Обеспечивает отрисовку чекбокса с текстом, обработку кликов и состояние наведения.
        /// </remarks>
        private class ClickableCheckbox
        {
            /// <summary>
            /// Получает границы чекбокса на экране.
            /// </summary>
            public Rectangle Bounds { get; }

            /// <summary>
            /// Получает текст метки чекбокса.
            /// </summary>
            public string Label { get; }

            /// <summary>
            /// Получает текущее состояние чекбокса (отмечен/не отмечен).
            /// </summary>
            public bool IsChecked { get; private set; }

            /// <summary>
            /// Получает значение, указывающее, находится ли курсор над чекбоксом.
            /// </summary>
            public bool IsHovered { get; private set; }

            private readonly Action<bool> _onToggle;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="ClickableCheckbox"/>.
            /// </summary>
            /// <param name="bounds">Границы чекбокса на экране.</param>
            /// <param name="label">Текст метки чекбокса.</param>
            /// <param name="initialState">Начальное состояние чекбокса.</param>
            /// <param name="onToggle">Действие, выполняемое при изменении состояния.</param>
            public ClickableCheckbox(Rectangle bounds, string label, bool initialState, Action<bool> onToggle)
            {
                Bounds = bounds;
                Label = label;
                IsChecked = initialState;
                _onToggle = onToggle;
            }

            /// <summary>
            /// Проверяет, содержит ли чекбокс указанную точку.
            /// </summary>
            /// <param name="x">X-координата точки.</param>
            /// <param name="y">Y-координата точки.</param>
            /// <returns>true, если точка находится внутри границ чекбокса; иначе false.</returns>
            public bool ContainsPoint(int x, int y)
            {
                return Bounds.Contains(x, y);
            }

            /// <summary>
            /// Обновляет состояние наведения чекбокса.
            /// </summary>
            /// <param name="x">X-координата курсора.</param>
            /// <param name="y">Y-координата курсора.</param>
            public void UpdateHover(int x, int y)
            {
                IsHovered = ContainsPoint(x, y);
            }

            /// <summary>
            /// Переключает состояние чекбокса.
            /// </summary>
            /// <remarks>
            /// Изменяет состояние чекбокса и вызывает действие onToggle с новым состоянием.
            /// </remarks>
            public void Toggle()
            {
                IsChecked = !IsChecked;
                _onToggle?.Invoke(IsChecked);
            }

            /// <summary>
            /// Отрисовывает чекбокс и его текстовую метку.
            /// </summary>
            /// <param name="batch">SpriteBatch для отрисовки.</param>
            /// <remarks>
            /// Отрисовывает фон при наведении, иконку чекбокса и текстовую метку.
            /// Цвет текста изменяется при наведении курсора.
            /// </remarks>
            public void Draw(SpriteBatch batch)
            {
                try
                {

                    // Иконка чекбокса
                    var texture = Game1.mouseCursors_1_6;
                    var sourceRect = IsChecked
                        ? new Rectangle(291, 253, 9, 9)
                        : new Rectangle(273, 253, 9, 9);

                    batch.Draw(texture,
                        new Vector2(Bounds.X, Bounds.Y),
                        sourceRect,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        5f,
                        SpriteEffects.None,
                        0.4f);

                    // Текст
                    Vector2 textPosition = new Vector2(
                        Bounds.X + 70,
                        Bounds.Y + Bounds.Height / 2 - Game1.dialogueFont.MeasureString(Label).Y / 2);

                    Utility.drawTextWithShadow(batch, Label, Game1.dialogueFont, textPosition, Game1.textColor);
                }
                catch (Exception)
                {
                    // Игнорируем ошибки отрисовки
                }
            }
        }
    }
    /*
    /// <summary>
    /// Класс для создания меню отслеживания NPC с различными настройками и функционалом.
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        /*
        private static int MEN
    U_X = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (MENU_WIDTH / 2);
        private static int MENU_Y = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (MENU_HEIGHT / 2);

        private int _activeTabIndex; // Индекс активной вкладки
        private readonly List<string> _tabs = new List<string> { "Main", "Settings" }; // Список вкладок
        private ClickableTextureComponent _tabLeftButton;
        private ClickableTextureComponent _tabRightButton;

        private ClickableCheckbox SwitchEnableDisplayCheckbox;     
        private ClickableCheckbox SwitchDisplayGridCheckbox;     
        private ClickableCheckbox SwitchTargetNPCCheckbox;
        private ClickableCheckbox SwitchGlobalPathCheckbox;
        private ClickableCheckbox SwitchTargetLocationsCheckbox;

        private List<ClickableCheckbox> _checkboxes;

        
        private string _displayText;

        private readonly _modInstance _modInstance;

        /// <summary>
        /// Конструктор для создания нового меню отслеживания NPC.
        /// </summary>
        public TrackingMenu(_modInstance modInstance)
            : base(
                MENU_X,
                MENU_Y,
                MENU_WIDTH,
                MENU_HEIGHT,
                true)
        {
            _modInstance = modInstance;
            InitializeComponents();
        }
        //Изменить файл. Исправить неверное отображение имени нпс и баг из-за выхода индекса за пределы когда при отображении всех маршрутов, выбор нпс вызывает еррор
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);


            // Пересоздайте компоненты с новыми координатами
            InitializeComponents();
        }


        /// <summary>
        /// Создает кнопку с изображением стрелки.
        /// </summary>
        /// <param name="x">Координата X для кнопки.</param>
        /// <param name="y">Координата Y для кнопки.</param>
        /// <param name="sourceX">X координата исходного изображения.</param>
        /// <param name="sourceY">Y координата исходного изображения.</param>
        /// <returns>Возвращает объект ClickableTextureComponent для кнопки стрелки.</returns>
        private ClickableTextureComponent CreateArrowButton(int x, int y, int sourceX, int sourceY)
        {
            return new ClickableTextureComponent(
                new Rectangle(x, y, 50, 50),
                Game1.mouseCursors,
                new Rectangle(sourceX, sourceY, 12, 11),
                4f
            );
        }

        /// <summary>
        /// Отрисовывает меню и его компоненты.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки элементов.</param>
        public override void draw(SpriteBatch b)
        {
            DrawBackground(b);
            DrawComponents(b);
            drawMouse(b);
        }

        /// <summary>
        /// Отрисовывает фон меню.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки фона.</param>
        private void DrawBackground(SpriteBatch b)
        {
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);
        }

        private void DrawComponents(SpriteBatch b)
        {
            DrawTabs(b);

            if (_activeTabIndex == 0) // Главная вкладка
            {
                foreach (var checkbox in _checkboxes)
                {
                    checkbox.draw(b, checkbox.label);
                }

                _leftArrowButton.draw(b);
                _rightArrowButton.draw(b);
            }
            else if (_activeTabIndex == 1) // Настройки
            {
                // Добавьте компоненты второй вкладки здесь
                DrawSettingsComponents(b);
            }

            //SwitchEnableDisplayCheckbox.draw(b, "Display");
            //SwitchDisplayGridCheckbox.draw(b, "Grid");
            //SwitchTargetLocationsCheckbox.draw(b, "Locations");
            //SwitchTargetNPCCheckbox.draw(b, "TargetNpc");
            //SwitchGlobalPathCheckbox.draw(b, "GlobalPath");

            //foreach (var checkbox in _checkboxes)
            //{
            //    checkbox.draw(b, checkbox.label);
            //}

            //_leftArrowButton.draw(b);
            //_rightArrowButton.draw(b);

            _exitButton.draw(b);
            DrawSelectedNpcText(b);
        }

        /// <summary>
        /// Отрисовка вкладок.
        /// </summary>
        private void DrawTabs(SpriteBatch b)
        {
            SpriteText.drawString(b, _tabs[_activeTabIndex], xPositionOnScreen + width / 2 - 50, yPositionOnScreen - 50);
            _tabLeftButton.draw(b);
            _tabRightButton.draw(b);
        }

        private void DrawSettingsComponents(SpriteBatch b)
        {
            // Добавьте отрисовку элементов второй вкладки
            Utility.drawTextWithShadow(b, "Settings Page", Game1.dialogueFont, new Vector2(xPositionOnScreen + 50, yPositionOnScreen + 100), Game1.textColor);
        }

        private void HandleSettingsClick(int x, int y)
        {
            // Логика обработки кликов для второй вкладки
        }

        /// <summary>
        /// Отрисовывает имя выбранного NPC.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки текста NPC.</param>
        private void DrawSelectedNpcText(SpriteBatch b)
        {
            _displayText = _modInstance.SwitchTargetNPC && _modInstance.NpcList.CurrentNpcList.Any()
                ? _modInstance.NpcList.GetNpcFromList()
                : "Npc Name";

            Utility.drawTextWithShadow(b, _displayText, Game1.dialogueFont, new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 260), Game1.textColor);
        }

        

        private void ToggleCheckbox(ClickableCheckbox checkbox)
        {
            checkbox.isChecked = !checkbox.isChecked;

            switch (checkbox.label)
            {
                case "Включение":
                    _modInstance.EnableDisplay = checkbox.isChecked;
                    break;
                case "Отображение сетки":
                    _modInstance.DisplayGrid = checkbox.isChecked;
                    break;
                case "Выбор NPC":
                    //_modInstance.SwitchTargetNPC = checkbox.isChecked;
                    ToggleTargetNPC(checkbox, ref _modInstance.SwitchTargetNPC);
                    break;
                case "Отображение всех локаций":
                    //_modInstance.SwitchTargetLocations = checkbox.isChecked;
                    ToggleTargetLocations(checkbox, ref _modInstance.SwitchTargetLocations);
                    break;
                case "Отображение всех маршрутов":
                    //_modInstance.SwitchGlobalNpcPath = checkbox.isChecked;
                    ToggleLoacationChange(checkbox, ref _modInstance.SwitchGlobalNpcPath);
                    break;
                
            }
        }




        /// <summary>
        /// Изменяет текущий выбранный NPC в зависимости от направления (влево/вправо).
        /// </summary>
        /// <param name="direction">Направление изменения: -1 для предыдущего, 1 для следующего NPC.</param>
        private void ChangeNPCSelection(int direction)
        {
            var npcList = _modInstance.NpcList.CurrentNpcList;
            _modInstance.NpcSelected = MathHelper.Clamp(_modInstance.NpcSelected + direction, 0, npcList.Count - 1);          
            _displayText = _modInstance.NpcList.GetNpcFromList();
            _modInstance.DrawTiles.tileStates.Clear();
            _modInstance.SwitchGetNpcPath = true;

            _modInstance.NpcList.CurrentNpcName = _displayText;
        }

        
        /// <summary>
        /// Обрабатывает клики по чекбоксам для изменения состояний.
        /// </summary>
        /// <param name="x">Координата X клика.</param>
        /// <param name="y">Координата Y клика.</param>
        private void HandleCheckBoxClick(int x, int y)
        {
            // Включение мода
            if (SwitchEnableDisplayCheckbox.containsPoint(x, y))
            {
                ToggleCheckBox(SwitchEnableDisplayCheckbox, ref _modInstance.EnableDisplay);
            }
            // отображение сетки
            if (SwitchEnableDisplayCheckbox.isChecked && SwitchDisplayGridCheckbox.containsPoint(x, y))
            {
                ToggleCheckBox(SwitchDisplayGridCheckbox, ref _modInstance.DisplayGrid);
            }
            // смена локации
            if (SwitchEnableDisplayCheckbox.isChecked && SwitchTargetLocationsCheckbox.containsPoint(x, y))
            {
                ToggleTargetLocations(SwitchTargetLocationsCheckbox, ref _modInstance.SwitchTargetLocations);
            }
            // выбор нпс
            if (SwitchEnableDisplayCheckbox.isChecked && SwitchTargetNPCCheckbox.containsPoint(x, y))
            {
                ToggleTargetNPC(SwitchTargetNPCCheckbox, ref _modInstance.SwitchTargetNPC);
            }
            // Отображение всех маршрутов
            if (SwitchEnableDisplayCheckbox.isChecked && SwitchGlobalPathCheckbox.containsPoint(x, y))
            {
                ToggleLoacationChange(SwitchGlobalPathCheckbox, ref _modInstance.SwitchGlobalNpcPath);
            }
        }

        /// <summary>
        /// Переключает состояния CheckBox.
        /// </summary>
        private void ToggleCheckBox(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;
        }

        /// <summary>
        /// Переключает состояние отображения всех локаций и обновляет данные NPC.
        /// </summary>
        /// <param name="checkbox">Чекбокс для переключения состояния.</param>
        /// <param name="state">Состояние отображения (включено/выключено).</param>
        private void ToggleTargetLocations(ClickableCheckbox checkbox, ref bool state)
        {

            state = !state;
            checkbox.isChecked = state;

            // Очищаем информацию о путях NPC
            _modInstance.DrawTiles.tileStates.Clear();           
            _modInstance.SwitchGetNpcPath = true;
            _modInstance.NpcList.CurrentNpcList.Clear();
            _modInstance.SwitchListFull = false;
            if (state)
            {
                _modInstance.Monitor.Log($"{_modInstance.NpcList.CurrentNpcName} {_modInstance.NpcSelected}", LogLevel.Debug);
                if (_modInstance.NpcList.CurrentNpcName != null) _modInstance.NpcSelected = _modInstance.NpcList.CurrentNpcList.IndexOf(_modInstance.NpcList.CurrentNpcName);
                else
                {
                    // Если включено отображение всех локаций, сбрасываем выбранного NPC
                    SelectFirstIdNpc();
                }

            }
            else
            {
                // Если отображаются только текущие локации, обновляем список NPC и проверяем текущий выбор
                //_modInstance.Insce.NpcList.RefreshNpcListForCurrentLocation();
                EnsureValidNpcSelection();
            }
        }
        
        

        /// <summary>
        /// Проверяет корректность текущего выбора NPC и сбрасывает выбор, если он некорректен.
        /// </summary>
        private void EnsureValidNpcSelection()
        {
            if (!_modInstance.NpcList.CurrentNpcList.Any() ||
                _modInstance.NpcSelected >= _modInstance.NpcList.CurrentNpcList.Count)
            {
                SelectFirstIdNpc();
            }
        }
        
        /// <summary>
        /// Переключает состояние выбора NPC.
        /// </summary>
        private void ToggleTargetNPC(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;

            _modInstance.DrawTiles.tileStates.Clear();
            _modInstance.NpcList.CurrentNpcList.Clear();
            _modInstance.SwitchGetNpcPath = true;
            _modInstance.SwitchListFull = false;
            if (state)
            {
                SelectFirstIdNpc();
            }
            else
            {
                _modInstance.NpcList.CurrentNpcName = null;
            }
        }
        private void SelectFirstIdNpc()
        {
            _modInstance.NpcSelected = 0;
        }      

        private void ToggleLoacationChange(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;

            _modInstance.DrawTiles.tileStates.Clear();
            _modInstance.SwitchGetNpcPath = true;
        }

        public class ClickableCheckbox : ClickableComponent
        {
            public bool isChecked;

            public ClickableCheckbox(Rectangle bounds, string label, bool isChecked)
                : base(bounds, label)
            {
                this.label = label;
                this.isChecked = isChecked;
            }

            public void draw(SpriteBatch b, string BoxName)
            {


                if (_modInstance.Instance.EnableDisplay || BoxName == "Включение")
                {
                    b.Draw(texture, new Vector2(bounds.X, bounds.Y), sourceRect, Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.4f);
                    
                }
                else if (BoxName != "Включение")
                {
                    b.Draw(Game1.mouseCursors, new Vector2(bounds.X, bounds.Y), 
                        new Rectangle(192, 256, 64, 64), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.4f);
                }

                drawText(b);
            }

        }
        
    }
    */
}

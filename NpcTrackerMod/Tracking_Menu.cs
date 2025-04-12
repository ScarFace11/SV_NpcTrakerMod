using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для создания меню отслеживания NPC с различными настройками и функционалом.
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        private static int MENU_X = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (MENU_WIDTH / 2);
        private static int MENU_Y = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (MENU_HEIGHT / 2);
        private const int MENU_WIDTH = 600;
        private const int MENU_HEIGHT = 600;

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

        private ClickableTextureComponent _exitButton;
        private ClickableTextureComponent _leftArrowButton;
        private ClickableTextureComponent _rightArrowButton;
        
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

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            // Обновите позиции всех элементов
            xPositionOnScreen = Game1.viewport.Width / 2 - width / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - height / 2;

            // Пересоздайте компоненты с новыми координатами
            InitializeComponents();
        }

        /// <summary>
        /// Инициализирует компоненты меню, включая кнопки и чекбоксы.
        /// </summary>
        private void InitializeComponents()
        {
            InitializeTabs();

            // Exit button
            _exitButton = CreateExitButton();

            // Checkboxes

            //SwitchEnableDisplayCheckbox = CreateCheckbox(100, "Включение", _modInstance.EnableDisplay);
            //SwitchDisplayGridCheckbox = CreateCheckbox(350, "Отображение сетки", _modInstance.DisplayGrid);
            //SwitchTargetLocationsCheckbox = CreateCheckbox(150, "Отображение всех локаций", _modInstance.SwitchTargetLocations);
            //SwitchTargetNPCCheckbox = CreateCheckbox(200, "Выбор NPC", _modInstance.SwitchTargetNPC);
            //SwitchGlobalPathCheckbox = CreateCheckbox(250, "Отображение всех маршрутов", _modInstance.SwitchGlobalNpcPath);

            _checkboxes = new List<ClickableCheckbox>
            {
                CreateCheckbox(100, "Включение", _modInstance.EnableDisplay),
                CreateCheckbox(150, "Отображение сетки", _modInstance.DisplayGrid),
                CreateCheckbox(200, "Выбор NPC", _modInstance.SwitchTargetNPC),
                CreateCheckbox(300, "Отображение всех локаций", _modInstance.SwitchTargetLocations),
                CreateCheckbox(350, "Отображение всех маршрутов", _modInstance.SwitchGlobalNpcPath),
                
            };

            // Arrow buttons
            _leftArrowButton = CreateArrowButton(xPositionOnScreen + 30, yPositionOnScreen + 250, 352, 495);
            _rightArrowButton = CreateArrowButton(xPositionOnScreen + 250, yPositionOnScreen + 250, 365, 495);
        }

        /// <summary>
        /// Инициализация вкладок.
        /// </summary>
        private void InitializeTabs()
        {
            _tabLeftButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - 40, 30, 30),
                Game1.mouseCursors,
                new Rectangle(352, 495, 12, 11),
                2f
            );

            _tabRightButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 50, yPositionOnScreen - 40, 30, 30),
                Game1.mouseCursors,
                new Rectangle(365, 495, 12, 11),
                2f
            );
        }

        private ClickableTextureComponent CreateExitButton()
        {
            return new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 30, 30, 30),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
        }

        private ClickableCheckbox CreateCheckbox(int offsetY, string label, bool isChecked)
        {
            return new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + offsetY, 300, 50),
                label,
                isChecked
            );
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

        /// <summary>
        /// Обрабатывает клик по элементам меню.
        /// </summary>
        /// <param name="x">Координата X клика.</param>
        /// <param name="y">Координата Y клика.</param>
        /// <param name="playSound">Если true, проигрывается звук клика.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Обработка нажатия на кнопку выхода
            if (_exitButton.containsPoint(x, y))
            {
                exitThisMenu();
                return;
            }

            if (_tabLeftButton.containsPoint(x, y))
            {
                _activeTabIndex = MathHelper.Clamp(_activeTabIndex - 1, 0, _tabs.Count - 1);
                return;
            }

            if (_tabRightButton.containsPoint(x, y))
            {
                _activeTabIndex = MathHelper.Clamp(_activeTabIndex + 1, 0, _tabs.Count - 1);
                return;
            }

            if (_activeTabIndex == 0)
            {
                if ((_leftArrowButton.containsPoint(x, y) || _rightArrowButton.containsPoint(x, y)) && _modInstance.SwitchTargetNPC)
                    HandleArrowButtonClick(x, y);

                foreach (var checkbox in _checkboxes)
                {
                    if (checkbox.containsPoint(x, y))
                    {
                        ToggleCheckbox(checkbox);
                        break;
                    }
                }
            }
            else if (_activeTabIndex == 1)
            {
                HandleSettingsClick(x, y); // Логика кликов для второй вкладки
            }

            //if ((_leftArrowButton.containsPoint(x,y) || _rightArrowButton.containsPoint(x,y)) && _modInstance.SwitchTargetNPC) 
            //    HandleArrowButtonClick(x, y);

            //HandleCheckBoxClick(x, y);

            //foreach (var checkbox in _checkboxes)
            //{
            //    if (checkbox.containsPoint(x, y))
            //    {
            //        ToggleCheckbox(checkbox);
            //        break;
            //    }
            //}
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
        ///  Обрабатывает действие при наведении курсора. 
        /// </summary>
        /// <param name="x">Координата X курсора.</param>
        /// <param name="y">Координата Y курсора.</param>
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);         
        }

        /// <summary>
        /// Обрабатывает клик по кнопкам-стрелкам для смены NPC.
        /// </summary>
        /// <param name="x">Координата X клика.</param>
        /// <param name="y">Координата Y клика.</param>
        private void HandleArrowButtonClick(int x, int y)
        {
            if (_leftArrowButton.containsPoint(x, y) && _modInstance.SwitchTargetNPC && _modInstance.NpcList.CurrentNpcList.Any())
            {
                ChangeNPCSelection(-1);
            }

            if (_rightArrowButton.containsPoint(x, y) && _modInstance.SwitchTargetNPC && _modInstance.NpcList.CurrentNpcList.Any())
            {
                ChangeNPCSelection(1);
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
                    //SelectFirstIdNpc();
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
                var texture = Game1.mouseCursors_1_6;
                var sourceRect = isChecked ? new Rectangle(291, 253, 9, 9) : new Rectangle(273, 253, 9, 9);

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
            public void drawText(SpriteBatch b)
            {
                Vector2 textPosition = new Vector2(bounds.X + 70, bounds.Y + (bounds.Height / 2) - (Game1.dialogueFont.MeasureString(label).Y / 2));
                Utility.drawTextWithShadow(b, label, Game1.dialogueFont, textPosition, Game1.textColor);
            }
        }
    }
}

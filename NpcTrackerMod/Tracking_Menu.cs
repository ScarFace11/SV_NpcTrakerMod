using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using System.Linq;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для создания меню отслеживания NPC с различными настройками и функционалом.
    /// </summary>
    public class TrackingMenu : IClickableMenu
    {
        private ClickableCheckbox DisplayGridCheckbox;     
        private ClickableCheckbox SwitchTargetNPCCheckbox;
        private ClickableCheckbox SwitchGlobalPathCheckbox;
        private ClickableCheckbox SwitchTargetLocationsCheckbox;

        private ClickableTextureComponent exitButton;
        private ClickableTextureComponent leftArrowButton;
        private ClickableTextureComponent rightArrowButton;

        private string displayText;

        /// <summary>
        /// Конструктор для создания нового меню отслеживания NPC.
        /// </summary>
        public TrackingMenu()
        : base(Game1.viewport.Width / 2 - 300, Game1.viewport.Height / 2 - 300, 600, 600, true)
        {
            InitializeMenuComponents();
        }

        /// <summary>
        /// Инициализирует компоненты меню, включая кнопки и чекбоксы.
        /// </summary>
        private void InitializeMenuComponents()
        {
            // Exit button
            exitButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 30, 30, 30),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );

            // Checkboxes
            DisplayGridCheckbox = new ClickableCheckbox(new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 100, 300, 50), "Включение сетки", NpcTrackerMod.Instance.DisplayGrid);
            SwitchTargetLocationsCheckbox = new ClickableCheckbox(new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 150, 300, 50), "Отображение всех локаций", NpcTrackerMod.Instance.SwitchTargetLocations);
            SwitchTargetNPCCheckbox = new ClickableCheckbox(new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 200, 300, 50), "Выбор нпс", NpcTrackerMod.Instance.SwitchTargetNPC);
            SwitchGlobalPathCheckbox = new ClickableCheckbox(new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 250, 300, 50), "Отображение всех маршрутов", NpcTrackerMod.Instance.SwitchGlobalNpcPath);

            // Arrow buttons
            leftArrowButton = CreateArrowButton(xPositionOnScreen + 30, yPositionOnScreen + 300, 352, 495);
            rightArrowButton = CreateArrowButton(xPositionOnScreen + 250, yPositionOnScreen + 300, 365, 495);
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
            DrawCheckBoxes(b); 
            DrawArrowButtons(b);
            DrawExitButton(b);
            DrawNPCText(b);

            drawMouse(b);
        }

        /// <summary>
        /// Отрисовывает фон меню.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки фона.</param>
        private void DrawBackground(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);
        }

        /// <summary>
        /// Отрисовывает чекбоксы меню.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки чекбоксов.</param>
        private void DrawCheckBoxes(SpriteBatch b)
        {
            DisplayGridCheckbox.draw(b, "Grid");
            SwitchTargetLocationsCheckbox.draw(b, "Locations");
            SwitchTargetNPCCheckbox.draw(b, "TargetNpc");
            SwitchGlobalPathCheckbox.draw(b, "GlobalPath");
        }

        /// <summary>
        /// Отрисовывает кнопки-стрелки для навигации.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки стрелок.</param>
        private void DrawArrowButtons(SpriteBatch b)
        {
            leftArrowButton.draw(b);
            rightArrowButton.draw(b);
        }

        /// <summary>
        /// Отрисовывает кнопку выхода.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки кнопки выхода.</param>
        private void DrawExitButton(SpriteBatch b)
        {
            exitButton.draw(b);
        }

        /// <summary>
        /// Отрисовывает имя выбранного NPC.
        /// </summary>
        /// <param name="b">SpriteBatch, используемый для отрисовки текста NPC.</param>
        private void DrawNPCText(SpriteBatch b)
        {
            if (SwitchTargetNPCCheckbox.isChecked && NpcTrackerMod.Instance.NpcList.NpcCurrentList.Any())
            {
                displayText = NpcTrackerMod.Instance.NpcList.GetNpcFromList();
            }
            else
            {
                displayText = "Npc Name";
            }

            Utility.drawTextWithShadow(b, displayText, Game1.dialogueFont, new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 315), Game1.textColor);
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
            if (exitButton.containsPoint(x, y)) exitThisMenu();

            if(DisplayGridCheckbox.isChecked) HandleArrowButtonClick(x, y);
            HandleCheckBoxClick(x, y);
        }

        /// <summary>
        /// Обрабатывает клик по кнопкам-стрелкам для смены NPC.
        /// </summary>
        /// <param name="x">Координата X клика.</param>
        /// <param name="y">Координата Y клика.</param>
        private void HandleArrowButtonClick(int x, int y)
        {
            if (leftArrowButton.containsPoint(x, y) && SwitchTargetNPCCheckbox.isChecked && NpcTrackerMod.Instance.NpcList.NpcCurrentList.Any())
            {
                ChangeNPCSelection(-1);
            }
            if (rightArrowButton.containsPoint(x, y) && SwitchTargetNPCCheckbox.isChecked && NpcTrackerMod.Instance.NpcList.NpcCurrentList.Any())
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
            NpcTrackerMod.Instance.NpcSelected = MathHelper.Clamp(NpcTrackerMod.Instance.NpcSelected + direction, 0, NpcTrackerMod.Instance.NpcList.NpcCurrentList.Count - 1);
            displayText = NpcTrackerMod.Instance.NpcList.GetNpcFromList();

            NpcTrackerMod.Instance.tileStates.Clear();
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
        }

        /// <summary>
        /// Обрабатывает клики по чекбоксам для изменения состояний.
        /// </summary>
        /// <param name="x">Координата X клика.</param>
        /// <param name="y">Координата Y клика.</param>
        private void HandleCheckBoxClick(int x, int y)
        {
            // отображение сетки
            if (DisplayGridCheckbox.containsPoint(x, y))
            {
                ToggleCheckBox(DisplayGridCheckbox, ref NpcTrackerMod.Instance.DisplayGrid);
            }
            // смена локации
            if (DisplayGridCheckbox.isChecked && SwitchTargetLocationsCheckbox.containsPoint(x, y))
            {
                ToggleTargetLocations(SwitchTargetLocationsCheckbox, ref NpcTrackerMod.Instance.SwitchTargetLocations);
            }
            // выбор нпс
            if (DisplayGridCheckbox.isChecked && SwitchTargetNPCCheckbox.containsPoint(x, y))
            {
                ToggleTargetNPC(SwitchTargetNPCCheckbox, ref NpcTrackerMod.Instance.SwitchTargetNPC);
            }
            // Отображение всех маршрутов
            if (DisplayGridCheckbox.isChecked && SwitchGlobalPathCheckbox.containsPoint(x, y))
            {
                ToggleLoacationChange(SwitchGlobalPathCheckbox, ref NpcTrackerMod.Instance.SwitchGlobalNpcPath);
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
        /// Переключает состояние отображения всех локаций и обновляет соответствующие данные NPC.
        /// </summary>
        /// <param name="checkbox">Чекбокс, который отображает текущее состояние.</param>
        /// <param name="state">Текущее состояние (включено/выключено).</param>
        private void ToggleTargetLocations(ClickableCheckbox checkbox, ref bool state)
        {

            state = !state;
            checkbox.isChecked = state;

            // Очищаем информацию о путях NPC
            NpcTrackerMod.Instance.tileStates.Clear();           
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            NpcTrackerMod.Instance.NpcList.NpcCurrentList.Clear();
            NpcTrackerMod.Instance.SwitchListFull = false;
            if (state)
            {
                // Если включено отображение всех локаций, сбрасываем выбранного NPC
                //NpcTrackerMod.Instance.NpcSelected = 0;
            }
            else
            {
                // Если отображаются только текущие локации, обновляем список NPC и проверяем текущий выбор
                //UpdateNpcListForCurrentLocation();
                //ValidateNpcSelection();
            }
        }

        /// <summary>
        /// Обновляет список NPC для текущей локации игрока.
        /// </summary>
        private void UpdateNpcListForCurrentLocation()
        {
            var currentLocation = Game1.player.currentLocation.Name;
            NpcTrackerMod.Instance.NpcList.NpcCurrentList = NpcTrackerMod.Instance.NpcList.NpcTotalList
                .Where(npcName => Game1.currentLocation.characters.Any(npc => npc.Name == npcName))
                .ToList();
        }

        /// <summary>
        /// Проверяет корректность текущего выбора NPC и сбрасывает выбор, если он некорректен.
        /// </summary>
        private void ValidateNpcSelection()
        {
            if (!NpcTrackerMod.Instance.NpcList.NpcCurrentList.Any() ||
                NpcTrackerMod.Instance.NpcSelected >= NpcTrackerMod.Instance.NpcList.NpcCurrentList.Count)
            {
                NpcTrackerMod.Instance.NpcSelected = 0;
            }
        }
        /// <summary>
        /// Переключает состояние выбора NPC.
        /// </summary>
        private void ToggleTargetNPC(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;

            NpcTrackerMod.Instance.tileStates.Clear();
            NpcTrackerMod.Instance.NpcList.NpcCurrentList.Clear();
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            NpcTrackerMod.Instance.SwitchListFull = false;
            if (state)
            {                           
                NpcTrackerMod.Instance.NpcSelected = 0;
            }
        }
        private void ToggleLoacationChange(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;

            NpcTrackerMod.Instance.tileStates.Clear();
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            //foreach( var x in NpcTrackerMod.Instance.NpcList.NpcTotalGlobalPath)
            //{
            //    NpcTrackerMod.Instance.Monitor.Log($"{x.Key}", LogLevel.Debug);
            //    foreach (var j in x.Value)
            //    {
            //        NpcTrackerMod.Instance.Monitor.Log($"{j.Item1} ", LogLevel.Debug);
            //    }
            //}

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

                if (NpcTrackerMod.Instance.DisplayGrid || BoxName == "Grid")
                {
                    b.Draw(texture, new Vector2(bounds.X, bounds.Y), sourceRect, Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.4f);
                    
                }
                else if (BoxName != "Grid")
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
            public bool ContainsPoint(int x, int y)
            {
                return bounds.Contains(x, y);
            }
        }

    }
}

using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using System.Threading;
using System.Linq;

namespace NpcTrackerMod
{

    public class TrackingMenu : IClickableMenu
    {
        private readonly ClickableCheckbox DisplayGridCheckbox;
        private readonly ClickableCheckbox SwitchTargetLocationsCheckbox;
        private readonly ClickableCheckbox SwitchTargetNPCCheckbox;

        private ClickableTextureComponent exitButton;

        private ClickableTextureComponent leftArrowButton;
        private ClickableTextureComponent rightArrowButton;
        private string displayText = "Npc Name"; // Пример текста, который будет отображаться между кнопками


        public TrackingMenu()
        : base(Game1.viewport.Width / 2 - 300, Game1.viewport.Height / 2 - 300, 600, 600, true)
        {

            // Ваша инициализация чекбоксов

            if (NpcTrackerMod.Instance == null)
            {
                // Здесь можно добавить обработку ошибки или попытаться инициализировать объект
                throw new System.Exception("NpcTrackerMod.Instance is null. Ensure it is initialized before creating TrackingMenu.");
            }
            // Инициализация кнопки выхода
            exitButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 80, yPositionOnScreen + 30, 30, 30),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
            // Инициализация чекбоксов
            DisplayGridCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 100, 300, 50),
                "Включение сетки",
                NpcTrackerMod.Instance.DisplayGrid
            );
            SwitchTargetLocationsCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 150, 300, 50),
                "Отображение всех локаций",
                NpcTrackerMod.Instance.SwitchTargetLocations
            );
            SwitchTargetNPCCheckbox = new ClickableCheckbox(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 200, 300, 50),
                "Выбор нпс",
                NpcTrackerMod.Instance.SwitchTargetNPC
            );
            

            // Инициализация кнопок с иконками стрелок
            leftArrowButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 30, yPositionOnScreen + 300, 50, 50),
                Game1.mouseCursors,
                new Rectangle(352, 495, 12, 11),
                4f
            );
            rightArrowButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + 250, yPositionOnScreen + 300, 50, 50),
                Game1.mouseCursors,
                new Rectangle(365, 495, 12, 11),
                4f
            );

        }

        public override void draw(SpriteBatch b)
        {
            
            // Отрисовка фона
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);

            // Отрисовка названия
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);

            // Отрисовка чекбоксов
            DisplayGridCheckbox.draw(b, "Grid");
            SwitchTargetLocationsCheckbox.draw(b, "Locations");
            SwitchTargetNPCCheckbox.draw(b, "TargetNpc");

            // Отрисовка кнопок со стрелками и текста между ними
            leftArrowButton.draw(b);
            rightArrowButton.draw(b);
            Utility.drawTextWithShadow(b, displayText, Game1.dialogueFont, new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 315), Game1.textColor);

            if (SwitchTargetNPCCheckbox.isChecked && NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList.Any())
            {
                displayText = NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList[NpcTrackerMod.Instance.NpcSelected];
            }

            exitButton.draw(b);
            // Отрисовка кнопки выхода
            drawMouse(b);

        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Обработка нажатия на кнопку выхода
            if (exitButton.containsPoint(x, y))
            {
                exitThisMenu();
                return;
            }

            // Обработка нажатия на кнопку с левой стрелкой
            if (leftArrowButton.containsPoint(x, y) && SwitchTargetNPCCheckbox.isChecked)
            {
                // Логика для нажатия на левую стрелку
                if (NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList.Any() && NpcTrackerMod.Instance.NpcSelected > 0)
                {
                    NpcTrackerMod.Instance.NpcSelected--;
                    displayText = NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList[NpcTrackerMod.Instance.NpcSelected];
                    NpcTrackerMod.Instance.tileStates.Clear();
                    NpcTrackerMod.Instance.Switchnpcpath = false;
                    NpcTrackerMod.Instance.SwitchGetNpcPath = true;
                }
            }

            // Обработка нажатия на кнопку с правой стрелкой
            if (rightArrowButton.containsPoint(x, y) && SwitchTargetNPCCheckbox.isChecked)
            {
                // Логика для нажатия на правую стрелку
                if (NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList.Any() && NpcTrackerMod.Instance.NpcSelected < NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList.Count() - 1)
                {
                    NpcTrackerMod.Instance.NpcSelected++;
                    displayText = NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList[NpcTrackerMod.Instance.NpcSelected];
                    NpcTrackerMod.Instance.tileStates.Clear();
                    NpcTrackerMod.Instance.Switchnpcpath = false;
                    NpcTrackerMod.Instance.SwitchGetNpcPath = true;
                }
            }

            // Переключение чекбоксов
            if (DisplayGridCheckbox.containsPoint(x, y)) // отображение сетки
            {
                NpcTrackerMod.Instance.DisplayGrid = !NpcTrackerMod.Instance.DisplayGrid;
                DisplayGridCheckbox.isChecked = NpcTrackerMod.Instance.DisplayGrid;
                //NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            }

            if (DisplayGridCheckbox.isChecked && SwitchTargetLocationsCheckbox.containsPoint(x, y)) // смена локаций
            {
                NpcTrackerMod.Instance.SwitchTargetLocations = !NpcTrackerMod.Instance.SwitchTargetLocations;
                SwitchTargetLocationsCheckbox.isChecked = NpcTrackerMod.Instance.SwitchTargetLocations;
                NpcTrackerMod.Instance.tileStates.Clear();
                NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            }

            if (DisplayGridCheckbox.isChecked && SwitchTargetNPCCheckbox.containsPoint(x, y)) // количество нпс
            {
                NpcTrackerMod.Instance.SwitchTargetNPC = !NpcTrackerMod.Instance.SwitchTargetNPC;
                SwitchTargetNPCCheckbox.isChecked = NpcTrackerMod.Instance.SwitchTargetNPC;
                NpcTrackerMod.Instance.tileStates.Clear();
                NpcTrackerMod.Instance.TotalNpcList.NpcCurrentList.Clear();
                NpcTrackerMod.Instance.SwitchGetNpcPath = true;
                NpcTrackerMod.Instance.SwitchListFull = false;

                if (SwitchTargetNPCCheckbox.isChecked)
                {
                    NpcTrackerMod.Instance.NpcSelected = 0;
                  
                }

                displayText = "Npc Name";
                
            }
        }

        public class ClickableCheckbox : ClickableComponent
        {
            public bool isChecked;
            private string label;

            public ClickableCheckbox(Rectangle bounds, string label, bool isChecked)
                : base(bounds, label)
            {
                this.label = label;
                this.isChecked = isChecked;
            }
            public void draw_Red_Button(SpriteBatch b)
            {

            }
            public void draw(SpriteBatch b, string BoxName)
            {
                if (NpcTrackerMod.Instance.DisplayGrid || BoxName == "Grid") // Если Чекбокс с сеткой true
                {
                    // Пустой квадрат и квадрат с галочкой
                    b.Draw(Game1.mouseCursors_1_6, new Vector2(bounds.X, bounds.Y),
                    isChecked ? new Rectangle(291, 253, 9, 9) : new Rectangle(273, 253, 9, 9),
                    Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.4f);
                }
                else if (BoxName != "Grid")
                {
                    // квадрат с красным знаком
                    b.Draw(Game1.mouseCursors, new Vector2(bounds.X, bounds.Y),
                    isChecked ? new Rectangle(273, 253, 9, 9) : new Rectangle(192, 256, 64, 64),
                    Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.4f);
                }
                

                draw_Text(b);
            }
            public void draw_Text(SpriteBatch b)
            {
                Vector2 textPosition = new Vector2(bounds.X + 70, bounds.Y + (bounds.Height / 2) - (Game1.dialogueFont.MeasureString(label).Y / 2));
                Utility.drawTextWithShadow(b, label, Game1.dialogueFont, textPosition, Game1.textColor);
            }
            public bool containsPoint(int x, int y)
            {
                return bounds.Contains(x, y);
            }
        }

    }
}

using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using System.Threading;

namespace NpcTrackerMod
{

    public class TrackingMenu : IClickableMenu
    {

        private string title = "Tracking Menu"; // Название меню

        private Texture2D background;
        //private readonly OptionsCheckbox DisplayGridCheckbox;    
        //private readonly OptionsCheckbox SwitchTargetLocationsCheckbox;
        //private readonly OptionsCheckbox SwitchTargetNPCCheckbox;

        private readonly ClickableCheckbox DisplayGridCheckbox;
        private readonly ClickableCheckbox SwitchTargetLocationsCheckbox;
        private readonly ClickableCheckbox SwitchTargetNPCCheckbox;

        private ClickableTextureComponent exitButton;

        public TrackingMenu()
        : base(Game1.viewport.Width / 2 - 300, Game1.viewport.Height / 2 - 300, 600, 600, true)
        {
            background = Game1.menuTexture;

            // Ваша инициализация чекбоксов
            //DisplayGridCheckbox = new OptionsCheckbox("Включение сетки", -1, -1);
            //SwitchTargetLocationsCheckbox = new OptionsCheckbox("Отображение всех локаций", -1, -1);
            //SwitchTargetNPCCheckbox = new OptionsCheckbox("Выбор нпс", -1, -1);
            if (NpcTrackerMod.Instance == null)
            {
                // Здесь можно добавить обработку ошибки или попытаться инициализировать объект
                throw new System.Exception("NpcTrackerMod.Instance is null. Ensure it is initialized before creating TrackingMenu.");
            }

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

            // Инициализация кнопки выхода
            exitButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - 60, yPositionOnScreen + 10, 50, 50),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );

        }

        /*
        public override void draw(SpriteBatch b)
        {
            // Draw background within menu bounds
            b.Draw(background, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), Color.White);

            // Draw title
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);

            // Draw checkboxes with spacing
            DisplayGridCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 100);
            SwitchTargetLocationsCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 150);
            SwitchTargetNPCCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 200);

            // Draw exit button
            if (exitButton != null)
                exitButton.draw(b);

            drawMouse(b);
        }
        */
        /*
        public override void draw(SpriteBatch b)
        {
            
            // Draw background within menu bounds
            //b.Draw(background, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), Color.White);
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);

            // Draw title
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);

            // Draw checkboxes with spacing
            //DisplayGridCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 100, this);
            //SwitchTargetLocationsCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 150, this);
            //SwitchTargetNPCCheckbox.draw(b, xPositionOnScreen + 30, yPositionOnScreen + 200, this);

            DisplayGridCheckbox.draw(b);
            SwitchTargetLocationsCheckbox.draw(b);
            SwitchTargetNPCCheckbox.draw(b);

            // Draw exit button
            //if (exitButton != null)
            //exitButton.draw(b);

            drawMouse(b);
        }
        */
        public override void draw(SpriteBatch b)
        {
            // Отрисовка фона
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);

            // Отрисовка названия
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);

            // Отрисовка чекбоксов
            DisplayGridCheckbox.draw(b);
            SwitchTargetLocationsCheckbox.draw(b);
            SwitchTargetNPCCheckbox.draw(b);

            // Отрисовка кнопки выхода
            drawMouse(b);
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Check if exit button is clicked
            if (exitButton.containsPoint(x, y))
            {
                exitThisMenu();
                return;
            }

            // Toggle the checkboxes
            if (DisplayGridCheckbox.containsPoint(x, y))
            {
                NpcTrackerMod.Instance.DisplayGrid = !NpcTrackerMod.Instance.DisplayGrid;
                DisplayGridCheckbox.isChecked = NpcTrackerMod.Instance.DisplayGrid;
                

            }
            
            if (DisplayGridCheckbox.isChecked && SwitchTargetLocationsCheckbox.containsPoint(x, y))
            {
                NpcTrackerMod.Instance.SwitchTargetLocations = !NpcTrackerMod.Instance.SwitchTargetLocations;
                SwitchTargetLocationsCheckbox.isChecked = NpcTrackerMod.Instance.SwitchTargetLocations;
                NpcTrackerMod.Instance.tileStates.Clear();
            }


            if (DisplayGridCheckbox.isChecked && SwitchTargetNPCCheckbox.containsPoint(x, y))
            {
                NpcTrackerMod.Instance.SwitchTargetNPC = !NpcTrackerMod.Instance.SwitchTargetNPC;
                SwitchTargetNPCCheckbox.isChecked = NpcTrackerMod.Instance.SwitchTargetNPC;
                NpcTrackerMod.Instance.tileStates.Clear();
            }
        }


        private void drawText(SpriteBatch b, string text, int x, int y, Color color)
        {
            SpriteText.drawString(b, text, x, y, color: color);
        }
        public class ClickableCheckbox : ClickableComponent
        {
            public bool isChecked;
            private string label;

            public ClickableCheckbox(Rectangle bounds, string label, bool isChecked)
                : base(bounds, label) // Передаем bounds и label в базовый конструктор
            {
                this.label = label;
                this.isChecked = isChecked;
            }

            public void draw(SpriteBatch b)
            {
                // Отрисовка флажка (checkbox)
                // Изначально показываем пустой квадрат
                b.Draw(Game1.mouseCursors, new Vector2(bounds.X, bounds.Y),
                    isChecked ? new Rectangle(128, 256, 64, 64) : new Rectangle(192, 256, 64, 64),
                    Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.4f);

                // Отрисовка текста рядом с флажком, с выравниванием по вертикали
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

using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using System.Linq;

namespace NpcTrackerMod
{
    public class TrackingMenu : IClickableMenu
    {
        private ClickableCheckbox DisplayGridCheckbox;
        private ClickableCheckbox SwitchTargetLocationsCheckbox;
        private ClickableCheckbox SwitchTargetNPCCheckbox;
        private ClickableCheckbox SwitchGlobalPathCheckbox;

        private ClickableTextureComponent exitButton;
        private ClickableTextureComponent leftArrowButton;
        private ClickableTextureComponent rightArrowButton;
        private string displayText = "Npc Name"; // Пример текста, который будет отображаться между кнопками


        public TrackingMenu()
        : base(Game1.viewport.Width / 2 - 300, Game1.viewport.Height / 2 - 300, 600, 600, true)
        {
            InitializeMenuComponents();

            if (NpcTrackerMod.Instance == null)
            {
                // Здесь можно добавить обработку ошибки или попытаться инициализировать объект
                throw new System.Exception("NpcTrackerMod.Instance is null. Ensure it is initialized before creating TrackingMenu.");
            }



        }
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
        private ClickableTextureComponent CreateArrowButton(int x, int y, int sourceX, int sourceY)
        {
            return new ClickableTextureComponent(
                new Rectangle(x, y, 50, 50),
                Game1.mouseCursors,
                new Rectangle(sourceX, sourceY, 12, 11),
                4f
            );
        }
        public override void draw(SpriteBatch b)
        {
            DrawBackground(b);
            DrawCheckBoxes(b); 
            DrawArrowButtons(b);
            DrawExitButton(b);
            DrawNPCText(b);

            drawMouse(b);
        }
        private void DrawBackground(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f, true);
            SpriteText.drawString(b, "NPC Tracker Menu", xPositionOnScreen + 100, yPositionOnScreen + 40);
        }

        private void DrawCheckBoxes(SpriteBatch b)
        {
            DisplayGridCheckbox.draw(b, "Grid");
            SwitchTargetLocationsCheckbox.draw(b, "Locations");
            SwitchTargetNPCCheckbox.draw(b, "TargetNpc");
            SwitchGlobalPathCheckbox.draw(b, "GlobalPath");
        }

        private void DrawArrowButtons(SpriteBatch b)
        {
            leftArrowButton.draw(b);
            rightArrowButton.draw(b);
        }

        private void DrawExitButton(SpriteBatch b)
        {
            exitButton.draw(b);
        }

        private void DrawNPCText(SpriteBatch b)
        {
            if (SwitchTargetNPCCheckbox.isChecked && NpcTrackerMod.Instance.NpcList.NpcCurrentList.Any())
            {
                displayText = NpcTrackerMod.Instance.NpcList.NpcCurrentList[NpcTrackerMod.Instance.NpcSelected];
            }

            Utility.drawTextWithShadow(b, displayText, Game1.dialogueFont, new Vector2(xPositionOnScreen + 100, yPositionOnScreen + 315), Game1.textColor);
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Обработка нажатия на кнопку выхода
            if (exitButton.containsPoint(x, y)) exitThisMenu();

            HandleArrowButtonClick(x, y);
            HandleCheckBoxClick(x, y);
        }
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
        private void ChangeNPCSelection(int direction)
        {
            NpcTrackerMod.Instance.NpcSelected = MathHelper.Clamp(NpcTrackerMod.Instance.NpcSelected + direction, 0, NpcTrackerMod.Instance.NpcList.NpcCurrentList.Count - 1);
            displayText = NpcTrackerMod.Instance.NpcList.NpcCurrentList[NpcTrackerMod.Instance.NpcSelected];
            NpcTrackerMod.Instance.tileStates.Clear();
            NpcTrackerMod.Instance.Switchnpcpath = false;
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
        }
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
                ToggleNPCSelection(SwitchTargetNPCCheckbox, ref NpcTrackerMod.Instance.SwitchTargetNPC);
            }
            // Отображение всех маршрутов
            if (DisplayGridCheckbox.isChecked && SwitchGlobalPathCheckbox.containsPoint(x, y))
            {
                ToggleCheckBox(SwitchGlobalPathCheckbox, ref NpcTrackerMod.Instance.SwitchGlobalNpcPath);
            }
        }
        private void ToggleCheckBox(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;
        }
        private void ToggleTargetLocations(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;
            NpcTrackerMod.Instance.tileStates.Clear();           
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
        }
        private void ToggleNPCSelection(ClickableCheckbox checkbox, ref bool state)
        {
            state = !state;
            checkbox.isChecked = state;
            NpcTrackerMod.Instance.tileStates.Clear();
            NpcTrackerMod.Instance.NpcList.NpcCurrentList.Clear();
            NpcTrackerMod.Instance.SwitchGetNpcPath = true;
            NpcTrackerMod.Instance.SwitchListFull = false;
            if (state) NpcTrackerMod.Instance.NpcSelected = 0;
            displayText = "Npc Name";
        }
        public class ClickableCheckbox : ClickableComponent
        {
            public bool isChecked;
            private readonly string label;

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

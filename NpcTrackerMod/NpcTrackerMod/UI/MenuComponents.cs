using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace NpcTrackerMod.UI
{
    // ── Вкладка ───────────────────────────────────────────────────────────────────

    /// <summary> Описание одной вкладки меню. </summary>
    internal class TabInfo
    {
        public string Name             { get; }
        public Action InitializeAction { get; }

        public TabInfo(string name, Action init)
        {
            Name             = name;
            InitializeAction = init;
        }
    }

    // ── Базовый компонент ─────────────────────────────────────────────────────────

    /// <summary> Кликабельный элемент меню. Базовый класс. </summary>
    internal class UIComponent
    {
        protected ClickableTextureComponent Texture { get; }
        public Action OnClick { get; set; }

        public UIComponent(ClickableTextureComponent texture, Action onClick = null)
        {
            Texture = texture;
            OnClick = onClick;
        }

        public virtual bool ContainsPoint(int x, int y) => Texture?.containsPoint(x, y) ?? false;

        public virtual void Draw(SpriteBatch b) => Texture?.draw(b);
    }

    // ── Кнопка-иконка ────────────────────────────────────────────────────────────

    /// <summary> Кнопка на основе игровой текстуры (стрелки и т.п.). </summary>
    internal class UIButton : UIComponent
    {
        public UIButton(ClickableTextureComponent texture, Action onClick)
            : base(texture, onClick) { }
    }

    // ── Кнопка с текстом ─────────────────────────────────────────────────────────

    /// <summary> Кнопка с текстовой подписью (переназначение клавиш). </summary>
    internal class UITextButton : UIComponent
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

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height,
                Color.White, 0.5f, false);

            var sz = Game1.smallFont.MeasureString(_label);
            Utility.drawTextWithShadow(b, _label, Game1.smallFont,
                new Vector2(
                    _bounds.X + (_bounds.Width  - sz.X) / 2f,
                    _bounds.Y + (_bounds.Height - sz.Y) / 2f),
                Game1.textColor);
        }
    }

    // ── Чекбокс ───────────────────────────────────────────────────────────────────

    /// <summary> Переключатель с галочкой и текстовой подписью. </summary>
    internal class ClickableCheckbox
    {
        public Rectangle  Bounds    { get; }
        public string     Label     { get; }
        public bool       IsChecked { get; private set; }

        private readonly Action<bool> _onToggle;

        public ClickableCheckbox(Rectangle bounds, string label, bool initial, Action<bool> onToggle)
        {
            Bounds    = bounds;
            Label     = label;
            IsChecked = initial;
            _onToggle = onToggle;
        }

        public bool ContainsPoint(int x, int y) => Bounds.Contains(x, y);

        public void Toggle()
        {
            IsChecked = !IsChecked;
            _onToggle?.Invoke(IsChecked);
        }

        public void Draw(SpriteBatch b)
        {
            try
            {
                var srcRect = IsChecked
                    ? new Rectangle(291, 253, 9, 9)
                    : new Rectangle(273, 253, 9, 9);

                b.Draw(Game1.mouseCursors_1_6,
                    new Vector2(Bounds.X, Bounds.Y),
                    srcRect, Color.White, 0f, Vector2.Zero, 5f, SpriteEffects.None, 0.4f);

                var textPos = new Vector2(
                    Bounds.X + 70,
                    Bounds.Y + Bounds.Height / 2f - Game1.dialogueFont.MeasureString(Label).Y / 2f);

                Utility.drawTextWithShadow(b, Label, Game1.dialogueFont, textPos, Game1.textColor);
            }
            catch { /* игнорируем ошибки отрисовки отдельного компонента */ }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;


namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для отрисовки тайлов и сетки.
    /// </summary>
    public class Draw_Tiles
    {
        private readonly int _tileSize;
        private readonly Texture2D _lineTexture;
        private readonly NpcTrackerMod modInstance;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Draw_Tiles"/>.
        /// </summary>
        /// <param name="instance">Экземпляр модификации.</param>
        /// <param name="tileSize">Размер тайла в пикселях.</param>
        /// <param name="lineTexture">Текстура линии для отрисовки.</param>
        public Draw_Tiles(NpcTrackerMod instance, int tileSize, Texture2D lineTexture)
        {
            _tileSize = tileSize;
            modInstance = instance;
            _lineTexture = lineTexture;
        }

        /// <summary>
        /// Отрисовывает сетку на экране.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр <see cref="SpriteBatch"/> для отрисовки.</param>
        /// <param name="cameraOffset">Смещение камеры.</param>
        public void DrawGrid(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            var location = Game1.currentLocation;
            var mapLayer = location.Map.Layers[0];

            for (int x = 0; x < mapLayer.LayerWidth; x++)
            {
                for (int y = 0; y < mapLayer.LayerHeight; y++)
                {
                    Vector2 tilePosition = new Vector2(x * _tileSize, y * _tileSize) - cameraOffset;
                    DrawTileHighlight(spriteBatch, tilePosition, Color.Black);
                }
            }
        }

        /// <summary>
        /// Отрисовывает выделение тайла.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр <see cref="SpriteBatch"/> для отрисовки.</param>
        /// <param name="tilePosition">Позиция тайла на экране.</param>
        /// <param name="color">Цвет линии.</param>
        public void DrawTileHighlight(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, _tileSize, _tileSize);

            spriteBatch.Draw(_lineTexture, tileRect, new Color(color, 0.05f));
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color);
        }

        /// <summary>
        /// Востанавливает исходный цвет тайла.
        /// </summary>
        /// <param name="tile">Координаты тайла.</param>
        public void RestoreTileColor(Point tile)
        {
            modInstance.npcTemporaryColors.Remove(tile);
        }

        /// <summary>
        /// Отмечает тайл для отрисовки с указанным приоритетом.
        /// </summary>
        /// <param name="tile">Координаты тайла.</param>
        /// <param name="color">Цвет отрисовки.</param>
        /// <param name="priority">Приоритет отрисовки.</param>
        public void DrawTileWithPriority(Point tile, Color color, int priority)
        {
            if (!modInstance.tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
            {
                modInstance.tileStates[tile] = (currentState.originalColor, color, priority);
            }
        }

        /// <summary>
        /// Отмечает тайл для отрисовки под NPC с указанным приоритетом.
        /// </summary>
        /// <param name="tile">Координаты тайла.</param>
        /// <param name="color">Цвет отрисовки.</param>
        /// <param name="priority">Приоритет отрисовки.</param>
        public void DrawTileForNpcMovement(Point tile, Color color, int priority)
        {
            modInstance.npcTemporaryColors[tile] = color;
            DrawTileWithPriority(tile, color, priority);
        }
    }
}


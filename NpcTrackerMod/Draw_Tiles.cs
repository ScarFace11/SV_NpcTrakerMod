using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using System.Collections.Generic;

namespace NpcTrackerMod
{
    /// <summary>
    /// Класс для отрисовки тайлов и сетки.
    /// </summary>
    public class Draw_Tiles
    {
        /// <summary> Словарь для хранения состояния плиток. </summary>
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();

        /// <summary> Словарь для временных цветов NPC. </summary>
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        /// <summary> Размер плитки. </summary>
        public readonly int tileSize = Game1.tileSize;

        /// <summary> Текстура линии для отрисовки. </summary>
        private readonly Texture2D _lineTexture;

        private readonly _modInstance modInstance;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Draw_Tiles"/>.
        /// </summary>
        /// <param name="instance">Экземпляр модификации.</param>
        /// <param name="tileSize">Размер тайла в пикселях.</param>
        /// <param name="lineTexture">Текстура линии для отрисовки.</param>
        public Draw_Tiles(_modInstance instance)
        {
            modInstance = instance;
            _lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);
        }

        /// <summary>
        /// Создает текстуру линии для отрисовки.
        /// </summary>
        /// <param name="graphicsDevice">Графическое устройство.</param>
        /// <returns>Созданная текстура.</returns>
        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
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
                    Vector2 tilePosition = new Vector2(x * tileSize, y * tileSize) - cameraOffset;
                    DrawGridTile(spriteBatch, tilePosition, Color.Black);
                }
            }
        }

        /// <summary>
        /// Рисует только сетку (границы тайлов).
        /// </summary>
        /// <param name="spriteBatch">Экземпляр <see cref="SpriteBatch"/> для отрисовки.</param>
        /// <param name="tilePosition">Позиция тайла.</param>
        /// <param name="color">Цвет линии сетки.</param>
        private void DrawGridTile(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);

            // Рисуем только границы
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color); // Верхняя линия
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color); // Нижняя линия
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color); // Левая линия
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color); // Правая линия
        }

        /// <summary>
        /// Отрисовывает все тайлы.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр <see cref="SpriteBatch"/> для отрисовки.</param>
        /// <param name="cameraOffset">Смещение камеры.</param>
        public void DrawTiles(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.TryGetValue(tile.Key, out var tempColor) ? tempColor : tile.Value.currentColor;

                DrawTileFill(spriteBatch, tilePosition, color); // Рисуем только внутреннюю часть тайла
            }
        }

        /// <summary>
        /// Отрисовывает только внутреннюю часть тайла.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр <see cref="SpriteBatch"/> для отрисовки.</param>
        /// <param name="tilePosition">Позиция тайла.</param>
        /// <param name="color">Цвет заливки тайла.</param>
        private void DrawTileFill(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);

            // Рисуем только заливку тайла
            spriteBatch.Draw(_lineTexture, tileRect, new Color(color, 0.05f));
        }

        /// <summary>
        /// Востанавливает исходный цвет тайла.
        /// </summary>
        /// <param name="tile">Координаты тайла.</param>
        public void RestoreTileColor(Point tile)
        {
            npcTemporaryColors.Remove(tile);
        }

        /// <summary>
        /// Отмечает тайл для отрисовки с указанным приоритетом.
        /// </summary>
        /// <param name="tile">Координаты тайла.</param>
        /// <param name="color">Цвет отрисовки.</param>
        /// <param name="priority">Приоритет отрисовки.</param>
        public void DrawTileWithPriority(Point tile, Color color, int priority)
        {
            if (!tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
            {
                tileStates[tile] = (currentState.originalColor, color, priority);
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
            npcTemporaryColors[tile] = color;
            DrawTileWithPriority(tile, color, priority);
        }
    }
}


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
        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates =
            new Dictionary<Point, (Color, Color, int)>();

        /// <summary> Словарь для временных цветов NPC. </summary>
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        /// <summary>
        /// Владельцы тайлов: тайл → (имя NPC, метка времени).
        /// Используется для всплывающих подсказок при наведении.
        /// </summary>
        public Dictionary<Point, (string npcName, string timeInfo)> tileOwners =
            new Dictionary<Point, (string, string)>();

        /// <summary> Размер плитки. </summary>
        public readonly int tileSize = Game1.tileSize;

        /// <summary> Текстура линии для отрисовки. </summary>
        private readonly Texture2D _lineTexture;

        private readonly _modInstance modInstance;

        public Draw_Tiles(_modInstance instance)
        {
            modInstance = instance;
            _lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);
        }

        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        /// <summary>
        /// Очищает tileStates и tileOwners.
        /// Используйте вместо прямого вызова tileStates.Clear().
        /// </summary>
        public void ClearTiles()
        {
            tileStates.Clear();
            tileOwners.Clear();
        }

        /// <summary>
        /// Регистрирует владельца тайла (для всплывающей подсказки при наведении).
        /// </summary>
        public void RegisterTileOwner(Point tile, string npcName, string timeInfo = null)
        {
            tileOwners[tile] = (npcName, timeInfo);
        }

        /// <summary>
        /// Отрисовывает сетку на экране.
        /// </summary>
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

        private void DrawGridTile(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color);
        }

        /// <summary>
        /// Отрисовывает все тайлы.
        /// </summary>
        public void DrawTiles(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            foreach (var tile in tileStates)
            {
                Vector2 tilePosition = new Vector2(tile.Key.X * tileSize, tile.Key.Y * tileSize) - cameraOffset;
                var color = npcTemporaryColors.TryGetValue(tile.Key, out var tempColor)
                    ? tempColor
                    : tile.Value.currentColor;
                DrawTileFill(spriteBatch, tilePosition, color);
            }
        }

        private void DrawTileFill(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);
            spriteBatch.Draw(_lineTexture, tileRect, new Color(color, 0.05f));
        }

        /// <summary>
        /// Восстанавливает исходный цвет тайла.
        /// </summary>
        public void RestoreTileColor(Point tile)
        {
            npcTemporaryColors.Remove(tile);
        }

        /// <summary>
        /// Отмечает тайл для отрисовки с указанным приоритетом.
        /// </summary>
        public void DrawTileWithPriority(Point tile, Color color, int priority)
        {
            if (!tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
                tileStates[tile] = (currentState.originalColor, color, priority);
        }

        /// <summary>
        /// Отмечает тайл для отрисовки под NPC с указанным приоритетом.
        /// </summary>
        public void DrawTileForNpcMovement(Point tile, Color color, int priority)
        {
            npcTemporaryColors[tile] = color;
            DrawTileWithPriority(tile, color, priority);
        }
    }
}
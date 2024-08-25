using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;


namespace NpcTrackerMod
{
    public class Draw_Tiles
    {
        private NpcTrackerMod modInstance;
        private readonly int _tileSize;
        private readonly Texture2D _lineTexture;

        public Draw_Tiles(NpcTrackerMod instance, int tileSize, Texture2D lineTexture)
        {
            this.modInstance = instance;
            _tileSize = tileSize;
            _lineTexture = lineTexture;
        }

        // Отрисовка Grid сетки
        public void DrawGrid(SpriteBatch spriteBatch, Vector2 cameraOffset)
        {
            var location = Game1.currentLocation;

            for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
                {
                    Vector2 tilePosition = new Vector2(x * _tileSize, y * _tileSize) - cameraOffset;
                    DrawTileHighlight(spriteBatch, tilePosition, Color.Black);
                }
            }
        }

        // Отрисовка квадрата
        public void DrawTileHighlight(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, _tileSize, _tileSize);

            spriteBatch.Draw(_lineTexture, tileRect, new Color(color, 0.05f));
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color);
            spriteBatch.Draw(_lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color);
        }
        public void RestoreTileColor(Point tile) // Востановление цвета (хз)
        {
            modInstance.npcTemporaryColors.Remove(tile);
        }
        public void DrawTileWithPriority(Point tile, Color color, int priority) // выставление приоритета на отображение тайлов
        {
            if (!modInstance.tileStates.TryGetValue(tile, out var currentState) || currentState.priority < priority)
            {
                modInstance.tileStates[tile] = (currentState.originalColor, color, priority);
            }
        }

        public void DrawTileForNpcMovement(Point tile, Color color, int priority) // Рисовка тайла под нпс
        {
            modInstance.npcTemporaryColors[tile] = color;
            DrawTileWithPriority(tile, color, priority);
        }
    }

}

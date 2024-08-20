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
        private int tileSize;
        private Texture2D lineTexture;

        public Draw_Tiles(int tileSize, Texture2D lineTexture)
        {
            this.tileSize = tileSize;
            this.lineTexture = lineTexture;
        }

        public void DrawGrid(SpriteBatch spriteBatch, Vector2 cameraOffset) // отрисовка сетки. исправить изменение разрешения при изменении размера интерфейса
        {
            var location = Game1.currentLocation;

            for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
                {
                    Vector2 tilePosition = new Vector2(x * tileSize, y * tileSize) - cameraOffset;

                    DrawTileHighlight(spriteBatch, tilePosition, Color.Black);
                }
            }
        }

        public void DrawTileHighlight(SpriteBatch spriteBatch, Vector2 tilePosition, Color color)
        {
            Rectangle tileRect = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, tileSize, tileSize);

            spriteBatch.Draw(lineTexture, tileRect, new Color(color, 0.05f));
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, tileRect.Width, 1), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Bottom - 1, tileRect.Width, 1), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Left, tileRect.Top, 1, tileRect.Height), color);
            spriteBatch.Draw(lineTexture, new Rectangle(tileRect.Right - 1, tileRect.Top, 1, tileRect.Height), color);
        }
    }

}

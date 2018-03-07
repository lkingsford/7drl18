using Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoGame.Extended.NuclexGui;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.NuclexGui.Controls.Desktop;
using Microsoft.Xna.Framework.Graphics;

namespace Desktop
{
    class Minimap : State
    {
        public Minimap(Game.Game Game, Microsoft.Xna.Framework.Game MonogameGame)
        {
            this.G = Game;
            MinimapTileSprites = AppContentManager.Load<Texture2D>("MinimapTiles");
        }

        Texture2D MinimapTileSprites;
        private Game.Game G;

        public override void Draw(GameTime GameTime)
        {
            AppSpriteBatch.Begin();
            var tileWidth = 13;
            var tileHeight = 13;

            var fullMapWidth = G.Metamap.GetLength(0) * tileWidth * 4;
            var fullMapHeight = G.Metamap.GetLength(1) * tileHeight * 4;

            var drawLeft = 100;
            var drawTop = 100;

            for(var ix = 0; ix < G.Metamap.GetLength(0); ++ix)
            {
                for(var iy = 0; iy < G.Metamap.GetLength(1); ++iy)
                {
                    int srcX = 0;
                    int srcY = 0;
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MajorRoadNorth))
                    {
                        srcX = 0;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MajorRoadSouth))
                    {
                        srcX = 1;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MajorRoadEast))
                    {
                        srcX = 2;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MajorRoadWest))
                    {
                        srcX = 3;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    srcY = 1;
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MinorRoadNorth))
                    {
                        srcX = 0;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MinorRoadSouth))
                    {
                        srcX = 1;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MinorRoadEast))
                    {
                        srcX = 2;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.MinorRoadWest))
                    {
                        srcX = 3;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    srcY = 2;
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.PathNorth))
                    {
                        srcX = 0;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.PathSouth))
                    {
                        srcX = 1;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.PathEast))
                    {
                        srcX = 2;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                    if (G.Metamap[ix, iy].Contains(Game.Game.MetamapTile.PathWest))
                    {
                        srcX = 3;
                        AppSpriteBatch.Draw(MinimapTileSprites,
                            new Rectangle(drawLeft + ix * tileWidth, drawTop + iy * tileHeight, tileWidth, tileHeight),
                            new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                            Color.White);
                    }
                }
            }

            AppSpriteBatch.End();
        }
    }
}

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
            CrimeRateSprite = AppContentManager.Load<Texture2D>("CrimeRate");
        }

        Texture2D MinimapTileSprites;
        private Texture2D CrimeRateSprite;
        private Game.Game G;
        public bool showCrime = false;

        public override void Draw(GameTime GameTime)
        {
            AppSpriteBatch.Begin();
            DrawMinimap(new XY(100, 100));
            AppSpriteBatch.End();
        }

        public void DrawMinimap(XY offset)
        {
            var tileWidth = 13;
            var tileHeight = 13;

            var fullMapWidth = G.Metamap.GetLength(0) * tileWidth * 4;
            var fullMapHeight = G.Metamap.GetLength(1) * tileHeight * 4;

            var drawLeft = offset.X;
            var drawTop = offset.Y;

            for (var ix = 0; ix < G.Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < G.Metamap.GetLength(1); ++iy)
                {
                    int srcX = 0;
                    int srcY = 2;
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
                    srcY = 0;
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
                }
            }

            if (showCrime)
            {
                for (var ix = 0; ix < G.Metamap.GetLength(0); ++ix)
                {
                    for (var iy = 0; iy < G.Metamap.GetLength(1); ++iy)
                    {
                        var enemies = G.Actors.Where(i => (i.Location.X / G.MetaTileWidth == ix) && (i.Location.Y / G.MetaTileHeight == iy)).Count();
                        var crimeLevel = enemies == 0 ? 0 : Math.Min(enemies / 6, 4) + 1;
                        AppSpriteBatch.Draw(CrimeRateSprite,
                            new Rectangle(drawLeft + (ix) * tileWidth,
                              drawTop + (iy) * tileHeight,
                              tileWidth, tileHeight),
                        new Rectangle(crimeLevel * tileWidth, 0, tileWidth, tileHeight),
                        Color.White);
                    }
                }
            }

            if (G.Player != null)
            {
                int srcY = 3;
                int srcX = 0;
                AppSpriteBatch.Draw(MinimapTileSprites,
                    new Rectangle(drawLeft + (G.Player.Location.X / G.MetaTileWidth) * tileWidth,
                                  drawTop + (G.Player.Location.Y / G.MetaTileHeight) * tileHeight, 
                                  tileWidth, tileHeight),
                    new Rectangle(srcX * tileWidth, srcY * tileHeight, tileWidth, tileHeight),
                    Color.White);
            }
        }
    }
}
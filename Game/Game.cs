using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    /// <summary>
    /// The Game itself. 
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Game(ContentManager content)
        {
            Content = content;

            var map = Content.Load<TiledMap>("Maps/Area1");

            GlobalMap = new MapTile[map.Width, map.Height];

            for (int ix = 0; ix < MapWidth; ++ix)
            {
                for (int iy = 0; iy < MapHeight; ++iy)
                {
                    var tileId = map.TileLayers[0].Tiles[ix + iy * MapWidth].GlobalIdentifier;
                    string walkableValue = null;

                    var tiledTileset = map.GetTilesetByTileGlobalIdentifier(tileId);
                    var tiledTile = tiledTileset.Tiles[tileId - tiledTileset.FirstGlobalIdentifier];
                    tiledTile?.Properties.TryGetValue("walkable", out walkableValue);
                    var walkable = walkableValue != null ? walkableValue == "true" : false;

                    GlobalMap[ix, iy] = new MapTile(walkable, tiledTile.LocalTileIdentifier);
                }
            }
        }

        private ContentManager Content;

        ///// <summary>
        ///// Active map is just the parts of the map that are currently active.
        ///// May not be what's visible
        ///// </summary>
        //public MapTile[,] ActiveMap
        //{

        //}

        /// <summary>
        /// Whole map
        /// </summary>
        public MapTile[,] GlobalMap;

        /// <summary>
        /// Map that is currently visible (duh)
        /// </summary>
        public MapTile[,] VisibleMap
        {
            get
            {
                var cameraX = CameraTopLeft.X;
                if ((cameraX + CameraWidth) > MapWidth)
                {
                    cameraX = MapWidth - CameraWidth;
                }
                cameraX = Math.Max(0, cameraX);

                var cameraY = CameraTopLeft.Y;
                if ((cameraY + CameraHeight) > MapHeight)
                {
                    cameraY = MapHeight - CameraHeight;
                }

                var result = new MapTile[CameraWidth, CameraHeight];
                for (int ix = 0; ix < CameraWidth; ++ix)
                {
                    for (int iy = 0; iy < CameraHeight; ++iy)
                    {
                        if ((ix + cameraX <= MapWidth) && (iy + cameraY <= MapHeight))
                        {
                            result[ix, iy] = GlobalMap[ix + cameraX, iy + cameraY];
                        }
                        else
                        {
                            result[ix, iy] = new MapTile(false, 0);
                        }
                    }
                }

                return result;
            }
        }

        private int MapWidth
        {
            get
            {
                return GlobalMap.GetLength(0);
            }
        }

        private int MapHeight
        {
            get
            {
                return GlobalMap.GetLength(1);
            }
        }

        public int CameraWidth = 9;
        public int CameraHeight = 9;

        public XY CameraTopLeft
        {
            get
            {
                return new XY(0, 0);
            }
        }

        public XY ActiveTopLeft
        {
            get
            {
                return new XY(0, 0);
            }
        }

        public Player Player;
    }
}

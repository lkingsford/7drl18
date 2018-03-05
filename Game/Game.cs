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

            // Tiled stores objects with pixels - hence, needing to do the math here
            foreach (var o in map.GetLayer<TiledMapObjectLayer>("spawn").Objects)
            {
                var tileX = (int)o.Position.X / map.TileWidth;
                var tileY = (int)o.Position.Y / map.TileHeight;

                Actor actor;

                switch (o.Name)
                {
                    case "pc":
                        actor = new Player(GlobalMap);
                        Player = (Player)actor;
                        break;
                    case "bad":
                        actor = new Actor(GlobalMap);
                        actor.Sprite = 1;
                        break;
                    case "knife":
                        actor = new Actor(GlobalMap);
                        actor.Sprite = 2;
                        break;
                    default:
                        // Bad.
                        // TODO: Log
                        continue;
                }

                actor.Location = new XY(tileX, tileY);
                Actors.Add(actor);
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

        /// <summary>
        /// Currently visible actors. Positions are still absolute./
        /// </summary>
        public IReadOnlyList<Actor> VisibleActors
        {
            get
            {
                return Actors.Where(i => 
                    i.Location.ContainedBy(CameraTopLeft.X, CameraTopLeft.Y,
                                           CameraTopLeft.X + CameraWidth - 1, CameraTopLeft.Y + CameraHeight - 1)).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Actors in game 
        /// </summary>
        public List<Actor> Actors = new List<Actor>();

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
                var centerPlayerCamera = Player.Location + new XY(-CameraWidth / 2, -CameraWidth / 2);
                var minBoundedCamera = new XY(Math.Max(0, centerPlayerCamera.X), Math.Max(0, centerPlayerCamera.Y));
                var maxBoundedCamera = new XY(Math.Min(MapWidth - CameraWidth, minBoundedCamera.X), Math.Min(MapHeight - CameraHeight, minBoundedCamera.Y));
                return maxBoundedCamera;
            }
        }

        /// <summary>
        /// Do the next turn
        /// </summary>
        internal void NextTurn()
        {
            foreach (var actor in VisibleActors)
            {
                actor.DoTurn();
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

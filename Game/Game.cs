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

                    tiledTileset = map.GetTilesetByTileGlobalIdentifier(tileId);
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
                        actor = new Player(GlobalMap, this);
                        Player = (Player)actor;
                        break;
                    case "bad":
                        actor = new Enemy(GlobalMap, this);
                        actor.Sprite = 1;
                        break;
                    case "knife":
                        actor = new Enemy(GlobalMap, this);
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

            // Generate metamap
            Metamap = new HashSet<MetamapTile>[32, 32];
            for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                {
                    Metamap[ix, iy] = new HashSet<MetamapTile>();
                }
            }

                    for (var ix = 0; ix <= 13; ++ix)
            {
                Metamap[ix, 12].Add(MetamapTile.MajorRoadEast);
                Metamap[ix, 12].Add(MetamapTile.MajorRoadWest);
            }

            for (var iy = 0; iy <= 13; ++iy)
            {
                Metamap[12, iy].Add(MetamapTile.PathNorth);
                Metamap[12, iy].Add(MetamapTile.PathSouth);
                Metamap[2, iy].Add(MetamapTile.MinorRoadNorth);
                Metamap[2, iy].Add(MetamapTile.MinorRoadSouth);
            }
        }

        private ContentManager Content;

        public enum TurnPhases { Player, Enemy };

        public TurnPhases CurrentPhase = TurnPhases.Player;

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

        public enum MetamapTile { MajorRoadNorth, MajorRoadEast, MajorRoadWest, MajorRoadSouth, MinorRoadNorth, MinorRoadEast, MinorRoadWest, MinorRoadSouth, PathNorth, PathEast, PathWest, PathSouth };

        /// <summary>
        /// The 'meta' map - the one which each tile becomes a prefab
        /// </summary>
        /// <remarks>It's neatish... but I wonder is it fast?</remarks>
        public HashSet<MetamapTile>[,] Metamap;

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
            switch (CurrentPhase)
            {
                case TurnPhases.Player:
                    Player.DoTurn();
                    CurrentPhase = TurnPhases.Enemy;

                    // Want to plan enemy actors before enemy turn
                    foreach (var actor in VisibleActors)
                    {
                        if (actor != Player && !actor.Stunned)
                        {
                            (actor as Enemy).WhatYouWannaDo();
                        }
                    }

                    // If no enemies in sight, do their phase and go
                    // straight back to the player one
                    if (!VisibleActors.Any(i=>(i is Enemy)))
                    {
                        NextTurn();
                    }
                    break;

                case TurnPhases.Enemy:
                    // Player can spend momentum to dodge or parry
                    Player.DoTurn();

                    foreach (var actor in VisibleActors)
                    {
                        if (actor != Player)
                        {
                            actor.DoTurn();
                        }
                    }
                    CurrentPhase = TurnPhases.Player;
                    break;
            }

            // Remove dead actors
            Actors.RemoveAll(i => i.HP <= 0);
        }

        public XY ActiveTopLeft
        {
            get
            {
                return new XY(0, 0);
            }
        }

        public TiledMapTileset tiledTileset { get; private set; }

        public Player Player;
    }
}

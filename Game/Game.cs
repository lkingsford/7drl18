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

            GlobalMap = new MapTile[MetaTileWidth * MetaGlobalWidth, MetaTileHeight * MetaGlobalHeight];

            for (int ix = 0; ix < MapWidth; ++ix)
            {
                for (int iy = 0; iy < MapHeight; ++iy)
                {
                    var tileId = 0;//map.TileLayers[0].Tiles[ix + iy * MapWidth].GlobalIdentifier;
                    //string walkableValue = null;

                    //tiledTileset = map.GetTilesetByTileGlobalIdentifier(tileId);
                    tiledTileset = map.Tilesets[0];
                    //var tiledTile = tiledTileset.Tiles[tileId - tiledTileset.FirstGlobalIdentifier];
                    //tiledTile?.Properties.TryGetValue("walkable", out walkableValue);
                    //var walkable = walkableValue != null ? walkableValue == "true" : false;
                    var walkable = true;

                    GlobalMap[ix, iy] = new MapTile(walkable, 0);
                }
            }

            //// Tiled stores objects with pixels - hence, needing to do the math here
            //foreach (var o in map.GetLayer<TiledMapObjectLayer>("spawn").Objects)
            //{
            //    var tileX = (int)o.Position.X / map.TileWidth;
            //    var tileY = (int)o.Position.Y / map.TileHeight;

            //    Actor actor;

            //    switch (o.Name)
            //    {
            //        case "pc":
            //            actor = new Player(GlobalMap, this);
            //            Player = (Player)actor;
            //            break;
            //        case "bad":
            //            actor = new Enemy(GlobalMap, this);
            //            actor.Sprite = 1;
            //            break;
            //        case "knife":
            //            actor = new Enemy(GlobalMap, this);
            //            actor.Sprite = 2;
            //            break;
            //        default:
            //            // Bad.
            //            // TODO: Log
            //            continue;
            //    }

            //    actor.Location = new XY(tileX, tileY);
            //    Actors.Add(actor);
            //}

            Player =new Player(GlobalMap, this);
            Actors.Add(Player);

            // Generate metamap

            GenerateMetamap();
            GenerateMap();
        }

        private int MetaTileWidth = 15;
        private int MetaTileHeight = 15;
        private int MetaGlobalWidth = 32;
        private int MetaGlobalHeight = 32;

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

        // Don't mess with order - playing nasty casting tricks
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

        /// <summary>
        /// Generate the map to base prefabs off
        /// </summary>
        private void GenerateMetamap()
        {
            Metamap = new HashSet<MetamapTile>[MetaGlobalWidth, MetaGlobalHeight];
            for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                {
                    Metamap[ix, iy] = new HashSet<MetamapTile>();
                }
            }


            var middle = new XY(Metamap.GetLength(0) / 2, Metamap.GetLength(1) / 2);
            Road(middle, new XY(1, 0), 0, 2, 0.9);
            while (MetamapOccupied() < 80)
            {
                var canDoList = new List<XY>();
                for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
                {
                    for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                    {
                        if (Metamap[ix, iy].Count > 0)
                        {
                            canDoList.Add(new XY(ix, iy));
                        }
                    }
                }
                if (canDoList.Count > 0)
                {
                    var goFrom = canDoList.RandomItem();
                    var goFromTile = Metamap[goFrom.X, goFrom.Y];

                    if (!(goFromTile.Contains(MetamapTile.MajorRoadEast) || goFromTile.Contains(MetamapTile.MinorRoadEast) || goFromTile.Contains(MetamapTile.PathEast)))
                    {
                        Road(canDoList.RandomItem(), new XY(1, 0), 0, 2, 0.8, true);
                    }

                    if (!(goFromTile.Contains(MetamapTile.MajorRoadWest) || goFromTile.Contains(MetamapTile.MinorRoadWest) || goFromTile.Contains(MetamapTile.PathWest)))
                    {
                        Road(canDoList.RandomItem(), new XY(-1, 0), 0, 2, 0.8, true);
                    }

                    if (!(goFromTile.Contains(MetamapTile.MajorRoadNorth) || goFromTile.Contains(MetamapTile.MinorRoadNorth) || goFromTile.Contains(MetamapTile.PathNorth)))
                    {
                        Road(canDoList.RandomItem(), new XY(0, -1), 0, 2, 0.8, true);
                    }

                    if (!(goFromTile.Contains(MetamapTile.MajorRoadSouth) || goFromTile.Contains(MetamapTile.MinorRoadSouth) || goFromTile.Contains(MetamapTile.PathSouth)))
                    {
                        Road(canDoList.RandomItem(), new XY(0, 1), 0, 2, 0.8, true);
                    }
                }
                else
                {
                    GenerateMetamap();
                }
            }
        }

        private int MetamapOccupied()
        {
            int count = 0;
            for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                {
                    if (Metamap[ix, iy].Count > 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="dxdy">Desired direction</param>
        /// <param name="maxMajor">Roads can spawn same or smaller</param>
        /// <param name="startRoadChance"></param>
        private void Road(XY thisPoint, XY dxdy, int windy, int majorness, double keepGoingChance = 0.8, bool forceGo = false)
        {
            if (forceGo || keepGoingChance > GlobalRandom.NextDouble())
            {
                if (windy != 1)
                {
                    dxdy += new XY(GlobalRandom.Next(2) - 1, GlobalRandom.Next(2) - 1);
                    dxdy = dxdy.Unit();
                }

                if (windy == 2) windy = 1;

                if (dxdy.X != 0 && dxdy.Y != 0)
                {
                    if (GlobalRandom.Next(2) == 0)
                    {
                        dxdy = new XY(dxdy.X, 0);
                    }
                    else
                    {
                        dxdy = new XY(0, dxdy.Y);
                    }
                }

                var n = thisPoint + dxdy;

                if (n.X < 0 || n.Y < 0 || n.X >= Metamap.GetLength(0) || n.Y >= Metamap.GetLength(1))
                {
                    return;
                }

                int nextDirection = 0;
                int thisDirection = 0;

                if (dxdy.X == -1)
                {
                    nextDirection = 2;
                    thisDirection = 1;
                }
                else if (dxdy.X == 1)
                {
                    nextDirection = 1;
                    thisDirection = 2;
                }
                else if (dxdy.Y == -1)
                {
                    nextDirection = 3;
                    thisDirection = 0;
                }
                else if (dxdy.Y == 1)
                {
                    nextDirection = 0;
                    thisDirection = 3;
                }

                Metamap[thisPoint.X, thisPoint.Y].Add((MetamapTile)(thisDirection + (4 * (2 - majorness))));
                Metamap[n.X, n.Y].Add((MetamapTile)(nextDirection + (4 * (2 - majorness))));
                Road(n, dxdy, windy, majorness, keepGoingChance);
                
                double newRoadChance = (double)majorness / 6.0;

                if (GlobalRandom.NextDouble() < newRoadChance)
                {
                    XY nextDir;
                    if (dxdy.X != 0)
                    {
                        if (GlobalRandom.Next(2) == 0)
                        {
                            nextDir = new XY(0, -1);
                        }
                        else
                        {
                            nextDir = new XY(0, 1);
                        }
                    }
                    else
                    {
                        if (GlobalRandom.Next(2) == 0)
                        {
                            nextDir = new XY(-1, 0);
                        }
                        else
                        {
                            nextDir = new XY(1, 0);
                        }
                    }
                    int nextWindy = GlobalRandom.Next(4) == 0 ? 2 : 0;
                    int nextMajorness = GlobalRandom.Next(majorness + 1);

                    Road(thisPoint, nextDir, nextWindy, nextMajorness, keepGoingChance, true);
                }
            }
        }


        /// <summary>
        /// Make a tile for a tile
        /// </summary>
        /// <param name="tile"></param>
        private void MakeTile(XY xy, HashSet<MetamapTile> tile)
        {
            var thisTileOrigin = new XY(xy.X * MetaTileWidth, xy.Y * MetaTileHeight);

            foreach(var toMake in tile)
            {
                var roadType = (int)toMake / 4;
                var roadDirection = (int)toMake % 4;
                switch (roadDirection)
                {
                    case 0:
                        {
                            // North
                            int x = thisTileOrigin.X + MetaTileWidth / 2;
                            for (int y = thisTileOrigin.Y + MetaTileHeight / 2; y >= thisTileOrigin.Y; --y)
                            {
                                Brush(new XY(x, y), roadType, false);
                            }
                            break;
                        }
                    case 1:
                        // East
                        {
                            int y = thisTileOrigin.Y + MetaTileHeight / 2;
                            for (int x = thisTileOrigin.X + MetaTileWidth / 2; x <= thisTileOrigin.X + MetaTileWidth; ++x)
                            {
                                Brush(new XY(x, y), roadType, false);
                            }
                            break;
                        }
                    case 2:
                        // West
                        {
                            int y = thisTileOrigin.Y + MetaTileHeight / 2;
                            for (int x = thisTileOrigin.X + MetaTileWidth / 2; x >= thisTileOrigin.X; --x)
                            {
                                Brush(new XY(x, y), roadType, false);
                            }
                            break;
                        }
                    case 3:
                            // South
                        {
                            int x = thisTileOrigin.X + MetaTileWidth / 2;
                            for (int y = thisTileOrigin.Y + MetaTileHeight / 2; y <= thisTileOrigin.Y + MetaTileHeight; ++y)
                            {
                                Brush(new XY(x, y), roadType, false);
                            }
                            break;
                        }
                }
            }

            GlobalMap[thisTileOrigin.X, thisTileOrigin.Y] = new MapTile(false, 6);
        }

        private void Brush(XY xy, int tile, bool horiz)
        {
            switch (tile)
            {
                case 0:
                    BrushSet(1, xyRect(xy - new XY(4, 4), xy + new XY(4, 4)));
                    BrushSet(2, xyRect(xy - new XY(2, 2), xy + new XY(2, 2)));
                    break;
                case 1:
                    BrushSet(1, xyRect(xy - new XY(2, 2), xy + new XY(2, 2)));
                    BrushSet(2, xyRect(xy - new XY(1, 1), xy + new XY(1, 1)));
                    break;
                case 2:
                    BrushSet(1, xyRect(xy - new XY(1, 1), xy + new XY(1, 1)));
                    break;
            }

            Player.Location = xy;
        }

        private void BrushSet(int tile, List<XY> todraw)
        {
            foreach(var xy in todraw)
                if (xy.ContainedBy(0, 0, MapWidth - 1, MapHeight - 1))
                {
                    if (tile > GlobalMap[xy.X, xy.Y].DrawTile)
                        GlobalMap[xy.X, xy.Y] = new MapTile(true, tile);
                }
        }

        private List<XY> xyRect(XY topLeft, XY bottomRight)
        {
            var result = new List<XY>();
            for (int ix = topLeft.X; ix <= bottomRight.X; ++ix)
            {
                for (int iy = topLeft.Y; iy <= bottomRight.Y; ++iy)
                {
                    result.Add(new XY(ix, iy));
                }
            }
            return result;
        }

        private void GenerateMap()
        {
            for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                {
                    MakeTile(new XY(ix, iy), Metamap[ix, iy]);
                }
            }
        }
    }
}

using Microsoft.Xna.Framework;
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
        public delegate void ReportProgressDelegate(int percentage, string message);
        public delegate void TellStoryDelegate(params string[] message);

        /// <summary>
        /// Default constructor
        /// </summary>
        public Game(ContentManager content, ReportProgressDelegate reporter = null)
        {
            ProgressReporter = reporter;

            ProgressReporter?.Invoke(0, "Loading Prefabs...");

            Content = content;

            var newMap = Content.Load<TiledMap>($"Maps/Prefabs");

            // Load tilesets
            foreach (var tileset in newMap.Tilesets)
            {
                Tilesets.Add(tileset);
            }

            foreach (var i in Tilesets)
            {
                int? anyLid = i.Tiles.FirstOrDefault(j => j.Properties.ContainsKey("name") && j.Properties["name"] == "BaseAny")?.LocalTileIdentifier;
                if (anyLid != null)
                    BaseAnyTile = i.FirstGlobalIdentifier + anyLid.Value;
                int? roadLid = i.Tiles.FirstOrDefault(j => j.Properties.ContainsKey("name") && j.Properties["name"] == "BaseRoad")?.LocalTileIdentifier;
                if (roadLid != null)
                    BaseRoadTile = i.FirstGlobalIdentifier + roadLid.Value;
                int? sidewalkLid = i.Tiles.FirstOrDefault(j => j.Properties.ContainsKey("name") && j.Properties["name"] == "BaseSidewalk")?.LocalTileIdentifier;
                if (sidewalkLid != null)
                    BaseSidewalkTile = i.FirstGlobalIdentifier + sidewalkLid.Value;
                int? wallLid = i.Tiles.FirstOrDefault(j => j.Properties.ContainsKey("name") && j.Properties["name"] == "BaseWall")?.LocalTileIdentifier;
                if (wallLid != null)
                    BaseWallTile = i.FirstGlobalIdentifier + wallLid.Value;

                foreach (var j in i.Tiles) TileByGid.Add(j.LocalTileIdentifier + i.FirstGlobalIdentifier, j);
            }

            PrefabMap = newMap;

            GlobalMap = new MapTile[MetaTileWidth * MetaGlobalWidth, MetaTileHeight * MetaGlobalHeight];

            for (int ix = 0; ix < MapWidth; ++ix)
            {
                for (int iy = 0; iy < MapHeight; ++iy)
                {
                    var walkable = true;

                    GlobalMap[ix, iy] = new MapTile(Tilesets, BaseAnyTile);
                }
            }

            Player = new Player(GlobalMap, this);
            Actors.Add(Player);
            // Generate metamap

            ProgressReporter?.Invoke(10, "Building road network...");
            GenerateMetamap();
            PlayerStart = new XY(3 + MetaTileWidth * Metamap.GetLength(0) / 2, 3 + MetaTileHeight * Metamap.GetLength(1) / 2);
            ProgressReporter?.Invoke(20, "Generating map...");
            GenerateMap();
            ProgressReporter?.Invoke(90, "Placing mobs...");
            GenerateMobs();
            Player.Location = PlayerStart;
        }

        private XY PlayerStart;

        private ReportProgressDelegate ProgressReporter = null;
        public TellStoryDelegate TellStory = null;
        private TiledMap PrefabMap;
        private List<Rectangle> Prefabs = new List<Rectangle>();
        public List<TiledMapTileset> Tilesets = new List<TiledMapTileset>();
        public Dictionary<int, TiledMapTilesetTile> TileByGid = new Dictionary<int, TiledMapTilesetTile>();

        private int BaseAnyTile;
        private int BaseRoadTile;
        private int BaseSidewalkTile;
        private int BaseWallTile;

        public int MetaTileWidth { get; private set; } = 10;
        public int MetaTileHeight { get; private set; } = 10;
        private int MetaGlobalWidth = 18;
        private int MetaGlobalHeight = 18;

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
                            result[ix, iy] = new MapTile(Tilesets, BaseAnyTile);
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
                var result =  Actors.Where(i =>
                    i.Location.ContainedBy(CameraTopLeft.X, CameraTopLeft.Y,
                                           CameraTopLeft.X + CameraWidth - 1, CameraTopLeft.Y + CameraHeight - 1)).ToList().AsReadOnly();


                if (!zuluSeen && result.Any(i=>i is Zulu))
                {
                    TellStory("There's not even that much of you!", "Let's take this slow. Give the boys a show.", "I don't know who you are, I don't know what you did...", "... but you're going down.");
                    zuluSeen = true;
                }

                return result;
            }
        }

        bool zuluSeen = false;

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

        public bool NoVisible()
        {
            return !VisibleActors.Any(i => (i is Enemy));
        }

        public void StartGame()
        {
            TellStory(@"It is New London, 19X3.
The thick stench of the sewers almost hides a familiar sickly odour.",
@"The Government has established borders around George Street: where the
enigmatic ZULU has taken firm control of the streets",
@"The death toll is too high and too close. It's time to reclaim London.");
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
                    if (NoVisible())
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
            Hearse();
        }

        public int DeadKnifes { private set; get; } = 0;
        public int DeadBrutes { private set; get; } = 0;
        public int DeadLackeys { private set; get; } = 0;
        public bool DeadZulu { private set; get; } = false;

        private void Hearse()
        {
            foreach (var i in Actors.Where(i => i.HP <= 0))
            {
                if (i is Knife) DeadKnifes++;
                if (i is Brute) DeadBrutes++;
                if (i is Lackey) DeadLackeys++;
                if (i is Zulu) DeadZulu = true;
            }
            Actors.RemoveAll(i => i.HP <= 0);

            if (trigger10 && (DeadKnifes + DeadBrutes + DeadLackeys) >= 10)
            {
                trigger10 = false;
                TellStory("Huh. Somebody's fighting back in Sector C. I like it.");
            }
            if (trigger20 && (DeadKnifes + DeadBrutes + DeadLackeys) >= 18)
            {
                trigger20 = false;
                TellStory("Jack? Please dispatch some men to Sector B - this is getting too close.");
            }
            if (trigger30 && (DeadKnifes + DeadBrutes + DeadLackeys) >= 25)
            {
                trigger30 = false;
                TellStory("Dammit Jack. They're dead. How about we end this here?", "You hear a thundering explosion nearby", "No doubt you can see me on that fancy crime-wizz. Come get me.");
                GenerateZulu();
            } 
        }

        bool trigger10 = true;
        bool trigger20 = true;
        bool trigger30 = true;


        public XY ActiveTopLeft
        {
            get
            {
                return new XY(0, 0);
            }
        }

        public TiledMapTileset tiledTileset { get; private set; }
        public int MetaTileWidth1 { get => MetaTileWidth; set => MetaTileWidth = value; }
        public int MetaTileWidth2 { get => MetaTileWidth; set => MetaTileWidth = value; }

        public Player Player;

        #region Metamap generation

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
                if (windy == 1)
                {
                    do
                    {
                        dxdy += new XY(GlobalRandom.Next(2) - 1, GlobalRandom.Next(2) - 1);
                        dxdy = dxdy.Unit();
                    } while (dxdy == new XY(0, 0));
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
                else
                {
                    // Oops
                    throw new Exception();
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

        #endregion
        #region Macrogeneration

        /// <summary>
        /// Make a tile for a tile
        /// </summary>
        /// <param name="tile"></param>
        private void MakeTile(XY xy, HashSet<MetamapTile> tile)
        {
            var thisTileOrigin = new XY(xy.X * MetaTileWidth, xy.Y * MetaTileHeight);

            foreach (var toMake in tile)
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
                                Brush(new XY(x, y), roadType);
                            }
                            break;
                        }
                    case 1:
                        // East
                        {
                            int y = thisTileOrigin.Y + MetaTileHeight / 2;
                            for (int x = thisTileOrigin.X + MetaTileWidth / 2; x < thisTileOrigin.X + MetaTileWidth; ++x)
                            {
                                Brush(new XY(x, y), roadType);
                            }
                            break;
                        }
                    case 2:
                        // West
                        {
                            int y = thisTileOrigin.Y + MetaTileHeight / 2;
                            for (int x = thisTileOrigin.X + MetaTileWidth / 2; x >= thisTileOrigin.X; --x)
                            {
                                Brush(new XY(x, y), roadType);
                            }
                            break;
                        }
                    case 3:
                        // South
                        {
                            int x = thisTileOrigin.X + MetaTileWidth / 2;
                            for (int y = thisTileOrigin.Y + MetaTileHeight / 2; y < thisTileOrigin.Y + MetaTileHeight; ++y)
                            {
                                Brush(new XY(x, y), roadType);
                            }
                            break;
                        }
                }
            }
        }

        private void Brush(XY xy, int roadtype)
        {
            switch (roadtype)
            {
                case 0:
                    BrushSet(new MapTile(Tilesets, BaseSidewalkTile), xyRect(xy - new XY(4, 4), xy + new XY(4, 4)));
                    BrushSet(new MapTile(Tilesets, BaseRoadTile), xyRect(xy - new XY(2, 2), xy + new XY(2, 2)));
                    break;
                case 1:
                    BrushSet(new MapTile(Tilesets, BaseSidewalkTile), xyRect(xy - new XY(2, 2), xy + new XY(2, 2)));
                    BrushSet(new MapTile(Tilesets, BaseRoadTile), xyRect(xy - new XY(1, 1), xy + new XY(1, 1)));
                    break;
                case 2:
                    BrushSet(new MapTile(Tilesets, BaseSidewalkTile), xyRect(xy - new XY(1, 1), xy + new XY(1, 1)));
                    break;
            }
        }

        private void BrushSet(MapTile tile, List<XY> todraw)
        {
            foreach (var xy in todraw)
                if (xy.ContainedBy(0, 0, MapWidth - 1, MapHeight - 1))
                {
                    if (tile.BrushPriority >= GlobalMap[xy.X, xy.Y].BrushPriority)
                        GlobalMap[xy.X, xy.Y] = tile;
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
            ApplyPrefabs();
        }

        private void ApplyPrefabs()
        {
            // Get prefabs
            // Abandon all hope ye who enter here
            // This code is pretty rubbish
            var prefabRectangles = ((TiledMapObjectLayer)PrefabMap
                .GetLayer("MapRegion")).Objects
                .Where(i => (i is TiledMapRectangleObject))
                .Select(i => (new Rectangle((int)i.Position.X / PrefabMap.TileWidth, (int)i.Position.Y / PrefabMap.TileHeight,
                                            (int)i.Size.Width / PrefabMap.TileWidth, (int)i.Size.Height / PrefabMap.TileHeight)))
                .ToList();

            var lastToReplace = int.MaxValue;

            var startToReplace = CountMustReplace();

            while (AnyMustReplace() && prefabRectangles.Count > 0)
            {
                var currentPrefab = prefabRectangles.RandomItem();

                var toPlace = FindFits(currentPrefab);
                if (toPlace == null)
                {
                    prefabRectangles.Remove(currentPrefab);
                    continue;
                }

                var prefabLayer = PrefabMap.GetLayer<TiledMapTileLayer>("Tiles");
                for (var ix = 0; ix < currentPrefab.Width; ++ix)
                {
                    for (var iy = 0; iy < currentPrefab.Height; ++iy)
                    {
                        var i = new XY(ix, iy);
                        var g = toPlace + i;
                        var p = new XY(currentPrefab.X, currentPrefab.Y) + i;
                        GlobalMap[g.X, g.Y] = new MapTile(Tilesets, prefabLayer.Tiles[p.X + p.Y * prefabLayer.Width].GlobalIdentifier);
                    }
                }

                var thisToReplace = CountMustReplace(); 
                lastToReplace = thisToReplace;
                ProgressReporter?.Invoke(20 + (int)(70.0 * (float)startToReplace / (float)thisToReplace), $"Generating map ({thisToReplace} of {startToReplace} remaining)...");
            }
        }

        private bool AnyMustReplace()
        {
            for (int ix = 0; ix < MapWidth; ++ix)
                for (int iy = 0; iy < MapHeight; ++iy)
                    if (GlobalMap[ix, iy].MustReplace)
                        return true;
            return false;
        }


        private int CountMustReplace()
        {
            var i = 0;
            for (int ix = 0; ix < MapWidth; ++ix)
                for (int iy = 0; iy < MapHeight; ++iy)
                    if (GlobalMap[ix, iy].MustReplace)
                        ++i;
            return i;
        }

        private XY FindFits(Rectangle p)
        {
            var result = new List<XY>();

            var tested = 0;
            var ix = GlobalRandom.Next(MapWidth);
            var iy = GlobalRandom.Next(MapHeight);
            while (tested < MapWidth * MapHeight)
            {
                ++tested;
                if (ix++ >= MapWidth)
                {
                    ix = 0;
                    if (iy++ >= MapHeight)
                    {
                        iy = 0; 
                    }
                }
                if (Fits(new XY(ix, iy), p))
                    return (new XY(ix, iy));
            }

            return null;
        }

        private bool Fits(XY offset, Rectangle p)
        {
            var prefabLayer = PrefabMap.GetLayer<TiledMapTileLayer>("Tiles");
            for (int ix = 0; ix < p.Width; ++ix)
            {
                for (int iy = 0; iy < p.Height; ++iy)
                {
                    var globalXy = new XY(ix, iy) + offset;
                    if (!globalXy.ContainedBy(0, 0, MapWidth- 1, MapHeight - 1)) return false;
                    var prefabXy = new XY(ix, iy) + new XY(p.X, p.Y);
                    var prefabMapTile = prefabLayer .Tiles[prefabXy.X + prefabXy.Y * prefabLayer.Width];
                    var prefabTile = TileByGid[prefabMapTile.GlobalIdentifier];
                    var curTile = GlobalMap[globalXy.X, globalXy.Y];

                    if (!curTile.MustReplace)
                    {
                        return false;
                    }
                    if (curTile.IsRoad && prefabTile.Properties.ContainsKey("isRoad") && prefabTile.Properties["isRoad"] == "true") { continue; }
                    else
                        if (!curTile.IsRoad && prefabTile.Properties.ContainsKey("isRoad") && prefabTile.Properties["isRoad"] == "true") { return false; }
                    if (curTile.IsSidewalk && prefabTile.Properties.ContainsKey("isSidewalk") && prefabTile.Properties["isSidewalk"] == "true") { continue; }
                    if (curTile.IsWall && prefabTile.Properties.ContainsKey("isWall") && prefabTile.Properties["isWall"] == "true") { continue; }
                    if (curTile.IsAny) { if (prefabTile.Properties.ContainsKey("isRoad")) continue; }
                    else return false;
                }
            }
            return true;
        }
        #endregion

        #region Mob generation

        void GenerateMobs(int amountOfMobs = 15)
        {
            int mobsPlaced = 0;
            do
            {
                var ox = GlobalRandom.Next(MapWidth);
                var oy = GlobalRandom.Next(MapHeight);

                var count = GlobalRandom.Next(6, 20);

                var spaceNeeded = count * 5;

                var spaces = new List<XY>();

                var dudesplaced = 0;

                var tl = new XY(ox - count / 2, oy - count / 2);
                var br = new XY(ox + count / 2, oy + count / 2);

                for (var ix = tl.X; ix < br.X; ix++)
                {
                    for (var iy = tl.Y; iy < br.Y; iy++)
                    {
                        if (new XY(ix, iy).ContainedBy(0, 0, MapWidth - 1, MapHeight - 1) && GlobalMap[ix, iy].Walkable
                            && !Actors.Any(i => i.Location == new XY(ix, iy)))
                        {
                            spaces.Add(new XY(ix, iy));
                        }
                    }
                }

                if (spaces.Count < spaceNeeded) continue;

                for (var i = 0; i<count;++i)
                {
                    var space = spaces.RandomItem();

                    Actor actor;
                    switch (GlobalRandom.Next(3))
                    { 
                        case 1:
                            actor = new Knife(GlobalMap, this);
                            actor.Sprite = 1;
                            ++i;
                            break;
                        case 2:
                            actor = new Brute(GlobalMap, this);
                            actor.Sprite = 3;
                            i += 2;
                            break;
                        case 0:
                        default:
                            actor = new Lackey(GlobalMap, this);
                            actor.Sprite = 2;
                            break;
                    }

                    actor.Location = space;
                    Actors.Add(actor);
                    spaces.Remove(space);
                }

                do
                {
                    dudesplaced++;
                } while (dudesplaced < count);

                mobsPlaced++;
            } while (mobsPlaced < amountOfMobs);
        }

        Zulu zulu;

        void GenerateZulu()
        {
            // Find a suitable place - end of a main road
            var mainRoadTiles = new List<XY>();

            for (var ix = 0; ix < Metamap.GetLength(0); ++ix)
            {
                for (var iy = 0; iy < Metamap.GetLength(1); ++iy)
                {
                    if ((Metamap[ix, iy].Contains(MetamapTile.MajorRoadEast) && Metamap[ix,iy].Contains(MetamapTile.MajorRoadWest)) ||
                        (Metamap[ix, iy].Contains(MetamapTile.MajorRoadNorth) && Metamap[ix,iy].Contains(MetamapTile.MajorRoadSouth)))
                    {
                        mainRoadTiles.Add(new XY(ix * MetaTileWidth + MetaTileWidth / 2, iy * MetaTileHeight + MetaTileHeight / 2));
                    }
                }
            }

            // Get at least 20 from player
            foreach(var tile in new List<XY>(mainRoadTiles))
            {
                var distance = Math.Abs((tile - Player.Location).X) + Math.Abs((tile - Player.Location).Y); 
                if (distance < 20)
                {
                    mainRoadTiles.Remove(tile);
                }
            }

            var currentTile = mainRoadTiles.RandomItem();

            // Get boom boom
            var bombedLayed = (TiledMapObjectLayer)PrefabMap.GetLayer("Bomb");
            var prefabLayer = PrefabMap.GetLayer<TiledMapTileLayer>("Tiles");
            var bombedRectRaw = bombedLayed.Objects.First();
            var bombedPrefabRect = new Rectangle((int)bombedRectRaw.Position.X / PrefabMap.TileHeight, (int)bombedRectRaw.Position.Y / PrefabMap.TileHeight,
                (int)bombedRectRaw.Size.Height / PrefabMap.TileHeight, (int)bombedRectRaw.Size.Height / PrefabMap.TileHeight);
            var o = currentTile - new XY(bombedPrefabRect.Width / 2, bombedPrefabRect.Height / 2);

            for (var ix = 0; ix < bombedPrefabRect.Width; ++ix)
            {
                for (var iy = 0; iy < bombedPrefabRect.Height; ++iy)
                {
                    var offset = new XY(ix, iy);
                    var global = o + offset;
                    var prefab = new XY(bombedPrefabRect.X, bombedPrefabRect.Y) + offset;

                    if(prefabLayer.Tiles[prefab.X + prefab.Y * prefabLayer.Width].GlobalIdentifier != 0)
                    { 
                        var tile = new MapTile(Tilesets, prefabLayer.Tiles[prefab.X + prefab.Y * prefabLayer.Width].GlobalIdentifier);
                        if (!tile.IsAny )
                            GlobalMap[global.X, global.Y] = tile;
                    }
                }
            }

            // Place ZULU there
            zulu = new Zulu(GlobalMap, this);
            zulu.Location = currentTile;
            zulu.Sprite = 4;
            Actors.Add(zulu);

            var spaces = new List<XY>();

            var dudesplaced = 0;

            var tl = new XY(currentTile.X - 9, currentTile.Y - 9);
            var br = new XY(currentTile.X + 9, currentTile.Y + 9);

            for (var ix = tl.X; ix < br.X; ix++)
            {
                for (var iy = tl.Y; iy < br.Y; iy++)
                {
                    if (new XY(ix, iy).ContainedBy(0, 0, MapWidth - 1, MapHeight - 1) && GlobalMap[ix, iy].Walkable
                        && !Actors.Any(i=>i.Location == new XY(ix, iy)))
                    {
                        spaces.Add(new XY(ix, iy));
                    }
                }
            }

            while (dudesplaced < 18 && spaces.Count > 0)
            {
                var space = spaces.RandomItem();

                Actor actor;
                switch (GlobalRandom.Next(3))
                {
                    case 1:
                        actor = new Knife(GlobalMap, this);
                        actor.Sprite = 1;
                        break;
                    case 2:
                        actor = new Brute(GlobalMap, this);
                        actor.Sprite = 3;
                        break;
                    case 0:
                    default:
                        actor = new Lackey(GlobalMap, this);
                        actor.Sprite = 2;
                        break;
                }

                actor.Location = space;
                Actors.Add(actor);
                spaces.Remove(space);
                dudesplaced++;
            }
        }

        #endregion
    }
}

using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class MapTile
    {
        //public MapTile(bool walkable, int drawTile, int brushPriority = 0)
        //{
        //    Walkable = walkable;
        //    DrawTile = drawTile;
        //    BrushPriority = brushPriority;
        //}

        public MapTile(List<TiledMapTileset> Tilesets, int gid)
        {
            Tileset = Tilesets.First(i => i.ContainsGlobalIdentifier(gid));
            var tile = Tileset.Tiles.First(i => i.LocalTileIdentifier == (gid - Tileset.FirstGlobalIdentifier));
            Walkable = tile.Properties.ContainsKey("walkable") && tile.Properties["walkable"] == "true";
            tile.Properties.TryGetValue("brushPriority", out string brushPriority);
            int.TryParse(brushPriority, out BrushPriority);
            DrawTile = tile.LocalTileIdentifier;
            MustReplace = tile.Properties.ContainsKey("mustReplace") && tile.Properties["mustReplace"] == "true";

            IsAny = tile.Properties.ContainsKey("isAny") && tile.Properties["isAny"] == "true";
            IsRoad = tile.Properties.ContainsKey("isRoad") && tile.Properties["isRoad"] == "true";
            IsSidewalk = tile.Properties.ContainsKey("isSidewalk") && tile.Properties["isSidewalk"] == "true";
            IsWall = tile.Properties.ContainsKey("isWall") && tile.Properties["isWall"] == "true";
        }

        /// <summary>
        /// Whether can walk on this tile
        /// </summary>
        public readonly bool Walkable = true;

        /// <summary>
        ///  What tile do draw for this tile
        /// </summary>
        public readonly int DrawTile;

        /// <summary>
        /// Whether brush pains over
        /// </summary>
        public readonly int BrushPriority = 0;

        public readonly TiledMapTileset Tileset;

        public readonly bool MustReplace = false;

        public readonly bool IsAny = true;
        public readonly bool IsRoad = true;
        public readonly bool IsSidewalk = true;
        public readonly bool IsWall = true;
    }
}

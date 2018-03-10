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
            var tile = Tileset.Tiles[gid - Tileset.FirstGlobalIdentifier];
            Walkable = tile.Properties.ContainsKey("walkable") && tile.Properties["walkable"] == "true";
            tile.Properties.TryGetValue("brushPriority", out string brushPriority);
            int.TryParse(brushPriority, out BrushPriority);
            DrawTile = tile.LocalTileIdentifier;
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
    }
}

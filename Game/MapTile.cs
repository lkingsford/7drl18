using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class MapTile
    {
        public MapTile(bool walkable, int drawTile)
        {
            Walkable = walkable;
            DrawTile = drawTile;
        }

        /// <summary>
        /// Whether can walk on this tile
        /// </summary>
        public readonly bool Walkable = true;

        /// <summary>
        ///  What tile do draw for this tile
        /// </summary>
        public readonly int DrawTile;
    }
}

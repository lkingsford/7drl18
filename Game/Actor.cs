using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    /// <summary>
    /// Anything that can Act. Really - players and baddies.
    /// </summary>
    public class Actor
    {
        public XY Location = new XY(0, 0);
        public int Sprite = 0;

        protected MapTile[,] gameMap;

        public Actor(MapTile[,] globalMap)
        {
            gameMap = globalMap; 
        }

        /// <summary>
        /// Move the actor, if the dungeon allows it
        /// Attack, if there's somebody there
        /// </summary>
        /// <param name="dxDy">Amount to move</param>
        public void Move(XY dxDy)
        {
            var newLocation = Location + dxDy;

            if (newLocation.X > 0 &&
                newLocation.X < gameMap.GetLength(0) &&
                newLocation.Y > 0 &&
                newLocation.Y < gameMap.GetLength(1) &&
                (gameMap[newLocation.X, newLocation.Y].Walkable))
            {
                Location = newLocation;
            }
        }
        public int HP = 4;
    }
}

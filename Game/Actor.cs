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
        protected Game game;

        public Actor(MapTile[,] globalMap, Game game)
        {
            gameMap = globalMap;
            this.game = game;
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

        public enum Action { NW, N, NE, W, Wait, E, SW, S, SE }

        /// <summary>
        /// NextMove is set if the actor has selected an action
        /// </summary>
        public Action? NextMove = null;

        public virtual void DoTurn()
        {
            switch (NextMove)
            {
                case Action.N:
                    Move(new XY(0, -1));
                    break;
                case Action.NE:
                    Move(new XY(1, -1));
                    break;
                case Action.E:
                    Move(new XY(1, 0));
                    break;
                case Action.SE:
                    Move(new XY(1, 1));
                    break;
                case Action.S:
                    Move(new XY(0, 1));
                    break;
                case Action.SW:
                    Move(new XY(-1, 1));
                    break;
                case Action.W:
                    Move(new XY(-1, 0));
                    break;
                case Action.NW:
                    Move(new XY(-1, -1));
                    break;
                default:
                    break;
                    // Do nothing... for now
            }
        }


        public int HP = 4;
    }
}

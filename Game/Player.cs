using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class Player : Actor
    {
        public Player(MapTile[,] globalMap) : base(globalMap)
        {
        }

        public int Momementum = 4;
        public int SpentMomentum = 3;

        public enum Action { N, NE, E, SE, S, SW, W, NW };

        /// <summary>
        /// NextMove is set if the player has selected an action
        /// </summary>
        public Action? NextMove = null;

        public override void DoTurn()
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
            NextMove = null;
        }
    }
}

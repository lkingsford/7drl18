using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Player : Actor
    {
        public Player(MapTile[,] globalMap, Game game) : base(globalMap, game)
        {
        }

        /// <summary>
        /// Move the actor, if the dungeon allows it
        /// Attack, if there's somebody there
        /// </summary>
        /// <param name="dxDy">Amount to move</param>
        public override void Move(XY dxDy)
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

        public int Momentum = 0;
        public int SpentMomentum = 0;

        public override void DoTurn()
        {
            if (NextMove != null)
            {
                if (FightTargets.ContainsKey(NextMove.Value))
                {
                    Hit(FightTargets[NextMove.Value]);
                }
                else
                {
                    base.DoTurn();
                    ResetMomentum();
                }
            }

            NextMove = null;
        }

        private void ResetMomentum()
        {
            Momentum = 0;
            SpentMomentum = 0;
        }

        protected override void Hit(Actor actor)
        {
            var attackDirection = (this.Location - actor.Location).Unit();
            var actorStartingLocation = actor.Location;
            this.Momentum += 1;
            actor.Move(-1 * attackDirection);
            if (actorStartingLocation == actor.Location)
            {
                Location = actorStartingLocation + attackDirection;
            }
            else
            {
                Location = actorStartingLocation;
                // Stun longer, if you hit them into something
                actor.Stun(1);
            }
            actor.GotHit(Math.Max(Momentum, 1));
            actor.Stun(1);
        }

        /// <summary>
        /// Who will be fought if going in each direction 
        /// </summary>
        public Dictionary<Action, Actor> FightTargets
        {
            get
            {
                var result = new Dictionary<Action, Actor>();

                // Walk in each direction up to 4 spaces until
                // run into somebody
                for (int ix = -1; ix <= 1; ix++)
                {
                    for (int iy = -1; iy <= 1; iy++)
                    {
                        for (int z = 1; z <= 4; z++)
                        {
                            var location = z * (new XY(ix, iy)) + Location;
                            var firstbad = game.Actors.FirstOrDefault(i => i != this && i.Location == location);
                            if (firstbad != null)
                            {
                                result[(Action)(ix + 1 + ((iy + 1) * 3))] = firstbad;
                                break;
                            }
                        }
                    }
                }

                return result;
            }
        }

        public override void GotHit(int damage)
        {
            base.GotHit(damage);
            ResetMomentum();
        }
    }
}

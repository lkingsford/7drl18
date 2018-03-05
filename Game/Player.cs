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

        protected void Hit(Actor actor)
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
            }
            actor.HP -= Math.Max(Momentum, 1);
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
    }
}

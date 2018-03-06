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

            if (CanWalk(newLocation))
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
                // Player is differently controlled depending
                // on atk or def phase
                if (game.CurrentPhase == Game.TurnPhases.Player)
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
                else
                {
                    // Defence phase
                    // Can spend momentum to move away, or wait to parry

                    if (DefenceAllowedMoves.Contains(NextMove.Value))
                    {
                        if (NextMove.Value == Action.Parry)
                        {
                            // Parry
                            var toParry = CanParry();
                            if (Momentum > toParry.Count())
                            {
                                SpendMomentum(toParry.Count());
                                foreach (var baddie in toParry)
                                {
                                    Hit(baddie);
                                }
                            }
                        }
                        else
                        {
                            if (Momentum >= 1)
                            {
                                SpendMomentum(1);
                                // Dodge
                                switch (NextMove.Value)
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
                        }
                    }
                }
            }

            NextMove = null;
        }

        private void ResetMomentum()
        {
            Momentum = 0;
            SpentMomentum = 0;
        }

        public void SpendMomentum(int amt)
        {
            if (amt <= Momentum)
            {
                Momentum -= amt;
                SpentMomentum += amt;
            }
            else
            {
                throw new BrokenRulesException("Spent more momentum then had");
            }
        }

        protected override void Hit(Actor actor, int dmg = 1)
        {
            var attackDirection = (this.Location - actor.Location).Unit();
            var actorStartingLocation = actor.Location;
            this.Momentum += 1;
            actor.Move(-1 * attackDirection);
            if (game.CurrentPhase == Game.TurnPhases.Enemy)
            {
                // Cancel attack move - 'cause parried (most likely)
                actor.NextMove = Action.Wait;
            }
            if (actorStartingLocation == actor.Location)
            {
                if (game.CurrentPhase == Game.TurnPhases.Player)
                {
                    Location = actorStartingLocation + attackDirection;
                }
            }
            else
            {
                if (game.CurrentPhase == Game.TurnPhases.Player)
                {
                    Location = actorStartingLocation;
                }
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

        /// <summary>
        /// Moves allowed in defence mode 
        /// </summary>
        public HashSet<Action> DefenceAllowedMoves
        {
            get
            {
                var result = new HashSet<Action>();
                for (int ix = -1; ix <= 1; ix++)
                {
                    for (int iy = -1; iy <= 1; iy++)
                    {
                        if (ix == 0 && iy == 0)
                        {
                            break;
                        }

                        var location = Location + new XY(ix, iy);
                        var firstbad = game.Actors.FirstOrDefault(i => i != this && i.Location == location);
                        if (firstbad != null && ((firstbad as Enemy)?.Attacking ?? false))
                        {
                            // Attacking from a dir, so can go backwards - if nobody there
                            // Invert ix and iy to get allowed direction
                            var travelLocation = Location - new XY(ix, iy);
                            if (!game.Actors.Any(i => i != this && i.Location == travelLocation))
                            {
                                // If nobody there
                                result.Add((Action)(-ix + 1 + ((-iy + 1) * 3)));
                            }
                        }
                    }
                }

                // Parry allowed
                if (CanParry().Count > 0 && Momentum > CanParry().Count)
                {
                    result.Add(Action.Parry);
                }

                return result;
            }
        }

        /// <summary>
        /// Dudes who can get parried
        /// </summary>
        /// <returns></returns>
        public List<Actor> CanParry()
        {
            return game.Actors.Where(i => (i as Enemy)?.Attacking ?? false && Location.Adjacent(i.Location)).ToList();
        }

        public bool CanWalk(XY newLocation)
        {
            return newLocation.X > 0 &&
                newLocation.X < gameMap.GetLength(0) &&
                newLocation.Y > 0 &&
                newLocation.Y < gameMap.GetLength(1) &&
                (gameMap[newLocation.X, newLocation.Y].Walkable);
        }

        public override void GotHit(int damage)
        {
            base.GotHit(damage);
            ResetMomentum();
        }
    }
}

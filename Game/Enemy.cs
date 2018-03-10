using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    class Enemy : Actor
    {
        public Enemy(MapTile[,] globalMap, Game game) : base(globalMap, game)
        {
            HP = 6;
        }

        protected int pushDistance = 1;
        protected int stunTime = 0;

        public void WhatYouWannaDo()
        {
            var player = game.Actors.FirstOrDefault(i => i is Player);

            if (player != null)
            {
                var direction = player.Location - Location;
                NextMove = (Action)(direction.Unit().X + 1 + ((direction.Unit().Y + 1) * 3));

                toAttack = ((Location + direction.Unit()) == player.Location);
            }
        }

        private bool toAttack = false;

        // Whether player will be attacked at end of this phase
        public bool Attacking
        {
            get
            {
                return game.CurrentPhase == Game.TurnPhases.Enemy && toAttack && !Stunned; 
            }
        }

        protected override void Hit(Actor actor)
        {
            toAttack = false;
            var thisdmg = dmg;
            if (actor.Location.Adjacent(Location))
            {
                for (var i = 0; i < pushDistance; ++i)
                {
                    // Enemy pushes away when hitting
                    var attackDirection = (this.Location - actor.Location).Unit();
                    var actorStartingLocation = actor.Location;
                    actor.Move(-1 * attackDirection);
                    if (actorStartingLocation == actor.Location)
                    {
                        // More damage, if hit into something
                        thisdmg += 1;
                    }
                }
                actor.GotHit(dmg);
                if (stunTime != 0)
                {
                    actor.Stun(stunTime);
                }
            };
        }

        public override void GotHit(int damage)
        {
            Stun(1);
            base.GotHit(damage);
        }
    }
}
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
        }

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
                return toAttack && !Stunned; 
            }
        }

        protected override void Hit(Actor actor, int dmg = 1)
        {
            toAttack = false;
            // Enemy pushes away when hitting
            var attackDirection = (this.Location - actor.Location).Unit();
            var actorStartingLocation = actor.Location;
            actor.Move(-1 * attackDirection);
            if (actorStartingLocation != actor.Location)
            {
                // More damage, if hit into something
                dmg += 1;
            }
            base.Hit(actor, dmg);
        }
    }
}
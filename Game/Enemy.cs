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

        public override void DoTurn()
        {
            var player = game.Actors.FirstOrDefault(i => i is Player);

            if (player != null)
            {
                var direction = player.Location - Location;
                NextMove = (Action)(direction.Unit().X + 1 + ((direction.Unit().Y + 1) * 3));
            }

            base.DoTurn();
        }
    }
}

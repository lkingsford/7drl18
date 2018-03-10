using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class Brute : Enemy
    {
        public Brute(MapTile[,] globalMap, Game game) : base(globalMap, game)
        {
            HP = 8;
            dmg = 2;
            pushDistance = 2;
        }

        public override void GotHit(int damage)
        {
            base.GotHit(damage);
            // Undo stun
            Stun(-2);
        }
    }
}

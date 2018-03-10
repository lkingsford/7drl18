using System;
using System.Collections.Generic;
using System.Text;
namespace Game
{
    class Knife : Enemy
    {
        public Knife(MapTile[,] globalMap, Game game) : base(globalMap, game)
        {
            HP = 6;
            dmg = 2;
            pushDistance = 0;
        }

        // Can't parry a knife
        public override bool CanParry
        {
            get
            {
                return false;
            }
        }
    }
}

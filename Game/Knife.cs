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
        }

        public override bool CanParry
        {
            get
            {
                return false;
            }
        }
    }
}

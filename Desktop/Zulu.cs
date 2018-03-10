using System;

namespace Game
{
    class Zulu : Enemy
    {
        private MapTile[,] globalMap;
        private Game game;

        public Zulu(MapTile[,] globalMap, Game game) : base(globalMap, game)
        {
            this.globalMap = globalMap;
            this.game = game;
            this.HP = 30;
            this.dmg = 1;
            this.pushDistance = 4;
            this.stunTime = 2;
            this.active = true;
        }

        public bool active = false;
    }
}
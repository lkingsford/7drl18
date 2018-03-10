using Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoGame.Extended.NuclexGui;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.NuclexGui.Controls.Desktop;
using Microsoft.Xna.Framework.Graphics;

namespace Desktop
{
    class GameState : State
    {
        /// <summary>
        /// The game being played
        /// </summary>
        protected Game.Game G;

        protected Minimap minimap;

        /// <summary>
        /// Create a game interface from a given game
        /// </summary>
        /// <param name="Game">Game object that is being played</param>
        public GameState(Game.Game Game, Microsoft.Xna.Framework.Game MonogameGame)
        {
            this.G = Game;

            MapTileSprites = AppContentManager.Load<Texture2D>("MapTiles");
            PCSprites = AppContentManager.Load<Texture2D>("CharacterSprites");
            MomentumSprite = AppContentManager.Load<Texture2D>("UiElements/MomentumMarker");
            SpentMomentumSprite = AppContentManager.Load<Texture2D>("UiElements/SpentMomentumMarker");
            HpSprite = AppContentManager.Load<Texture2D>("UiElements/HpMarker");
            MovementArrowsSprite = AppContentManager.Load<Texture2D>("UiElements/Arrows");
            MonsterDetailsFont = AppContentManager.Load<SpriteFont>("UiElements/MonsterDisplay");
            StateFont = AppContentManager.Load<SpriteFont>("UiElements/TurnState");

            arrowWidth = MovementArrowsSprite.Width / 9;
            arrowHeight = MovementArrowsSprite.Height / 3;

            minimap = new Minimap(Game, MonogameGame);
            minimap.showCrime = true;
        }

        Texture2D MapTileSprites;
        Texture2D PCSprites;
        Texture2D MomentumSprite;
        Texture2D SpentMomentumSprite;
        Texture2D HpSprite;
        Texture2D MovementArrowsSprite;

        SpriteFont MonsterDetailsFont;
        SpriteFont StateFont;
        private int arrowWidth;
        private int arrowHeight;

        /// <summary>
        /// State as of the previous Update
        /// </summary>
        private KeyboardState LastState;

        private TimeSpan lastActionTime;

        /// <summary>
        /// Run logic for this state - including input
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Update(GameTime GameTime)
        {
            if (G.Player.HP <= 0)
            {
                // Player Dead
                Dead();
            }

            if (G.DeadZulu)
            {
                Win();
            }

            if (lastActionTime == null)
            {
                lastActionTime = GameTime.TotalGameTime;
            }

            // Get the current state, get the last state, and any new buttons are acted upon
            var currentState = Keyboard.GetState();
            foreach (var i in currentState.GetPressedKeys())
            {
                if (LastState.IsKeyUp(i) 
                    || 
                   (G.CurrentPhase == Game.Game.TurnPhases.Player && G.NoVisible() &&
                    (GameTime.TotalGameTime - lastActionTime).TotalMilliseconds > 100))
                {
                    KeyPressed(i);
                    lastActionTime = GameTime.TotalGameTime;
                }
            }
            LastState = currentState;
        }

        private void Win()
        {
            StateStack.Remove(this);
            StateStack.Add(new DeadState(G));
        }

        private void Dead()
        {
            StateStack.Remove(this);
            StateStack.Add(new DeadState(G));
        }

        /// <summary>
        /// Act upon a pressed key
        /// </summary>
        /// <param name="Key"></param>
        private void KeyPressed(Keys Key)
        {
            switch (Key)
            {
                case Keys.H:
                case Keys.Left:
                case Keys.NumPad4:
                    G.Player.NextMove = Player.Action.W;
                    break;
                case Keys.K:
                case Keys.Up:
                case Keys.NumPad8:
                    G.Player.NextMove = Player.Action.N;
                    break;
                case Keys.L:
                case Keys.Right:
                case Keys.NumPad6:
                    G.Player.NextMove = Player.Action.E;
                    break;
                case Keys.J:
                case Keys.Down:
                case Keys.NumPad2:
                    G.Player.NextMove = Player.Action.S;
                    break;
                case Keys.Y:
                case Keys.NumPad7:
                    G.Player.NextMove = Player.Action.NW;
                    break;
                case Keys.U:
                case Keys.NumPad9:
                    G.Player.NextMove = Player.Action.NE;
                    break;
                case Keys.B:
                case Keys.NumPad1:
                    G.Player.NextMove = Player.Action.SW;
                    break;
                case Keys.N:
                case Keys.NumPad3:
                    G.Player.NextMove = Player.Action.SE;
                    break;
                case Keys.NumPad5:
                case Keys.Space:
                    if (G.CurrentPhase == Game.Game.TurnPhases.Player)
                    {
                        G.Player.NextMove = Player.Action.Wait;
                    }
                    else
                    {
                        if (G.Player.DefenceAllowedMoves.Contains(Actor.Action.Parry))
                        {
                            G.Player.NextMove = Player.Action.Parry;
                        }
                        else
                        {
                            G.Player.NextMove = Player.Action.Wait;
                        }
                    }
                    break;
                case Keys.OemPeriod:
                case Keys.Decimal:
                    G.Player.NextMove = Player.Action.Wait;
                    break;
            }

            if (G.Player.NextMove != null)
            {
                G.NextTurn();
            }
        }

        int drawLeft = 352;
        int drawTop = 72;

        XY actionDrawLocation = new XY(20, 300);

        /// <summary>
        /// Draw this state
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Draw(GameTime GameTime)
        {
            AppSpriteBatch.Begin();
            // Draw things here

            var mapToDraw = G.VisibleMap;

            var tileWidth = 64;
            var tileHeight = 64;

            for (var ix = 0; ix < G.CameraWidth; ++ix)
            {
                for (var iy = 0; iy < G.CameraHeight; ++iy)
                {
                    var srcX = tileWidth * (mapToDraw[ix, iy].DrawTile % mapToDraw[ix, iy].Tileset.Columns);
                    var srcY = tileHeight * (mapToDraw[ix, iy].DrawTile / mapToDraw[ix, iy].Tileset.Columns);
                    AppSpriteBatch.Draw(mapToDraw[ix, iy].Tileset.Texture,
                        // Dest
                        new Rectangle(drawLeft + tileWidth * ix, drawTop + tileHeight * iy, tileWidth, tileHeight),
                        // Src
                        new Rectangle(srcX, srcY, tileWidth, tileHeight),
                        Color.White);
                }
            }

            foreach (var i in G.VisibleActors)
            {
                var drawX = drawLeft + tileWidth * (i.Location.X - G.CameraTopLeft.X);
                var drawY = drawTop + tileHeight * (i.Location.Y - G.CameraTopLeft.Y);
                AppSpriteBatch.Draw(PCSprites,
                    //Dest
                    new Rectangle(drawX, drawY, tileWidth, tileHeight),
                    //Src
                    new Rectangle(i.Sprite * tileWidth, 0, tileWidth, tileHeight),
                    Color.White);

                // Draw HP
                var message = i.HP.ToString();
                if (i.Stunned)
                {
                    message += "\n(Stunned)";
                }

                if ((i as Enemy)?.Attacking ?? false)
                {
                    message += "\n(Attacking)";
                }

                AppSpriteBatch.DrawString(MonsterDetailsFont, message,
                    new Vector2(drawX - 1 + tileWidth * 3 / 4, drawY - 1), Color.Black);
                AppSpriteBatch.DrawString(MonsterDetailsFont, message,
                    new Vector2(drawX + tileWidth * 3 / 4, drawY), Color.Red);
            }

            var momentumTop = AppGraphicsDevice.Viewport.Height - drawTop;
            // Draw player momentum
            for (var i = 0; i < G.Player.SpentMomentum; ++i)
            {
                momentumTop -= SpentMomentumSprite.Height;
                AppSpriteBatch.Draw(SpentMomentumSprite, new Vector2(drawLeft - SpentMomentumSprite.Width, momentumTop), Color.White);
            }

            for (var i = 0; i < G.Player.Momentum; ++i)
            {
                momentumTop -= MomentumSprite.Height;
                AppSpriteBatch.Draw(MomentumSprite, new Vector2(drawLeft - MomentumSprite.Width, momentumTop), Color.White);
            }

            for (var i = 0; i < G.Player.HP; ++i)
            {
                AppSpriteBatch.Draw(HpSprite, new Vector2(drawLeft + tileWidth * G.CameraWidth, drawTop + i * HpSprite.Height), Color.White);
            }

            // Draw phase
            string phaseMessage = "";
            switch (G.CurrentPhase)
            {
                case Game.Game.TurnPhases.Enemy:
                    phaseMessage = "Defence turn";
                    break;
                case Game.Game.TurnPhases.Player:
                    phaseMessage = "Attack turn";
                    break;
            }

            // Draw allowed moves
            bool addWait = false;
            for (var ix = 0; ix <= 2; ++ix)
            {
                for (var iy = 0; iy <= 2; ++iy)
                {
                    int whatWillDo = 0;
                    // 0: Nothing
                    // 1: Walk
                    // 2: Attack
                    // 3: Wait
                    // 4: Parry
                    // 5: Dodge

                    var action = (Actor.Action)(ix + iy * 3);
                    var newLocation = G.Player.Location + new XY(ix - 1, iy - 1);

                    if (G.CurrentPhase == Game.Game.TurnPhases.Player)
                    {
                        if (G.Player.CanWalk(newLocation))
                        {
                            whatWillDo = 1;
                        }
                        if (G.Player.FightTargets.ContainsKey(action))
                        {
                            whatWillDo = 2;
                        }
                        if (ix == 1 && iy == 1)
                        {
                            whatWillDo = 3;
                        }
                    }
                    else
                    {
                        var defMoves = G.Player.DefenceAllowedMoves;
                        if (ix == 1 && iy == 1)
                        {
                            if (defMoves.Contains(Actor.Action.Parry))
                            {
                                whatWillDo = 4;
                                addWait = true;
                            }
                        }
                        else
                        {
                            if (defMoves.Contains(action))
                            {
                                whatWillDo = 5;
                            }
                        }
                    }


                    var horizPosition = (ix + iy * 3 + 1 - ((ix + iy * 3) > 3 ? 1 : 0));

                    XY pos = null;

                    switch (whatWillDo)
                    {
                        case 0:
                            pos = new XY(0, 2);
                            break;
                        case 1:
                            pos = new XY(horizPosition, 0);
                            break;
                        case 2:
                            pos = new XY(horizPosition, 1);
                            break;
                        case 3:
                            pos = new XY(0, 0);
                            break;
                        case 4:
                            pos = new XY(0, 1);
                            break;
                        case 5:
                            pos = new XY(horizPosition, 2);
                            break;
                    }

                    if (pos != null)
                    {
                        AppSpriteBatch.Draw(MovementArrowsSprite,
                            //Dest
                            new Rectangle(actionDrawLocation.X + arrowWidth * ix, actionDrawLocation.Y + arrowHeight * iy, arrowWidth, arrowHeight),
                            //Source
                            new Rectangle(pos.X * arrowWidth, pos.Y * arrowHeight, arrowWidth, arrowHeight),
                            Color.White);
                    }
                }

            }
            if (addWait)
            {
                XY pos = new XY(0, 0);
                var ix = 2;
                var iy = 3;
                if (pos != null)
                {
                    AppSpriteBatch.Draw(MovementArrowsSprite,
                        //Dest
                        new Rectangle(actionDrawLocation.X + arrowWidth * ix, actionDrawLocation.Y + arrowHeight * iy, arrowWidth, arrowHeight),
                        //Source
                        new Rectangle(pos.X * arrowWidth, pos.Y * arrowHeight, arrowWidth, arrowHeight),
                        Color.White);
                }
            }
            AppSpriteBatch.DrawString(StateFont, phaseMessage, new Vector2(20, 60), Color.White);

            minimap.DrawMinimap(new XY(1000, 120));

            AppSpriteBatch.End();

//            gui.Draw(GameTime);
        }
    }
}

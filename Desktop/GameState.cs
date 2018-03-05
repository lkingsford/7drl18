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

        /// <summary>
        /// Create a game interface from a given game
        /// </summary>
        /// <param name="Game">Game object that is being played</param>
        public GameState(Game.Game Game, Microsoft.Xna.Framework.Game MonogameGame)
        {
            this.G = Game;

            //inputListener = new InputListenerComponent(MonogameGame);
            //inputManager = new MonoGame.Extended.NuclexGui.GuiInputService(inputListener);

            //gui = new GuiManager(MonogameGame.Services, inputManager);
            //gui.Screen = new GuiScreen();
            //gui.Initialize();
            //var button = new GuiButtonControl
            //{
            //    Name = "button",
            //    Bounds = new UniRectangle(new UniVector(new UniScalar(10), new UniScalar(10)), new UniVector(new UniScalar(200), new UniScalar(100))),
            //    Text = "Dang!"
            //};

            //button.Pressed += OnButtonPressed;
            //gui.Screen.Desktop.Children.Add(button);

            MapTileSprites = AppContentManager.Load<Texture2D>("MapTiles");
            PCSprites = AppContentManager.Load<Texture2D>("CharacterSprites");
            MomentumSprite = AppContentManager.Load<Texture2D>("UiElements/MomentumMarker");
            SpentMomentumSprite = AppContentManager.Load<Texture2D>("UiElements/SpentMomentumMarker");
            HpSprite = AppContentManager.Load<Texture2D>("UiElements/HpMarker");
            MonsterDetailsFont = AppContentManager.Load<SpriteFont>("UiElements/MonsterDisplay");
            StateFont = AppContentManager.Load<SpriteFont>("UiElements/TurnState");
        }

        Texture2D MapTileSprites;
        Texture2D PCSprites;
        Texture2D MomentumSprite;
        Texture2D SpentMomentumSprite;
        Texture2D HpSprite;

        SpriteFont MonsterDetailsFont;
        SpriteFont StateFont;

        //GuiScreen oldScreen;

        //private void OnButtonPressed(object sender, EventArgs e)
        //{
        //    toggle = !toggle;
        //    if (toggle)
        //    {
        //        gui.Screen = new GuiScreen();
        //    }
        //}

        //private readonly GuiManager gui;
        //private GuiInputService inputManager;
        //private InputListenerComponent inputListener;

        /// <summary>
        /// State as of the previous Update
        /// </summary>
        private KeyboardState LastState;

        /// <summary>
        /// Run logic for this state - including input
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Update(GameTime GameTime)
        {
            // Get the current state, get the last state, and any new buttons are acted upon
            var currentState = Keyboard.GetState();
            foreach (var i in currentState.GetPressedKeys())
            {
                if (LastState.IsKeyUp(i))
                {
                    KeyPressed(i);
                }
            }
            LastState = currentState;

            // Do turn, if player next move is set
            //if (G.Player.NextMove != Player.Instruction.NOT_SET)
            //{
            //    G.DoTurn();
            //}

            //if (G.GameOver)
            //{
            //    StateStack.Add(new AtlasWarriors.LoseState(G));
            //}

            //inputListener.Update(GameTime);
            //gui.Update(GameTime);
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
                    G.Player.NextMove = Player.Action.Wait;
                    break;
            }

            if (G.Player.NextMove != null)
            {
                G.NextTurn();
            }
        }

        bool toggle = false;

        int drawLeft = 352;
        int drawTop = 72;

        /// <summary>
        /// Draw this state
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Draw(GameTime GameTime)
        {
            if (toggle)
            {
                AppGraphicsDevice.Clear(Color.Red);
            }

            AppSpriteBatch.Begin();
            // Draw things here

            var mapToDraw = G.VisibleMap;

            var tileWidth = 64;
            var tileHeight = 64;

            for (var ix = 0; ix < G.CameraWidth; ++ix)
            {
                for (var iy = 0; iy < G.CameraHeight; ++iy)
                {
                    AppSpriteBatch.Draw(MapTileSprites,
                        // Dest
                        new Rectangle(drawLeft + tileWidth * ix, drawTop + tileHeight * iy, tileWidth, tileHeight),
                        // Src
                        new Rectangle(tileWidth * mapToDraw[ix, iy].DrawTile, 0, tileWidth, tileHeight),
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
            AppSpriteBatch.DrawString(StateFont, phaseMessage, new Vector2(20, 60), Color.White);

            AppSpriteBatch.End();

//            gui.Draw(GameTime);
        }
    }
}

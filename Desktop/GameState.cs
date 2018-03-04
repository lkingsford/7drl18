﻿using Game;
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

            inputListener = new InputListenerComponent(MonogameGame);
            inputManager = new MonoGame.Extended.NuclexGui.GuiInputService(inputListener);

            gui = new GuiManager(MonogameGame.Services, inputManager);
            gui.Screen = new GuiScreen();
            gui.Initialize();
            var button = new GuiButtonControl
            {
                Name = "button",
                Bounds = new UniRectangle(new UniVector(new UniScalar(10), new UniScalar(10)), new UniVector(new UniScalar(200), new UniScalar(100))),
                Text = "Dang!"
            };

            button.Pressed += OnButtonPressed;
            gui.Screen.Desktop.Children.Add(button);
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            toggle = !toggle;
        }

        private readonly GuiManager gui;
        private GuiInputService inputManager;
        private InputListenerComponent inputListener;

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

            inputListener.Update(GameTime);
            gui.Update(GameTime);
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
                    //G.Player.NextMove = Player.Instruction.MOVE_W;
                    break;
                case Keys.K:
                case Keys.Up:
                case Keys.NumPad8:
                    //G.Player.NextMove = Player.Instruction.MOVE_N;
                    break;
                case Keys.L:
                case Keys.Right:
                case Keys.NumPad6:
                    //G.Player.NextMove = Player.Instruction.MOVE_E;
                    break;
                case Keys.J:
                case Keys.Down:
                case Keys.NumPad2:
                    //G.Player.NextMove = Player.Instruction.MOVE_S;
                    break;
                case Keys.Y:
                case Keys.NumPad7:
                    //G.Player.NextMove = Player.Instruction.MOVE_NW;
                    break;
                case Keys.U:
                case Keys.NumPad9:
                    //G.Player.NextMove = Player.Instruction.MOVE_NE;
                    break;
                case Keys.B:
                case Keys.NumPad1:
                    //G.Player.NextMove = Player.Instruction.MOVE_SW;
                    break;
                case Keys.N:
                case Keys.NumPad3:
                    //G.Player.NextMove = Player.Instruction.MOVE_SE;
                    break;
            }
        }

        bool toggle = false; 

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
            AppSpriteBatch.End();

            gui.Draw(GameTime);
        }
    }
}

using Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.NuclexGui;
using MonoGame.Extended.NuclexGui.Controls;
using MonoGame.Extended.NuclexGui.Controls.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop
{
    class TitleState : State
    {
        private readonly Texture2D background;

        GuiButtonControl btnStartGame;
        GuiButtonControl btnQuitGame;
        GuiWindowControl windowStarting;
        GuiLabelControl lblStatus;

        public TitleState()
        {
            background = AppContentManager.Load<Texture2D>("Title/Title");


            if (gui == null)
            {
                inputListener = new InputListenerComponent(GameApp);
                inputManager = new MonoGame.Extended.NuclexGui.GuiInputService(inputListener);
                gui = new GuiManager(GameApp.Services, inputManager);
                gui.Initialize();
            }
                gui.Screen = new GuiScreen();
            btnStartGame = new GuiButtonControl
            {
                Name = "Start Game",
                Bounds = new UniRectangle(new UniVector(new UniScalar(540), new UniScalar(200)), new UniVector(new UniScalar(200), new UniScalar(70))),
                Text = "Start Game"
            };
            btnQuitGame = new GuiButtonControl
            {
                Name = "Quit Game",
                Bounds = new UniRectangle(new UniVector(new UniScalar(540), new UniScalar(300)), new UniVector(new UniScalar(200), new UniScalar(70))),
                Text = "Quit Game"
            };
            btnQuitGame.Pressed += BtnQuitGame_Pressed;
            gui.Screen.Desktop.Children.Add(btnQuitGame);

            windowStarting = new GuiWindowControl()
            {
                Name = "Starting",
                Bounds = new UniRectangle(new UniVector(new UniScalar(240), new UniScalar(200)), new UniVector(new UniScalar(800), new UniScalar(100))),
                Title = "Starting Game..."
            };

            btnStartGame.Pressed += OnStartGamePressed;
            gui.Screen.Desktop.Children.Add(btnStartGame);

            lblStatus = new GuiLabelControl()
            {
                Bounds = new UniRectangle(new UniVector(new UniScalar(10), new UniScalar(0)), new UniVector(new UniScalar(800), new UniScalar(100))),
                Text = ""
            };

            windowStarting.Children.Add(lblStatus);

        }

        private void BtnQuitGame_Pressed(object sender, EventArgs e)
        {
            StateStack.Remove(this);
        }

        BackgroundWorker w;
        Game.Game g;

        private void OnStartGamePressed(object sender, EventArgs e)
        {
            w = new BackgroundWorker();
            w.ProgressChanged += ReportProgress;
            w.WorkerReportsProgress = true;

            gui.Screen.Desktop.Children.Remove(btnStartGame);
            gui.Screen.Desktop.Children.Remove(btnQuitGame);
            gui.Screen.Desktop.Children.Add(windowStarting);

            w.DoWork += W_DoWork;
            w.RunWorkerCompleted += W_RunWorkerCompleted;

            w.RunWorkerAsync();
        }

        private void W_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StateStack.Add(new GameState(g, GameApp));
            StateStack.Remove(this);
        }

        private void W_DoWork(object sender, DoWorkEventArgs e)
        {
            g = new Game.Game(AppContentManager, w.ReportProgress);
        }

        private void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            lblStatus.Text = e.UserState.ToString();
        }

        /// <summary>
        /// Draw this state
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Draw(GameTime GameTime)
        {
            AppSpriteBatch.Begin();
            AppSpriteBatch.Draw(background, new Rectangle(0, 0, 1280, 720), Color.White);
            // Draw things here
            AppSpriteBatch.End();

            gui.Draw(GameTime);
        }

        /// <summary>
        /// Run logic for this state - including input
        /// </summary>
        /// <param name="GameTime">Snapshot of timing</param>
        public override void Update(GameTime GameTime)
        {
            inputListener.Update(GameTime);
            gui.Update(GameTime);
        }
    }
}

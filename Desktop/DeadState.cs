using Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.NuclexGui;
using MonoGame.Extended.NuclexGui.Controls.Desktop;

namespace Desktop
{
    internal class DeadState : State
    {
        private Game.Game g;
        private Texture2D background;

        private GuiButtonControl btnTitle;

        public DeadState(Game.Game g)
        {
            this.g = g;

            background = AppContentManager.Load<Texture2D>("Title/Gameover");
            gui.Screen = new GuiScreen();
            gui.Initialize();

            btnTitle = new GuiButtonControl
            {
                Name = "Return to title",
                Bounds = new UniRectangle(new UniVector(new UniScalar(490), new UniScalar(600)), new UniVector(new UniScalar(300), new UniScalar(70))),
                Text = "Return to title"
            };
            btnTitle.Pressed += BtnTitle_Pressed;
            gui.Screen.Desktop.Children.Add(btnTitle);
        }

        private void BtnTitle_Pressed(object sender, System.EventArgs e)
        {
            StateStack.Remove(this);
            StateStack.Add(new TitleState());
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
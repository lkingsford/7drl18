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

        private string DeathText = "";
        private SpriteFont DeathFont;

        public DeadState(Game.Game g)
        {
            this.g = g;

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

            if (g.DeadZulu)
            {
                background = AppContentManager.Load<Texture2D>("Title/Victory");
                DeathText = $@"Zulu has fallen, and the denizens of New London are at peace once more.

You slayed {g.DeadLackeys} of Zulu's lackeys.
You slayed {g.DeadKnifes} of Zulu's cutters.
You slayed {g.DeadBrutes} of Zulu's brutes.
Zulu has been slain.";
            }
            else
            {
                background = AppContentManager.Load<Texture2D>("Title/Gameover");
                DeathText = $@"Your vegeance was not satisfied.
The city of New London remains under the criminal guard of Zulu.

You slayed {g.DeadLackeys} of Zulu's lackeys.
You slayed {g.DeadKnifes} of Zulu's cutters.
You slayed {g.DeadBrutes} of Zulu's brutes.
Zulu remains alive.";
            }

            DeathFont = AppContentManager.Load<SpriteFont>("GameOverFont");
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
            AppSpriteBatch.DrawString(DeathFont, DeathText, new Vector2(100, 200), Color.LightGray);
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GameboyZelda
{
    internal enum GameState
    { logo, startscreen, playing, paused };

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RetroScreen gameboy;
        private Sprite player;
        private Background background;
        private Texture2D titleScreen;
        private Texture2D logo;
        private GameState gs;
        private float timeForLogo = 4;
        private Stopwatch stopwatch;
        private const int screenWidth = 160;
        private const int screenHeight = 144;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            SetDefaultSize();
            gs = GameState.logo;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void SetDefaultSize()
        {
            int height = MaxHeight();
            _graphics.PreferredBackBufferHeight = height;

            _graphics.PreferredBackBufferWidth = (height / 144) * 160;
            _graphics.ApplyChanges();
        }

        private void SetDefaultSize(Screen screen)
        {
            int height = MaxHeight(screen);
            _graphics.PreferredBackBufferHeight = height;

            _graphics.PreferredBackBufferWidth = (height / 144) * 160;
            _graphics.ApplyChanges();
        }

        private void CenterScreen(Screen screen)
        {
            Window.Position = new Point((screen.Bounds.Width / 2) - (Window.ClientBounds.Width / 2), (screen.Bounds.Height / 2) - (Window.ClientBounds.Height / 2));
        }

        private void SwitchFullscreen()
        {
            var f = (Form)Control.FromHandle(Window.Handle);
            Screen screen = Screen.FromControl(f);
            _graphics.HardwareModeSwitch = false;
            Window.BeginScreenDeviceChange(!_graphics.IsFullScreen);
            bool wasFullscreen = _graphics.IsFullScreen;
            if (!_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = screen.Bounds.Width;
                _graphics.PreferredBackBufferHeight = screen.Bounds.Height;
            }
            else
            {
                SetDefaultSize(screen);
            }
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
            _graphics.HardwareModeSwitch = true;
            if (wasFullscreen)
            {
                CenterScreen(screen);
            }
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //renderTarget = new RenderTarget2D(GraphicsDevice, 160, 144);
            gameboy = new RetroScreen(new RenderTarget2D(GraphicsDevice, 160, 144), ScreenSize());
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += ScreenChange;
            //screenSize = ScreenSize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D texture = Content.Load<Texture2D>("linktransdown0");
            titleScreen = Content.Load<Texture2D>("TheTitleScreen");
            player = new Sprite(texture, new Vector2(160 / 2, 144 / 2));
            logo = Content.Load<Texture2D>("Logo");
            background = new Background(logo, new Rectangle(0, 0, 160, 144));
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            Input.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();
            if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.F11))
            {
                SwitchFullscreen();
            }
            if (gs == GameState.logo && stopwatch.Elapsed.TotalSeconds > timeForLogo)
            {
                gs = GameState.startscreen;
                background.SetTex(titleScreen);
                stopwatch.Restart();
            }
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(gameboy.renderTarget);
            Color color = new Color(48, 104, 80);
            GraphicsDevice.Clear(color);

            //_spriteBatch.Draw();
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            background.Draw(_spriteBatch);

            if (gs == GameState.playing || gs == GameState.paused)
            {
                player.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            // TODO: Add your drawing code here
            float transitionTime = 0.5f;
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            if (gs == GameState.logo && stopwatch.Elapsed.TotalSeconds - timeForLogo > -transitionTime)
            {
                gameboy.Draw(_spriteBatch, ((float)stopwatch.Elapsed.TotalSeconds - timeForLogo) * 1 / transitionTime + 1);
            }
            else if (gs == GameState.startscreen && stopwatch.Elapsed.TotalSeconds < transitionTime)
            {
                gameboy.Draw(_spriteBatch, 1 - ((float)stopwatch.Elapsed.TotalSeconds) * 1 / transitionTime);
            }
            else
            {
                gameboy.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void ScreenChange(object sender, EventArgs e)
        {
            //int height = GetNearestMultiple(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 144);
            //if (height > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            //{
            //    height -= 144;
            //}
            //float ratio = height / 144;
            //int width = (int)Math.Round(ratio * 160);
            //_graphics.PreferredBackBufferWidth = width;
            //_graphics.PreferredBackBufferHeight = height;
            //_graphics.ApplyChanges();
            if (Window.ClientBounds.Width < 160 || Window.ClientBounds.Height < 144)
            {
                if (Window.ClientBounds.Width < 160)
                {
                    _graphics.PreferredBackBufferWidth = 160;
                }
                if (Window.ClientBounds.Height < 144)
                {
                    _graphics.PreferredBackBufferHeight = 144;
                }
                _graphics.ApplyChanges();
            }
            gameboy.screenSize = ScreenSize();
        }

        private Rectangle ScreenSize()
        {
            Rectangle rectangle;
            int height = GetNearestMultiple(Window.ClientBounds.Height, 144);
            if (height > Window.ClientBounds.Height && height > 144)
            {
                height -= 144;
            }
            else if (height < 144)
            {
                height = 144;
            }
            int ratio = height / 144;
            int width = (ratio * 160);
            if (width > Window.ClientBounds.Width)
            {
                width = GetNearestMultiple(Window.ClientBounds.Width, 160);
                if (width > Window.ClientBounds.Width && width > 160)
                {
                    width -= 160;
                }
                else if (width < 160)
                {
                    width = 160;
                }
                ratio = width / 160;
                height = ratio * 144;
            }
            return new Rectangle(Window.ClientBounds.Width / 2 - width / 2, Window.ClientBounds.Height / 2 - height / 2, width, height);
        }

        private int MaxHeight()
        {
            int height = GetNearestMultiple(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 144);
            if (height > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            {
                height -= 144;
            }
            return height - 144;
        }

        private int MaxHeight(Screen screen)
        {
            int height = GetNearestMultiple(screen.Bounds.Height, 144);
            if (height > screen.Bounds.Height)
            {
                height -= 144;
            }
            return height - 144;
        }

        private int GetNearestMultiple(int value, int factor)
        {
            return (int)Math.Round(
                              (value / (double)factor),
                              MidpointRounding.AwayFromZero
                          ) * factor;
        }
    }

    internal class RetroScreen
    {
        public RenderTarget2D renderTarget { private set; get; }
        public Rectangle screenSize;

        public RetroScreen(RenderTarget2D _renderTarget, Rectangle _rectangle)
        {
            renderTarget = _renderTarget;
            screenSize = _rectangle;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(renderTarget, screenSize, Color.White);
        }

        public void Draw(SpriteBatch _spriteBatch, float lerpAmount)
        {
            _spriteBatch.Draw(renderTarget, screenSize, Color.Lerp(Color.White, Color.Black, lerpAmount));
        }
    }

    internal class Background
    {
        public Texture2D texture { private set; get; }
        public Rectangle rectangle { private set; get; }

        public Background(Texture2D _texture, Rectangle _rectangle)
        {
            texture = _texture;
            rectangle = _rectangle;
        }

        public void SetTex(Texture2D _texture)
        {
            texture = _texture;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(texture, rectangle, Color.White);
        }
    }
}
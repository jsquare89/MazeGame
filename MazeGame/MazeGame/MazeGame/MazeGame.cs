using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MazeGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MazeGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Camera camera;
		Maze maze;
        Effect effect;

        // Input handling
        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;

        // fog and day/night settings
        bool fog;
        bool ambient;
        bool flash;
        Color skyColor;

		public MazeGame()
		{
			graphics = new GraphicsDeviceManager( this );
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Initialize game variables/objects
		/// </summary>
		protected override void Initialize()
		{
            // Setup window
            Window.Title = "MazeGame";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            // Setup camera
			camera = new Camera(
				new Vector3(0.5f, 0.5f, 0.5f),
				0,
				GraphicsDevice.Viewport.AspectRatio,
				0.05f,
				100f);

            // Other variables to initialize
            maze = new Maze(GraphicsDevice);

            // Set default feature states
            fog = false;
            ambient = true;
            skyColor = Color.DeepSkyBlue;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch( GraphicsDevice );

            // Load shader and set defaults
            effect = Content.Load<Effect>("shader");
            effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.65f, 0.65f, 0.6f, 1.0f));
            effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(false);
            effect.Parameters["light"].StructureMembers["radius"].SetValue(0.1f);
            
			// TODO: use this.Content to load your game content here
			maze.LoadContent(Content);

		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update( GameTime gameTime )
		{

            // Handle input for camera movement and feature toggle
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            camera.initializeInput();
            camera.HandleKeyboadInput(currentKeyboardState, gameTime);
            camera.HandleGamePadInput(currentGamePadState, gameTime);
            HandleInput(currentKeyboardState, currentGamePadState, gameTime);

            // update camera movement and collision based on input
            camera.Update(maze);

			base.Update( gameTime );
		}

        private void HandleInput(KeyboardState currentKeyboardState, GamePadState currentGamePadState,GameTime gameTime)
        {
            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();
            // toggle features on keyboard/gamepad input
            FogToggle(currentKeyboardState, currentGamePadState);
            AmbientToggle(currentKeyboardState, currentGamePadState);
            FlashToggle(currentKeyboardState, currentGamePadState);

            // save previous states
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;
            
        }

        /// <summary>
        /// Handles Toggle Fog on and off 
        /// </summary>
        /// <param name="currentKeyboardState">Current Keyboard state</param>
        /// <param name="currentGamePadState">Current GamePad state</param>
        protected void FogToggle(KeyboardState currentKeyboardState, GamePadState currentGamePadState)
        {
            
            if ((previousKeyboardState.IsKeyDown(Keys.Q) &&
                currentKeyboardState.IsKeyUp(Keys.Q)) ||
                (previousGamePadState.IsButtonDown(Buttons.X) &&
                currentGamePadState.IsButtonUp(Buttons.X)))
            {
                fog = !fog;

                // toggle Fog on/off
                if (fog)
                {
                    effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(true);
                }
                else
                {
                    effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(false);
                }
            }

        }

        protected void AmbientToggle(KeyboardState currentKeyboardState, GamePadState currentGamePadState)
        {
            // toggle Ambient - Night/Day effect
            if (previousKeyboardState.IsKeyDown(Keys.E) &&
                currentKeyboardState.IsKeyUp(Keys.E) ||
                previousGamePadState.IsButtonDown(Buttons.RightShoulder) &&
                currentGamePadState.IsButtonUp(Buttons.RightShoulder))
            {
                ambient = !ambient;

                // Ambient true sets to day time effect, false sets to night time effect
                if(ambient)
                {
                    skyColor = Color.DeepSkyBlue;
                    effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.65f, 0.65f, 0.6f, 1.0f));
                }
                else
                {
                    skyColor = Color.DarkSlateGray;
                    effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.1f, 0.1f, 0.15f, 1.0f));
                }
                
            }
        }

        protected void FlashToggle(KeyboardState currentKeyboardState, GamePadState currentGamePadState)
        {
            if ((previousKeyboardState.IsKeyDown(Keys.F) &&
                currentKeyboardState.IsKeyUp(Keys.F)) ||
                (previousGamePadState.IsButtonDown(Buttons.LeftShoulder) &&
                currentGamePadState.IsButtonUp(Buttons.LeftShoulder)))
            {
                flash = !flash;

                // toggle Fog on/off
                if (flash)
                {
                    effect.Parameters["light"].StructureMembers["radius"].SetValue(12.0f);
                }
                else
                {
                    effect.Parameters["light"].StructureMembers["radius"].SetValue(0.1f);
                }
            }
            
        }


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear(skyColor);

			maze.Draw(camera, effect);

			base.Draw( gameTime );
		}
	}
}

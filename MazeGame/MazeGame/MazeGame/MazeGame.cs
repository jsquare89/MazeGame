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
		BasicEffect effect;
		BasicEffect wallEffect;

        Vector3 moveAmount = Vector3.Zero;
		float moveScale = 1.5f;
		float rotateScale = MathHelper.PiOver2;

        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;

        bool zoom = false;
        bool fog = false;
        bool collision;
        bool night = false;

		public MazeGame()
		{
			graphics = new GraphicsDeviceManager( this );
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
            // Setup window
            Window.Title = "MazeGame";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

			camera = new Camera(
				new Vector3(0.5f, 0.5f, 0.5f),
				0,
				GraphicsDevice.Viewport.AspectRatio,
				0.05f,
				100f);
			effect = new BasicEffect(GraphicsDevice);
			wallEffect = new BasicEffect(GraphicsDevice);
			maze = new Maze(GraphicsDevice);
            collision = true;
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
            
            HandleInput(gameTime);

			if (moveAmount.Z != 0 || moveAmount.X !=0)
			{
				Vector3 newLocation = camera.PreviewMove(moveAmount);
				bool moveOk = true;

				if (newLocation.X < 0 || newLocation.X > Maze.MAZE_WIDTH)
					moveOk = false;
				if (newLocation.Z < 0 || newLocation.Z > Maze.MAZE_HEIGHT)
					moveOk = false;

                if(collision)
                foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                {
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                        moveOk = false;
                }

				if (moveOk)
					camera.MoveForward(moveAmount);
			}

			base.Update( gameTime );
		}

        private void HandleInput(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // Handle Input
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            moveAmount = Vector3.Zero;

            // Move Forward
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W) ||
                currentGamePadState.IsButtonDown(Buttons.LeftThumbstickUp))
            {
                //camera.MoveForward(moveScale * elapsed);
                moveAmount.Z = moveScale * elapsed;
            }

            // Move Backward
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S) ||
                currentGamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
            {
                //camera.MoveForward(-moveScale * elapsed);
                moveAmount.Z = -moveScale * elapsed;
            }

            // Strafe Left
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A) ||
                currentGamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
            {
                moveAmount.X = moveScale * elapsed;
                //camera.Rotation = MathHelper.WrapAngle(
                //camera.Rotation + (rotateScale * elapsed));
            }

            // Strafe Right
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D) ||
                currentGamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
            {
                moveAmount.X = -moveScale * elapsed;
                //camera.Rotation = MathHelper.WrapAngle(
                //	camera.Rotation - (rotateScale * elapsed));
            }

            // Look Left
            if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickLeft) ||
                currentKeyboardState.IsKeyDown(Keys.Left))
            {
                camera.RotationX = MathHelper.WrapAngle(
                    camera.RotationX + (rotateScale * elapsed));
            }

            // Look Right
            if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickRight) ||
                currentKeyboardState.IsKeyDown(Keys.Right))
            {
                camera.RotationX = MathHelper.WrapAngle(
                    camera.RotationX - (rotateScale * elapsed));
            }

            // Look Up
            if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickUp))
            {
                camera.RotationY = MathHelper.WrapAngle(
                    camera.RotationY + (rotateScale * elapsed));
            }

            // Look Down
            if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickDown))
            {
                camera.RotationY = MathHelper.WrapAngle(
                    camera.RotationY - (rotateScale * elapsed));
            }  
       
            // toggle Fog on/off
            if( (previousKeyboardState.IsKeyDown(Keys.Q) &&
                currentKeyboardState.IsKeyUp(Keys.Q)) ||
                (previousGamePadState.IsButtonDown(Buttons.X) &&
                currentGamePadState.IsButtonUp(Buttons.X)))
            {
                fog = !fog;
                if (fog)
                {
                    effect.FogEnabled = true;
                    effect.FogColor = Color.Black.ToVector3();
                    effect.FogStart = 0.2f;
                    effect.FogEnd = 2f;
                }
                else
                {
                    effect.FogEnabled = false;
                }
            }
            
            // toggle Collision on/off - using C instead of W
            if ((previousKeyboardState.IsKeyDown(Keys.C) &&
                currentKeyboardState.IsKeyUp(Keys.C)) ||
                (previousGamePadState.IsButtonDown(Buttons.Y) &&
                currentGamePadState.IsButtonUp(Buttons.Y)))
            {
                collision = !collision;
            }

            // toggle Zoom on/off
            if (currentKeyboardState.IsKeyDown(Keys.F) ||
                currentGamePadState.IsButtonDown(Buttons.RightStick))
            {
                zoom = !zoom;
                
            }

            // toggle Night/Day
            if (currentKeyboardState.IsKeyDown(Keys.E) ||
                currentGamePadState.IsButtonDown(Buttons.B))
            {
                night = !night;
            }

            // Reset position to start position
            if (currentKeyboardState.IsKeyDown(Keys.Home) ||
                currentGamePadState.IsButtonDown(Buttons.Start))
            {
                camera.resetPosition();
            }

            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;
        }

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.CornflowerBlue );

			maze.Draw(camera, effect, wallEffect);

			base.Draw( gameTime );
		}
	}
}

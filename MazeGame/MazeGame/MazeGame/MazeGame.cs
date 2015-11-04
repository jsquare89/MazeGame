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

        Vector3 moveAmount = Vector3.Zero;
		float moveScale = 1.5f;
		float rotateScale = MathHelper.PiOver2;

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
            KeyboardState keyState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            moveAmount = Vector3.Zero;

            // Move Forward
            if (keyState.IsKeyDown(Keys.Up) ||
                keyState.IsKeyDown(Keys.W) ||
                gamePadState.IsButtonDown(Buttons.LeftThumbstickUp))
            {
                //camera.MoveForward(moveScale * elapsed);
                moveAmount.Z = moveScale * elapsed;
            }

            // Move Backward
            if (keyState.IsKeyDown(Keys.Down) ||
                keyState.IsKeyDown(Keys.S) ||
                gamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
            {
                //camera.MoveForward(-moveScale * elapsed);
                moveAmount.Z = -moveScale * elapsed;
            }

            // Strafe Left
            if (keyState.IsKeyDown(Keys.Left) ||
                keyState.IsKeyDown(Keys.A) ||
                gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
            {
                moveAmount.X = moveScale * elapsed;
                //camera.Rotation = MathHelper.WrapAngle(
                //camera.Rotation + (rotateScale * elapsed));
            }

            // Strafe Right
            if (keyState.IsKeyDown(Keys.Right) ||
                keyState.IsKeyDown(Keys.D) ||
                gamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
            {
                moveAmount.X = -moveScale * elapsed;
                //camera.Rotation = MathHelper.WrapAngle(
                //	camera.Rotation - (rotateScale * elapsed));
            }

            // Look Left
            if (gamePadState.IsButtonDown(Buttons.RightThumbstickLeft) ||
                keyState.IsKeyDown(Keys.Left))
            {
                camera.RotationX = MathHelper.WrapAngle(
                    camera.RotationX + (rotateScale * elapsed));
            }

            // Look Right
            if (gamePadState.IsButtonDown(Buttons.RightThumbstickRight) ||
                keyState.IsKeyDown(Keys.Right))
            {
                camera.RotationX = MathHelper.WrapAngle(
                    camera.RotationX - (rotateScale * elapsed));
            }

            // Look Up
            if (gamePadState.IsButtonDown(Buttons.RightThumbstickUp))
            {
                camera.RotationY = MathHelper.WrapAngle(
                    camera.RotationY + (rotateScale * elapsed));
            }

            // Look Down
            if (gamePadState.IsButtonDown(Buttons.RightThumbstickDown))
            {
                camera.RotationY = MathHelper.WrapAngle(
                    camera.RotationY - (rotateScale * elapsed));
            }  
       
            // toggle Fog on/off
            if( keyState.IsKeyDown(Keys.Q) ||
                gamePadState.IsButtonDown(Buttons.X))
            {
                fog = !fog;
            }
            
            // toggle Collision on/off - using C instead of W
            if (keyState.IsKeyDown(Keys.C) ||
                gamePadState.IsButtonDown(Buttons.Y))
            {
                collision = !collision;
            }

            // toggle Zoom on/off
            if (keyState.IsKeyDown(Keys.F) ||
                gamePadState.IsButtonDown(Buttons.RightStick))
            {
                zoom = !zoom;
            }

            // toggle Night/Day
            if (keyState.IsKeyDown(Keys.E) ||
                gamePadState.IsButtonDown(Buttons.B))
            {
                zoom = !zoom;
            }
            
        }

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.CornflowerBlue );

            {
                
                effect.FogEnabled = true;
                effect.FogColor = Color.Black.ToVector3();
                effect.FogStart = 1f;
                effect.FogEnd = 100f;
            }

			// TODO: Add your drawing code here
			maze.Draw(camera, effect);

            //if(fog)
            

			base.Draw( gameTime );
		}
	}
}

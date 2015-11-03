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

		float moveScale = 1.5f;
		float rotateScale = MathHelper.PiOver2;

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
			// TODO: Add your initialization logic here
			camera = new Camera(
				new Vector3(0.5f, 0.5f, 0.5f),
				MathHelper.PiOver4,
				GraphicsDevice.Viewport.AspectRatio,
				0.05f,
				100f);
			effect = new BasicEffect(GraphicsDevice);
			maze = new Maze(GraphicsDevice);

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
			// Allows the game to exit
			if ( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed )
				this.Exit();

			// TODO: Add your update logic here
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
			KeyboardState keyState = Keyboard.GetState();
			float moveAmount = 0;

			if (keyState.IsKeyDown(Keys.Right))
			{
				camera.Rotation = MathHelper.WrapAngle(
					camera.Rotation - (rotateScale * elapsed));
			}

			if (keyState.IsKeyDown(Keys.Left))
			{
				camera.Rotation = MathHelper.WrapAngle(
					camera.Rotation + (rotateScale * elapsed));
			}

			if (keyState.IsKeyDown(Keys.Up))
			{
				//camera.MoveForward(moveScale * elapsed);
				moveAmount = moveScale * elapsed;
			}

			if (keyState.IsKeyDown(Keys.Down))
			{
				//camera.MoveForward(-moveScale * elapsed);
				moveAmount = -moveScale * elapsed;
			}

			if (moveAmount != 0)
			{
				Vector3 newLocation = camera.PreviewMove(moveAmount);
				bool moveOk = true;

				if (newLocation.X < 0 || newLocation.X > Maze.MAZE_WIDTH)
					moveOk = false;
				if (newLocation.Z < 0 || newLocation.Z > Maze.MAZE_HEIGHT)
					moveOk = false;
				
				if (moveOk)
					camera.MoveForward(moveAmount);
			}

			base.Update( gameTime );
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear( Color.CornflowerBlue );

			// TODO: Add your drawing code here
			maze.Draw(camera, effect);

			base.Draw( gameTime );
		}
	}
}

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
        // debug purpose to toggle walls
        bool drawWall = true;
		Enemy enemy;
        Effect effect;

        // Input handling
        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;

        // fog and day/night settings
        bool fog;
        bool ambient;
        bool flash;
        Color skyColor;

        // Sounds & Music
        bool music; // used to turn on and off
        
        SoundEffectInstance currentMusic;
        SoundEffect wallCollisionAudio;
        SoundEffect leftFootStepSound;
        SoundEffect rightFootStepSound;
        SoundEffect musicDay;
        SoundEffect musicNight;
        AudioListener listener;



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


            SoundEffect.DistanceScale = 1;
            SoundEffect.DopplerScale = 0.1f;

            // initialise sound for camera
            wallCollisionAudio = Content.Load<SoundEffect>("Sounds/wallcollision_sound");

            // Other variables to initialize
            maze = new Maze(GraphicsDevice);

            // Set default feature states
            fog = false;
            ambient = true;
            skyColor = Color.DeepSkyBlue;
            listener = new AudioListener();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{

            // initialise sound for camera
            wallCollisionAudio = Content.Load<SoundEffect>("Sounds/wallcollision_sound");
            leftFootStepSound = Content.Load<SoundEffect>("Sounds/leftfootstep_sound");
            rightFootStepSound = Content.Load<SoundEffect>("Sounds/rightfootstep_sound");

            // Setup camera
            camera = new Camera(
                new Vector3(0.5f, 0.5f, 0.5f),
                0,
                GraphicsDevice.Viewport.AspectRatio,
                0.05f,
                100f, wallCollisionAudio,
                leftFootStepSound,
                rightFootStepSound);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch( GraphicsDevice );

            // Load shader and set defaults
            effect = Content.Load<Effect>("shader");
            effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.65f, 0.65f, 0.6f, 1.0f));
            effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(false);
            effect.Parameters["light"].StructureMembers["radius"].SetValue(0.1f);
            
            // Load sound effects
            wallCollisionAudio = Content.Load<SoundEffect>("Sounds/wallcollision_sound");
            leftFootStepSound = Content.Load<SoundEffect>("Sounds/leftfootstep_sound");
            rightFootStepSound = Content.Load<SoundEffect>("Sounds/rightfootstep_sound");

            musicDay = Content.Load<SoundEffect>("Sounds/medievil_music");
            musicNight = Content.Load<SoundEffect>("Sounds/medievil_music2");
            currentMusic = musicDay.CreateInstance();
            currentMusic.Volume = 1f;
            currentMusic.IsLooped = true;
            music = true;
            
			maze.LoadContent(Content);
            
            // Setup camera
            camera = new Camera(
				new Vector3(0.5f, 0.5f, 0.5f),
				0,
				GraphicsDevice.Viewport.AspectRatio,
				0.05f,
				100f, 
                wallCollisionAudio,
                leftFootStepSound,
                rightFootStepSound);

			enemy = new Enemy(this, GraphicsDevice, 
                Content.Load<Model>("enemy"), 
                new Vector3(2.5f, 0, 2.5f),
                leftFootStepSound,
                rightFootStepSound);
            enemy.Apply3DAudio(listener, enemy.Position, currentMusic);
            currentMusic.Play();
           
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
            camera.Update( gameTime, maze, listener );
            enemy.Update( gameTime, maze, listener, currentMusic );

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
            MusicToggle(currentKeyboardState, currentGamePadState);
            WallToggle(currentKeyboardState, currentGamePadState);

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
                    // enable fog
                    effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(true);
                    // if music is on then half volume set while there is fog
                    if (music) 
                        currentMusic.Volume = 0.5f;
                }
                else
                {
                    //diable fog
                    effect.Parameters["fog"].StructureMembers["FogEnabled"].SetValue(false);
                    // if music is on then full volume while there is no fog
                    if (music)
                        currentMusic.Volume = 1f;
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
                
                currentMusic.Stop();
                currentMusic.Dispose();

                // Ambient true sets to day time effect, false sets to night time effect
                if(ambient)
                {
                    skyColor = Color.DeepSkyBlue;
                    effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.65f, 0.65f, 0.6f, 1.0f));
                    currentMusic = musicDay.CreateInstance();
                    
                }
                else
                {
                    skyColor = Color.DarkSlateGray;
                    effect.Parameters["material"].StructureMembers["ambient"].SetValue(new Vector4(0.1f, 0.1f, 0.15f, 1.0f));
                    currentMusic = musicNight.CreateInstance();
                }
                enemy.Apply3DAudio(listener, enemy.Position, currentMusic);
                currentMusic.Play();
                
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

        protected void MusicToggle(KeyboardState currentKeyboardState, GamePadState currentGamePadState)
        {

            if ((previousKeyboardState.IsKeyDown(Keys.X) &&
                currentKeyboardState.IsKeyUp(Keys.X)) ||
                (previousGamePadState.IsButtonDown(Buttons.RightStick) &&
                currentGamePadState.IsButtonUp(Buttons.RightStick)))
            {
                music = !music;

                // toggle music on/off
                if (music)
                {
                    currentMusic.Volume = 1f;
                }
                else
                {
                    currentMusic.Volume = 0f;
                }
            }

        }

        protected void WallToggle(KeyboardState currentKeyboardState, GamePadState currentGamePadState)
        {

            if ((previousKeyboardState.IsKeyDown(Keys.R) &&
                currentKeyboardState.IsKeyUp(Keys.R)) ||
                (previousGamePadState.IsButtonDown(Buttons.DPadDown) &&
                currentGamePadState.IsButtonUp(Buttons.DPadDown)))
            {
                drawWall = !drawWall;
            }

        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
		{
			GraphicsDevice.Clear(skyColor);

			maze.Draw(camera, effect, drawWall);
			enemy.Draw(camera);

			base.Draw( gameTime );
		}
	}
}

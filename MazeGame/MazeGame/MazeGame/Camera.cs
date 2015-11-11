using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace MazeGame
{
    class Camera
	{
		private float zoom = 1;
		private Vector3 position = Vector3.Zero;
        private Vector3 initialPosition;
        private float initialRotation;
		private float rotationX;
        private float rotationY;
		private Vector3 lookAt;
		private Vector3 baseCameraReference = new Vector3(0, 0, 1);
		private bool needViewResync = true;
		private Matrix cachedViewMatrix;

        // used for zoom function
        float zoomScale = 1.5f;

        // Camera Setup
        float aspectRatio;
        float nearClip;
        float farClip;

        const float MAX_Y= MathHelper.PiOver2;
        const float MIN_Y = MathHelper.PiOver2;

        // Input handling
        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;

        // Movement and rotation handling
        Vector3 moveAmount;
        float moveScale = 1.5f;
        float rotateScale = MathHelper.PiOver2;
        bool collision;

        /// <summary>
        /// Changes the Field of view to zoom in and out
        /// </summary>
        public float Zoom
        {
            get
            {
                return zoom;
            }
            set
            {
                // Zoom factor between 1 and 4
                zoom = MathHelper.Min(4, MathHelper.Max(1, value));
                // create new zoom projection
                Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.PiOver4 / Zoom,
                        aspectRatio,
                        nearClip,
                        farClip);
            }
        }

		public Matrix Projection
		{
			get; private set;
		}

		public Vector3 Position
		{
			get
			{
				return position;		
			}
			set
			{
				position = value;
				UpdateLookAt();		
			}
		}

		public float RotationX
		{
			get
			{
				return rotationX;		
			}	
			set
			{
				rotationX = value;
				UpdateLookAt();
			}
		}

        public float RotationY
        {
            get
            {
                return rotationY;
            }
            set
            {
                rotationY = MathHelper.Max(-MathHelper.PiOver4, MathHelper.Min(value, MathHelper.PiOver4));
                UpdateLookAt();
            }
        }

		public Matrix View
		{
			get
			{
				if ( needViewResync )
				{
					cachedViewMatrix = Matrix.CreateLookAt(
						Position,
						lookAt,
						Vector3.Up);
					cachedViewMatrix *= Matrix.CreateRotationX(-rotationY);
				}
					
				return cachedViewMatrix;
			}
		}



        /// <summary>
        /// Constructor for the camera, creates projection
        /// </summary>
        /// <param name="position">initial position of the camera</param>
        /// <param name="rotation">rotation</param>
        /// <param name="aspectRatio">aspect ratio determines zoom fisheye or telephoto</param>
        /// <param name="nearClip">near clip plane for rendring</param>
        /// <param name="farClip">far clip plane for rendering</param>
		public Camera(Vector3 position, float rotation, float aspectRatio, float nearClip, float farClip)
        {
			// Setup camera projection
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4 / Zoom,
                aspectRatio,
                nearClip,
                farClip);

            //Store local variables for later use. recalculating zoom and resetting initial position
            this.aspectRatio = aspectRatio;
            this.nearClip = nearClip;
            this.farClip = farClip;
            rotationX = rotation;

			MoveTo(position, rotation);

            initialPosition = position;
            initialRotation = rotation;

            // Set collision 
            collision = true;
		}


        /// <summary>
        /// Moves camera position
        /// </summary>
        /// <param name="position">position to move to</param>
        /// <param name="rotation">rotation to rotate camera by</param>
		public void MoveTo( Vector3 position, float rotation )
		{
			this.position = position;
			this.rotationX = rotation;
			UpdateLookAt();
		}

        /// <summary>
        /// Update the camera look at position based on rotation
        /// </summary>
		private void UpdateLookAt()
		{
			Matrix rotationMatrix = Matrix.CreateRotationY(rotationX);
			Vector3 lookAtOffset = Vector3.Transform(
				baseCameraReference,
				rotationMatrix);
			lookAt = position + lookAtOffset;

            // Resync view to reflect camera changes
			needViewResync = true;
		}
		
        /// <summary>
        /// Calvulates the camera movement and used to detect whether there will be collision before actually making the camera move
        /// </summary>
        /// <param name="movement"></param>
        /// <returns></returns>
		public Vector3 PreviewMove(Vector3 movement)
		{
			Matrix rotate = Matrix.CreateRotationY(rotationX);
			//Vector3 forward = new Vector3(0, 0, scale);
			movement = Vector3.Transform(movement, rotate);
            
			return (position + movement);
		}

        /// <summary>
        /// Move camera forward by movement vector
        /// </summary>
        /// <param name="movement">the amount and direction the camera should move forward by</param>
		public void MoveForward(Vector3 movement)
		{
			MoveTo(PreviewMove(movement), rotationX);	
		}

        /// <summary>
        /// Resest position to the initial position the was stored when the camera was first created
        /// </summary>
        public void resetPosition()
        {
            MoveTo(initialPosition, initialRotation);
        }

        /// <summary>
        /// Uses a pair of keys to simulate a positive or negative axis input.
        /// </summary>
        public static float ReadKeyboardAxis(KeyboardState keyState, Keys downKey,
            Keys upKey)
        {
            float value = 0;

            if (keyState.IsKeyDown(downKey))
                value -= 1.0f;

            if (keyState.IsKeyDown(upKey))
                value += 1.0f;

            return value;
        }

        /// <summary>
        /// initialize before handling input. set moveAmount to zero vector
        /// </summary>
        public void initializeInput()
        {
            moveAmount = Vector3.Zero;
        }


        public void HandleKeyboadInput(KeyboardState currentKeyboardState, GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;            

            // Move Forward
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                moveAmount.Z = moveScale * elapsedTime;
            }

            // Move Backward
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                moveAmount.Z = -moveScale * elapsedTime;
            }

            // Strafe Left
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                moveAmount.X = moveScale * elapsedTime;
            }

            // Strafe Right
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                moveAmount.X = -moveScale * elapsedTime;
            }

            // Look Left
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                RotationX = MathHelper.WrapAngle(
                    RotationX + (rotateScale * elapsedTime));
            }

            // Look Right
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                RotationX = MathHelper.WrapAngle(RotationX - (rotateScale * elapsedTime));
            }

            // Look Up
            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                RotationY = MathHelper.WrapAngle(RotationY + (rotateScale * elapsedTime));
            }

            // Look Down
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                RotationY = MathHelper.WrapAngle(RotationY - (rotateScale * elapsedTime));
            }

            // toggle Collision on/off - using C instead of W
            if ((previousKeyboardState.IsKeyDown(Keys.C) && currentKeyboardState.IsKeyUp(Keys.C)))
                collision = !collision;

            // Zoom in/out for keyboard
            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                if ((currentKeyboardState.IsKeyDown(Keys.LeftShift) ||
                    currentKeyboardState.IsKeyDown(Keys.RightShift)))
                {
                    Zoom -= zoomScale * elapsedTime;
                }
                else
                {
                    Zoom += zoomScale * elapsedTime;
                }
            }

            // Reset position to start position
            if (currentKeyboardState.IsKeyDown(Keys.Home))
                resetPosition();

            previousKeyboardState = currentKeyboardState;
        }

        public void HandleGamePadInput(GamePadState currentGamePadState, GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (currentGamePadState.IsConnected)
            {
                // Move Forward
                if (currentGamePadState.IsButtonDown(Buttons.LeftThumbstickUp))
                {
                    moveAmount.Z = moveScale * elapsedTime;
                }

                // Move Backward
                if (currentGamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
                {
                    moveAmount.Z = -moveScale * elapsedTime;
                }

                // Strafe Left
                if (currentGamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                {
                    moveAmount.X = moveScale * elapsedTime;
                }

                // Strafe Right
                if (currentGamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
                {
                    moveAmount.X = -moveScale * elapsedTime;
                }

                // Look Left
                if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickLeft))
                {
                    RotationX = MathHelper.WrapAngle(RotationX + (rotateScale * elapsedTime));
                }

                // Look Right
                if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickRight))
                {
                    RotationX = MathHelper.WrapAngle(RotationX - (rotateScale * elapsedTime));
                }

                // Look Up
                if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickUp))
                {
                    RotationY = MathHelper.WrapAngle(RotationY + (rotateScale * elapsedTime));
                }

                // Look Down
                if (currentGamePadState.IsButtonDown(Buttons.RightThumbstickDown))
                {
                    RotationY = MathHelper.WrapAngle(RotationY - (rotateScale * elapsedTime));
                }

                // toggle Collision on/off - using C instead of W
                if (previousGamePadState.IsButtonDown(Buttons.Y) && currentGamePadState.IsButtonUp(Buttons.Y))
                    collision = !collision;

                // Zoom out for gamepad
                if (currentGamePadState.IsButtonDown(Buttons.A))
                    Zoom -= zoomScale * elapsedTime;
                // Zoom in for gamepad
                if (currentGamePadState.IsButtonDown(Buttons.B))
                    Zoom += zoomScale * elapsedTime;

                // Reset position to start position
                if (currentGamePadState.IsButtonDown(Buttons.Start))
                    resetPosition();

                previousGamePadState = currentGamePadState;
            }
        }

        public void Update(Maze maze)
        {
            // Restrain movement to the maze width and height
            if (moveAmount.Z != 0 || moveAmount.X != 0)
            {
                Vector3 newLocation = PreviewMove(moveAmount);
                bool moveOk = true;

                if (newLocation.X < 0 || newLocation.X > Maze.MAZE_WIDTH)
                    moveOk = false;
                if (newLocation.Z < 0 || newLocation.Z > Maze.MAZE_HEIGHT)
                    moveOk = false;

                // Handle Collision based on bounding box
                if (collision)
                    foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                    {
                        if (box.Contains(newLocation) == ContainmentType.Contains)
                            moveOk = false;
                    }

                if (moveOk)
                    MoveForward(moveAmount);
            }
        }
        
    }
}

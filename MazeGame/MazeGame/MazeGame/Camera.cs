using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
    class Camera
	{
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
        float scale;

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
                rotationY = value;
                UpdateLookAt();
            }
        }

		public Matrix View
		{
			get
			{
				if (needViewResync)
					cachedViewMatrix = Matrix.CreateLookAt(
						Position,
						lookAt,
						Vector3.Up);
				return cachedViewMatrix;
			}
		}

		public Camera(Vector3 position, float rotation, float aspectRatio, float nearClip, float farClip) {
			Projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.PiOver4,
				aspectRatio,
				nearClip,
				farClip);
			MoveTo(position, rotation);

            initialPosition = position;
            initialRotation = rotation;
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
			Matrix rotationMatrix = Matrix.CreateRotationY(rotationX);// * Matrix.CreateRotationX(rotationY);
			Vector3 lookAtOffset = Vector3.Transform(
				baseCameraReference,
				rotationMatrix);
			lookAt = position + lookAtOffset;
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
	}
}

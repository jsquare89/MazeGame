﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
    class Camera
	{
		private Vector3 position = Vector3.Zero;
		private float rotation;
		private Vector3 lookAt;
		private Vector3 baseCameraReference = new Vector3(0, 0, 1);
		private bool needViewResync = true;
		private Matrix cachedViewMatrix;

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

		public float Rotation
		{
			get
			{
				return rotation;		
			}	
			set
			{
				rotation = value;
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
		}


		public void MoveTo( Vector3 position, float rotation )
		{
			this.position = position;
			this.rotation = rotation;
			UpdateLookAt();
		}


		private void UpdateLookAt()
		{
			Matrix rotationMatrix = Matrix.CreateRotationY(rotation);
			Vector3 lookAtOffset = Vector3.Transform(
				baseCameraReference,
				rotationMatrix);
			lookAt = position + lookAtOffset;
			needViewResync = true;
		}
		
	}
}
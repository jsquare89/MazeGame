using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
	class Enemy
	{
        MazeGame game;
		GraphicsDevice device;

		private Model model;
        private Matrix[] boneTransforms;
        private Matrix leftLegTransform;
        private Matrix rightLegTransform;

        private Vector3 position;
		public Vector3 Position {
            get {
                return position;
            }
        }

        public int direction;
        public float Rotation {
            get {
                switch (direction) {
                case 0:
                    return 0;
                case 1:
                    return MathHelper.PiOver2;
                case 2:
                    return MathHelper.Pi;
                case 3:
                    return -MathHelper.PiOver2;
                default:
                    return 0;
                }
            }
        }

        private float leftLegRotation;
        private float rightLegRotation;
        private float legRotationScale = 2.4f;
        private float MIN_LEG_ROTATION = -30;
        private bool willCollide;
        private bool isLeftLegForward;
        private float moveScale = 0.6f;
        private float PREVIEW_FRAMES = 46f;

        private SoundEffect leftFootStepAudio;
        private SoundEffect rightFootStepAudio;

        TimeSpan timeDelay = TimeSpan.Zero;

		public Enemy(MazeGame game, GraphicsDevice device, Model model, Vector3 position, 
            SoundEffect leftFootStepAudio, SoundEffect rightFootStepAudio)
		{
            this.game = game;
			this.device = device;
            this.model = model;
			this.position = position;
            this.leftFootStepAudio = leftFootStepAudio;
            this.rightFootStepAudio = rightFootStepAudio;
            boneTransforms = new Matrix[model.Bones.Count];
            leftLegTransform = model.Bones["left_leg"].Transform;
            rightLegTransform = model.Bones["right_leg"].Transform;
		}

		public void LoadContent(ContentManager content)
        {
            // Load model
			model = content.Load<Model>("enemy");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="movement"></param>
        /// <returns></returns>
		public Vector3 PreviewMove(Vector3 movement)
		{
			Matrix r = Matrix.CreateRotationY(Rotation);
			movement = Vector3.Transform(movement, r);
            
			return (position + movement);
		}

        public void Update(GameTime gameTime, Maze maze, AudioListener listener, SoundEffectInstance music)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 moveAmount = Vector3.Zero;
            moveAmount.Z += moveScale * elapsedTime * PREVIEW_FRAMES;
            Vector3 newLocation = PreviewMove(moveAmount);
                
            willCollide = false;
            foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
            {
                if (box.Contains(newLocation) == ContainmentType.Contains)
                {
                    willCollide = true;
                }
            }

            if ( willCollide )
            {
                // try turn left
                direction = (direction + 1) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += moveScale * elapsedTime * PREVIEW_FRAMES;
                newLocation = PreviewMove(moveAmount);

                willCollide = false;
                foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                {
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                    {
                        willCollide = true;
                    }
                }
            }

            if ( willCollide )
            {
                // try turn right
                direction = (direction + 2) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += moveScale * elapsedTime * PREVIEW_FRAMES;
                newLocation = PreviewMove(moveAmount);

                willCollide = false;
                foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                {
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                    {
                        willCollide = true;
                    }
                }
            }

            if ( willCollide )
            {
                // try walk backward
                direction = (direction + 3) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += moveScale * elapsedTime * PREVIEW_FRAMES;
                newLocation = PreviewMove(moveAmount);

                willCollide = false;
                foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                {
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                    {
                        willCollide = true;
                    }
                }
            }
            
            // Animate legs movement
            if (isLeftLegForward)
            {
                leftLegRotation -= legRotationScale;
                rightLegRotation += legRotationScale;
            }
            else
            {
                rightLegRotation -= legRotationScale;
                leftLegRotation += legRotationScale;
            }

            moveAmount = Vector3.Zero;
            moveAmount.Z += moveScale * elapsedTime;
            newLocation = PreviewMove(moveAmount);
            
            Apply3DAudio(listener, newLocation, music);
                
            position = newLocation;
        }

        public void Apply3DAudio(AudioListener listener, Vector3 newLocation, SoundEffectInstance music)
        {
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = newLocation;
            emitter.Up = Vector3.Up;
            emitter.Velocity = newLocation - position;
            emitter.Forward = Vector3.Normalize(newLocation - position);
            music.Apply3D(listener, emitter);

            if ( leftLegRotation < MIN_LEG_ROTATION || rightLegRotation < MIN_LEG_ROTATION )
            {
                if (isLeftLegForward)
                {
                    SoundEffectInstance sfxInstance = leftFootStepAudio.CreateInstance();
                    sfxInstance.IsLooped = false;
                    sfxInstance.Apply3D(listener, emitter);
                    sfxInstance.Play();
                }
                else
                {
                    SoundEffectInstance sfxInstance = rightFootStepAudio.CreateInstance();
                    sfxInstance.IsLooped = false;
                    sfxInstance.Apply3D(listener, emitter);
                    sfxInstance.Play();
                }
                // Switch legs
                isLeftLegForward = !isLeftLegForward;
            }
        }

		public void Draw(Camera camera, Effect effect)
		{
            model.Root.Transform = Matrix.Identity *
                Matrix.CreateScale(0.001f) *
                Matrix.CreateRotationY(Rotation) *
                Matrix.CreateTranslation(Position);

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            model.Bones["left_leg"].Transform = Matrix.CreateRotationX(MathHelper.ToRadians(leftLegRotation)) * leftLegTransform;
            model.Bones["right_leg"].Transform = Matrix.CreateRotationX(MathHelper.ToRadians(rightLegRotation)) * rightLegTransform;

			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (BasicEffect e in mesh.Effects)
				{
					e.World = boneTransforms[mesh.ParentBone.Index];
					e.View = camera.View;
					e.Projection = camera.Projection;

                    e.EnableDefaultLighting();
				}

				mesh.Draw();
			}
		}
	}
}

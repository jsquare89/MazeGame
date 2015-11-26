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
    /// <summary>
    /// Enemy class represents the enemy object that implements basic AI that
    /// walks around the maze and also enemy model, sound effects, effecs on music volume.
    /// </summary>
	class Enemy
	{
        MazeGame game;
		GraphicsDevice device;

		private Model model;
        private Matrix[] boneTransforms;
        private Matrix leftLegTransform;
        private Matrix rightLegTransform;
        
        private float LEG_ROTATION_SCALE = 2.4f;
        private float MIN_LEG_ROTATION = -30;
        private float leftLegRotation;
        private float rightLegRotation;
        private bool isLeftLegForward;

        private float MOVE_SCALE = 0.6f;
        private float PREVIEW_FRAMES = 46f;
        
        private bool willCollide;

        private SoundEffect leftFootStepAudio;
        private SoundEffect rightFootStepAudio;
        
        private Vector3 position;
        /// <summary>
        /// Enemy's position
        /// </summary>
		public Vector3 Position {
            get {
                return position;
            }
        }

        /// <summary>
        /// Enemy's walking direction. 0 is North, 1 is East, 2 is South, 3 is West
        /// </summary>
        public int Direction;

        /// <summary>
        /// Rotation angle of enemy
        /// </summary>
        public float Rotation {
            get {
                switch (Direction) {
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

        TimeSpan timeDelay = TimeSpan.Zero;


        /// <summary>
        /// Enemy constructor that position enemy in the maze and assign loaded content such
        /// as model asset and audio.
        /// </summary>
        /// <param name="game">The maze game</param>
        /// <param name="device">grapfics device</param>
        /// <param name="model">model for enemy</param>
        /// <param name="position">initial position of enemy</param>
        /// <param name="leftFootStepAudio">left foot step sound for enemy</param>
        /// <param name="rightFootStepAudio">right foot step sound for enemy</param>
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
        
        /// <summary>
        /// Predict the location the enemy going to be by the given movement
        /// </summary>
        /// <param name="movement">The vector value that the enemy moves</param>
        /// <returns></returns>
		public Vector3 PreviewMove(Vector3 movement)
		{
            // makes sure we rotate to the direction where the enemy currently is facing at
			Matrix r = Matrix.CreateRotationY(Rotation);
			movement = Vector3.Transform(movement, r);
            
			return (position + movement);
		}

        /// <summary>
        /// Update the enemy location depending on AI logic and play sound effects
        /// </summary>
        /// <param name="gameTime">Snapshot of game timing state</param>
        /// <param name="maze">The maze the enemy is in</param>
        /// <param name="listener">The listener to enemy's footstep sound effect and music</param>
        /// <param name="music">The music that the enemy objec will be effecting depends on distance to the listener</param>
        public void Update(GameTime gameTime, Maze maze, AudioListener listener, SoundEffectInstance music)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 moveAmount = Vector3.Zero;

            // first we looking into future time so that enemy won't walk into walls
            moveAmount.Z += MOVE_SCALE * elapsedTime * PREVIEW_FRAMES;
            Vector3 newLocation = PreviewMove(moveAmount);
                
            willCollide = false;
            foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
            {
                if (box.Contains(newLocation) == ContainmentType.Contains)
                {
                    willCollide = true;
                }
            }

            // if walking stright will collide with wall, try turning left
            if ( willCollide )
            {
                // try turn left
                Direction = (Direction + 1) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += MOVE_SCALE * elapsedTime * PREVIEW_FRAMES;
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

            // if turn left fails, try opposite direction (turn right from original direction)
            if ( willCollide )
            {
                // try turn right
                Direction = (Direction + 2) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += MOVE_SCALE * elapsedTime * PREVIEW_FRAMES;
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

            // if still going to collide, make a u-turn
            if ( willCollide )
            {
                // try walk backward
                Direction = (Direction + 3) % 4;
                moveAmount = Vector3.Zero;
                moveAmount.Z += MOVE_SCALE * elapsedTime * PREVIEW_FRAMES;
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
                leftLegRotation -= LEG_ROTATION_SCALE;
                rightLegRotation += LEG_ROTATION_SCALE;
            }
            else
            {
                rightLegRotation -= LEG_ROTATION_SCALE;
                leftLegRotation += LEG_ROTATION_SCALE;
            }

            // reset move amount to just timing the elapsed time instead of a future time
            moveAmount = Vector3.Zero;
            moveAmount.Z += MOVE_SCALE * elapsedTime;
            newLocation = PreviewMove(moveAmount);
            
            Apply3DAudio(listener, newLocation, music);
                
            position = newLocation;
        }

        /// <summary>
        /// Helps apply the distance effect of our music and enemy foot step
        /// </summary>
        /// <param name="listener">The listener of the sound and music</param>
        /// <param name="location">The location that emits the sound and music</param>
        /// <param name="music"></param>
        public void Apply3DAudio(AudioListener listener, Vector3 location, SoundEffectInstance music)
        {
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = location;
            emitter.Up = Vector3.Up;
            emitter.Velocity = location - position;
            emitter.Forward = Vector3.Normalize(location - position);
            music.Apply3D(listener, emitter);

            if ( leftLegRotation < MIN_LEG_ROTATION || rightLegRotation < MIN_LEG_ROTATION )
            {
                // switch between left and right foot step sound
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

        /// <summary>
        /// Draw the enemy model
        /// </summary>
        /// <param name="camera">The camera that is seeing this model</param>
		public void Draw(Camera camera)
		{
            // apply transformation to whole model
            model.Root.Transform = Matrix.Identity *
                Matrix.CreateScale(0.001f) *
                Matrix.CreateRotationY(Rotation) *
                Matrix.CreateTranslation(Position);

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // rotate the enemy legs
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

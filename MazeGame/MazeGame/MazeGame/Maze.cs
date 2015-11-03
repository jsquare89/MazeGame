using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
	class Maze
	{
		public const int MAZE_WIDTH = 20;
		public const int MAZE_HEIGHT = 20;

		GraphicsDevice device;

		VertexBuffer floorBuffer;

		Color[] floorColors = new Color[2] { Color.White, Color.Gray };

		public Maze(GraphicsDevice device)
		{
			this.device = device;
			
			BuildFloorBuffer();
		}

		private void BuildFloorBuffer()
		{
			List<VertexPositionColor> vertexList = 
				new List<VertexPositionColor>();

			int counter = 0;

			for (int x = 0 ; x < MAZE_WIDTH ; x++)
			{
				counter++;
				for (int z = 0; z < MAZE_HEIGHT; z++) {
					counter++; 
					foreach ( VertexPositionColor vertex in
						FloorTile( x, z, floorColors[ counter % 2 ] ) )
					{
						vertexList.Add(vertex);
					}
				}
			}

			floorBuffer = new VertexBuffer(
				device,
				VertexPositionColor.VertexDeclaration,
				vertexList.Count,
				BufferUsage.WriteOnly);

			floorBuffer.SetData<VertexPositionColor>(vertexList.ToArray());
		}

		private List<VertexPositionColor> FloorTile(
			int xOffset,
			int zOffset,
			Color tileColor)
		{
			List<VertexPositionColor> vList = new List<VertexPositionColor>();
			vList.Add(new VertexPositionColor(
				new Vector3(0 + xOffset, 0, 0 + zOffset), tileColor));
			vList.Add(new VertexPositionColor(
				new Vector3(1 + xOffset, 0, 0 + zOffset), tileColor));
			vList.Add(new VertexPositionColor(
				new Vector3(0 + xOffset, 0, 1 + zOffset), tileColor));

			vList.Add(new VertexPositionColor(
				new Vector3(1 + xOffset, 0, 0 + zOffset), tileColor));
			vList.Add(new VertexPositionColor(
				new Vector3(1 + xOffset, 0, 1 + zOffset), tileColor));
			vList.Add(new VertexPositionColor(
				new Vector3(0 + xOffset, 0, 1 + zOffset), tileColor));

			return vList;
		}

		public void Draw( Camera camera, BasicEffect effect )
		{
			effect.VertexColorEnabled = true;
			effect.World = Matrix.Identity;
			effect.View = camera.View;
			effect.Projection = camera.Projection;

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.SetVertexBuffer(floorBuffer);
				device.DrawPrimitives(
					PrimitiveType.TriangleList,
					0,
					floorBuffer.VertexCount / 3);
			}
		}
	}
}

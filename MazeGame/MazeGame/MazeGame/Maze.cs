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

		private Random rand = new Random();
		public MazeCell[,] MazeCells = new MazeCell[MAZE_WIDTH, MAZE_HEIGHT];

		VertexBuffer wallBuffer;
		Vector3[] wallPoints = new Vector3[8];
		Color[] wallColors = new Color[4] { Color.Red, Color.Orange, Color.Blue, Color.Purple };


		public Maze(GraphicsDevice device)
		{
            this.device = device;

            BuildFloorBuffer();

            for (int x = 0; x < MAZE_WIDTH; x++)
                for (int z = 0; z < MAZE_HEIGHT; z++)
                {
                    MazeCells[x, z] = new MazeCell();
                }
            GenerateMaze();

            wallPoints[0] = new Vector3(0, 1, 0);
            wallPoints[1] = new Vector3(0, 1, 1);
            wallPoints[2] = new Vector3(0, 0, 0);
            wallPoints[3] = new Vector3(0, 0, 1);
            wallPoints[4] = new Vector3(1, 1, 0);
            wallPoints[5] = new Vector3(1, 1, 1);
            wallPoints[6] = new Vector3(1, 0, 0);
            wallPoints[7] = new Vector3(1, 0, 1);

            BuildWallBuffer();
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

		public void GenerateMaze()
		{
            for (int x = 0; x < MAZE_WIDTH; x++)
            {
                for (int z = 0; z < MAZE_HEIGHT; z++)
                {
                    MazeCells[x, z].Walls[0] = true;
                    MazeCells[x, z].Walls[1] = true;
                    MazeCells[x, z].Walls[2] = true;
                    MazeCells[x, z].Walls[3] = true;
                    MazeCells[x, z].Visited = false;
                }
            }

            MazeCells[0, 0].Visited = true;
            EvaluateCell(new Vector2(0, 0));
		}

        private BoundingBox BuildBoundingBox(
                int x,
                int z,
                int point1,
                int point2)
        {
            BoundingBox thisBox = new BoundingBox(
            wallPoints[point1],
            wallPoints[point2]);

            thisBox.Min.X += x;
            thisBox.Min.Z += z;
            thisBox.Max.X += x;
            thisBox.Max.Z += z;

            thisBox.Min.X -= 0.1f;
            thisBox.Min.Z -= 0.1f;
            thisBox.Max.X += 0.1f;
            thisBox.Max.Z += 0.1f;
            return thisBox;
        }

        public List<BoundingBox> GetBoundsForCell(int x, int z)
        {
            List<BoundingBox> boxes = new List<BoundingBox>();
            if (MazeCells[x, z].Walls[0])
                boxes.Add(BuildBoundingBox(x, z, 2, 4));
            if (MazeCells[x, z].Walls[1])
                boxes.Add(BuildBoundingBox(x, z, 6, 5));
            if (MazeCells[x, z].Walls[2])
                boxes.Add(BuildBoundingBox(x, z, 3, 5));
            if (MazeCells[x, z].Walls[3])
                boxes.Add(BuildBoundingBox(x, z, 2, 1));
            return boxes;
        }


        private void EvaluateCell(Vector2 cell)
        {
            List<int> neighborCells = new List<int>();
            neighborCells.Add(0);
            neighborCells.Add(1);
            neighborCells.Add(2);
            neighborCells.Add(3);

            while (neighborCells.Count > 0)
            {
                int pick = rand.Next(0, neighborCells.Count);
                int selectedNeighbor = neighborCells[pick];
                neighborCells.RemoveAt(pick);

                Vector2 neighbor = cell;

                switch (selectedNeighbor)
                {
                    case 0: neighbor += new Vector2(0, -1);
                        break;
                    case 1: neighbor += new Vector2(1, 0);
                        break;
                    case 2: neighbor += new Vector2(0, 1);
                        break;
                    case 3: neighbor += new Vector2(-1, 0);
                        break;
                }

                if ((neighbor.X >= 0) &&
                    (neighbor.X < MAZE_WIDTH) &&
                    (neighbor.Y >= 0) &&
                    (neighbor.Y < MAZE_HEIGHT))
                {
                    if (!MazeCells[(int)neighbor.X, (int)neighbor.Y].Visited)
                    {
                        MazeCells[
                            (int)neighbor.X,
                            (int)neighbor.Y].Visited = true;
                        MazeCells[
                            (int)cell.X,
                            (int)cell.Y].Walls[selectedNeighbor] = false;
                        MazeCells[
                            (int)neighbor.X,
                            (int)neighbor.Y].Walls[
                            (selectedNeighbor + 2) % 4] = false;
                        EvaluateCell(neighbor);
                    }
                }

            }
        }

		private void BuildWallBuffer()
		{
			List<VertexPositionColor> wallVertexList = new List<VertexPositionColor>();
			for ( int x = 0 ; x < MAZE_WIDTH ; x++ )
			{
				for ( int z = 0 ; z < MAZE_HEIGHT ; z++ )
				{
					foreach ( VertexPositionColor vertex in BuildMazeWall( x, z ) )
					{
						wallVertexList.Add(vertex);
					}
				}
			}

			wallBuffer = new VertexBuffer(
				device,
				VertexPositionColor.VertexDeclaration,
				wallVertexList.Count,
				BufferUsage.WriteOnly);
			wallBuffer.SetData<VertexPositionColor>(wallVertexList.ToArray());
		}

		private List<VertexPositionColor> BuildMazeWall( int x, int z )
		{
			List<VertexPositionColor> triangles = new List<VertexPositionColor>();

			if (MazeCells[x, z].Walls[0])
			{
				triangles.Add(CalcPoint(0, x, z, wallColors[0]));
				triangles.Add(CalcPoint(4, x, z, wallColors[0]));
				triangles.Add(CalcPoint(2, x, z, wallColors[0]));
				triangles.Add(CalcPoint(4, x, z, wallColors[0]));
				triangles.Add(CalcPoint(6, x, z, wallColors[0]));
				triangles.Add(CalcPoint(2, x, z, wallColors[0]));
			}

			if (MazeCells[x, z].Walls[1])
			{
				triangles.Add(CalcPoint(4, x, z, wallColors[1]));
				triangles.Add(CalcPoint(5, x, z, wallColors[1]));
				triangles.Add(CalcPoint(6, x, z, wallColors[1]));
				triangles.Add(CalcPoint(5, x, z, wallColors[1]));
				triangles.Add(CalcPoint(7, x, z, wallColors[1]));
				triangles.Add(CalcPoint(6, x, z, wallColors[1]));
			}

			if (MazeCells[x, z].Walls[2])
			{
				triangles.Add(CalcPoint(5, x, z, wallColors[2]));
				triangles.Add(CalcPoint(1, x, z, wallColors[2]));
				triangles.Add(CalcPoint(7, x, z, wallColors[2]));
				triangles.Add(CalcPoint(1, x, z, wallColors[2]));
				triangles.Add(CalcPoint(3, x, z, wallColors[2]));
				triangles.Add(CalcPoint(7, x, z, wallColors[2]));
			}

			if (MazeCells[x, z].Walls[3])
			{
				triangles.Add(CalcPoint(1, x, z, wallColors[3]));
				triangles.Add(CalcPoint(0, x, z, wallColors[3]));
				triangles.Add(CalcPoint(3, x, z, wallColors[3]));
				triangles.Add(CalcPoint(0, x, z, wallColors[3]));
				triangles.Add(CalcPoint(2, x, z, wallColors[3]));
				triangles.Add(CalcPoint(3, x, z, wallColors[3]));
			}

			return triangles;	
		}

		private VertexPositionColor CalcPoint( int wallPoint, int xOffset, int zOffset, Color color )
		{
			return new VertexPositionColor(wallPoints[wallPoint] + new Vector3(xOffset, 0, zOffset), color);
		}

		public void Draw( Camera camera, BasicEffect effect )
		{

            effect.VertexColorEnabled = true;
            effect.World = Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            //effect.AmbientLightColor = Color.Blue.ToVector3();

            //effect.Parameters["World"].SetValue(Matrix.Identity);
            //effect.Parameters["View"].SetValue(camera.View);
            //effect.Parameters["Projection"].SetValue(camera.Projection);

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
               
				device.SetVertexBuffer(floorBuffer);
				device.DrawPrimitives(
					PrimitiveType.TriangleList,
					0,
					floorBuffer.VertexCount / 3);
				device.SetVertexBuffer(wallBuffer);
				device.DrawPrimitives(
					PrimitiveType.TriangleList,
					0,
					wallBuffer.VertexCount / 3);                
			}
            
		}
	}
}

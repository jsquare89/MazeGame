using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
		VertexPositionNormalTexture[] floorVertices;
		Texture2D floorTexture;
		BasicEffect floorEffect;

		Color[] floorColors = new Color[2] { Color.White, Color.Gray };

		private Random rand = new Random();
		public MazeCell[,] MazeCells = new MazeCell[MAZE_WIDTH, MAZE_HEIGHT];

		VertexPositionNormalTexture[][] wallVertices = new VertexPositionNormalTexture[4][];
		Vector3[] wallPoints = new Vector3[8];
		Texture2D[] wallTextures = new Texture2D[4];
		BasicEffect[] wallEffects = new BasicEffect[4];


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

			floorEffect = new BasicEffect(device);
			wallEffects = new BasicEffect[]{
				new BasicEffect(device), 
				new BasicEffect(device), 
				new BasicEffect(device), 
				new BasicEffect(device)};

            BuildWallBuffer();
		}

		public void LoadContent(ContentManager content)
		{
			floorTexture = content.Load<Texture2D>("floor");
			wallTextures[0] = content.Load<Texture2D>("wall");
			wallTextures[1] = content.Load<Texture2D>("wall2");
			wallTextures[2] = content.Load<Texture2D>("wall3");
			wallTextures[3] = content.Load<Texture2D>("wall4");
		}

		private void BuildFloorBuffer()
		{
			List<VertexPositionNormalTexture> vertexList = 
				new List<VertexPositionNormalTexture>();

			int counter = 0;

			for (int x = 0 ; x < MAZE_WIDTH ; x++)
			{
				counter++;
				for (int z = 0; z < MAZE_HEIGHT; z++) {
					counter++; 
					foreach ( VertexPositionNormalTexture vertex in
						FloorTile( x, z, floorColors[ counter % 2 ] ) )
					{
						vertexList.Add(vertex);
					}
				}
			}

			floorVertices = vertexList.ToArray();
		}

		private List<VertexPositionNormalTexture> FloorTile(
			int xOffset,
			int zOffset,
			Color tileColor)
		{
			List<VertexPositionNormalTexture> vList = new List<VertexPositionNormalTexture>();
			vList.Add(new VertexPositionNormalTexture(
				new Vector3(0 + xOffset, 0, 0 + zOffset), Vector3.UnitY, new Vector2(0, 0)));
			vList.Add(new VertexPositionNormalTexture(
				new Vector3(1 + xOffset, 0, 0 + zOffset), Vector3.UnitY, new Vector2(1, 0)));
			vList.Add(new VertexPositionNormalTexture(
				new Vector3(0 + xOffset, 0, 1 + zOffset), Vector3.UnitY, new Vector2(0, 1)));

			vList.Add(new VertexPositionNormalTexture(
				new Vector3(1 + xOffset, 0, 0 + zOffset), Vector3.UnitY, new Vector2(1, 0)));
			vList.Add(new VertexPositionNormalTexture(
				new Vector3(1 + xOffset, 0, 1 + zOffset), Vector3.UnitY, new Vector2(1, 1)));
			vList.Add(new VertexPositionNormalTexture(
				new Vector3(0 + xOffset, 0, 1 + zOffset), Vector3.UnitY, new Vector2(0, 1)));

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
			List<VertexPositionNormalTexture>[] wallVertexLists = new List<VertexPositionNormalTexture>[4] {
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>()};
			for ( int x = 0 ; x < MAZE_WIDTH ; x++ )
			{
				for ( int z = 0 ; z < MAZE_HEIGHT ; z++ )
				{
					BuildMazeWall( x, z, wallVertexLists );
				}
			}

			wallVertices[0] = wallVertexLists[0].ToArray();
			wallVertices[1] = wallVertexLists[1].ToArray();
			wallVertices[2] = wallVertexLists[2].ToArray();
			wallVertices[3] = wallVertexLists[3].ToArray();
		}

		private void BuildMazeWall( int x, int z, List<VertexPositionNormalTexture>[] triangleLists)
		{
			if (MazeCells[x, z].Walls[0])
			{
				triangleLists[0].Add(CalcPoint(0, x, z, Vector3.UnitZ, new Vector2(0, 1)));
				triangleLists[0].Add(CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(1, 1)));
				triangleLists[0].Add(CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(0, 0)));
				triangleLists[0].Add(CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(1, 1)));
				triangleLists[0].Add(CalcPoint(6, x, z, Vector3.UnitZ, new Vector2(1, 0)));
				triangleLists[0].Add(CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(0, 0)));
			}

			if (MazeCells[x, z].Walls[1])
			{
				triangleLists[1].Add(CalcPoint(4, x, z, -Vector3.UnitX, new Vector2(0, 1)));
				triangleLists[1].Add(CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(1, 1)));
				triangleLists[1].Add(CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(0, 0)));
				triangleLists[1].Add(CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(1, 1)));
				triangleLists[1].Add(CalcPoint(7, x, z, -Vector3.UnitX, new Vector2(1, 0)));
				triangleLists[1].Add(CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(0, 0)));
			}

			if (MazeCells[x, z].Walls[2])
			{
				triangleLists[2].Add(CalcPoint(5, x, z, -Vector3.UnitZ, new Vector2(0, 1)));
				triangleLists[2].Add(CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(1, 1)));
				triangleLists[2].Add(CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(0, 0)));
				triangleLists[2].Add(CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(1, 1)));
				triangleLists[2].Add(CalcPoint(3, x, z, -Vector3.UnitZ, new Vector2(1, 0)));
				triangleLists[2].Add(CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(0, 0)));
			}

			if (MazeCells[x, z].Walls[3])
			{
				triangleLists[3].Add(CalcPoint(1, x, z, Vector3.UnitX, new Vector2(0, 1)));
				triangleLists[3].Add(CalcPoint(0, x, z, Vector3.UnitX, new Vector2(1, 1)));
				triangleLists[3].Add(CalcPoint(3, x, z, Vector3.UnitX, new Vector2(0, 0)));
				triangleLists[3].Add(CalcPoint(0, x, z, Vector3.UnitX, new Vector2(1, 1)));
				triangleLists[3].Add(CalcPoint(2, x, z, Vector3.UnitX, new Vector2(1, 0)));
				triangleLists[3].Add(CalcPoint(3, x, z, Vector3.UnitX, new Vector2(0, 0)));
			}
		}

		private VertexPositionNormalTexture CalcPoint( int wallPoint, int xOffset, int zOffset, Vector3 normal, Vector2 textCoords )
		{
			return new VertexPositionNormalTexture(wallPoints[wallPoint] + new Vector3(xOffset, 0, zOffset), normal, textCoords);
		}

		public void Draw( Camera camera, BasicEffect effect )
		{	
			device.SamplerStates[0] = SamplerState.LinearClamp;

			floorEffect.TextureEnabled = true;
			floorEffect.Texture = floorTexture;
			floorEffect.World = Matrix.Identity;
			floorEffect.View = camera.View;
			floorEffect.Projection = camera.Projection;
			if ( effect.FogEnabled )
			{
				floorEffect.FogEnabled = true;
                floorEffect.FogColor = Color.White.ToVector3();
                floorEffect.FogStart = 0.1f;
                floorEffect.FogEnd = 5f;
			}
			else
			{
				floorEffect.FogEnabled = false;
			}
			foreach (EffectPass pass in floorEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, floorVertices, 0, floorVertices.Count() / 3);
			}

			for ( int i = 0; i < wallEffects.Count(); i++ )
			{
				wallEffects[i].TextureEnabled = true;
				wallEffects[i].Texture = wallTextures[i];
				wallEffects[i].World = Matrix.Identity;
				wallEffects[i].View = camera.View;
				wallEffects[i].Projection = camera.Projection;
				if ( effect.FogEnabled )
				{
					wallEffects[i].FogEnabled = true;
					wallEffects[i].FogColor = Color.White.ToVector3();
					wallEffects[i].FogStart = 0.1f;
					wallEffects[i].FogEnd = 5f;
				}
				else
				{
					wallEffects[i].FogEnabled = false;
				}
				
				foreach(EffectPass pass in wallEffects[i].CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, wallVertices[i], 0, wallVertices[i].Count() / 3);
				}
			}
		}
	}
}

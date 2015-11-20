using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
	/// <summary>
	/// The Maze class contains the algorithm to generate random walls inside the maze using breath-first algorithm.
	/// It also draws the maze that the player will see.
	/// </summary>
    class Maze
    {
        public const int MAZE_WIDTH = 20;
        public const int MAZE_HEIGHT = 20;

        GraphicsDevice device;

        private Random rand = new Random();
        public MazeCell[,] MazeCells = new MazeCell[MAZE_WIDTH, MAZE_HEIGHT];

        // Floor
        VertexPositionNormalTexture[] floorVertices;
        Texture2D floorTexture;

        // Walls
        VertexPositionNormalTexture[][] wallVertices = new VertexPositionNormalTexture[4][];
        Vector3[] wallPoints = new Vector3[8];
        Texture2D[] wallTextures = new Texture2D[4];
        BasicEffect[] wallEffects = new BasicEffect[4];

		/// <summary>
		/// The Maze construcotr generates floor and walls vertices
		/// </summary>
		/// <param name="device"></param>
        public Maze(GraphicsDevice device)
        {
            this.device = device;

            // Create maze grid
            for (int x = 0; x < MAZE_WIDTH; x++)
                for (int z = 0; z < MAZE_HEIGHT; z++)
                {
                    MazeCells[x, z] = new MazeCell();
                }

            // Define vertices of walls
            wallPoints[0] = new Vector3(0, 1, 0);
            wallPoints[1] = new Vector3(0, 1, 1);
            wallPoints[2] = new Vector3(0, 0, 0);
            wallPoints[3] = new Vector3(0, 0, 1);
            wallPoints[4] = new Vector3(1, 1, 0);
            wallPoints[5] = new Vector3(1, 1, 1);
            wallPoints[6] = new Vector3(1, 0, 0);
            wallPoints[7] = new Vector3(1, 0, 1);

            GenerateMaze();
            BuildFloorVertices();
            BuildWallVertices();
        }

        public void LoadContent(ContentManager content)
        {
            // Load Textures
            floorTexture = content.Load<Texture2D>("floor");
            wallTextures[0] = content.Load<Texture2D>("wall");
            wallTextures[1] = content.Load<Texture2D>("wall2");
            wallTextures[2] = content.Load<Texture2D>("wall3");
            wallTextures[3] = content.Load<Texture2D>("wall4");
        }


        // This helper function generates all the vertices for floor tiles.
		// Each loor tiles has a width and height of 1.
        private void BuildFloorVertices()
        {
            List<VertexPositionNormalTexture> vertexList =
                new List<VertexPositionNormalTexture>();

            int counter = 0;

            for (int x = 0; x < MAZE_WIDTH; x++)
            {
                counter++;
                for (int z = 0; z < MAZE_HEIGHT; z++)
                {
                    counter++;
					// creates vertices for the floor tiles
                    foreach (VertexPositionNormalTexture vertex in
                        FloorTile(x, z, Color.White))
                    {
                        vertexList.Add(vertex);
                    }
                }
            }

            floorVertices = vertexList.ToArray();
        }

        // A helper function to create the 3 triangles that make up one floor tile
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

        /// <summary>
        /// The main function that use the breathe-first algorithm to create random walls in the maze
        /// </summary>
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

        // Helper function to create bounding boxes for collision detection for player and the wall
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

        /// <summary>
        /// Get all bounding boxs of the walls that exist
        /// </summary>
        /// <param name="x">The row of our maze </param>
        /// <param name="z">The column of our maze</param>
        /// <returns>List of BoundingBoxs that can collides with player</returns>
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

        // Helper function that does the heavy lifting of generating random maze through recursive calls
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

        /// Helper functions that gathers vertices of walls for the four different directions
        private void BuildWallVertices()
        {
            List<VertexPositionNormalTexture>[] wallVertexLists = new List<VertexPositionNormalTexture>[4] {
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>(),
				new List<VertexPositionNormalTexture>()};
            for (int x = 0; x < MAZE_WIDTH; x++)
            {
                for (int z = 0; z < MAZE_HEIGHT; z++)
                {
                    BuildMazeWall(x, z, wallVertexLists);
                }
            }

            wallVertices[0] = wallVertexLists[0].ToArray();
            wallVertices[1] = wallVertexLists[1].ToArray();
            wallVertices[2] = wallVertexLists[2].ToArray();
            wallVertices[3] = wallVertexLists[3].ToArray();
        }

        // Helper function that determine if the wall exist, add the triangle vertices so the wall will be
		// drawn later on
        private void BuildMazeWall(int x, int z, List<VertexPositionNormalTexture>[] triangleLists)
        {
			// North
            if (MazeCells[x, z].Walls[0])
            {
                triangleLists[0].Add(CalcPoint(0, x, z, Vector3.UnitZ, new Vector2(0, 1)));
                triangleLists[0].Add(CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(1, 1)));
                triangleLists[0].Add(CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(0, 0)));
                triangleLists[0].Add(CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(1, 1)));
                triangleLists[0].Add(CalcPoint(6, x, z, Vector3.UnitZ, new Vector2(1, 0)));
                triangleLists[0].Add(CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(0, 0)));
            }

			// East
            if (MazeCells[x, z].Walls[1])
            {
                triangleLists[1].Add(CalcPoint(4, x, z, -Vector3.UnitX, new Vector2(0, 1)));
                triangleLists[1].Add(CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(1, 1)));
                triangleLists[1].Add(CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(0, 0)));
                triangleLists[1].Add(CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(1, 1)));
                triangleLists[1].Add(CalcPoint(7, x, z, -Vector3.UnitX, new Vector2(1, 0)));
                triangleLists[1].Add(CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(0, 0)));
            }

			// South
            if (MazeCells[x, z].Walls[2])
            {
                triangleLists[2].Add(CalcPoint(5, x, z, -Vector3.UnitZ, new Vector2(0, 1)));
                triangleLists[2].Add(CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(1, 1)));
                triangleLists[2].Add(CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(0, 0)));
                triangleLists[2].Add(CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(1, 1)));
                triangleLists[2].Add(CalcPoint(3, x, z, -Vector3.UnitZ, new Vector2(1, 0)));
                triangleLists[2].Add(CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(0, 0)));
            }

			// West
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

        // helper function that calculate the coordinates of wall
        private VertexPositionNormalTexture CalcPoint(int wallPoint, int xOffset, int zOffset, Vector3 normal, Vector2 textCoords)
        {
            return new VertexPositionNormalTexture(wallPoints[wallPoint] + new Vector3(xOffset, 0, zOffset), normal, textCoords);
        }

		// helper function to draw the floor
        private void DrawFloors(Camera camera, Effect effect)
        {
            effect.Parameters["colorMapTexture"].SetValue(floorTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, floorVertices, 0, floorVertices.Count() / 3);
            }
        }

        // helper function to draw walls
        private void DrawWalls(Camera camera, Effect effect)
        {
            // iterate each wall and draw 
            for (int i = 0; i < wallEffects.Count(); i++)
            {
                effect.Parameters["colorMapTexture"].SetValue(wallTextures[i]);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, wallVertices[i], 0, wallVertices[i].Count() / 3);
                }
            }
        }

        /// <summary>
        /// Configures shader with parameters
        /// </summary>
        /// <param name="camera">camera used for world,view and projection</param>
        /// <param name="effect">applies shader to effect</param>
        private void ConfigureShader(Camera camera, Effect effect)
        {
            // Calculate the transposed inverse of the world matrix.
            Matrix worldInvTrans = Matrix.Transpose(Matrix.Invert(Matrix.Identity));

            // Calculate combined world-view-projection matrix.
            Matrix worldViewProj = Matrix.Identity * camera.View * camera.Projection;

            // Create camera position from camera rotation 
            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(camera.RotationX, -camera.RotationY, 0);
            Vector3 direction = Vector3.Transform(Vector3.Backward, rotationMatrix);

            // Set current lighting technieque for flahs light spot
            effect.CurrentTechnique = effect.Techniques["PerPixelSpotLighting"];

            // Set shader matrix parameters.
            effect.Parameters["worldMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldInverseTransposeMatrix"].SetValue(worldInvTrans);
            effect.Parameters["worldViewProjectionMatrix"].SetValue(worldViewProj);

            // Set the shader camera position parameter.
            effect.Parameters["cameraPos"].SetValue(camera.Position);

            // Set the shader global ambiance parameters.
            effect.Parameters["globalAmbient"].SetValue(Color.White.ToVector4());

            // Set the shader lighting parameters.
            effect.Parameters["light"].StructureMembers["dir"].SetValue(Vector3.Transform(Vector3.Backward, rotationMatrix));
            effect.Parameters["light"].StructureMembers["pos"].SetValue(camera.Position);
            effect.Parameters["light"].StructureMembers["ambient"].SetValue(Color.White.ToVector4());
            effect.Parameters["light"].StructureMembers["diffuse"].SetValue(Color.White.ToVector4());
            effect.Parameters["light"].StructureMembers["specular"].SetValue(Color.White.ToVector4());
            effect.Parameters["light"].StructureMembers["spotInnerCone"].SetValue(MathHelper.ToRadians(20.0f));
            effect.Parameters["light"].StructureMembers["spotOuterCone"].SetValue(MathHelper.ToRadians(30.0f));
            //effect.Parameters["light"].StructureMembers["radius"].SetValue(10.0f); - flash toggle moved to MazeGame 

            // Set the shader material parameters.
            // ambient set in MazeGame toggleAmbient
            effect.Parameters["material"].StructureMembers["diffuse"].SetValue(new Vector4(0.50754f, 0.50754f, 0.50754f, 1.0f));
            effect.Parameters["material"].StructureMembers["emissive"].SetValue(Color.Black.ToVector4());
            effect.Parameters["material"].StructureMembers["specular"].SetValue(new Vector4(0.508273f, 0.508273f, 0.508273f, 1.0f));
            effect.Parameters["material"].StructureMembers["shininess"].SetValue(51.2f);

            // Set the shader fog parameters.
            // enable and disabled set in MazeGame fog toggle
            effect.Parameters["fog"].StructureMembers["FogStart"].SetValue(0.5f);
            effect.Parameters["fog"].StructureMembers["FogEnd"].SetValue(2.5f);
            effect.Parameters["fog"].StructureMembers["FogColor"].SetValue(Color.Black.ToVector3());
        }

        public void Draw(Camera camera, Effect effect)
        {

            device.SamplerStates[0] = SamplerState.LinearClamp;

            ConfigureShader(camera, effect);
            DrawFloors(camera, effect);
            DrawWalls(camera, effect);
        }
    }
}

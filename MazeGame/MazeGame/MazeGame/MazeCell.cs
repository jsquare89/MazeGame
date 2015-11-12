using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGame
{
	/// <summary>
	/// Simple class to represent a cell in our maze with 4 walls.
	/// This is used in the maze random generation algorithm.
	/// </summary>
	class MazeCell
	{
		public bool[] Walls = new bool[4] {true, true, true, true};
		public bool Visited = false;
	}
}

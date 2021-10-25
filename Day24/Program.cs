using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 24 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var grid = ReadInput();
	var biodiversityCache = new HashSet<ulong>();
	while (true)
	{
		Print(grid, false);
		var biodiversity = CalculateBioDiversity(grid);
		if (biodiversityCache.Contains(biodiversity))
		{
			Console.WriteLine($"Biodiversity rating {biodiversity} appears twice.");
			Console.WriteLine();
			break;
		}
		biodiversityCache.Add(biodiversity);
		grid = Simulate(grid);
	}
}

static void Part2()
{
	var grid = ReadInput();
	var grids = new Dictionary<int, bool[,]>
	{
		{ 0, grid }
	};
	for (var i = 0; i < 200; i++)
	{
		SimulateAll(grids);
	}
	var count = PrintAll(grids);
	Console.WriteLine($"{count} bugs found.");
}

static void SimulateAll(Dictionary<int, bool[,]> grids)
{
	var minDim = grids.Keys.Min() - 1;
	var maxDim = grids.Keys.Max() + 1;
	EnsureGrid(grids, minDim - 1);
	EnsureGrid(grids, minDim);
	EnsureGrid(grids, maxDim);
	EnsureGrid(grids, maxDim + 1);
	var newGrids = new Dictionary<int, bool[,]>();
	for (var i = minDim; i <= maxDim; i++)
	{
		newGrids.Add(i, SimulateComplex(grids[i], grids, i));
	}
	foreach (var e in newGrids)
	{
		grids[e.Key] = newGrids[e.Key];
	}
}

static bool[,] Simulate(bool[,] grid)
{
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	var copy = new bool[h, w];
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			var n = CountAdjacentBugs(grid, x, y);
			copy[y, x] = grid[y, x];
			if (grid[y, x])
			{
				copy[y, x] = n == 1;
			}
			else
			{
				copy[y, x] = n == 1 || n == 2;
			}
		}
	}
	return copy;
}

static bool[,] SimulateComplex(bool[,] grid, Dictionary<int, bool[,]> grids, int dimension)
{
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	var copy = new bool[h, w];
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			if (x != 2 || y != 2)
			{
				var n = CountAdjacentBugsComplex(grid, x, y, grids, dimension);
				copy[y, x] = grid[y, x];
				if (grid[y, x])
				{
					copy[y, x] = n == 1;
				}
				else
				{
					copy[y, x] = n == 1 || n == 2;
				}
			}
		}
	}
	return copy;
}

static int CountAdjacentBugsComplex(bool[,] grid, int x, int y, Dictionary<int, bool[,]> grids, int dimension)
{
	var count = 0;
	var previousGrid = grids[dimension - 1];
	var nextGrid = grids[dimension + 1];
	//
	// up
	//
	if (y == 0)
	{
		count += GetState(previousGrid, 2, 1);
	}
	else if (y == 3 && x == 2)
	{
		count += GetState(nextGrid, 0, 4);
		count += GetState(nextGrid, 1, 4);
		count += GetState(nextGrid, 2, 4);
		count += GetState(nextGrid, 3, 4);
		count += GetState(nextGrid, 4, 4);
	}
	else
	{
		count += GetState(grid, x, y - 1);
	}
	//
	// down
	//
	if (y == 4)
	{
		count += GetState(previousGrid, 2, 3);
	}
	else if (y == 1 && x == 2)
	{
		count += GetState(nextGrid, 0, 0);
		count += GetState(nextGrid, 1, 0);
		count += GetState(nextGrid, 2, 0);
		count += GetState(nextGrid, 3, 0);
		count += GetState(nextGrid, 4, 0);
	}
	else
	{
		count += GetState(grid, x, y + 1);
	}
	// left
	if (x == 0)
	{
		count += GetState(previousGrid, 1, 2);
	}
	else if (x == 3 && y == 2)
	{
		count += GetState(nextGrid, 4, 0);
		count += GetState(nextGrid, 4, 1);
		count += GetState(nextGrid, 4, 2);
		count += GetState(nextGrid, 4, 3);
		count += GetState(nextGrid, 4, 4);
	}
	else
	{
		count += GetState(grid, x - 1, y);
	}
	// right
	if (x == 4)
	{
		count += GetState(previousGrid, 3, 2);
	}
	else if (x == 1 && y == 2)
	{
		count += GetState(nextGrid, 0, 0);
		count += GetState(nextGrid, 0, 1);
		count += GetState(nextGrid, 0, 2);
		count += GetState(nextGrid, 0, 3);
		count += GetState(nextGrid, 0, 4);
	}
	else
	{
		count += GetState(grid, x + 1, y);
	}

	return count;
}

static void EnsureGrid(Dictionary<int, bool[,]> grids, int dimension)
{
	var h = grids.Values.First().GetLength(0);
	var w = grids.Values.First().GetLength(1);
	if (!grids.ContainsKey(dimension))
	{
		grids.Add(dimension, new bool[h, w]);
	}
}

static int CountAdjacentBugs(bool[,] grid, int x, int y)
{
	return
		GetState(grid, x - 1, y) +
		GetState(grid, x + 1, y) +
		GetState(grid, x, y - 1) +
		GetState(grid, x, y + 1);
}

static int GetState(bool[,] grid, int x, int y)
{
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	return x >= 0 && x < w && y >= 0 && y < h && grid[y, x] ? 1 : 0;
}

static int PrintAll(Dictionary<int, bool[,]> grids)
{
	var count = 0;
	foreach (var k in grids.Keys.OrderBy(k => k))
	{
		Console.WriteLine($"Depth {k}:");
		count += Print(grids[k], true);
	}
	return count;
}

static int Print(bool[,] grid, bool complex)
{
	var count = 0;
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			var c = '.';
			if (complex && x == 2 && y == 2)
			{
				c = '?';
			}
			else if (grid[y, x])
			{
				c = '#';
				count++;
			}
			Console.Write(c);
		}
		Console.WriteLine();
	}
	Console.WriteLine();
	return count;
}

static ulong CalculateBioDiversity(bool[,] grid)
{
	var v = 0UL;
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	var i = 1UL;
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			if (grid[y, x])
			{
				v += i;
			}
			i <<= 1;
		}
	}
	return v;
}

static bool[,] ReadInput()
{
	var lines = File.ReadAllLines("input.txt");
	var h = lines.Length;
	var w = lines.First().Length;
	var grid = new bool[h, w];
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			grid[y, x] = lines[y][x] == '#';
		}
	}
	return grid;
}

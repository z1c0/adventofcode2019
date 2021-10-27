using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 20 - START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var (maze, portals) = ReadInput();
	foreach (var portal in portals)
	{
		BFS_Maze(portal, maze, portals);
	}
	var startPortal = portals.Single(p => p.Id == "AA" && p.OutSide);
	Print(maze, portals);
	//BFS_Portals(startPortal, portals);
}

static void BFS_Maze(Portal startPortal, char[,] maze, List<Portal> portals)
{
	var visitedNodes = new HashSet<(int, int)>
	{
		(startPortal.X, startPortal.Y)
	};
	var queue = new List<(int, int)>
	{
		(startPortal.X, startPortal.Y)
	};
	var distance = 0;
	while (queue.Any())
	{
		var node = queue.First();
		queue.RemoveAt(0);
		distance++;
		foreach (var n in GetAdjacentNodes(node, maze))
		{
			if (!visitedNodes.Contains(n))
			{
				visitedNodes.Add(n);
				var portal = portals.SingleOrDefault(p => p.X == n.X && p.Y == n.Y);
				if (portal != null)
				{
					portal.Distance = distance;
					startPortal.ConnectedPortals.Add(portal);
				}
				else
				{
					queue.Add(n);
				}
			}
		}
	}
}

static void BFS_Portals(Portal startPortal, List<Portal> portals)
{
	var visitedPortals = new HashSet<Portal>
	{
		startPortal
	};
	var queue = new List<Portal>
	{
		startPortal
	};
	while (queue.Any())
	{
		var portal = queue.First();
		queue.RemoveAt(0);
		foreach (var p in GetConnectedPortals(portal, portals))
		{
			if (!visitedPortals.Contains(p))
			{
				p.From = portal;
				visitedPortals.Add(p);
				if (p.Id == "ZZ" && p.OutSide)
				{
					var zz = p;
					var distance = 0;
					while (zz != null)
					{
						Console.WriteLine($"{zz} - {zz.Distance}");
						distance += zz.Distance;
						zz = zz.From;
					}
					Console.WriteLine(distance);
					return;
				}
				queue.Add(p);
			}
		}
	}
}

static List<Portal> GetConnectedPortals(Portal portal, List<Portal> portals)
{
	var connectedPortals = new List<Portal>(portal.ConnectedPortals);
	foreach (var p in portal.ConnectedPortals)
	{
		connectedPortals.AddRange(portals.Where(pp => pp.Id == p.Id && pp.OutSide != p.OutSide));
	}
	return connectedPortals;
}

static IEnumerable<(int X, int Y)> GetAdjacentNodes((int X, int Y) node, char[,] maze)
{
	if (CheckNode(node.X - 1, node.Y, maze)) yield return (node.X - 1, node.Y);
	if (CheckNode(node.X + 1, node.Y, maze)) yield return (node.X + 1, node.Y);
	if (CheckNode(node.X, node.Y - 1, maze)) yield return (node.X, node.Y - 1);
	if (CheckNode(node.X, node.Y + 1, maze)) yield return (node.X, node.Y + 1);
}

static bool CheckNode(int x, int y, char[,] maze)
{
	var h = maze.GetLength(0);
	var w = maze.GetLength(1);
	return x >= 0 && y >= 0 && y < h && x < w && maze[y, x] == '.';
}

static void Print(char[,] maze, List<Portal> portals)
{
	foreach (var p in portals)
	{
		Console.WriteLine(p);
		foreach (var pp in p.ConnectedPortals)
		{
			Console.WriteLine($"  -> {pp} {pp.Distance}");
		}
	}
	var h = maze.GetLength(0);
	var w = maze.GetLength(1);
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			Console.Write(maze[y, x] != 0 ? maze[y, x] : ' ');
		}
		Console.WriteLine();
	}
}

static (char[,] Maze, List<Portal> portals) ReadInput()
{
	var lines = File.ReadAllLines("input.txt");
	var h = lines.Length;
	var w = lines.First().Length;
	var maze = new char[h - 4, w - 4];
	var portals = new List<Portal>();
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			var c = lines[y][x];
			if (Char.IsLetter(c))
			{
				if (y < h - 1 && lines[y + 1][x] == '.')  // top
				{
					portals.Add(new Portal($"{lines[y - 1][x]}{c}", x - 2, y + 1 - 2, y == 1));
				}
				else if (y > 0 && lines[y - 1][x] == '.') // bottom
				{
					portals.Add(new Portal($"{c}{lines[y + 1][x]}", x - 2, y - 1 - 2, y == h - 2));
				}
				if (x < w - 1 && lines[y][x + 1] == '.') // left
				{
					portals.Add(new Portal($"{lines[y][x - 1]}{c}", x + 1 - 2, y - 2, x == 1));
				}
				else if (x > 0 && lines[y][x - 1] == '.') // right
				{
					portals.Add(new Portal($"{c}{lines[y][x + 1]}", x - 1 - 2, y - 2, x == w - 2));
				}
			}
			if (c == '#' || c == '.')
			{
				maze[y - 2, x - 2] = c;
			}
		}
	}
	return (maze, portals);
}

public record Portal(string Id, int X, int Y, bool OutSide)
{
	public List<Portal> ConnectedPortals { get; } = new();

	public override string ToString()
	{
		return Id + " (" + (OutSide ? "out" : "in") + ")";
	}
	public int Distance  { get; set; }

	public Portal From  { get; set; }
}
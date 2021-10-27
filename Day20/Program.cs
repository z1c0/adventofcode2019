using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 20 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
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
	var path = BFS_Portals(startPortal, portals);
	CalculateDistance(path);
}

static void Part2()
{
	var (maze, portals) = ReadInput();
	foreach (var portal in portals)
	{
		BFS_Maze(portal, maze, portals);
	}
	var startPortal = portals.Single(p => p.Id == "AA" && p.OutSide);
	var path = BFS_PortalsRecursive(startPortal, portals);
	CalculateDistance(path);
}

static void CalculateDistance(List<Portal> path)
{
	var distance = 0;
	for (var i = 0; i < path.Count - 1; i++)
	{
		var current = path[i];
		var next = path[i + 1];
		var c = current.ConnectedPortals.Single(p => p.Portal.Id == next.Id);
		distance += c.Distance;
		if (c.Portal.OutSide != next.OutSide)
		{
			distance++;
		}
	}
	Console.WriteLine($"Distance: {distance}");
}

static List<Portal> BFS_PortalsRecursive(Portal startPortal, List<Portal> portals)
{
	var result = new List<Portal>();
	var visitedPortals = new List<HashSet<Portal>>
	{
		new HashSet<Portal> { startPortal }
	};
	var queue = new List<(Portal Portal, int Level)>
	{
		(startPortal, 0)
	};
	while (queue.Any())
	{
		var item = queue.First();
		queue.RemoveAt(0);
		foreach (var e in GetConnectedPortalsRecursive(item, portals))
		{
			if (e.Level >= visitedPortals.Count)
			{
				visitedPortals.Add(new());
			}
			if (!visitedPortals[e.Level].Contains(e.Portal))
			{
				if (e.Portal.From != null)
				{
					throw new Exception();
				}
				Debug.Assert(e.Portal.From == null);
				e.Portal.From = item.Portal;
				visitedPortals[e.Level].Add(e.Portal);
				if (e.Portal.Id == "ZZ" && e.Level == 0)
				{
					var curr = e.Portal;
					while (curr != null)
					{
						result.Add(curr);
						curr = curr.From;
					}
					result.Reverse();
					return result;
				}
				queue.Add(e);
			}
		}
	}
	return null;
}

static List<(Portal Portal, int Level)> GetConnectedPortalsRecursive((Portal Portal, int Level) item, List<Portal> portals)
{
	var list = new List<(Portal Portal, int Level)>();
	foreach (var c in item.Portal.ConnectedPortals)
	{
		if (item.Level == 0 && c.Portal.Id == "ZZ")
		{
			list.Add((c.Portal with {}, 0));
		}
		var p = portals.SingleOrDefault(pp => pp.Id == c.Portal.Id && pp.OutSide != c.Portal.OutSide);
		if (p != null)
		{
			if (!c.Portal.OutSide)
			{
				list.Add((p with {}, item.Level + 1));
			}
			else if (item.Level > 0)
			{
				list.Add((p with {}, item.Level - 1));
			}
		}
	}
	//Console.WriteLine($"{item} -> {string.Join(" / ", list)}");
	return list;
}

static void BFS_Maze(Portal startPortal, char[,] maze, List<Portal> portals)
{
	var visitedNodes = new HashSet<(int, int)>
	{
		(startPortal.X, startPortal.Y)
	};
	var queue = new List<(int, int, int)>
	{
		(startPortal.X, startPortal.Y, 0)
	};
	while (queue.Any())
	{
		var (x, y, distance) = queue.First();
		queue.RemoveAt(0);
		foreach (var n in GetAdjacentNodes((x, y), maze))
		{
			if (!visitedNodes.Contains(n))
			{
				visitedNodes.Add(n);
				var portal = portals.SingleOrDefault(p => p.X == n.X && p.Y == n.Y);
				if (portal != null)
				{
					startPortal.ConnectedPortals.Add((portal, distance + 1));
				}
				else
				{
					queue.Add((n.X, n.Y, distance + 1));
				}
			}
		}
	}
}

static List<Portal> BFS_Portals(Portal startPortal, List<Portal> portals)
{
	var result = new List<Portal>();
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
					var curr = p;
					while (curr != null)
					{
						result.Add(curr);
						curr = curr.From;
					}
					result.Reverse();
					return result;
				}
				queue.Add(p);
			}
		}
	}
	return null;
}

static List<Portal> GetConnectedPortals(Portal portal, List<Portal> portals)
{
	var connectedPortals = new List<Portal>(portal.ConnectedPortals.Select(e => e.Portal));
	foreach (var p in portal.ConnectedPortals)
	{
		connectedPortals.AddRange(portals.Where(pp => pp.Id == p.Portal.Id && pp.OutSide != p.Portal.OutSide));
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
			Console.WriteLine($"  -> {pp}");
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
	public List<(Portal Portal, int Distance)> ConnectedPortals { get; } = new();

	public override string ToString()
	{
		return Id + " (" + (OutSide ? "out" : "in") + "side)";
	}

	public Portal From  { get; set; }
}
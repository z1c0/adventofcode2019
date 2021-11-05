using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 18 - START");
var sw = Stopwatch.StartNew();
Part1();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{	
	var map = ReadInput();
	var (pos, numberOfKeys) = AnalyzeMap(map);
	Console.WriteLine($"Start position: {pos}");
	Console.WriteLine($"Number of keys: {numberOfKeys}");
	var keys = new HashSet<Node>();
	var currentNode = new Node(pos.x, pos.y);
	var minDistance = int.MaxValue;
	var cache = new HashSet<string>();
	FindPath(map, currentNode, keys, numberOfKeys, ref minDistance, cache);
	Console.WriteLine($"Minimum distance: {minDistance}");
}

static void FindPath(char[,] map, Node currentNode, HashSet<Node> keys, int numberOfKeys, ref int minDistance, HashSet<string> cache)
{
	var distance = keys.Sum(k => k.Distance);
	if (distance >= minDistance)
	{
		return;
	}
	var fingerPrint = FingerPrint(keys);
	if (cache.Contains(fingerPrint))
	{
		return;
	}
	cache.Add(fingerPrint);

	var result = BFS(currentNode, map, keys);
	if (result.Any())
	{
		foreach (var n in result)
		{
			var copy = keys.ToHashSet();
			copy.Add(n);
			FindPath(map, new Node(n.X, n.Y, n.Key), copy, numberOfKeys, ref minDistance, cache);
		}
	}

	if (keys.Count == numberOfKeys)
	{
		//minDistance = Math.Min(distance, minDistance);
		if (distance < minDistance)
		{
			minDistance = distance;
			Console.WriteLine(minDistance);
		}
		/*
		var distance = 0;
		foreach (var n in keys)
		{
			Console.WriteLine(n);
			distance += n.Distance;
		}
		Console.WriteLine(distance);
		Console.WriteLine();
		*/
	}
}

static string FingerPrint(HashSet<Node> keys)
{
	var k = string.Join('|', keys.OrderBy(k => k.Key).Select(k => $"{k.Key}-{k.Distance}"));
	return string.Format($"{k}");
}

static List<Node> BFS(Node startNode, char[,] map, HashSet<Node> collectedKeys)
{
	var newKeys = new List<Node>();
	var visitedNodes = new List<Node>
	{
		startNode
	};
	var queue = new LinkedList<Node>();
	queue.AddLast(startNode);
	startNode.Distance = 0;
	while (queue.Any())
	{
		startNode = queue.First.Value;
		queue.RemoveFirst();
		var adjacentNodes = GetAdjacentNodes(startNode, map, collectedKeys).ToList();
		if (adjacentNodes.Any())
		{
			foreach (var node in adjacentNodes)
			{
				node.Distance = startNode.Distance + 1;
				if (!visitedNodes.Contains(node))
				{
					visitedNodes.Add(node);
					if (node.Key != 0 && !collectedKeys.Contains(node))
					{
						newKeys.Add(node);
					}
					else
					{
						queue.AddLast(node);
					}
				}
			}
		}
	}
	return newKeys;
}

static IEnumerable<Node> GetAdjacentNodes(Node node, char[,] map, HashSet<Node> keys)
{
	var n1 = new Node(node.X - 1, node.Y);
	var n2 = new Node(node.X + 1, node.Y);
	var n3 = new Node(node.X, node.Y - 1);
	var n4 = new Node(node.X, node.Y + 1);
	if (CheckNode(n1, map, keys)) yield return n1;
	if (CheckNode(n2, map, keys)) yield return n2;
	if (CheckNode(n3, map, keys)) yield return n3;
	if (CheckNode(n4, map, keys)) yield return n4;
}

static bool CheckNode(Node node, char[,] map, HashSet<Node> keys)
{
	var w = map.GetLength(1);
	var h = map.GetLength(0);
	if (node.X < 0 || node.X >= w || node.Y < 0 || node.Y >= h)
	{
		return false;
	}
	var c = map[node.Y, node.X];
	if (c == '#')
	{
		return false;
	}
	if (c >= 'A' && c <= 'Z')
	{		
		// Do we have a key for this door?
		return keys.Any(n => n.Key == Char.ToLower(c));
	}
	if (c >= 'a' && c <= 'z')
	{
		node.Key = c;
		return true;
	}
	return true;
}

static ((int x, int y) pos, int numberOfKeys) AnalyzeMap(char[,] map)
{
	var pos = (-1, -1);
	var numberOfKeys = 0;
	for (var y = 0; y < map.GetLength(0); y++)
	{
		for (var x = 0; x < map.GetLength(1); x++)
		{
			var c = map[y, x];
			if (c == '@')
			{
				pos.Item1 = x;
				pos.Item2 = y;
			}
			if (c >= 'a' && c <= 'z')
			{
				numberOfKeys++;
			}
		}		
	}
	return (pos, numberOfKeys);
}

static char[,] ReadInput()
{
	var lines = File.ReadAllLines("input.txt");
	var h = lines.Length;
	var w = lines.First().Length;
	var map = new char[h, w];
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			map[y, x] = lines[y][x];
		}
	}
	return map;
}
class Node
{
	internal Node(int x, int y, char key = '\0')
	{
		X = x;
		Y = y;
		Key = key;
	}

	internal int X { get; }
	internal int Y { get; }
	internal char Key { get; set; }

	internal int Distance { get; set; }

	public override bool Equals(object obj)
	{
		var other = (Node)obj;
		return X == other.X && Y == other.Y;
	}	

	public override string ToString()
	{
		return $"{Key} @ {X}/{Y} ({Distance})";
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}
}

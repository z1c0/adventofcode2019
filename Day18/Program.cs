using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 18 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{	
	var map = ReadInput();
	var (start, numberOfKeys) = AnalyzeMap(map);
	var node = BFS(new Node(start.x, start.y), map, numberOfKeys);
	Console.WriteLine($"Shortest path: {node.Distance}");
}

static void Part2()
{	
}

static Node BFS(Node startNode, char[,] map, int numberOfKeys)
{
	var visitedNodes = new Dictionary<string, int>
	{
		{ startNode.FingerPrint(), 0 }
	};
	var queue = new PriorityQueue<Node>();
	queue.Add(startNode);
	startNode.Distance = 0;
	while (queue.Count > 0)
	{
		startNode = queue.RemoveFirst();
		var adjacentNodes = GetAdjacentNodes(startNode, map).ToList();	
		if (adjacentNodes.Any())
		{
			foreach (var node in adjacentNodes)
			{
				node.Distance = startNode.Distance + 1;
				var fingerPrint = node.FingerPrint();
				if (!visitedNodes.ContainsKey(fingerPrint) || visitedNodes[fingerPrint] > node.Distance)
				{
					visitedNodes.Add(fingerPrint, node.Distance);
					queue.Add(node);

					if (node.Keys.Count == numberOfKeys)
					{
						return node;
					}
				}
			}
		}
	}
	return null;
}

static IEnumerable<Node> GetAdjacentNodes(Node node, char[,] map)
{
	var n1 = new Node(node.X - 1, node.Y) { Keys = node.Keys.ToList() };
	var n2 = new Node(node.X + 1, node.Y) { Keys = node.Keys.ToList() };
	var n3 = new Node(node.X, node.Y - 1) { Keys = node.Keys.ToList() };
	var n4 = new Node(node.X, node.Y + 1) { Keys = node.Keys.ToList() };
	if (CheckNode(n1, map)) yield return n1;
	if (CheckNode(n2, map)) yield return n2;
	if (CheckNode(n3, map)) yield return n3;
	if (CheckNode(n4, map)) yield return n4;
}

static bool CheckNode(Node node, char[,] map)
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
		return node.Keys.Contains(char.ToLower(c));
	}
	if (c >= 'a' && c <= 'z' && !node.Keys.Contains(c))
	{
		node.Keys.Add(c);
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
class Node : IComparable
{
	internal Node(int x, int y)
	{
		X = x;
		Y = y;
	}

	internal int X { get; }
	internal int Y { get; }
	internal List<char> Keys { get; init; } = new ();

	internal int Distance { get; set; }

	public override string ToString()
	{
		return $"{X}/{Y}";
	}

	internal string FingerPrint()
	{
		var k = string.Join('|', Keys.OrderBy(k => k));
		return string.Format($"{X}/{Y}-{k}");
	}

	public int CompareTo(object obj)
	{
		return ((Node)obj).Distance.CompareTo(Distance);
	}
}

// from: https://stackoverflow.com/a/33888482/1051140
internal class PriorityQueue<T>
{
	readonly IComparer<T> comparer;
	T[] heap;
	public int Count { get; private set; }
	public PriorityQueue() : this(null) { }
	public PriorityQueue(int capacity) : this(capacity, null) { }
	public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }
	public PriorityQueue(int capacity, IComparer<T> comparer)
	{
		this.comparer = comparer ?? Comparer<T>.Default;
		this.heap = new T[capacity];
	}
	public void Add(T v)
	{
		if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
		heap[Count] = v;
		SiftUp(Count++);
	}
	public T RemoveFirst()
	{
		var v = Top();
		heap[0] = heap[--Count];
		if (Count > 0) SiftDown(0);
		return v;
	}
	public T Top()
	{
		if (Count > 0) return heap[0];
		throw new InvalidOperationException();
	}
	void SiftUp(int n)
	{
		var v = heap[n];
		for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
		heap[n] = v;
	}
	void SiftDown(int n)
	{
		var v = heap[n];
		for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
		{
			if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
			if (comparer.Compare(v, heap[n2]) >= 0) break;
			heap[n] = heap[n2];
		}
		heap[n] = v;
	}
}
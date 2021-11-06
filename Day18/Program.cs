using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using aoc;

Console.WriteLine("Day 18 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{	
	var map = ReadInput();
	var (x, y) = map.Find('@');
	var numberOfKeys = map.Count(c => c >= 'a' && c <= 'z');
	var state = BFS(new State(x, y), map, numberOfKeys);
	Console.WriteLine($"Shortest path: {state.Distance}");
}

static void Part2()
{	
	var map = ReadInput();
	var (x, y) = map.Find('@');
	map.Fill('#', (xx, yy) => xx == x || yy == y);
	var start1 = (x - 1, y - 1);
	var start2 = (x + 1, y - 1);
	var start3 = (x - 1, y + 1);
	var start4 = (x + 1, y + 1);
	map[start1] = '@';
	map[start2] = '@';
	map[start3] = '@';
	map[start4] = '@';
	map.Print();
}

static State BFS(State startState, Grid map, int numberOfKeys)
{
	var visitedStates = new Dictionary<string, int>
	{
		{ startState.FingerPrint(), 0 }
	};
	var queue = new PriorityQueue<State>();
	queue.Add(startState);
	while (queue.Count > 0)
	{
		startState = queue.RemoveFirst();
		var adjacentStates = startState.GetAdjacent(map).ToList();	
		if (adjacentStates.Any())
		{
			foreach (var state in adjacentStates)
			{
				var fingerPrint = state.FingerPrint();
				if (!visitedStates.ContainsKey(fingerPrint) || visitedStates[fingerPrint] > state.Distance)
				{
					visitedStates.Add(fingerPrint, state.Distance);
					queue.Add(state);

					if (state.Keys.Count == numberOfKeys)
					{
						return state;
					}
				}
			}
		}
	}
	return null;
}

static Grid ReadInput()
{
	return Grid.FromFile("input.txt");
}

class State : IComparable
{
	internal State(int x, int y)
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
		return ((State)obj).Distance.CompareTo(Distance);
	}

	internal IEnumerable<State> GetAdjacent(Grid map)
	{
		var n1 = new State(X - 1, Y) { Keys = Keys.ToList(), Distance = Distance + 1 };
		var n2 = new State(X + 1, Y) { Keys = Keys.ToList(), Distance = Distance + 1 };
		var n3 = new State(X, Y - 1) { Keys = Keys.ToList(), Distance = Distance + 1 };
		var n4 = new State(X, Y + 1) { Keys = Keys.ToList(), Distance = Distance + 1 };
		if (n1.Check(map)) yield return n1;
		if (n2.Check(map)) yield return n2;
		if (n3.Check(map)) yield return n3;
		if (n4.Check(map)) yield return n4;
	}

	private bool Check(Grid map)
	{
		if (X < 0 || X >= map.Width || Y < 0 || Y >= map.Height)
		{
			return false;
		}
		var c = map[X, Y];
		if (c == '#')
		{
			return false;
		}
		if (c >= 'A' && c <= 'Z')
		{		
			// Do we have a key for this door?
			return Keys.Contains(char.ToLower(c));
		}
		if (c >= 'a' && c <= 'z' && !Keys.Contains(c))
		{
			Keys.Add(c);
		}
		return true;
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
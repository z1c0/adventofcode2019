using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using aoc;

Console.WriteLine("Day 18 - START");
var sw = Stopwatch.StartNew();
//Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{	
	var map = ReadInput();
	var (x, y) = map.Find('@');
	var numberOfKeys = map.Count(c => c >= 'a' && c <= 'z');
	var goal = BFS(new State(map, x, y, new HashSet<string>()), numberOfKeys);
	Console.WriteLine($"Shortest path: {goal.Distance}");
}

static void Part2()
{	
	var map = ReadInput();
	
	var x = map.Width / 2;
	var y = map.Height / 2;
	var numberOfKeys = map.Count(c => c >= 'a' && c <= 'z');

	var state1 = new State(map, x - 1, y - 1, new HashSet<string>());
	var state2 = new State(map, x + 1, y - 1, new HashSet<string>());
	var state3 = new State(map, x - 1, y + 1, new HashSet<string>());
	var state4 = new State(map, x + 1, y + 1, new HashSet<string>());

	map.Fill('#', (xx, yy) => xx == x || yy == y);
	map[state1.X, state1.Y] = '@';
	map[state2.X, state2.Y] = '@';
	map[state3.X, state3.Y] = '@';
	map[state4.X, state4.Y] = '@';
	map.Print();

	var superState = new SuperState(map, state1, state2, state3, state4);
	var goal = BFS(superState, numberOfKeys);
	Console.WriteLine($"Shortest path: {goal.Distance}");
}

static IState BFS(IState startState, int numberOfKeys)
{
	var queue = new PriorityQueue<IState>();
	queue.Add(startState);
	while (queue.Count > 0)
	{
		startState = queue.RemoveFirst();
		System.Console.WriteLine($"{queue.Count} - {startState.KeyCount}");
		var adjacentStates = startState.GetAdjacent().ToList();
		if (adjacentStates.Any())
		{
			foreach (var state in adjacentStates)
			{
				queue.Add(state);
				if (state.KeyCount == numberOfKeys)
				{
					return state;
				}
			}
		}
	}
	throw new InvalidOperationException();
}

static Grid ReadInput()
{
	return Grid.FromFile("input.txt");
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
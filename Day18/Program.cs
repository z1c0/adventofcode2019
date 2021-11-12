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
	var (startState, numberOfKeys) = PrepareState(false);
	var goalState = BFS(startState, numberOfKeys);
	Console.WriteLine($"Shortest path: {goalState.Distance}");
}

static void Part2()
{	
	var (startState, numberOfKeys) = PrepareState(true);
	var goalState = BFS(startState, numberOfKeys);
	Console.WriteLine($"Shortest path: {goalState.Distance}");
}

static (State StartState, int NumberOfKeys) PrepareState(bool complex)
{
	var map = ReadInput();
	var x = map.Width / 2;
	var y = map.Height / 2;
	var numberOfKeys = map.Count(c => c >= 'a' && c <= 'z');
	var startState = new State(map);

	if (complex)
	{
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

		startState.AddPosition(start1);
		startState.AddPosition(start2);
		startState.AddPosition(start3);
		startState.AddPosition(start4);
	}
	else
	{
		startState.AddPosition((x, y));
	}
	return (startState, numberOfKeys);
}

static State BFS(State startState, int numberOfKeys)
{
	var queue = new PriorityQueue<State>();
	queue.Add(startState);
	var i = 0;
	while (queue.Count > 0)
	{
		startState = queue.RemoveFirst();
		if (i++ % 500_000 == 0) Console.WriteLine($"Keys: {startState.Keys.Count}, Distance: {startState.Distance}");
		var adjacentStates = startState.GetAdjacent().ToList();
		if (adjacentStates.Any())
		{
			foreach (var state in adjacentStates)
			{
				queue.Add(state);
				if (state.Keys.Count == numberOfKeys)
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
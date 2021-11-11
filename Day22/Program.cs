using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 22 - START");
var sw = Stopwatch.StartNew();
//Part1(10007, 10007);

//Part2(10007, 1);
Part2(10007, 5010);
//Part2(10007, 10007 * 3);

//Part2(10007, 2);

//Part2(119315717514047, 17574135437386);

// 119315717514047
// 101741582076661

//System.Console.WriteLine(119315717514047 - 101741582076661);
//17574135437386
System.Console.WriteLine(119315717514047 % 17574135437386);
System.Console.WriteLine(119315717514047 / 17574135437386);

//Part2(119315717514047, 101741582076661);

Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1(int count, int iterations)
{
	var deck = new List<int>();
	for (var i = 0; i < count; i++)
	{
		deck.Add(i);
	}
	var deck2 = deck.ToList();
	var instructions = ReadInput().ToList();
	while (iterations-- > 0)
	{
		foreach (var instr in instructions)
		{
			switch (instr.Technique)
			{
				case Technique.NewStack:
					deck.Reverse();
					break;

				case Technique.Cut:
					var cut = (instr.Value < 0) ? deck.Count + instr.Value : instr.Value;
					for (var i = 0; i < deck.Count; i++)
					{
						var to = deck.Count - cut + i;
						if (i >= cut)
						{
							to = i - cut;
						}
						deck2[to] = deck[i];
					}
					(deck, deck2) = (deck2, deck);
					break;

				case Technique.DealWithIncrement:
					{
						var to = 0;
						for (var i = 0; i < deck.Count; i++)
						{
							deck2[to] = deck[i];
							to = (to + instr.Value) % deck.Count;
						}
						(deck, deck2) = (deck2, deck);
					}
					break;

				default:
					throw new InvalidOperationException($"{instr}");
			}
		}
	}

	var pos = deck.IndexOf(2019);
	Console.WriteLine($"Position of card {2019} is {pos}");
	Console.WriteLine($"Card at {2020} is {deck[2020]}");
}

static void Part2(long count, long iterations)
{
	var pos = 2020L;
	var oldPos = pos;
	var instructions = ReadInput().ToList();
	instructions.Reverse();
	var i = 0;
	while (i++ < iterations)
	{
		foreach (var instr in instructions)
		{
			switch (instr.Technique)
			{
				case Technique.NewStack:
					pos = Math.Abs(pos + 1 - count);
					break;

				case Technique.Cut:
					var cut = (instr.Value < 0) ? count + instr.Value : instr.Value;
					pos = (pos + cut) % count;
					break;

				case Technique.DealWithIncrement:
					var inc = instr.Value;
					while (pos % inc != 0)
					{
						pos += count;
					}
					pos /= inc;
					break;

				default:
					throw new InvalidOperationException($"{instr}");
			}
		}
		Console.WriteLine($"{oldPos} <- {pos}");
		oldPos = pos;
	}
	Console.WriteLine($"-> {pos}");
	//Console.WriteLine($"-> {(pos * iterations) % count}");
	
	// 53403051010030 too low  (1 iteration)
}

static IEnumerable<(Technique Technique, int Value)> ReadInput()
{
	foreach (var line in File.ReadAllLines("input.txt"))
	{
		if (line == "deal into new stack")
		{
			yield return (Technique.NewStack, -1);
		}
		else if (line.StartsWith("deal with increment"))
		{
			yield return (Technique.DealWithIncrement, int.Parse(line.Split(' ').Last()));
		}
		else if (line.StartsWith("cut"))
		{
			yield return (Technique.Cut, int.Parse(line.Split(' ').Last()));
		}
		else
		{
			throw new InvalidOperationException(line);
		}
	}
}

enum Technique
{
	NewStack,
	DealWithIncrement,
	Cut,
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 22 - START");
var sw = Stopwatch.StartNew();
Part1(10007, 1);
//Part1(119315717514047, 1);
//Part1(119315717514047);
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1(long count, long iterations)
{
	var deck = new List<long>();
	for (var l = 0L; l < count; l++)
	{
		deck.Add(l);
	}
	var deck2 = deck.ToList();
	var instructions = ReadInput().ToList();
	for (var l = 0L; l < iterations; l++)
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

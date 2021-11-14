using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

Console.WriteLine("Day 22 - START");
var sw = Stopwatch.StartNew();
Part1(10017, 1);
Part2(new BigInteger(119315717514047), new BigInteger(101741582076661));
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

static void Part2(BigInteger count, BigInteger iterations)
{
	//
	// I shamelessly stole this from https://github.com/PJohannessen/AdventOfCode/blob/master/2019/22-2.linq
	// No way I could have solved this.
	// No idea how many hours I had already wasted on that.
	//
	BigInteger lookAt = 2020;
	
	BigInteger increment_mul = 1;
	BigInteger offset_diff = 0;

	var instructions = ReadInput().ToList();
	foreach (var i in instructions)
	{
		switch  (i.Technique)
		{
			case Technique.NewStack:
				increment_mul = increment_mul * -1 % count;
				offset_diff = (offset_diff + increment_mul) % count;
				break;

			case Technique.Cut:
				offset_diff = (offset_diff + i.Value * increment_mul) % count;
				break;

			case Technique.DealWithIncrement:
				increment_mul = increment_mul * Inv(i.Value, count) % count;
				break;
		}
	}
	
	(BigInteger increment, BigInteger offset) = Sequence(iterations, count, increment_mul, offset_diff);
  var answer = Nth(offset, increment, lookAt, count);
  if (answer < 0)
	{
		answer = count + answer;
	}
	Console.WriteLine(answer);
}

static BigInteger Inv(BigInteger n, BigInteger count)
{
	return BigInteger.ModPow(n, count - 2, count);
}

static BigInteger Nth(BigInteger offset, BigInteger increment, BigInteger i, BigInteger count)
{
		return (offset + i * increment) % count;
}

static (BigInteger increment, BigInteger offset) Sequence(BigInteger interations, BigInteger count, BigInteger increment_mul, BigInteger offset_diff)
{
		var increment = BigInteger.ModPow(increment_mul, interations, count);
		var offset = offset_diff * (1 - increment) * Inv((1 - increment_mul) % count, count);
		offset %= count;
		return (increment, offset);
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

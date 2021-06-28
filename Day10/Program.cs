using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 10

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
var location = Part1();
Part2(location);
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static (int, int) Part1()
{
	var max = -1;
	var location = (-1, -1);
	var asteroids = ReadInput().ToList();
	foreach (var a in asteroids)
	{
		var inLineOfSight = GetInLineOfSight(a, asteroids);
		if (inLineOfSight.Count > max)
		{
			max = inLineOfSight.Count;
			location = a;
		}
	}
	Console.WriteLine($"Best location is {location} with {max} asteroids in line of sight.");
	return location;
}

static void Part2((int, int) location)
{
	var asteroids = ReadInput().ToList();
	var i = 1;
	while (asteroids.Count > 1)
	{
		foreach (var a in GetInLineOfSight(location, asteroids))
		{
			//Console.WriteLine($"{i++} vaporizing {a}");
			if (i == 200)
			{
				Console.WriteLine($"The 200th asteroid to be vaporized is at {a} -> code {a.x * 100 + a.y}");
			}
			asteroids.Remove(a);
			i++;
		}
	}
}

static List<(int x, int y)> GetInLineOfSight((int x, int y) a, List<(int x, int y)> asteroids)
{
	var inLineOfSight = new List<(int x, int y)>();
	inLineOfSight.AddRange(GetInLineOfSightForQuarter(a, asteroids, 1));
	inLineOfSight.AddRange(GetInLineOfSightForQuarter(a, asteroids, 2));
	inLineOfSight.AddRange(GetInLineOfSightForQuarter(a, asteroids, 3));
	inLineOfSight.AddRange(GetInLineOfSightForQuarter(a, asteroids, 4));
	return inLineOfSight;
}

static List<(int x, int y)> GetInLineOfSightForQuarter((int x, int y) a, List<(int x, int y)> asteroids, int quarter)
{
	var inLineOfSight = new List<(int x, int y)>();
	var filtered = asteroids.Where(aa => a != aa && quarter switch
	{
		1 => (aa.x - a.x) >= 0 && (a.y - aa.y) > 0,
		2 => (aa.x - a.x) > 0 && (a.y - aa.y) <= 0,
		3 => (aa.x - a.x) <= 0 && (a.y - aa.y) <= 0,
		4 => (aa.x - a.x) < 0 && (a.y - aa.y) > 0,
		_ => throw new InvalidOperationException()
	});

	foreach (var b in filtered)
	{
		var haveLineOfSight = true;
		var distAB = GetDistance(a, b);
		foreach (var c in filtered)
		{
			var distAC = GetDistance(a, c);
			// if c is closer to a than b, only then can it occlude the line-of-sight
			if (distAC < distAB)
			{
				// check if a, b, c share a line-of-sight
				var sAB = GetSlope(a, b);
				var sAC = GetSlope(a, c);
				if (AreEqual(sAB, sAC))
				{
					haveLineOfSight = false;
					break;
				}
			}
		}
		if (haveLineOfSight)
		{
			inLineOfSight.Add(b);
		}
	}
	inLineOfSight.Sort((x, y) => GetSlope(a, y).CompareTo(GetSlope(a, x)));
	return inLineOfSight;
}

static double GetSlope((int x, int y) a, (int x, int y) b)
{
	var dy = (double)b.y - a.y;
	var dx = (double)b.x - a.x;
	return dy == 0 ? double.PositiveInfinity : dx / dy;
}

static bool AreEqual(double x, double y)
{
	const double delta = 0.0000001;
	return x == y || Math.Abs(x - y) < delta;
}

static double GetDistance((int x, int y) a, (int x, int y) b)
{
	var dx = Math.Abs(a.x - b.x);
	var dy = Math.Abs(a.y - b.y);
	return Math.Sqrt(dx * dx + dy * dy);
}

static IEnumerable<(int x, int y)> ReadInput()
{
	var lines = File.ReadAllLines("input.txt");
	var w = lines.First().Length;
	var h = lines.Length;
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			if (lines[y][x] == '#')
			{
				yield return (x, y);
			}
		}
	}
}

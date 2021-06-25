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
	location = (8, 3);
	var asteroids = ReadInput().ToList();
	foreach (var a in GetInLineOfSight(location, asteroids))
	{
		System.Console.WriteLine(a);
	}
}

static List<(int x, int y)> GetInLineOfSight((int x, int y) a, List<(int x, int y)> asteroids)
{
	var inLineOfSight = new List<(int x, int y)>();
	foreach (var b in asteroids)
	{
		if (a != b)
		{
			var haveLineOfSight = true;
			var distAB = GetDistance(a, b);
			foreach (var c in asteroids)
			{
				if (a != c)
				{
					var distAC = GetDistance(a, c);
					// if c is closer to a than b, only then can it occlude the line-of-sight
					if (distAC < distAB)
					{
						// check if a, b, c share a line-of-sight
						var normAB = Normalize(a, b);
						var normAC = Normalize(a, c);
						if (AreEqual(normAB, normAC))
						{
							haveLineOfSight = false;
							break;
						}
					}
				}
			}
			if (haveLineOfSight)
			{
				inLineOfSight.Add(b);
			}
		}
	}
	return inLineOfSight;
}

static bool AreEqual((double dx, double dy) a, (double dx, double dy) b)
{
	const double delta = 0.0000001;
	return
		Math.Abs(b.dx - a.dx) < delta &&
		Math.Abs(b.dy - a.dy) < delta;
}

static (double dx, double dy) Normalize((int x, int y) a, (int x, int y) b)
{
	var distance = GetDistance(a, b);
	var dx = (b.x - a.x) / distance;
	var dy = (b.y - a.y) / distance;
	return (dx, dy);
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

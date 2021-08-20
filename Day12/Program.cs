using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

Console.WriteLine("Day 12 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var moons = ReadInput().ToArray();
	var steps = 1000;
	for (var i = 0; i < steps; i++)
	{
		/*
		Console.WriteLine();
		Console.WriteLine($"After {i} steps:");
		foreach (var m in moons)
		{
			Console.WriteLine(m);
		}
		*/
		Simulate(moons, Dimension.X);
		Simulate(moons, Dimension.Y);
		Simulate(moons, Dimension.Z);
	}	
	Console.WriteLine($"Energy after {steps} steps: {moons.Sum(m => m.Energy)}");
}

static void Part2()
{
	var x = FindRepeatSteps(Dimension.X);
	var y = FindRepeatSteps(Dimension.Y);
	var z = FindRepeatSteps(Dimension.Z);
	Console.WriteLine($"After {LCM3(x, y, z)} steps the universe repeats itself");
}

static int FindRepeatSteps(Dimension dimension)
{
	var moons = ReadInput().ToArray();
	var steps = 0;
	var cache = new HashSet<string>();
	while (true)
	{
		var key = string.Join('-', moons.Select(m => m.ToString()));
		if (cache.Contains(key))
		{
			break;
		}
		cache.Add(key);
		Simulate(moons, dimension);
		steps++;
	}
	return steps;
}

static long LCM3(long a, long b, long c)
{
	return LCM2(a, LCM2(b, c));
}

static long LCM2(long a, long b)
{
	long num1 = 0;
	long num2 = 0;
	if (a > b)
	{
		num1 = a;
		num2 = b;
	}
	else
	{
		num1 = b;
		num2 = a;
	}

	for (var i = 1; i < num2; i++)
	{
		long mult = num1 * i;
		if (mult % num2 == 0)
		{
			return mult;
		}
	}
	return num1 * num2;
}


static void Simulate(Moon[] moons, Dimension dimension)
{
	foreach (var m1 in moons)
	{
		foreach (var m2 in moons)
		{
			if (m1 != m2)
			{
				m1.ApplyGravity(m2, dimension);
			}
		}
	}
	foreach (var m in moons)
	{
		m.UpdatePosition(dimension);
	}
}

static IEnumerable<Moon> ReadInput()
{
	foreach (var line in File.ReadAllLines("input.txt"))
	{
		var tokens = line[1..^1].Split(", ");
		yield return new Moon((
			int.Parse(tokens[0].Split('=')[1]),
			int.Parse(tokens[1].Split('=')[1]),
			int.Parse(tokens[2].Split('=')[1])
		));
	}
}

enum Dimension
{
	X,
	Y,
	Z,
}
class Moon
{
	private (int x, int y, int z) _pos;
	private (int x, int y, int z) _vel;

	internal Moon((int x, int y, int z) position)
	{
		_pos = position;
	}

	public override string ToString()
	{
		return $"pos<x={_pos.x}, y={_pos.y}, z={_pos.z}>, vel<x={_vel.x}, y={_vel.y}, z={_vel.z}>";
	}

	public int Energy { get => PotentialEnergy * KineticEnergy; }
	public int PotentialEnergy
	{ 
		get
		{
			return Math.Abs(_pos.x) + Math.Abs(_pos.y) + Math.Abs(_pos.z);
		}
	}
	public int KineticEnergy
	{ 
		get
		{
			return Math.Abs(_vel.x) + Math.Abs(_vel.y) + Math.Abs(_vel.z);
		}
	}

	internal void ApplyGravity(Moon m, Dimension dimension)
	{
		if (dimension == Dimension.X)
		{
			if (_pos.x > m._pos.x)
			{
				_vel.x -= 1;
			}
			else if (_pos.x < m._pos.x)
			{
				_vel.x += 1;
			}
		}
		else if (dimension == Dimension.Y)
		{
			if (_pos.y > m._pos.y)
			{
				_vel.y -= 1;
			}
			else if (_pos.y < m._pos.y)
			{
				_vel.y += 1;
			}
		}
		else if (dimension == Dimension.Z)
		{
			if (_pos.z > m._pos.z)
			{
				_vel.z -= 1;
			}
			else if (_pos.z < m._pos.z)
			{
				_vel.z += 1;
			}
		}
	}

	internal void UpdatePosition(Dimension dimension)
	{
		if (dimension == Dimension.X)
		{
			_pos.x += _vel.x;
		}
		else if (dimension == Dimension.Y)
		{
			_pos.y += _vel.y;
		}
		else if (dimension == Dimension.Z)
		{
			_pos.z += _vel.z;
		}
	}
}
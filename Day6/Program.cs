using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 6

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var orbits = ReadInput();
	var count = 0;
	foreach (var o in orbits)
	{
		count += GetAllOrbitsFor(o.Key, orbits);
	}
	Console.WriteLine($"Total number of orbits {count}.");
}

static void Part2()
{
	var orbits = ReadInput();
	var orbitedByMe = GetAllOrbited(orbits["YOU"], orbits);
	var orbitedBySanta = GetAllOrbited(orbits["SAN"], orbits);
	var commonOrbited = orbitedByMe.Keys.Intersect(orbitedBySanta.Keys);
	var minimumHops = commonOrbited.Min(o => orbitedByMe[o] + orbitedBySanta[o]);
	Console.WriteLine("Minimum orbital transfers: " + minimumHops);
}

static Dictionary<string, int> GetAllOrbited(string o, Dictionary<string, string> orbits)
{
	var orbited = new Dictionary<string, int>();
	var distance = 1;
	while (orbits.ContainsKey(o))
	{
		o = orbits[o];
		orbited.Add(o, distance++);
	}
	return orbited;
}

static int GetAllOrbitsFor(string o, Dictionary<string, string> orbits)
{
	var count = 0;
	if (orbits.ContainsKey(o))
	{
		count = 1 + GetAllOrbitsFor(orbits[o], orbits);
	}
	return count;
}

static Dictionary<string, string> ReadInput()
{
	var orbits = new Dictionary<string, string>();
	foreach (var line in File.ReadAllLines("input.txt"))
	{
		var tokens = line.Split(')');
		orbits[tokens[1]] = tokens[0];
	}
	return orbits;
}

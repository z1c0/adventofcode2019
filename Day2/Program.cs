using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 2

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var result = new IntCodeCpu().Run(ReadInput(), 12, 2);
	Console.WriteLine($"result: {result}");
}

static void Part2()
{
	for (var noun = 0; noun <= 99; noun++)
	{
		for (var verb = 0; verb <= 99; verb++)
		{
			if (new IntCodeCpu().Run(ReadInput(), noun, verb) == 19690720)
			{
				Console.WriteLine($"100 * noun ({noun}) + verb ({verb}) = {100 * noun + verb}");
			}
		}
	}
}

static IEnumerable<int> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return int.Parse(line);
	}
}

class IntCodeCpu
{
	private int _pc;

	internal int Run(IEnumerable<int> input, int noun, int verb)
	{
		_pc = 0;
		var memory = input.ToList();
		memory[1] = noun;
		memory[2] = verb;
		while (true)
		{
			switch (memory[_pc])
			{
				case 1:
					memory[memory[_pc + 3]] = memory[memory[_pc + 1]] + memory[memory[_pc + 2]];
					_pc += 4;
					break;
					
				case 2:
					memory[memory[_pc + 3]] = memory[memory[_pc + 1]] * memory[memory[_pc + 2]];
					_pc += 4;
					break;

				case 99:
					return memory[0];

				default:
					throw new InvalidProgramException();
			}	
		}
	}
}
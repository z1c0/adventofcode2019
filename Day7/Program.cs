using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 7

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var instructions = ReadInput();
	var permutations = CreatePermutations(new[]{ 0, 1, 2, 3, 4 });
	var max = permutations.Max(p => RunPermutation(p, instructions));
	Console.WriteLine($"Maximum thrust: {max}");
}

static void Part2()
{
	var instructions = ReadInput();
	var permutations = CreatePermutations(new[]{ 5, 6, 7, 8, 9 });
	var max = permutations.Max(p => RunPermutation(p, instructions));
	Console.WriteLine($"Maximum thrust: {max}");
}

static int RunPermutation(int[] input, IEnumerable<int> instructions)
{
	var cpuA = new IntCodeCpu(instructions);
	var cpuB = new IntCodeCpu(instructions);
	var cpuC = new IntCodeCpu(instructions);
	var cpuD = new IntCodeCpu(instructions);
	var cpuE = new IntCodeCpu(instructions);
	cpuA.Inputs.Add(input[0]);
	cpuB.Inputs.Add(input[1]);
	cpuC.Inputs.Add(input[2]);
	cpuD.Inputs.Add(input[3]);
	cpuE.Inputs.Add(input[4]);

	var v = 0;
	do
	{
		cpuA.Inputs.Add(v);
		v = cpuA.Run();
		cpuB.Inputs.Add(v);
		v = cpuB.Run();
		cpuC.Inputs.Add(v);
		v = cpuC.Run();
		cpuD.Inputs.Add(v);
		v = cpuD.Run();
		cpuE.Inputs.Add(v);
		v = cpuE.Run();
	}
	while (cpuA.IsRunning ||cpuB.IsRunning || cpuC.IsRunning || cpuD.IsRunning || cpuE.IsRunning);

	return v;
}

static List<int[]> CreatePermutations(int[] input, int size = -1)
{
	if (size == -1)
	{
		size = input.Length;
	}
	var permutations = new List<int[]>();
	// if size becomes 1 then prints the obtained
	// permutation
	if (size == 1)
	{
		permutations.Add(input.ToArray());
	}
	for (var i = 0; i < size; i++)
	{
		permutations.AddRange(CreatePermutations(input, size - 1));

		// if size is odd, swap 0th i.e (first) and
		// (size-1)th i.e (last) element
		if (size % 2 == 1)
		{
			var temp = input[0];
			input[0] = input[size - 1];
			input[size - 1] = temp;
		}

		// If size is even, swap ith and
		// (size-1)th i.e (last) element
		else
		{
			int temp = input[i];
			input[i] = input[size - 1];
			input[size - 1] = temp;
		}
	}
	return permutations;
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
	private readonly List<int> _memory;

	public IntCodeCpu(IEnumerable<int> instructions)
	{
		_pc = 0;
		_memory = instructions.ToList();
	}

	public List<int> Inputs { get; } = new List<int>();
	public int Output { get; private set; }
	public bool IsRunning { get; private set; }

	internal int Run()
	{
		IsRunning = true;
		while (true)
		{
			var instruction = new Instruction(_memory, _pc);
			switch (instruction.OpCode)
			{
				case 1:
					// add
					_memory[_memory[_pc + 3]] = instruction.Parameter1 + instruction.Parameter2;
					_pc += 4;
					break;

				case 2:
					// multiply
					_memory[_memory[_pc + 3]] = instruction.Parameter1 * instruction.Parameter2;
					_pc += 4;
					break;

				case 3:
					// read input
					_memory[_memory[_pc + 1]] = Inputs.First();
					Inputs.RemoveAt(0);
					_pc += 2;
					break;

				case 4:
					// write output
					Output = instruction.Parameter1;
					_pc += 2;
					return Output;

				case 5:
					// jump-if-true
					_pc = (instruction.Parameter1 != 0) ? instruction.Parameter2 : _pc + 3;
					break;

				case 6:
					// jump-if-false
					_pc = (instruction.Parameter1 == 0) ? instruction.Parameter2 : _pc + 3;
					break;

				case 7:
					// less than
					_memory[_memory[_pc + 3]] = (instruction.Parameter1 < instruction.Parameter2) ? 1 : 0;
					_pc += 4;
					break;

				case 8:
					// equals
					_memory[_memory[_pc + 3]] = (instruction.Parameter1 == instruction.Parameter2) ? 1 : 0;
					_pc += 4;
					break;

				case 99:
					// halt
					IsRunning = false;
					return Output;

				default:
					throw new InvalidProgramException($"Invalid OpCode {instruction.OpCode}");
			}
		}
	}
}

internal enum ParameterMode
{
	Position,
	Immediate,
}

internal class Instruction
{
	private readonly List<int> _memory;
	private readonly int _pc;

	internal Instruction(List<int> memory, int pc)
	{
		_memory = memory;
		_pc = pc;
	}

	private static ParameterMode ParseParameterMode(int code) => code switch
	{
		0 => ParameterMode.Position,
		1 => ParameterMode.Immediate,
		_ => throw new InvalidProgramException()
	};

	public override string ToString()
	{
		return $"{OpCode}";
	}


	public int OpCode { get => _memory[_pc] % 100; }
	public int Parameter1 { get => GetParameter(1, 100); }
	public int Parameter2 { get => GetParameter(2, 1000); }
	public int Parameter3 { get => GetParameter(3, 10000); }

	private int GetParameter(int pos, int modePos)
	{
		var fullCode = _memory[_pc];
		var v = _memory[_pc + pos];
		var parameterMode = ParseParameterMode(fullCode / modePos % 10);
		return parameterMode == ParameterMode.Immediate ?
			v :
			_memory[v];
	}
}
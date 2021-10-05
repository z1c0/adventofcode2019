using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 17 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();

Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var instructions = ReadInput().ToList();
	var grid = BuildGrid(instructions);

	Print(grid);

	var sumOfAlignmentParameters = 0;
	for (var y = 1; y < grid.Count - 1; y++)
	{
		for (var x = 1; x < grid[y].Count - 1; x++)
		{
			if (grid[y][x] == '#' && grid[y - 1][x] == '#' && grid[y + 1][x] == '#' && grid[y][x - 1] == '#' && grid[y][x + 1] == '#')
			{
				sumOfAlignmentParameters += (x * y);
			}
		}
	}
	Console.WriteLine($"Sum of the alignment parameters: {sumOfAlignmentParameters}");
}

static void Part2()
{
	var instructions = ReadInput().ToList();
	instructions[0] = 2;

	var i = 0;
	var main = "A,B,A,B,C,C,B,A,B,C";
	var A = "L,4,R,8,L,6,L,10";
	var B = "L,6,R,8,R,10,L,6,L,6";
	var C = "L,4,L,4,L,10";
	var video = "n";
	var input = $"{main}\n{A}\n{B}\n{C}\n{video}\n".Select(c => (int)c).ToArray();

	var run = true;
	var cpu = new IntCodeCpu(instructions.ToList())
	{
		WriteOutput = o => 
		{
			if (o < 255)
			{
				Console.Write((char)o);
			}
			else
			{
				Console.WriteLine($"Score: {o}");
			}
			run = true;
		},
		ReadInput = () => input[i++],
	};
	while (run)
	{
		run = false;
		cpu.Run();
	};
}

static List<List<char>> BuildGrid(List<long> instructions)
{
	var grid = new List<List<char>>();
	var line = new List<char>();
	var run = true;
	var cpu = new IntCodeCpu(instructions.ToList())
	{
		WriteOutput = o => 
		{
			if (o == 10)
			{
				if (line.Any())
				{
					grid.Add(line);
					line = new();
				}
			}
			else
			{
				line.Add((char)o);
			}
			run = true;
		}
	};
	while (run)
	{
		run = false;
		cpu.Run();
	}
	return grid;
}

static void Print(List<List<char>> grid)
{
	for (var y = 0; y < grid.Count; y++)
	{
		for (var x = 0; x < grid[y].Count; x++)
		{
			Console.Write(grid[y][x]);
		}
		Console.WriteLine();
	}
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class IntCodeCpu
{
	private long _pc;
	private long _relativeBase;
	private readonly Dictionary<long, long> _memory;

	private IntCodeCpu(long pc, long relativeBase, Dictionary<long, long> memory)
	{
			_pc = pc;
			_relativeBase = relativeBase;
			_memory = memory;
	}

	public IntCodeCpu(IEnumerable<long> instructions)
	{
		_pc = 0;
		_relativeBase = 0;
		long i = 0;
		_memory = instructions.ToDictionary(a => i++);
	}

	public bool IsRunning { get; private set; }

	internal Func<long> ReadInput { get; set; }
	internal Action<long> WriteOutput { get; set; }

	internal IntCodeCpu Clone()
	{
		return new IntCodeCpu(_pc, _relativeBase, new Dictionary<long, long>(_memory));
	}

	internal void Run()
	{
		IsRunning = true;
		while (true)
		{
			var opCode = GetCurrentOpCode();
			switch (opCode)
			{
				case 1:
					// add
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) + ReadFrom(GetParameter(2)));
					_pc += 4;
					break;

				case 2:
					// multiply
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) * ReadFrom(GetParameter(2)));
					_pc += 4;
					break;

				case 3:
					// read input
					WriteTo(GetParameter(1), ReadInput());
					_pc += 2;
					break;

				case 4:
					// write output
					WriteOutput(ReadFrom(GetParameter(1)));
					_pc += 2;
					return;

				case 5:
					// jump-if-true
					_pc = ReadFrom(GetParameter(1)) != 0 ? ReadFrom(GetParameter(2)) : _pc + 3;
					break;

				case 6:
					// jump-if-false
					_pc = ReadFrom(GetParameter(1)) == 0 ? ReadFrom(GetParameter(2)) : _pc + 3;
					break;

				case 7:
					// less than
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) < ReadFrom(GetParameter(2)) ? 1 : 0);
					_pc += 4;
					break;

				case 8:
					// equals
					WriteTo(GetParameter(3), ReadFrom(GetParameter(1)) == ReadFrom(GetParameter(2)) ? 1 : 0);
					_pc += 4;
					break;

				case 9:
					// adjust relative base
					_relativeBase += ReadFrom(GetParameter(1));
					_pc += 2;
					break;

				case 99:
					// halt
					IsRunning = false;
					return;

				default:
					throw new InvalidProgramException($"Invalid OpCode {opCode}");
			}
		}
	}

	private long GetCurrentOpCode() => _memory[_pc] % 100;

	private long GetParameter(int pos)
	{
		var modePos = (long)Math.Pow(10, pos + 1);
		var fullCode = _memory[_pc];
		var v = _pc + pos;
		var parameterMode = ParseParameterMode(fullCode / modePos % 10);
		return parameterMode switch
		{
			ParameterMode.Immediate => v,
			ParameterMode.Position => ReadFrom(v),
			ParameterMode.Relative => ReadFrom(v) + _relativeBase,
			_ => throw new InvalidOperationException(),
		};
	}

	private void WriteTo(long address, long value)
	{
		_memory[address] = value;
	}

	private long ReadFrom(long address)
	{
		if (_memory.TryGetValue(address, out var value))
		{
			return value;
		}
		return 0;
	}

	private static ParameterMode ParseParameterMode(long code) => code switch
	{
		0 => ParameterMode.Position,
		1 => ParameterMode.Immediate,
		2 => ParameterMode.Relative,
		_ => throw new InvalidProgramException()
	};
}

internal enum ParameterMode
{
	Position,
	Immediate,
	Relative,
}
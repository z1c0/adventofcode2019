using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// Day 11

Console.WriteLine("START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var instructions = ReadInput();
	var robot = new Robot(0);
	robot.Run(instructions);
	Console.WriteLine($"Number of panels painted at least once: {robot.Panels.Count}");
}

static void Part2()
{
	var instructions = ReadInput();
	var robot = new Robot(1);
	robot.Run(instructions);
	robot.PrintPanel();
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class Robot
{
	enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}
	internal Dictionary<(int, int), long> Panels { get; set;} = new();
	private int _x;
	private int _y;
	private Direction _direction = Direction.Up;
	private bool _expectColor = true;
	private readonly long _defaultColor;

	public Robot(long defaultColor)
	{
		_defaultColor = defaultColor;
	}

	internal void Run(IEnumerable<long> instructions)
	{
		var cpu = new IntCodeCpu(instructions)
		{
			ReadInput = () => 
			{
				if (Panels.TryGetValue((_x, _y), out var color))
				{
					return color;
				}
				return _defaultColor;
			},
			WriteOutput = (value) =>
			{
				if (value > 1)
				{
					throw new Exception($"Unexpected output: {value}");
				}

				if (_expectColor)
				{
					Panels[(_x, _y)] = value;
					_expectColor = false;
				}
				else
				{
					if (value == 0)
					{
						TurnLeft();
					}
					else
					{
						TurnRight();
					}
					_x = _direction switch
					{
						Direction.Left => _x - 1,
						Direction.Right => _x + 1,
						_ => _x,
					};
					_y = _direction switch
					{
						Direction.Up => _y - 1,
						Direction.Down => _y + 1,
						_ => _y,
					};
					_expectColor = true;
				}
			}
		};
		
		do
		{
			cpu.Run();
		}
		while (cpu.IsRunning);
	}

	private void TurnRight()
	{
		_direction = _direction switch
		{
			Direction.Up => Direction.Right,
			Direction.Right => Direction.Down,
			Direction.Down => Direction.Left,
			Direction.Left => Direction.Up,
			_ => throw new Exception("Unexpected direction"),
		};
	}

	private void TurnLeft()
	{
		_direction = _direction switch
		{
			Direction.Up => Direction.Left,
			Direction.Left => Direction.Down,
			Direction.Down => Direction.Right,
			Direction.Right => Direction.Up,
			_ => throw new Exception("Unexpected direction"),
		};
	}

	internal void PrintPanel()
	{
		var w = Panels.Keys.Max(k => k.Item1);
		var h = Panels.Keys.Max(k => k.Item2);
		for (var y = 0; y <= h; y++)
		{
			for (var x = 0; x <= w; x++)
			{
				var color = Panels.TryGetValue((x, y), out var value) ? value : 0;
				Console.Write(color == 1 ? '#' : ' ');
			}
			Console.WriteLine();
		}
	}
}

class IntCodeCpu
{
	private long _pc;
	private long _relativeBase;
	private readonly Dictionary<long, long> _memory;

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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

Console.WriteLine("Day 23 - START");
var sw = Stopwatch.StartNew();
Part1();
Part2();
Console.WriteLine($"END (after {sw.Elapsed.TotalSeconds} seconds)");

static void Part1()
{
	var nics = Setup();
	while (true)
	{
		foreach (var n in nics)
		{
			n.Step();
			var nat = nics.Last();
			if (nat.QueueLength > 0)
			{
				Console.WriteLine(nat.GetQueueElementAt(0));
				return;
			}
		}
	}
}

static void Part2()
{
	var nics = Setup();
	var nat = (Nat)nics.Last();
	while (!nat.CanStop)
	{
		foreach (var n in nics)
		{
			n.Step();
		}
	}
}

static List<Nic> Setup()
{
	var instructions = ReadInput();
	const int count = 50;
	var nics = new List<Nic>();
	for (var i = 0; i < count; i++)
	{
		nics.Add(new Nic(instructions, i, nics));
	}
	nics.Add(new Nat()); // NAT
	return nics;
}

static IEnumerable<long> ReadInput()
{
	foreach (var line in File.ReadAllText("input.txt").Split(','))
	{
		yield return long.Parse(line);
	}
}

class Packet
{
	public Packet(long to)
	{
		To = to;
	}

	public override string ToString()
	{
		return $"X: {X}, Y: {Y} -> to: {To}";
	}

	public long X { get; internal set; }
	public long Y { get; internal set; }
	public bool XWritten { get; internal set; }
	public bool XRead { get; internal set; }
	public long To { get; }
}

class Nat : Nic
{
	private readonly Dictionary<int, bool> _idleList = new();
	private HashSet<long> _history = new();

	public bool CanStop { get; private set; }

	public Nat() : base(Array.Empty<long>(), 255, null)
	{
	}

	public override void Step()
	{
		// do nothing
	}

	public override void Enqueue(Packet packet)
	{
		_queue.Clear();
		_queue.Add(packet);
	}

	internal void NotifyIdle(Nic nic, bool state, List<Nic> nics)
	{
		_idleList[nic.Id] = state;
		if (_idleList.Count(e => e.Value) == 50)
		{
			if (QueueLength > 0)
			{
				var p = _queue[0];
				_queue.Clear();
				if (_history.Contains(p.Y))
				{
					Console.WriteLine($"{p.Y} is the first Y value sent twice.");
					CanStop = true;
				}
				else
				{
					_history.Add(p.Y);
					nics[0].Enqueue(p);
				}
			}
		}
	}
}

class Nic
{
	public int Id { get; }

	private bool _init;
	private Packet _currentPacket;
	private readonly IntCodeCpu _cpu;
	protected readonly List<Packet> _queue = new();
	public Nic(IEnumerable<long> instructions, int id, List<Nic> nics)
	{
		Id = id;
		_init = true;
		_cpu = new IntCodeCpu(instructions)
		{
			ReadInput = () =>
			{
				if (_init)
				{
					_init = false;
					return id;
				}
				var nat = (Nat)nics.Last();
				if (!_queue.Any())
				{
					nat.NotifyIdle(this, true, nics);
					return -1;
				}
				nat.NotifyIdle(this, false, nics);
				var p = _queue.First();
				if (!p.XRead)
				{
					p.XRead = true;
					return p.X;
				}
				_queue.RemoveAt(0);
				return p.Y;
			},
			WriteOutput = o =>
			{
				if (_currentPacket == null)
				{
					_currentPacket = new Packet(o);
				}
				else if (!_currentPacket.XWritten)
				{
					_currentPacket.X = o;
					_currentPacket.XWritten = true;
				}
				else
				{
					_currentPacket.Y = o;
					nics.Single(n => n.Id == _currentPacket.To).Enqueue(_currentPacket);
					_currentPacket = null;
				}
			}
		};
	}

	public virtual void Enqueue(Packet packet)
	{
		_queue.Add(packet);
	}

	public int QueueLength { get => _queue.Count; }

	public virtual void Step()
	{
		_cpu.Step();
	}

	internal Packet GetQueueElementAt(int index)
	{
		return _queue[index];
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
		while (IsRunning)
		{
			Step();
		}
	}

	public void Step()
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
				break;

			default:
				throw new InvalidProgramException($"Invalid OpCode {opCode}");
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
using System;
using System.Collections.Generic;
using System.Linq;
using aoc;

class State : IComparable
{
	private readonly Grid _map;
	private readonly List<(int X, int Y)> _positions = new();
	private List<HashSet<string>> _caches = new();

	public State(Grid map)
	{
		_map = map;
	}

	internal void AddPosition((int X, int Y) p)
	{
		_positions.Add(p);
		_caches.Add(new());
	}
	internal List<char> Keys { get; init; } = new ();

	internal int Distance { get; set; }

	public override string ToString()
	{
		return string.Join(" - ", _positions.Select(p => $"{p.X}/{p.Y}"));
	}

	internal State Copy()
	{
		return new State(_map)
		{ 
			Keys = Keys.ToList(),
			Distance = Distance,
			_caches = _caches,
		};
	}

	internal string FingerPrint(int pos)
	{
		var xy = _positions[pos - 1].ToString();
		var k = string.Join('|', Keys.OrderBy(k => k));
		return string.Format($"[{xy}]-{k}");
	}

	public int CompareTo(object obj)
	{
		var other = (State)obj;
		return other.Distance.CompareTo(Distance);
	}

	internal IEnumerable<State> GetAdjacent()
	{
		static (int X, int Y) GetNextPosition((int X, int Y) p, int i, bool move)
		{
			if (!move)
			{
				return p;
			}
			return i switch
			{
				0 => (p.X - 1, p.Y),
				1 => (p.X + 1, p.Y),
				2 => (p.X, p.Y - 1),
				3 => (p.X, p.Y + 1),
				_ => throw new InvalidOperationException(),
			};
		}

		for (var i = 0; i < _positions.Count; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				var state = Copy();
				state.Distance++;
				for (var k = 0; k < _positions.Count; k++)
				{
					state._positions.Add(GetNextPosition(_positions[k], j, i == k));
				}
				var fingerPrint = state.FingerPrint(i + 1);
				if (!_caches[i].Contains(fingerPrint))
				{
					_caches[i].Add(fingerPrint);
					if (state.Check())
					{
						yield return state;
					}
				}
			}
		}
	}

	private bool Check()
	{
		foreach (var p in _positions)
		{
			if (!_map.IsInBounds(p.X, p.Y))
			{
				return false;
			}
			var c = _map[p.X, p.Y];
			if (c == '#')
			{
				return false;
			}
			if (c >= 'A' && c <= 'Z')
			{		
				// Do we have a key for this door?
				if (!Keys.Contains(char.ToLower(c)))
				{
					return false;
				}
			}
			if (c >= 'a' && c <= 'z' && !Keys.Contains(c))
			{
				Keys.Add(c);
			}
		}
		return true;
	}
}

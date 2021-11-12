using System.Collections.Generic;
using System.Linq;
using aoc;

internal class SuperState : IState
{
	private readonly Grid _map;
	private readonly State _state1;
	private readonly State _state2;
	private readonly State _state3;
	private readonly State _state4;
	private int _lastKey;

	public SuperState(Grid map, State state1, State state2, State state3, State state4)
	{
		_map = map;
		_state1 = state1;
		_state2 = state2;
		_state3 = state3;
		_state4 = state4;
	}

	public int KeyCount => _state1.KeyCount + _state2.KeyCount + _state3.KeyCount + _state4.KeyCount + _lastKey;

	public int Distance => _state1.Distance + _state2.Distance + _state3.Distance + _state4.Distance;

	public int CompareTo(object obj)
	{
		return ((SuperState)obj).Distance.CompareTo(Distance);
	}

	public IEnumerable<IState> GetAdjacent()
	{
		var states1 = _state1.GetAdjacent(this).ToList();
		var states2 = _state2.GetAdjacent(this).ToList();
		var states3 = _state3.GetAdjacent(this).ToList();
		var states4 = _state4.GetAdjacent(this).ToList();
		if (states1.Any() || states2.Any() || states3.Any() || states4.Any())
		{
			if (!states1.Any())
			{
				states1.Add(_state1.Clone());
			}
			if (!states2.Any())
			{
				states2.Add(_state2.Clone());
			}
			if (!states3.Any())
			{
				states3.Add(_state3.Clone());
			}
			if (!states4.Any())
			{
				states4.Add(_state4.Clone());
			}
			foreach (var s1 in states1)
			{
				foreach (var s2 in states2)
				{
					foreach (var s3 in states3)
					{
						foreach (var s4 in states4)
						{
							var superState = new SuperState(_map, s1, s2, s3, s4)
							{
								_lastKey = _lastKey
							};
							yield return superState;
						}
					}
				}
			}
		}
	}

	public bool HasKeyForDoor(char door)
	{
		return
			_state1.HasKeyForDoor(door) ||
			_state2.HasKeyForDoor(door) ||
			_state3.HasKeyForDoor(door) ||
			_state4.HasKeyForDoor(door);
	}

	public void AddKey(char key)
	{
		var door = char.ToUpper(key);
		var (x, y) = _map.Find(door);
		if ((x, y) == (-1, -1))
		{
			// No door, last key
			_lastKey = 1;
		}
		else
		{
			var isLeft = x < _map.Width / 2;
			var isTop = y < _map.Height / 2;
			if (isLeft)
			{
				if (isTop)
				{
					_state1.Keys.Add(key);
				}
				else
				{
					_state3.Keys.Add(key);
				}
			}
			else
			{
				if (isTop)
				{
					_state2.Keys.Add(key);
				}
				else
				{
					_state4.Keys.Add(key);
				}
			}
		}
	}
}
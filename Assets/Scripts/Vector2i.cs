using System;
using UnityEngine;
using System.Collections;

[Serializable]
public struct Vector2i {
	public int x;
	public int y;

	public Vector2i(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public Vector2i(float _x, float _y)
	{
		x = (int)_x;
		y = (int)_y;
	}

	public bool Equals(Vector2i v2)
	{
		if (this.x == v2.x && this.y == v2.y)
			return true;
		return false;
	}

}

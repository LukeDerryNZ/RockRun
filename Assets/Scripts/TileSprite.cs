using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TileSprite 
{
	public string Name;
	public Sprite tileImage;
	public Tiles tileType;
	public int frameNum;

	public bool rounded;		//If a falling object hits me will it roll to the side or stop?
	public bool consumable;		//Can this object be consumed by an explosion?
	public bool explosive;		//Does this object explode when hit by an object?

	void Start()
	{
		
	}

	//

	public TileSprite()
	{
		Name = "DEFAULT";
		tileImage = new Sprite();
		tileType = Tiles.Air;
		rounded = true;
		consumable = false;
		explosive = false;
		frameNum = 0;
	}

	//

	public TileSprite(string _name, Sprite img, Tiles tile, bool _rounded, bool _consumable, bool _explosive, int frame=0)
	{
		Name = _name;
		tileImage = img;
		tileType = tile;
		rounded = _rounded;
		consumable = _consumable;
		explosive = _explosive;
		frameNum = frame;
	}
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lean;
using UnityEngine.Audio;

public class TileManager : MonoBehaviour 
{
	
	#region GLOBAL VARS

	[HideInInspector]
	public Vector2i MapSize;
	public GameObject TileContainer;
	public GameObject TilePrefab;
	private int Frame;

	[HideInInspector]
	public Vector2i curPos;
	public Vector2i ViewPortSize;
	private Vector2i ViewOffset;

	//Managers
	private GameManager GM;
	private LevelLoader LL;
	private InputManager IM;
	private AudioManager AM;
	public GameObject UIContainer;
	private UIManager UI;
	public ParticleSystem PS;

	//Game Logic Variables
	public int explodeSize = 1;

	//DEBUGGING
	public bool DEBUG = false;

	//Our Tile Library
	public List<TileSprite> TileSprites;

	//Avatar vars
	public List<Sprite> avatarMoveSprites;
	public List<Sprite> avatarStationarySprites;
	public int avatarSpriteIndex = 0;
	public bool avatarMirrored = false;

	//Monster vars
	public List<Sprite> monsterAnimSprites;
	private int monsterAnimIndex = 0;

	//Input
	//[HideInInspector]
	public direction avatarMoveDir;
	public int frameNumSinceMoved; //Not used

	public TileSprite[,] tileSpriteMap; //Our Current Map Data
	private GameObject controller;     
	private GameObject tileContainer;
	private List<GameObject> _tiles = new List<GameObject>(); //Our Tiles

	[HideInInspector]
	public Vector2i StartPos; //Set in <initMapTiles>
	public Vector2i ExitPos;  //Set in <initMapTiles>

	//Directions
	public enum direction
	{
		up,
		right,
		down,
		left,
		none
	}
	int[] dirx;
	int[] diry;

	//Game Vars
	private float time;
	public float FPS;
	private float timeStep;

	//Display
	float tileSizex;
	float tileSizey;
	Vector3 tileSizeVector;

	#endregion

	//

	void Start ()
	{
		//This class is initialized via our GameManager
		var emission = PS.emission;
		var rate = emission.rate;
		rate.constantMax = 0f;
		emission.rateOverTime = rate;

	}

	public void Initialize()
	{
		init();
		initMapTiles();
		initAvatar();
	}

	public void updateTime()
	{
		if ( time > timeStep )
		{
			time = 0;
			updateLogic();
			updateDisplay();
			Frame++;			//Update our Frame Count
		}
		else
		{
			time += Time.deltaTime;
		}
	}

	public void restartLevel()
	{
		Initialize();
	}
	//

	#region INITIALIZATION

	public void updateTimeStepValue ()
	{
		timeStep = 1.0f/GM.GameSpeed;
	}

	//

	private void init()
	{
		//Initialise our Map Variables and Managers / Loaders
		controller = GameObject.Find("Controller");
		GM = GetComponent<GameManager>();
		LL = GetComponent<LevelLoader>();
		IM = GetComponent<InputManager>();
		AM = GetComponent<AudioManager>();
		UI = UIContainer.GetComponent<UIManager>();

		//Init Time
		time = 0.0f;
		timeStep = 1.0f/GM.GameSpeed;

		//Index dir         ^   >   v    <   none
		dirx = new int[5] { 0,  1,  0,  -1,  0};
		diry = new int[5] {+1,  0, -1,   0,  0};

		//Set Diamonds Required
		GM.DiamondsNeeded = LL.DiamondsRequiredPerLevel[GM.levelNum];
		//Get + Set MapSize
		MapSize = LL.getMapSize();
		if ( DEBUG ) Debug.Log("MAPSIZE FROM LL ["+MapSize.x+","+MapSize.y+"].");
		tileSpriteMap = new TileSprite[MapSize.x, MapSize.y];

		//Sanity Check, Ensure viewport size is reasonable
		if ( ViewPortSize.x > 32 || ViewPortSize.y > 32 )
			ViewPortSize = new Vector2i(32, 32);
		
		//Set offset for visible tiles
		ViewOffset = new Vector2i( (int)(ViewPortSize.x*0.5f), (int)(ViewPortSize.y*0.5f) );

		//Display : Set Tile Sizes
		float screenHeight = (float)Camera.main.orthographicSize * 2.0f;
		float screenWidth = screenHeight / Screen.height * Screen.width;
		Vector3 xWidth = transform.localScale;
		Vector3 yHeight = transform.localScale;

		tileSizex = (screenWidth / ViewPortSize.x);
		tileSizey = (screenHeight / ViewPortSize.y);
		tileSizeVector = new Vector3(tileSizex, tileSizey);
	}

	//Set Player starting position and direction
	private void initAvatar()
	{
		//Just middle of map for now.. <THIS SHOULD BE DONE AFTER SETTING MAP TILES>
		if ( DEBUG ) Debug.Log("["+(StartPos.x)+","+(StartPos.y)+"] Avatar Set.");

		//Avatar starting position is set by initMapTiles

		//Set Avatar starting direction as none
		avatarMoveDir = direction.none;
	}

	//Build our level
	private void initMapTiles()
	{
		int[,] tempIndexMap = new int[MapSize.x, MapSize.y];
		tempIndexMap = (int[,])LL.parseLevel().Clone();

		int startExitExists = 0;
		for (int y=0; y < MapSize.y; y++)
		{	
			for (int x = 0; x < MapSize.x; x++)
			{
				//Load from csv
				tileSpriteMap[x,y] = setTile( tempIndexMap[x,y] );

				//Set Start Position
				if (tempIndexMap [x, y] == (int)Tiles.Avatar)
				{
					if (DEBUG) Debug.Log ("Start found at ["+x+", "+y+"]");
					StartPos = new Vector2i (x, y);
					curPos = StartPos;
					startExitExists++;
				}
				if (tempIndexMap [x, y] == (int)Tiles.ExitClosed)
				{
					if ( DEBUG ) Debug.Log ("exit found at ["+x+", "+y+"]");
					ExitPos = new Vector2i (x, y);
					startExitExists++;
				}
			}
		}
		if ( !(startExitExists == 2) )
			Debug.LogError("Error: Incorrect Start/Exit Tile Count.");

		if ( DEBUG ) Debug.Log("Spawned ["+(MapSize.y)+","+(MapSize.x)+"] Tiles.");
	}

	#endregion

	//

	#region QUERIES
	
	
	//Returns tile
	public TileSprite getTile(int x, int y) { return tileSpriteMap[x,y]; }


	//Return current frame count for object at [x,y]
	int getTileFrame(int x, int y) { return tileSpriteMap[x,y].frameNum; }


	//
	public bool isEnemy(int x, int y) 
	{ 
		Tiles t = tileSpriteMap[x,y].tileType;
		return ( t == Tiles.enemy01_UP || t == Tiles.enemy01_RIGHT || t == Tiles.enemy01_DOWN || t == Tiles.enemy01_LEFT) ? true : false;
	}

	public bool isAir(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Air ? true : false; }
	public bool isFlash(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Flash ? true : false; }
	public bool isAvatar(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Avatar ? true : false; }
	public bool isDirt(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Dirt ? true : false; }
	public bool isBoulder(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Boulder ? true : false; }
	public bool isBoulderFalling(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.BoulderFalling ? true : false; }
	public bool isDiamond(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.Diamond ? true : false; }
	public bool isDiamondFalling(int x, int y) { return tileSpriteMap[x,y].tileType == Tiles.DiamondFalling ? true : false; }
	public bool isExplosive(int x, int y) { return tileSpriteMap[x,y].explosive ? true : false; }
	public bool isRounded(int x, int y) { return tileSpriteMap[x,y].rounded ? true : false; }
	public bool isConsumable(int x, int y) { return tileSpriteMap[x,y].consumable ? true : false; }
	public bool isExitOpen(int x, int y) { return tileSpriteMap [x, y].tileType == Tiles.ExitOpen ? true : false; }

	public bool isObstructed(int x, int y) { return (isDirt(x,y) || isAir(x,y) || isDiamond(x,y) || isExitOpen(x,y)) ? false : true; }
	public bool isOutOfMapBounds(int x, int y) { return ( x<0 || y<0 || x>MapSize.x-1 || y>MapSize.y-1 ) ? true : false; }


	//
	public bool isWithinAudialRange(int x, int y)
	{
		//Checks whether a given tile should be heard
		Vector2 v1 = new Vector2(x,y);
		Vector2 v2 = new Vector2(curPos.x,curPos.y);
		if ( DEBUG ) Debug.Log("Testing distance between ["+v1.x+","+v1.y+"] and ["+v2.x+","+v2.y+"]");
		if ( DEBUG ) Debug.Log("Distance is "+Vector2.Distance(v1,v2));
		if ( Vector2.Distance(v1,v2) < 10 )
			return true;
		return false;
	}

	#endregion

	//

	#region TILE EVENTS

	private void finishLevel()
	{
		AM.play_SFX_WinLevel();
		//set GM.levelNum to current + 1
		GM.LoadLevel(GM.levelNum+1);

		GM.Score += GM.tempLevelScore;

		//restart level
		GM.currentState = GameManager.GameState.levelRestarting;
	}

	//

	//Moves a tile (replaces with air) Collision/Obstruction checks are done prior to move call
	private void move( int tileIndex, int x, int y, int d )
	{
		if( isDiamond( x + dirx[ d ], y + diry[ d ] ) ) 
		{
			collectDiamond();
			PS.Emit( 32 );
		}

		if ( isExitOpen( x+dirx[d], y+diry[d] ) )
			finishLevel ();
		tileSpriteMap[x,y] = setTile(0);
		tileSpriteMap[x+(dirx[d]),y+(diry[d])] = setTile(tileIndex);
	}

	private void showExit()
	{
		AM.play_SFX_Success();
		//Open the way to the next level.
		if ( DEBUG ) Debug.Log("Opening Exit...");
		tileSpriteMap[ExitPos.x, ExitPos.y] =  setTile( (int)Tiles.ExitOpen );
	}

	//

	//Clear map to Air tiles <( Vector, width, height )>
	public void clearTiles( int _x, int _y, int w, int h )
	{
		AM.play_SFX_Explosion();
		for (int y=_y; y < _y + h; y++)
		{
			for (int x=_x; x < _x + w; x++)
			{
				//Turn all Dirt tiles to Air tiles
				if ( isDirt(x,y) )
					tileSpriteMap[x,y] = setTile( (int)Tiles.Air );
			}
		}
	}

	//Give player Diamonds
	public void gimmeDiamonds( int _x, int _y )
	{
		AM.play_SFX_Explosion();
		for ( int y=-explodeSize; y<=explodeSize; y++ )
		{
			for ( int x=-explodeSize; x<=explodeSize; x++ )
			{
				if ( isDirt(_x+x,_y+y) || isAir(_x+x,_y+y) )
					tileSpriteMap[_x+x,_y+y] = setTile( 6 ); //Diamonds!
			}
		}
	}

	//Increment Diamond counter
	void collectDiamond ()
	{
		AM.play_SFX_PickUp ();

		if (GM.DiamondsNeeded > 0)
			GM.DiamondsNeeded -= 1; //Only decrement if less than required.

		GM.DiamondsCollected += 1;

		//Check if we have enough to open the exit
		if ( GM.DiamondsCollected == LL.DiamondsRequiredPerLevel[GM.levelNum] )
			showExit (); //Open exit to next level

		//Increment score
		GM.tempLevelScore += GM.diamondValue;
		GM.livesIncrementCounter += GM.diamondValue;
		//Finally update
		UI.updateScoreBoard();
	}

	//Create and return a new Sprite of index i
	public TileSprite setTile( int i )
	{
		TileSprite t = new TileSprite(TileSprites[i].Name,
			TileSprites[i].tileImage,
			TileSprites[i].tileType,
			TileSprites[i].rounded,
			TileSprites[i].consumable,
			TileSprites[i].explosive);
		t.frameNum = Frame;
		return t;
	}

	//

	//Set explodeSize * explodeSize to Tiles.index
	void explodeToTileIndex( int _x, int _y, int tileIndex)
	{
		AM.play_SFX_Explosion();

		//Set cause of explosion as non-explosive to stop infinite loops
		tileSpriteMap[_x,_y].explosive = false;

		for ( int y=-explodeSize; y<=explodeSize; y++ )
		{
			for ( int x=-explodeSize; x<=explodeSize; x++ )
			{
				if ( isAvatar(_x+x,_y+y) )
				{
					killAvatar(_x+x,_y+y);
					return;
				}
				else if ( isExplosive(x+_x, y+_y) )
					explodeToTileIndex( x+_x, y+_y, tileIndex );
				else if ( isConsumable(_x+x, _y+y) )
					tileSpriteMap[_x+x,_y+y] = setTile( tileIndex ); 
			}
		}
	}

	void killAvatar(int x, int y)
	{
		//Just return if cheating
		if ( GM.CHEATMODE ) return;

		AM.play_SFX_Failure(); //PLAY AUDIO
		if ( GM.lives > 0 )
		{
			GM.lives--;
			Debug.Log("TileManager.KillAvatar : Game Restarting");
			GM.currentState = GameManager.GameState.levelRestarting;
		}
		else
		{
			Debug.Log("TileManager.KillAvatar : Game Lost");
			GM.currentState = GameManager.GameState.gameLost;
		}
		UI.updateScoreBoard();
	}

	#endregion

	//

	#region UPDATE

	//Iterate over ALL TILES (one at a time) in map and process one at a time
	void updateLogic()
	{
		for ( int x = 0; x < MapSize.x; x++)
		{
			for ( int y = 0; y < MapSize.y; y++)
			{
				if ( isOutOfMapBounds(x,y) ) //EXIT IF OOB
				{
					Debug.LogError("TileManager.updateLogic : OOB");
					continue;
				}
			
				//Only update frames that have not been updated already this frame. [ FIXES DOUBLE UPDATE OF FALLING OBJECTS ]
				if ( getTileFrame(x,y) < Frame )
				{
					if ( isAvatar(x,y) )
						updateAvatar(x,y);	
					else if ( isBoulder(x,y) ) 
						updateBoulder(x,y);
					else if ( isBoulderFalling(x,y) ) 
						updateBoulderFalling(x,y);
					else if ( isDiamond(x,y) )
						updateDiamond(x,y);
					else if ( isDiamondFalling(x,y) )
						updateDiamondFalling(x,y);
					else if ( isEnemy(x,y) )
						updateEnemy01(x,y);
				}
			}
		}
	}

	void updateObjectFrameCount(int x, int y)
	{
		tileSpriteMap[x,y].frameNum = Frame;
	}
		
	void updateAvatar(int x, int y)
	{
		int d = (int)avatarMoveDir;

		//Sprite mirroring
		if ( avatarMoveDir == direction.left )
			avatarMirrored = true;
		if ( avatarMoveDir == direction.right )
			avatarMirrored = false;

		//Move Avatar
		if ( !isObstructed(x+dirx[d], y+diry[d]) )
		{
			//Play Dig / Walk sound <Using same audio>
			AM.play_SFX_Dig( isDirt(x+dirx[d], y+diry[d]) );

			//Move the sprite
			move( (int)Tiles.Avatar, x, y, d );

			//And update our global avatar position curPos
			curPos = new Vector2i(x+dirx[d],y+diry[d]);

			//Reset direction after each move [FIXES UP/LEFT TELEPORTATION]
			//avatarMoveDir = direction.none;
		}
		else 
		{
			if (DEBUG) Debug.Log("Avatar obstructed. ["+avatarMoveDir+"] type: ["+tileSpriteMap[x+dirx[d],y+diry[d]].tileType+"].");
		}
	}

	#region ENEMY

	void updateEnemy01(int x, int y)
	{
		int originalDir = (int)getEnemyDirection(x,y);				//Get original facing of enemy this frame
		int dirLeft = (int)rotateLeft( getEnemyDirection(x,y) );    //Get left direction of enemy
		//int dirRight = (int)rotateRight( getEnemyDirection(x,y) );  //Get right direction of enemy

		if ( isAvatar(x,y+1) || isAvatar(x,y-1) || isAvatar(x-1,y) || isAvatar(x+1,y) )
		{
			explodeToTileIndex( x,y, (int)Tiles.Air );
		}
		else if ( isAir(x+dirx[dirLeft], y+diry[dirLeft]) )
		{
			rotateEnemyLeft(x,y);
			move( (int)tileSpriteMap[x,y].tileType, x, y, (int)getEnemyDirection(x,y) ); //Move in new direction taken from getEnemyDir()..
		}
		else if ( isAir(x+dirx[originalDir], y+diry[originalDir]) )
		{
			move( (int)tileSpriteMap[x,y].tileType, x, y, originalDir );  //Move in original direction
		}
		else
		{
			rotateEnemyRight( x,y );
		}
	}

	direction rotateLeft( direction dir )
	{
		if ( dir == direction.up )
			return direction.left;
		else if ( dir == direction.right )
			return direction.up;
		else if ( dir == direction.down )
			return direction.right;
		else if ( dir == direction.left )
			return direction.down;
		else
			return direction.none;
	}

	direction rotateRight( direction dir )
	{
		if ( dir == direction.up )
			return direction.right;
		else if (dir == direction.right )
			return direction.down;
		else if (dir == direction.down )
			return direction.left;
		else if (dir == direction.left )
			return direction.up;
		else 
			return direction.none;
	}

	//Replaces enemy with left-turned variant at [x,y]
	void rotateEnemyLeft(int x, int y)
	{
		switch ( (int)tileSpriteMap[x,y].tileType )
		{
			case (int)Tiles.enemy01_UP:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_LEFT );
				break;
			}
			case (int)Tiles.enemy01_RIGHT:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_UP );
				break;
			}
			case (int)Tiles.enemy01_DOWN:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_RIGHT );
				break;
			}
			case (int)Tiles.enemy01_LEFT:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_DOWN );
				break;
			}
		}
	}

	//Replaces enemy with right-turned variant at [x,y]
	void rotateEnemyRight(int x, int y)
	{
		if ( !isEnemy(x,y) ) return; //Sanity check

		switch ( (int)tileSpriteMap[x,y].tileType )
		{
		case (int)Tiles.enemy01_UP:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_RIGHT );
				break;
			}
		case (int)Tiles.enemy01_RIGHT:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_DOWN );
				break;
			}
		case (int)Tiles.enemy01_DOWN:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_LEFT );
				break;
			}
		case (int)Tiles.enemy01_LEFT:
			{
				tileSpriteMap[x,y] = setTile( (int)Tiles.enemy01_UP );
				break;
			}
		}
	}

	//Returns direction of given enemy
	direction getEnemyDirection(int x, int y)
	{
		int index = (int)tileSpriteMap[x,y].tileType;
		if ( index == (int)Tiles.enemy01_UP )
			return direction.up;
		else if ( index == (int)Tiles.enemy01_RIGHT )
			return direction.right;
		else if ( index == (int)Tiles.enemy01_DOWN )
			return direction.down;
		else if ( index == (int)Tiles.enemy01_LEFT )
			return direction.left;
		else
		{
			Debug.LogError( "TileManager.getEnemyDirection" );
			return direction.none;
		}
	}

	#endregion

	void updateBoulder(int x, int y)
	{
		if ( isAir(x,y-1) )
			tileSpriteMap[x,y] = setTile( (int)Tiles.BoulderFalling ); //Create BoulderFalling
		else if ( isRounded(x,y-1) && isAir(x-1,y) && isAir(x-1,y-1) ) //below=rounded,left and belowLeft = Air
			move( (int)Tiles.BoulderFalling, x, y, (int)direction.left ); //ROLL LEFT
		else if ( isRounded(x,y-1) && isAir(x+1,y) && isAir(x+1,y-1) ) //below=rounded,right and belowRight = Air
			move( (int)Tiles.BoulderFalling, x, y, (int)direction.right ); //ROLL RIGHT
	}

	void updateBoulderFalling (int x, int y)
	{
		if (isAir (x, y - 1)) //Below is Air:
			move ((int)Tiles.BoulderFalling, x, y, (int)direction.down);
		else if (isAvatar (x, y - 1))
			{
				if (!GM.CHEATMODE)
					explodeToTileIndex (x, y - 1, (int)Tiles.Air); //Kill player
			} else if (isExplosive (x, y - 1) && isEnemy (x, y - 1))
				{
					explodeToTileIndex (x, y - 1, (int)Tiles.DiamondFalling);
					//Increment Score!
					GM.tempLevelScore += GM.monsterValue;
					GM.livesIncrementCounter += GM.monsterValue;
					UI.updateScoreBoard();
				}
		else if ( isExplosive(x,y-1) )
			explodeToTileIndex( x,y-1, 0 );
		else if ( isRounded(x,y-1) && isAir(x-1,y) && isAir(x-1,y-1) ) //BelowLeft is empty:
			move( (int)Tiles.BoulderFalling, x, y, (int)direction.left ); //ROLL LEFT
		else if ( isRounded(x,y-1) && isAir(x+1,y) && isAir(x+1,y-1) ) //BelowRight is empty:
			move( (int)Tiles.BoulderFalling, x, y, (int)direction.right ); //ROLL RIGHT
		else
		{
			if (isWithinAudialRange(x,y)) AM.play_SFX_RockHit(); //PLAY AUDIO
			tileSpriteMap[x,y] = setTile( (int)Tiles.Boulder ); //Turn to stationary Boulder
		}
	}

	void updateDiamond( int x, int y )
	{
		if ( isAir(x,y-1) )
			tileSpriteMap[x,y] = setTile( (int)Tiles.DiamondFalling ); //Create DiamondFalling
		else if ( isRounded(x,y-1) && isAir(x-1,y) && isAir(x-1,y-1) ) //below=rounded,left and belowLeft = Air
			move( (int)Tiles.DiamondFalling, x, y, (int)direction.left ); //ROLL LEFT
		else if ( isRounded(x,y-1) && isAir(x+1,y) && isAir(x+1,y-1) ) //below=rounded,right and belowRight = Air
			move( (int)Tiles.DiamondFalling, x, y, (int)direction.right ); //ROLL RIGHT
	}

	void updateDiamondFalling( int x, int y) 
	{
		if ( isAir(x, y-1) ) //Below is Air:
			move( (int)Tiles.DiamondFalling, x, y, (int)direction.down );
		else if ( isAvatar(x,y-1) )
		{
			if ( !GM.CHEATMODE )
				explodeToTileIndex( x, y-1, (int)Tiles.Air ); //Kill player
		}
		else if ( isExplosive(x,y-1) && isEnemy(x,y-1) )
			explodeToTileIndex( x,y-1, (int)Tiles.DiamondFalling );
		else if ( isExplosive( x, y-1 ) )
			explodeToTileIndex( x,y-1, (int)Tiles.Air );
		else if ( isRounded(x,y-1) && isAir(x-1,y) && isAir(x-1,y-1) ) //BelowLeft is empty:
			move( (int)Tiles.DiamondFalling, x, y, (int)direction.left ); //ROLL LEFT
		else if ( isRounded(x,y-1) && isAir(x+1,y) && isAir(x+1,y-1) ) //BelowRight is empty:
			move( (int)Tiles.DiamondFalling, x, y, (int)direction.right ); //ROLL RIGHT
		else
		{
			if (isWithinAudialRange(x,y)) AM.play_SFX_DiamondHit(); //PLAY AUDIO
			tileSpriteMap[x,y] = setTile( (int)Tiles.Diamond ); //Turn to stationary Diamond
		}
	}

	#endregion

	//
	
	//Update our display area
	public void updateDisplay ()
	{

		foreach (GameObject go in _tiles)
		{
			LeanPool.Despawn (go);
		}
		_tiles.Clear ();

		LeanPool.Despawn (tileContainer);
		tileContainer = LeanPool.Spawn (TileContainer);

		//Iterate over our viewport tiles
		for (int y = -ViewOffset.y; y < ViewOffset.y; y++)
		{
			for (int x = -ViewOffset.x; x < ViewOffset.x; x++)
			{
				//If out of map bounds:
				if (!isOutOfMapBounds (x + curPos.x, y + curPos.y))
				{
					GameObject tile = LeanPool.Spawn (TilePrefab);

					//Set tile position
					tile.transform.position = new Vector3 (x * tileSizex, y * tileSizey, 0);

					//Set tile size
					tile.transform.localScale = tileSizeVector;

					tile.transform.SetParent (tileContainer.transform);
					SpriteRenderer renderer = tile.GetComponent<SpriteRenderer> ();

					//ANIMATE AVATAR SPRITE /////////////////////////////////////////////////////////
					if ( isAvatar( x+curPos.x, y+curPos.y ) )
					{
						//Handle sprite mirroring
						renderer.flipX = avatarMirrored ? true : false;

						if (avatarSpriteIndex > 3 || avatarSpriteIndex < 0)
						{
							Debug.Log ("Sprite Index Out Of Bounds.");
							avatarSpriteIndex = 0;
							continue;
						}
						//Loop index of first half of frames
						if (avatarSpriteIndex >= 3)
							avatarSpriteIndex = 0;
						else //Increment index
							avatarSpriteIndex++;

						//Finally set sprite[index]
						if (avatarMoveDir == direction.none)
							renderer.sprite = avatarStationarySprites [avatarSpriteIndex];
						else
							renderer.sprite = avatarMoveSprites[avatarSpriteIndex];
					} //ENEMY ANIMATION
					else if ( isEnemy( x+curPos.x, y+curPos.y ) )
					{ //Directionless - no need for flipping
						monsterAnimIndex++;
						//If cur frame index < numFrames
						if ( monsterAnimIndex < monsterAnimSprites.Count )
							renderer.sprite = monsterAnimSprites[monsterAnimIndex];
						else 
						{
							renderer.sprite = monsterAnimSprites[0];
							monsterAnimIndex = 0;
						}
					}
					else //All other non-animating sprites
					{
						//Set all other sprites flipx to false;
						renderer.flipX = false;
						renderer.sprite = tileSpriteMap [x + curPos.x, y + curPos.y].tileImage;
					}
					tile.name = "["+(x+curPos.x)+","+(y+curPos.y)+"]";
					_tiles.Add(tile);
				}
			}
		}
	}
		
}

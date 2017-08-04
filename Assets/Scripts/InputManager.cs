using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.Audio;

public class InputManager : MonoBehaviour {

	GameManager GM;
	TileManager TM;

	//ANDROID SPECIFIC
	TouchInput TI;

	public KeyCode UP;
	public KeyCode DOWN;
	public KeyCode LEFT;
	public KeyCode RIGHT;

	public KeyCode CHEAT_GIVEDIAMONDS;
	public KeyCode CHEAT_REMOVEALL;
	public KeyCode PAUSEUNPAUSE;

//	public enum MovingDirection
//	{
//		RIGHT,
//		LEFT,
//		NONE
//	}
//	public MovingDirection CurrentMovingDirection;

	void Awake()
	{
		GM = GetComponent<GameManager>();
		TM = GetComponent<TileManager>();
		TI = GetComponent<TouchInput>();
	}

	//

	public void processInput()
	{
		checkPauseKey();

		TM.avatarMoveDir = getInputDirection();

		cheat();
	}

	//

	public void cheat()
	{
		if ( GM.CHEATMODE )
		{
			if ( Input.GetKey( CHEAT_REMOVEALL ) )
				TM.clearTiles( 0, 0, TM.MapSize.y-1, TM.MapSize.y-1 ); //Entiremap

			//CHEAT: Surround the player with diamons of explodeSize*explodeSize
			if ( Input.GetKey( CHEAT_GIVEDIAMONDS ) )
				TM.gimmeDiamonds( TM.curPos.x, TM.curPos.y );
		}
	}

	//

	public TileManager.direction getInputDirection()
	{
		#if UNITY_ANDROID

		return TI.getTouchDirection();

		#elif UNITY_5_3_OR_NEWER || UNITY_EDITOR_WIN

		if ( Input.GetKey( UP ) )
			return TileManager.direction.up;
		else if ( Input.GetKey( DOWN ) )
			return TileManager.direction.down;
		else if ( Input.GetKey( LEFT ) )
			return TileManager.direction.left;
		else if ( Input.GetKey( RIGHT ) )
			return TileManager.direction.right;
		else
			return TileManager.direction.none;

		#endif
	}

	//

	public void checkPauseKey()
	{
		if (Input.GetKeyDown (PAUSEUNPAUSE)) 
		{
			GM.PauseToggle ();
		}
	}
}

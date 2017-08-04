using UnityEngine;
using System.Collections;

public class TouchInput : MonoBehaviour 
{
	private bool DEBUG = false;

	public TileManager.direction DIR;

	private bool isTouching = false;

	void Start()
	{
		DIR = TileManager.direction.none;
		isTouching = false;
	}

	void Update () 
	{
		for ( int i=0; i < Input.touchCount; i++ )
		{
			TouchPhase tp = Input.GetTouch(i).phase;
			if ( tp == TouchPhase.Began || tp == TouchPhase.Stationary || tp == TouchPhase.Moved  )
			{
				isTouching = true;
				if ( DEBUG ) Debug.Log("touchbegan");
			}
			else if ( tp == TouchPhase.Ended )
			{
				isTouching = false;
				if ( DEBUG ) Debug.Log("touchended");
				DIR = TileManager.direction.none;
			}
		}
	}

	#region SET DIRECTIONS

	public void setNone()
	{
		DIR = TileManager.direction.none;
	}

	public void setLeft ()
	{
		DIR = isTouching ? TileManager.direction.left : TileManager.direction.none;
//		if (isTouching)
//		{
//			DIR = TileManager.direction.left;
//		}
//		else { DIR = TileManager.direction.none; }
	}

	public void setRight ()
	{
		DIR = isTouching ? TileManager.direction.right : TileManager.direction.none;
//		if (isTouching)
//		{
//			DIR = TileManager.direction.right;
//		}
//		else { DIR = TileManager.direction.none; }
	}

	public void setUp ()
	{
		DIR = isTouching ? TileManager.direction.up : TileManager.direction.none;
//		if (isTouching)
//		{
//			DIR = TileManager.direction.up;
//		}
//		else { DIR = TileManager.direction.none; }
	}

	public void setDown ()
	{
		DIR = isTouching ? TileManager.direction.down : TileManager.direction.none;
//		if (isTouching)
//		{
//			DIR = TileManager.direction.down;
//		}
//		else { DIR = TileManager.direction.none; }
	}

	#endregion

	public TileManager.direction getTouchDirection()
	{
		return DIR;
	}

}

using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;

public class LevelLoader : MonoBehaviour 
{
	//public int[,] tileIndexMap;
	public Vector2i mapSize;
	private string dirPath;
	private string filePath;
	
	public bool DEBUG;
	public bool DEBUGVERBOSE;

	public List<TextAsset> LevelsList;

	public List<int> DiamondsRequiredPerLevel;

	private string elementSeparator = ",";
	private string lineSeparator = "\n";

	public GameManager GM;


	void Awake()
	{
		GM = GetComponent<GameManager>();

		//OLD STREAMREADER CODE:
			//filePath = "/Levels/" + LevelList[levelIndex] + ".txt";
			//dirPath = getDirPath();
	}

	//

	//Retrieve the first 2 elements <mapsize.x, mapsize.y> of the entire map file.
	public Vector2i getMapSize ()
	{
		if (DEBUGVERBOSE) Debug.Log ("< getMapSize Entered >");

		if (GM.levelNum > LevelsList.Count-1)
		{
		Debug.Log( "Level Index Out Of Bounds. Returning To Level One.");
			GM.levelNum = 0;
		}

		//Obtain all lines from level text
		string ta = LevelsList[GM.levelNum].text;

		//Attempt to split first xy pair in line 0
		string x = Regex.Split( ta, elementSeparator ).ElementAt(0);
		string y = Regex.Split( ta, elementSeparator ).ElementAt(1);

		try
		{
			mapSize.x = int.Parse(x);
			mapSize.y = int.Parse(y);
		}
		catch (System.Exception ex) 
		{
			Debug.LogError( "[ <LL : getMapSize> ERROR - Unable to parse int("+x+") -> string ]  ["+ex.Message+"]");
		}

		if ( DEBUG ) Debug.Log("<LL> Calculated MapSize as: [ X:"+mapSize.x + "  ,   Y:" + mapSize.y + " ]");
		return mapSize;
	}

	//

	#region OLD STREAMREADER CODE

	//public void getMapSize()
//	{
//		if (DEBUG) Debug.Log("LL.getMapSize : dirPath= "+dirPath+", filePath= "+filePath);
//
//		using ( StreamReader sr = new StreamReader(dirPath + filePath) )
//		{
//			string line = sr.ReadLine();
//			string[] xyPair = line.Split(separator);
//			int.TryParse(xyPair[0], out mapSize.x);
//			int.TryParse(xyPair[1], out mapSize.y);
//		}
//		if (DEBUG) Debug.Log("MAPSIZE DETECTED: ["+mapSize.x+","+mapSize.y+"]");
//	}

	//	//Read Map into our tileIndexMap and return to then be read by the TileManager
	//	public int[,] parseLevel()
	//	{
	//		if (DEBUG) Debug.Log("parseLevel("+levelIndex+") Entered.");
	//
	//		//Create IndexMap
	//		int[,] indexMap = new int[mapSize.x,mapSize.y];
	//
	//		//Get lines from file
	//		List<string> lines = File.ReadAllLines( dirPath + filePath ).ToList();
	//
	//		//Remove first mapSize line ( 0 )
	//		lines.RemoveAt(0);
	//
	//		int x = 0;			 //Increase to mapSize.x-1
	//		int y = mapSize.y-1; //Decrease to 0
	//
	//		foreach (string line in lines)
	//		{
	//			x=0;
	//			if (DEBUGVERBOSE) Debug.Log(line);
	//			string[] vals = line.Split(elementSeparator);
	//
	//			foreach (string sVal in vals )
	//			{
	//				int val;
	//				int.TryParse(sVal, out val);
	//				indexMap[x,y] = val;
	//				if (x<mapSize.y-1)
	//					x++;
	//			}
	//			y--;
	//		}
	//		return indexMap;
	//	}

	//	private string getDirPath()
	//	{
	//		#if UNITY_ANDROID
	//
	//			Debug.Log("UNITY_ANDROID");
	//			WWW www = new WWW(Application.streamingAssetsPath);
	//			return www.text;
	//
	//		#endif
	//
	//		#if UNITY_5_3_OR_NEWER || UNITY_EDITOR
	//			
	//			Debug.Log("UNITY_EDITOR");
	//			return Application.dataPath; //gets C:\Levels\02.txt
	//
	//		#endif
	//	}

	#endregion

	//

	public int[,] parseLevel()
	{
		//Create our indexmap
		int[,] indexMap = new int[mapSize.x, mapSize.y];

		//read whole textAsset
		string ta = LevelsList[GM.levelNum].text;

		//Obtain collection of lines
		List<string> lines = Regex.Split( ta, lineSeparator ).ToList();
		//And remove first line
		lines.RemoveAt(0);

		int x = 0;				//Increase to mapSize.x-1
		int y = mapSize.y-1;	//Decrease to 0

		foreach (string line in lines)
		{
			x=0;
			if ( DEBUGVERBOSE ) Debug.Log(line);
			string[] vals = line.Split(',');
			foreach (string sVal in vals)
			{
				int val;
				int.TryParse(sVal, out val);
				if ( DEBUG ) Debug.Log("X:"+x+", Y:"+y);
				indexMap[x,y] = val;
				if ( x < mapSize.x-1 ) 
					x++;
			}
			y--;
		}
		return indexMap;
	}


}

using UnityEngine;
using System.Collections;
//using UnityEditor.Animations;
using UnityEngine.UI;

public class UIManager : MonoBehaviour 
{
	public bool DEBUG = false;

	public GameObject[] pauseObjects;
	public GameObject[] optionObjects;
	public GameObject[] mainMenuObjects;
	public GameObject[] gameRunningObjects;

	public GameObject btn_mainMenu;
	public GameObject btn_pause;
	public GameObject btn_options;

	public GameObject scoreDisplay;
	public GameObject diamondsNeededDisplay;
	public GameObject livesDisplay;

	//Sliders
	public GameObject VolumeSlider;
	public GameObject GameSpeedSlider;
	private int gameSpeedMultiplier = 20;

	public GameObject Controller;
	GameManager GM;
	LevelLoader LL;
	AudioManager AM;

	// Use this for initialization
	void Start () 
	{
		//Start with all inactive
		SetPauseVisible(false);
		SetOptionsVisible(false);

		GM = Controller.GetComponent<GameManager>();
		LL = Controller.GetComponent<LevelLoader>();
		AM = Controller.GetComponent<AudioManager>();

		updateScoreBoard();
		updateVolume();
	}

	//

	public void updateVolume()
	{
		AM.Volume = VolumeSlider.GetComponent<Scrollbar>().value;
	}

	//

	public void updateGameSpeed ()
	{
		int fps = (int)(gameSpeedMultiplier * GameSpeedSlider.GetComponent<Scrollbar>().value);
		GM.setGameSpeed(fps+1); //+1 as we cannot have a val of 0
	}

	//

	public void updateScoreBoard()
	{
		int tempscore = GM.Score + GM.tempLevelScore;

		scoreDisplay.GetComponent<Text>().text = "Score " + tempscore.ToString();

		diamondsNeededDisplay.GetComponent<Text>().text = GM.DiamondsNeeded.ToString();
		livesDisplay.GetComponent<Text>().text = GM.lives.ToString();
	}

	#region SHOW/HIDE BUTTONS

	public void SetBtn_Pause(bool b)
	{
		if ( DEBUG ) Debug.Log ("Setting Pause Button to " + b + ".");
		btn_pause.SetActive (b);
	}

	//
	public void SetBtn_MainMenu(bool b)
	{
		if ( DEBUG ) Debug.Log ("Setting MainMenu Button to " + b + ".");
		btn_mainMenu.SetActive (b);
	}

	public void SetBtn_Options (bool b)
	{
		if ( DEBUG ) Debug.Log ("Setting Options Button to " + b + ".");
		btn_options.SetActive (b);
	}

	#endregion

	//

	public void SetGameRunningObjectsVisible(bool b)
	{
		foreach (GameObject go in gameRunningObjects)
		{
			if (DEBUG) Debug.Log ("Setting " + go.name + " to " + b + ".");
			go.SetActive(b);
		}
	}

	//

	public void SetOptionsVisible(bool b)
	{
		foreach (GameObject go in optionObjects)
		{
			if (DEBUG) Debug.Log ("Setting " + go.name + " to " + b + ".");
			go.SetActive(b);
		}
	}

	//

	public void SetMainMenuVisible(bool b)
	{
		foreach (GameObject go in mainMenuObjects)
		{
			if (DEBUG) Debug.Log ("Setting " + go.name + " to " + b + ".");
			go.SetActive (b);
		}
	}

	public void SetPauseVisible(bool b)
	{
		foreach(GameObject go in pauseObjects)
		{
			if ( go.name == "Btn_Pause" ) continue; //Skip pause button
			if ( DEBUG ) Debug.Log("Setting "+go.name+" "+b+".");
			go.SetActive(b);
		}

	}

}

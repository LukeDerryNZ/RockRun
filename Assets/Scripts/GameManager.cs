using UnityEngine;
using System.Collections;

//Manages Input and Updates : Controls TileManager
//using System.Diagnostics;
using UnityEngine.UI;


public class GameManager : MonoBehaviour 
{
	private TileManager TM;
	private UIManager UI;
	public GameObject uiContainer;
	private InputManager IM;
	private AudioManager AM;
	private LevelLoader LL;

	//Game Stats
	public int DiamondsNeeded = 10;
	public int DiamondsCollected = 0;
	public int TimeLeft = 500;
	public int Score = 0;
	public int tempLevelScore = 0;

	public int diamondValue = 5;
	public int monsterValue = 15;
	public int lives = 0;
	public int StartingLives = 5;
	public int livesIncrementCounter = 0;
	private int IncreaseLivesEveryXPoints = 200;

	public int GameSpeed = 10;

	public bool CHEATMODE;

	public bool DEBUG;

	public enum GameState
	{
		mainMenu,
		options,
		gameRunning,
		gamePaused,
		levelRestarting,
		gameRestarting,
		gameLost
	}

	//Current GameState
	public GameState currentState;

	//Current Level
	public int levelNum;

	void Awake()
	{
		TM = GetComponent<TileManager>();
		UI = uiContainer.GetComponent<UIManager>();
		IM = GetComponent<InputManager>();
		AM = GetComponent<AudioManager>();
		LL = GetComponent<LevelLoader>();
	}

	// Use this for initialization
	void Start () 
	{
		TM.Initialize();

	}

	// Update is called once per frame
	void Update () 
	{
		switch (currentState)
		{
			case GameState.mainMenu:
			{
				UI.SetMainMenuVisible (true);
				//Run main menu loop

					//PLAY AUDIO
					//BACKGROUND ANIMATION?

				break;
			}
			case GameState.options:
			{
				IM.processInput();
				break;
			}
			case GameState.gameRunning:
			{
				increaseLives();
				TM.updateTime();     //TimeStep
				IM.processInput();   //Every Frame
				break;
			}
			case GameState.gamePaused:
			{
				//Debug.Log("GAME PAUSED");
				TM.updateDisplay();  //Update Display
				IM.processInput();   //Allows unpause if pauseKey pressed again
				break;
			}
			case GameState.levelRestarting:
			{
				//Debug.Log("YOU HAVE DIED. LEVEL RESTARTING.");
				resetScores();
				TM.restartLevel();
				UI.updateScoreBoard();

				currentState = GameState.gameRunning;
				break;
			}
			case GameState.gameRestarting:
			{
				//SET LEVEL TO 0
				//levelNum = 0;
				
				currentState = GameState.levelRestarting;
				break;
			}
			case GameState.gameLost:
			{
				//Lives should == 0 here
				//Debug.Log("YOU HAVE LOST THE GAME.");
				currentState = GameState.mainMenu;
				break;
			}
			default: //On error/default, return to mainmenu
			{
				//On State Error : Return to main Menu
				if ( DEBUG ) Debug.Log("State Error.. Returning to Main Menu.");
				currentState = GameState.mainMenu;
				break;
			}
		}
	}

	public void setGameSpeed (int s)
	{
		GameSpeed = s;
		TM.FPS = GameSpeed;
		TM.updateTimeStepValue();
	}

	//

	private void resetScores ()
	{
		if ( levelNum > LL.LevelsList.Count-1 )
			levelNum = 0;
		DiamondsNeeded = LL.DiamondsRequiredPerLevel[levelNum];
		DiamondsCollected = 0;
		tempLevelScore = 0;
	}

	//

	public void LoadLevel (int l)
	{
		levelNum = l;
		if ( levelNum > LL.LevelsList.Count-1 )
			levelNum = 0;
		currentState = GameState.levelRestarting;
		UI.updateScoreBoard();
	}

	//

	public void CheatToggle ()
	{
		CHEATMODE = !CHEATMODE;
	}

	//

	public void increaseLives ()
	{
		if (livesIncrementCounter >= IncreaseLivesEveryXPoints)
		{
			livesIncrementCounter = 0;
			lives++;
		}
	}

	public void OptionsToggle ()
	{
		AM.play_UI_ConfirmAudio ();

		if (currentState == GameState.gamePaused)
		{	//Enter Options
			UI.SetBtn_Pause (false);		//Hide pause btn
			UI.SetOptionsVisible (true);	//Show options screen
			UI.SetBtn_MainMenu (true);		//Show menu btn
			currentState = GameState.options;
		}
		else if (currentState == GameState.mainMenu)
		{	//Enter Options From Main Menu
			UI.SetBtn_Pause(false);
			UI.SetPauseVisible(false);
			UI.SetBtn_MainMenu (true);		//Show menu btn
			UI.SetOptionsVisible(true);		//Show options screen
			currentState = GameState.options;
		}
		else if (currentState == GameState.options)
		{	//Hide Options
			UI.SetMainMenuVisible(false);
			UI.SetOptionsVisible(false);	//Hide options screen
			UI.SetBtn_Pause(true);			//Show pause btn
			currentState = GameState.gamePaused;
		}
	}

	//
	public void MainMenuToggle()
	{
		AM.play_UI_ConfirmAudio();
		if (currentState != GameState.mainMenu)
		{
			//SHOW
			UI.SetMainMenuVisible(true);

			//HIDE
			UI.SetOptionsVisible(false);
			UI.SetBtn_MainMenu(false);
			UI.SetBtn_Pause(false);
			UI.SetBtn_Options(false);
			UI.SetPauseVisible(false);
			UI.SetGameRunningObjectsVisible(false);

			//Set State
			currentState = GameState.mainMenu;
		}
		else //Is exiting mainmenu
		{
			UI.SetMainMenuVisible(false);
			UI.SetGameRunningObjectsVisible(true);
			currentState = GameState.gameRunning; //Should this not hold the last gamestate?
		}
	}

	//

	public void PauseToggle()
	{
		AM.play_UI_ConfirmAudio();
		if ( currentState == GameState.gameRunning )
		{
			UI.btn_pause.GetComponent<RectTransform>().anchoredPosition = new Vector3(167,-32,0);
			UI.SetPauseVisible(true);
			UI.SetBtn_MainMenu(true);
			UI.SetBtn_Options(true);
			//Set State
			currentState = GameState.gamePaused;
		}
		else if ( currentState == GameState.gamePaused )
		{
			UI.btn_pause.GetComponent<RectTransform>().anchoredPosition = new Vector3(36,-32,0);
			UI.SetPauseVisible(false);
			UI.SetBtn_Options(false);
			UI.SetBtn_MainMenu(false);
			currentState = GameState.gameRunning;
		}
	}

	//

	public void StartGame()
	{
		lives = StartingLives; //Reset lives
		UI.SetMainMenuVisible(false);
		UI.SetPauseVisible(false);
		UI.SetOptionsVisible(false);
		UI.SetGameRunningObjectsVisible(true); //Set DPAD visible
		currentState = GameState.gameRestarting;
	}
}

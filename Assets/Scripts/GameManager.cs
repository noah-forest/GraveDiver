using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	#region singleton

	public static GameManager singleton;

	private void Awake()
	{
		if (singleton)
		{
			Destroy(this.gameObject);
			return;
		}

		singleton = this;
		DontDestroyOnLoad(this.gameObject);
	}
	#endregion

	#region Changed Events
	[HideInInspector]
	public UnityEvent goldChangedEvent;
	[HideInInspector]
	public UnityEvent livesChangedEvent;
	[HideInInspector]
	public UnityEvent battleScoreEvent;
	#endregion

	#region Battle Events
	[HideInInspector]
	public UnityEvent battleWonEvent;
	[HideInInspector]
	public UnityEvent battleLostEvent;
	[HideInInspector]
	public UnityEvent gameOverEvent;
	[HideInInspector]
	public UnityEvent battleStartedEvent;
	[HideInInspector]
	public UnityEvent combatBeganEvent;
	[HideInInspector]
	public UnityEvent battleEndedEvent;
	[HideInInspector]
	public UnityEvent loadShopEvent;

	[HideInInspector] public UnityEvent playerActorHit;
	#endregion

	#region Transition Events
	[HideInInspector]
	public UnityEvent startGame;
	[HideInInspector]
	public UnityEvent startBattle;
	[HideInInspector]
	public UnityEvent startBattleTransition;
	[HideInInspector]
	public UnityEvent startShopTransition;
    [HideInInspector]
    public UnityEvent startBoatTransition;

    [HideInInspector]
	public UnityEvent<bool> transitionPlaying;

	[HideInInspector]
	public UnityEvent loadUI;

	#endregion

	#region Shop Events
	[HideInInspector]
	public UnityEvent shopRefreshed = new();
	[HideInInspector]
	public UnityEvent unitPurchased = new();
	[HideInInspector]
	public UnityEvent<Slot> unitSold = new();
	[HideInInspector]
	public UnityEvent enemyUnitsCreated = new();
	[HideInInspector]
	public UnityEvent previewRolled = new();
	[HideInInspector]
	public UnityEvent unitRevealed = new();
	[HideInInspector]
	public UnityEvent shopLocked = new();
	[HideInInspector]
	public UnityEvent shopUnlocked = new();
	[HideInInspector]
	public UnityEvent<Slot> unitAddedToSlot = new();
	#endregion

	#region Unit Events
	[HideInInspector]
	public UnityEvent unitExpAdded;
	[HideInInspector]
	public UnityEvent unitLevelUp;
	[HideInInspector]
	public UnityEvent hitParentActor;
	#endregion

	#region Getters and Setters
	/// <summary>
	///dont use this variable, use Cash
	/// </summary>
	private int _internalCash;
	public int Cash
	{
		get => _internalCash;
		set
		{
			_internalCash = value;
			goldChangedEvent.Invoke();
		}
	}

	/// <summary>
	///dont use this variable, use Lives
	/// </summary>
	private int _internalLives;
	public int Lives
	{
		get => _internalLives;
		set
		{
			_internalLives = value;
			livesChangedEvent.Invoke();
		}
	}

	/// <summary>
	///dont use this variable, use BattlesWon
	/// </summary>
	private int _internalBattlesWon;
	public int BattlesWon
	{
		get => _internalBattlesWon;
		set
		{
			_internalBattlesWon = value;
			battleScoreEvent.Invoke();
		}
	}
	#endregion

	[Space(10)]

	public DebugMenu debugMenu;
	public Settings settings;
	public GameObject settingsMenu;

	[Space(10)]

	public BattleManager battleManager;
	public UIManager uiManager;

	public Actor player;
	
	[Header("everything else")]

	public GameObject HUD;
	public GameObject wonParticles;

	public AudioSource MusicPlayer;
	public AudioMixer audioMixer;

	private MouseUtils mouseUtils;

	public bool gameIsPaused { private set; get; }
	public bool openPauseMenu;
	public bool controlDown;

	[HideInInspector]
	public UnityEvent pauseGame;
	[HideInInspector]
	public UnityEvent resumeGame;

	public int revealCost = 1;

	public SaveData saveData;
	
	private void Start()
	{
		mouseUtils = MouseUtils.singleton;
		
		// check if there is save data before loading it
		if (saveData.CheckForSaveData())
		{ 
			saveData.unlockMatrix = saveData.LoadFromJson();
		}
		
		pauseGame.AddListener(PauseGame);
		resumeGame.AddListener(UnPauseGame);
	}

	private void Update()
	{
		if (SceneManager.GetActiveScene().name == "MainMenu") return;
		
		//if they holding down control
		controlDown = Input.GetKey(KeyCode.LeftControl);
		
		//open pause menu
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePauseMenu();
		}
	}

	public void BackToMainMenu()
	{
		resumeGame.Invoke();
		startGame.Invoke();
	}

	public void TogglePauseMenu()
	{
		if (gameIsPaused)
		{
			openPauseMenu = false;
			mouseUtils.SetToDefaultCursor();
			resumeGame.Invoke();
		}
		else
		{
			openPauseMenu = true;
			mouseUtils.SetToDefaultCursor();
			pauseGame.Invoke();
		}
	}

	public void PauseGame()
	{
		gameIsPaused = true;
		Time.timeScale = 0;
	}

	public void UnPauseGame()
	{
		gameIsPaused = false;
		Time.timeScale = 1;
	}

	public void StartGame()
	{
		battleManager.StartGame();
	}

	public void LoadShop()
	{
		battleManager.LoadShop();
	}

	public void NextBattleButton()
	{
		battleManager.NextBattleButton();
	}
}
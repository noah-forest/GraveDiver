using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
	public Animator transition;
	public float transitionTime = 1.0f;

	public bool transitionIsPlaying;

	private GameManager gameManager;
	private UIManager uiManager;

	private void Start()
	{
		transitionIsPlaying = false;

		gameManager = GameManager.singleton;
		if(gameManager)
		{
            uiManager = gameManager.uiManager;

            gameManager.startGame.AddListener(StartGame);
            gameManager.startBattle.AddListener(StartBattle);
            gameManager.loadUI.AddListener(LoadUI);

            gameManager.startShopTransition.AddListener(LoadShopTrans);
            gameManager.startBattleTransition.AddListener(LoadBattleTrans);

            gameManager.startBoatTransition.AddListener(LoadBoatTrans);
        }
		else
		{
			Debug.Log("[LEVEL LOADER] no game manager in scene. Unable to load levels.");
		}
	}

	private void StartGame()
	{
		StartCoroutine(LoadLevel("MainMenu"));
		StartCoroutine(WaitToHide());
	}

	private void StartBattle()
	{
		StartCoroutine(LoadLevel("battle"));
		StartCoroutine(WaitToLoad());
	}

	private void LoadUI()
	{
		gameManager.LoadShop();
		gameManager.HUD.SetActive(true);
	}

	private void LoadBoatTrans()
	{
		Debug.Log("starting boat transition");
		StartCoroutine(LoadLevel("transition"));
		StartCoroutine(WaitToLoadLevel());
		StartCoroutine(LoadLevel("boat"));
	}

	private void LoadShopTrans()
	{
		StartCoroutine(LoadLevel("transition"));
		StartCoroutine(WaitToTransShop());
		gameManager.wonParticles.SetActive(false);
		StartCoroutine(LoadLevel("battle"));
	}

	private void LoadBattleTrans()
	{
		StartCoroutine(LoadLevel("transition"));
		StartCoroutine(WaitToTransBattle());
		StartCoroutine(LoadLevel("battle"));
	}

	IEnumerator LoadLevel(string levelName)
	{
		transition.SetTrigger("start");

		transitionIsPlaying = true;
		gameManager.transitionPlaying.Invoke(transitionIsPlaying);

		yield return new WaitForSeconds(transitionTime);

		SceneManager.LoadScene(levelName);

		transitionIsPlaying = false;
		gameManager.transitionPlaying.Invoke(transitionIsPlaying);
	}

	IEnumerator WaitToTransShop()
	{
		if (Time.timeScale != 1)
		{
			Time.timeScale = 1;
		}

		yield return new WaitForSeconds(transitionTime);

		gameManager.LoadShop();
	}

	IEnumerator WaitToTransBattle()
	{
		yield return new WaitForSeconds(transitionTime);

		gameManager.NextBattleButton();
	}

	IEnumerator WaitToLoad()
	{
		yield return new WaitForSeconds(transitionTime);

		gameManager.loadUI.Invoke();
	}

	IEnumerator WaitToHide()
	{
		yield return new WaitForSeconds(transitionTime);

		uiManager.HideMenus();
	}

	IEnumerator WaitToLoadLevel()
	{
		if (Time.timeScale != 1)
		{
			Time.timeScale = 1;
		}
		
		yield return new WaitForSeconds(transitionTime);
	}
}
                                              
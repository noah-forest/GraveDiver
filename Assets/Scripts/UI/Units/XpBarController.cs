using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpBarController : MonoBehaviour, ISlotPayloadChangeHandler
{
	public List<Color> pipColors = new(); //store both colors for pips
	public List<GameObject> pips = new(); //store a list of pips

	public GameObject cont; //pip parent

	public TextMeshProUGUI unitLevelDisplay;

	private Experience unitExp;
	private Slot slot;
	private GameObject xpBar;

	private void Awake()
	{

		slot = GetComponent<Slot>();
		xpBar = transform.GetChild(0).gameObject;

		//find pips in the XpBar
		foreach (Transform child in cont.transform.GetComponentsInChildren<Transform>())
		{
			if (child.name != "cont")
			{
				pips.Add(child.gameObject);
			}
		}

		ShowXpBar();
	}

	public void SlotPayloadChanged(GameObject payload)
	{
		if (payload == null)
		{
			if (xpBar) HideXpBar();

			if (unitExp)
			{
				unitExp.expGained.RemoveListener(UpdateXpBarUI);
				unitExp.unitLevelUp.RemoveListener(UpdateLevelUI);
				unitExp = null;
			}
			//unitNameDisplay.text = "no unit";
			//unitLevelDisplay.text = "level: 0";
			//unitXPDisplay.text = "xp: 0";
		}
		else
		{
			ShowXpBar();

			if (unitExp)
			{
				unitExp.expGained.RemoveListener(UpdateXpBarUI);
				unitExp.unitLevelUp.RemoveListener(UpdateLevelUI);
			}

			unitExp = payload.GetComponent<Experience>();
			//unitNameDisplay.text = payload.name;
			unitExp.expGained.AddListener(UpdateXpBarUI);
			unitExp.unitLevelUp.AddListener(UpdateLevelUI);

			if (unitExp == null || unitExp.GetExpNeeded() < pips.Count)
			{
				Destroy(pips[0]);
				pips.RemoveAt(0);
			} else if(unitExp.GetExpNeeded() > pips.Count)
			{
				AddPip();
			}

			UpdateXpBarUI(0);
		}
	}

	private void UpdateXpBarUI(int xp)
	{
		unitLevelDisplay.text = unitExp.curLevel.ToString();

		int xpNeeded = unitExp.curLevel == 1 ? Experience.ExpToLevel2 : Experience.ExpToLevel3;

		if (unitExp.curLevel == Experience.MaxLevel)
		{
			foreach (GameObject pip in pips)
			{
				pip.GetComponent<Image>().color = pipColors[1];
			}
			return;
		}

		for (int i = 0; i < xpNeeded; i++)
		{
			if (i < unitExp.Exp)
			{
				pips[i].GetComponent<Image>().color = pipColors[1];
			}
			else
			{
				pips[i].GetComponent<Image>().color = pipColors[0];  
			}
		}
	}

	private void UpdateLevelUI(int xp)
	{
		if (unitExp.curLevel == 2)
		{
			AddPip();
		}

		UpdateXpBarUI(xp);
	}

	private void ShowXpBar()
	{
		xpBar.SetActive(true);
	}

	private void HideXpBar()
	{
		xpBar.SetActive(false);
	}

	private void AddPip()
	{
		GameObject newPip = pips[0];
		newPip = Instantiate(newPip, cont.transform);
		newPip.GetComponent<Image>().color = pipColors[0];
		pips.Add(newPip);
	}
}

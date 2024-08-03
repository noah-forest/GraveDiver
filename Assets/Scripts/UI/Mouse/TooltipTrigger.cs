using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private void Start()
	{
		TooltipSystem.Hide();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TooltipSystem.Show();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TooltipSystem.Hide();
	}
}

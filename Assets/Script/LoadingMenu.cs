using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
	public float loadingPercentage;

	[SerializeField] private TextMeshProUGUI percentageText;
	[SerializeField] private Player player;
	private Slider slider;
	private Transform container;


	private void Awake()
	{
		slider = GetComponentInChildren<Slider>();
		container = transform.GetChild(0);
		ResetLoading();
	}

	public void ResetLoading()
	{
		player.ActiveMovement = false;
		container.gameObject.SetActive(true);
		slider.maxValue = 100;
		slider.minValue = 0;
		slider.value = 0;

	}
	public void UpdateLoadingPercentage(float percentage)
	{
		loadingPercentage = percentage;
		if (percentageText != null)
			percentageText.text = $"{percentage.ToString("0.0")}%";
		if (slider != null)
			slider.value = percentage;
		if (percentage == 100)
			StartCoroutine(CloseLoadingMenuCoroutine());
	}

	private IEnumerator CloseLoadingMenuCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		container.gameObject.SetActive(false);
		player.ActiveMovement = true;
	}
}

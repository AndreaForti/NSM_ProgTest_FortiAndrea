using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Player;

public class ResultMenu : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI MainText;
	[SerializeField] private TextMeshProUGUI DescriptionText;

	[SerializeField] private string MainTextDeathString;
	[SerializeField] private string MainTextWinString;
	[SerializeField] private string DescriptionTextMonsterString;
	[SerializeField] private string DescriptionTextWellString;
	[SerializeField] private string DescriptionTextPlayerString;
	[SerializeField] private string DescriptionTextAmmoString;
	[SerializeField] private string DescriptionTextPlayerWinString;

	private Transform container;

	public void Clear()
	{
		container.gameObject.SetActive(false);
		MainText.text = "";
		DescriptionText.text = "";
	}

	private void Awake()
	{
		container = transform.GetChild(0);
		Clear();
	}

	public void EnableResultMenu(ResultType resultType)
	{
		container.gameObject.SetActive(true);
		MainText.text = MainTextDeathString;
		MainText.color = Color.red;

		switch (resultType)
		{
			case ResultType.DeathByMonster:
				DescriptionText.text = DescriptionTextMonsterString;
				break;
			case ResultType.DeathByWell:
				DescriptionText.text = DescriptionTextWellString;
				break;
			case ResultType.DeathByplayer:
				DescriptionText.text = DescriptionTextPlayerString;
				break;
			case ResultType.OutOfAmmo:
				DescriptionText.text = DescriptionTextAmmoString;
				break;
			case ResultType.Win:
				DescriptionText.text = DescriptionTextPlayerWinString;
				MainText.text = MainTextWinString;
				MainText.color = Color.green;
				break;

		}
	}

	/// <summary>
	/// Start new Game, used by GUI Button
	/// </summary>
	public void NewGame()
	{
		GameManager.Instance.RestartGame();
	}
}

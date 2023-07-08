using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultMenu : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI MainText;
	[SerializeField] private TextMeshProUGUI DescriptionText;

	[SerializeField] private string MainTextDeathString;
	[SerializeField] private string MainTextWinString;
	[SerializeField] private string DescriptionTextMonsterString;
	[SerializeField] private string DescriptionTextWellString;
	[SerializeField] private string DescriptionTextPlayerString;
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
	private void SetDefaultDeath()
	{
		container.gameObject.SetActive(true);
		MainText.text = MainTextDeathString;
		MainText.color = Color.red;
	}
	private void SetDefaultWin()
	{
		container.gameObject.SetActive(true);
		MainText.text = MainTextWinString;
		MainText.color = Color.green;
	}
	public void SetDeathByMonster()
	{
		SetDefaultDeath();
		DescriptionText.text = DescriptionTextMonsterString;
	}
	public void SetDeathByWell()
	{
		SetDefaultDeath();
		DescriptionText.text = DescriptionTextWellString;
	}
	public void SetDeathByplayer()
	{
		SetDefaultDeath();
		DescriptionText.text = DescriptionTextPlayerString;
	}
	public void SetWin()
	{
		SetDefaultWin();
		DescriptionText.text = DescriptionTextPlayerWinString;
	}

	public void NewGame()
	{
		GameManager.Instance.RestartGame();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public static GameManager Instance { get => _instance; }

	private void Awake()
	{
		if (_instance == null)
			_instance = this;
		else if (_instance != this)
			Destroy(gameObject);
	}

	public Player player;
	public Grid grid;
	public LoadingMenu loadingMenu;
	public ResultMenu resultMenu;


	public void RestartGame()
	{
		loadingMenu.ResetLoading();
		resultMenu.Clear();
		grid.ResetGame();
	}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI loading;

	/// <summary>
	/// Load the Game Scene, used by GUI Button
	/// </summary>
	public void Play()
	{
		SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
	}
}

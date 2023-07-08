using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI loading;
	public void Play()
	{
		SceneManager.LoadScene("WorldGen2", LoadSceneMode.Single);
	}
}

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
		loading.gameObject.SetActive(true);
		StartLoadGameScene();
	}

	void StartLoadGameScene()
	{
		StartCoroutine(LoadGameSceneAsync());
	}

	IEnumerator LoadGameSceneAsync()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("WorldGen2", LoadSceneMode.Single);
		yield return asyncLoad.isDone;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName("WorldGen2"));
	}
}

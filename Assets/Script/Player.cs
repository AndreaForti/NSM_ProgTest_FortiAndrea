using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private Arrow arrowPrefab;
	public bool ActiveMovement = false;
	private int arrowCount = 5;
	private Camera gameGamera;

	public void Shoot(Vector3 direction)
	{
		if (arrowCount <= 0)
			return;
		if (!GameManager.Instance.grid.GetPlayerCurrentCell().IsPathAvailable(direction))
			return;

		Vector3 arrowPosition = transform.position;
		Arrow arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
		//arrow.spriteRenderer.gameObject.transform.Rotate(0f, 0f, 90);
		arrow.direction = direction;
		arrowCount--;
	}

	private void Awake()
	{
		gameGamera = GetComponentInChildren<Camera>(true);
		gameGamera.orthographicSize *= GameManager.Instance.grid.cellSize;
	}

	private void Update()
	{
		if (!ActiveMovement)
			return;

		if (Input.GetKeyDown(KeyCode.W))
			GameManager.Instance.grid.MovePlayer(Vector3.up);
		if (Input.GetKeyDown(KeyCode.S))
			GameManager.Instance.grid.MovePlayer(Vector3.down);
		if (Input.GetKeyDown(KeyCode.D))
			GameManager.Instance.grid.MovePlayer(Vector3.right);
		if (Input.GetKeyDown(KeyCode.A))
			GameManager.Instance.grid.MovePlayer(Vector3.left);

		if (Input.GetKeyDown(KeyCode.R))
			GameManager.Instance.grid.ResetGame();

		if (Input.GetKeyDown(KeyCode.I))
			Shoot(Vector3.up);
		if (Input.GetKeyDown(KeyCode.J))
			Shoot(Vector3.left);
		if (Input.GetKeyDown(KeyCode.K))
			Shoot(Vector3.down);
		if (Input.GetKeyDown(KeyCode.L))
			Shoot(Vector3.right);
	}

	public void EndPlayerGame(ResultType resultType)
	{
		ActiveMovement = false;
		GameManager.Instance.resultMenu.EnableResultMenu(resultType);
	}

	public enum ResultType
	{
		DeathByMonster,
		DeathByWell,
		DeathByplayer,
		Win
	}
}

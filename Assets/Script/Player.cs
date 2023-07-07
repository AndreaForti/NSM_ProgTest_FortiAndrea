using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private Arrow arrowPrefab;
	[SerializeField] private Grid grid;
	public bool ActiveMovement = false;
	private int arrowCount = 5;

	public void Shoot(Vector3 direction)
	{
		if (arrowCount <= 0)
			return;
		if (!grid.GetPlayerCurrentCell().IsPathAvailable(direction))
			return;

		Vector3 arrowPosition = transform.position;
		Arrow arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
		//arrow.spriteRenderer.gameObject.transform.Rotate(0f, 0f, 90);
		arrow.direction = direction;
		arrow.grid = grid;
		arrowCount--;
	}

	private void Update()
	{
		if (!ActiveMovement)
			return;

		if (Input.GetKeyDown(KeyCode.W))
			grid.MovePlayer(Vector3.up);
		if (Input.GetKeyDown(KeyCode.S))
			grid.MovePlayer(Vector3.down);
		if (Input.GetKeyDown(KeyCode.D))
			grid.MovePlayer(Vector3.right);
		if (Input.GetKeyDown(KeyCode.A))
			grid.MovePlayer(Vector3.left);

		if (Input.GetKeyDown(KeyCode.R))
			grid.ResetGame();

		if (Input.GetKeyDown(KeyCode.I))
			Shoot(Vector3.up);
		if (Input.GetKeyDown(KeyCode.J))
			Shoot(Vector3.left);
		if (Input.GetKeyDown(KeyCode.K))
			Shoot(Vector3.down);
		if (Input.GetKeyDown(KeyCode.L))
			Shoot(Vector3.right);
	}
}

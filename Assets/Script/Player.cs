using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private Arrow arrowPrefab;
	private int arrowCount = 5;

	public void Shoot(Vector3 direction, Grid gridRef)
	{
		if (arrowCount <= 0)
			return;
		if (!gridRef.GetPlayerCellRelative(Vector3.zero).IsPathAvailable(direction))
			return;

		Vector3 arrowPosition = transform.position;
		Arrow arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
		//arrow.spriteRenderer.gameObject.transform.Rotate(0f, 0f, 90);
		arrow.direction = direction;
		arrow.grid = gridRef;
		arrowCount--;
	}
}

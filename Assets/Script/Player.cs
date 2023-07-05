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
		Vector3 arrowPositin = new Vector3(transform.position.x + direction.x * gridRef.cellSize, transform.position.y + direction.y * gridRef.cellSize, -0.04f);
		Arrow arrow = Instantiate(arrowPrefab, , Quaternion.identity);
		//arrow.spriteRenderer.gameObject.transform.Rotate(0f, 0f, 90);
		arrow.direction = direction;
		arrow.grid = gridRef;
		arrowCount--;
	}
}

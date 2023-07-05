using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Arrow : MonoBehaviour
{
	public Vector3 direction;
	public Grid grid;
	public SpriteRenderer spriteRenderer;


	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}
	private void Start()
	{
		spriteRenderer.transform.localScale *= grid.cellSize;
		spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, direction, Vector3.forward));
		StartPositinUpdateCoroutine();
	}

	public void StartPositinUpdateCoroutine()
	{
		StartCoroutine(UpdatePositionCoroutine());
	}

	IEnumerator UpdatePositionCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);
			grid.UpdateArrowMove(this);
			transform.position += direction * grid.cellSize;
		}
	}

	public void DestroyArrow()
	{
		grid.RepositionMonster();
		StopAllCoroutines();
		Destroy(gameObject);
	}
}

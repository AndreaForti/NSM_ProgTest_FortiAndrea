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
			yield return new WaitForSeconds(2);
			grid.UpdateArrowMove(this);
			transform.position += direction * grid.cellSize;
		}
	}

	public void DestroyArrow()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}
}

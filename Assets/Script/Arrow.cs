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
	public SpriteRenderer spriteRenderer;


	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}
	private void Start()
	{
		spriteRenderer.transform.localScale *= GameManager.Instance.grid.cellSize;
		spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, new Vector3(direction.x, direction.y), Vector3.forward));
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
			GameManager.Instance.grid.UpdateArrowMove(this);
		}
	}

	public void ApplyMovement()
	{
		transform.position += new Vector3(direction.x, direction.y) * GameManager.Instance.grid.cellSize;
		spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, new Vector3(direction.x, direction.y), Vector3.forward));
	}

	public void DestroyArrow()
	{
		GameManager.Instance.grid.RepositionMonster();
		StopAllCoroutines();
		Destroy(gameObject);
	}
}

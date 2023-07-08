using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Arrow : MonoBehaviour
{
	public Vector3 Direction;
	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
	}
	private void Start()
	{
		spriteRenderer.transform.localScale *= GameManager.Instance.grid.cellSize;
		AlignSpriteRotationToDirection();
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
		transform.position += new Vector3(Direction.x, Direction.y) * GameManager.Instance.grid.cellSize;
		AlignSpriteRotationToDirection();
	}

	private void AlignSpriteRotationToDirection()
	{
		spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, new Vector3(Direction.x, Direction.y), Vector3.forward));
	}

	public void DestroyArrow()
	{
		GameManager.Instance.player.flyingArrowCount--;
		GameManager.Instance.grid.RepositionMonster();
		StopAllCoroutines();
		Destroy(gameObject);
	}
}

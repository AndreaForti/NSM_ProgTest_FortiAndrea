using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] int ArrowsOnSpawn;

	private int currentArrowsCount;


	private void Start()
	{
		currentArrowsCount = ArrowsOnSpawn;
	}

	public void FireArrow(Vector3 direction)
	{
		if (currentArrowsCount > 0)
		{

		}
	}
}

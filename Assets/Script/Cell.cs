using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

	[SerializeField] private SpriteRenderer[] doors;
	[SerializeField] private SpriteRenderer fogOfWar;
	[SerializeField] private SpriteRenderer entityIcon;

	public IconManager IconManager;

	public bool canThreatsSpawn = false;
	public bool generated = false;
	public bool hasMonster = false;
	public bool hasTeleporter = false;
	public bool hasWell = false;

	private Vector3 gridPosition;
	private int[] paths = new int[4] { 0, 0, 0, 0 };
	public bool CanMonsterSpawn() { return !(hasMonster || hasTeleporter || hasWell) && !canThreatsSpawn; }
	public bool CanTeleporterSpawn() { return !(hasMonster || hasTeleporter || hasWell) && !canThreatsSpawn; }
	public bool CanWellSpawn() { return !(hasMonster || hasTeleporter || hasWell) && !canThreatsSpawn; }
	public bool IsCellSafeFromThreats() { return !(hasMonster || hasTeleporter || hasWell); }

	private void Awake()
	{
		IconManager = GetComponentInChildren<IconManager>();
	}

	public void SetPath(Vector3 direction, int value)
	{
		if (direction == Vector3.up)
			paths[0] = value;
		if (direction == Vector3.right)
			paths[1] = value;
		if (direction == Vector3.down)
			paths[2] = value;
		if (direction == Vector3.left)
			paths[3] = value;
		UpdateDoors();
	}

	private void UpdateDoors()
	{
		for (int i = 0; i < paths.Length; i++)
		{
			doors[i].enabled = paths[i] == 0;
		}
	}

	public bool IsPathAvailable(Vector3 direction)
	{
		if (direction == Vector3.up)
			return paths[0] == 1;
		if (direction == Vector3.right)
			return paths[1] == 1;
		if (direction == Vector3.down)
			return paths[2] == 1;
		if (direction == Vector3.left)
			return paths[3] == 1;
		return false;
	}

	public void HideFogOfWar()
	{
		fogOfWar.gameObject.SetActive(false);
	}

	public void SetGridPosition(int x, int y)
	{
		gridPosition = new Vector3(x, y);
	}

	public Vector3 GetGridPosition()
	{
		return gridPosition;
	}
	public void SetEntity(Color color)
	{
		entityIcon.color = color;
		entityIcon.gameObject.SetActive(true);
	}

	public void OnPlayerEnter(Grid grid)
	{
		if (hasMonster)
			grid.KillPlayer();
		else if (hasTeleporter)
			grid.TeleportPlayer();
		else if (hasWell)
			grid.KillPlayer();
	}

	public void OnArrowEnter(Grid grid, Arrow arrow)
	{
		if(hasMonster)
			grid.PlayerWin();
	}
}

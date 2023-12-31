using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
	[SerializeField] private SpriteRenderer backgroundSprite;
	[SerializeField] private SpriteRenderer[] doors;
	[SerializeField] private SpriteRenderer fogOfWar;
	[SerializeField] private SpriteRenderer entityIcon;


	[SerializeField] private Sprite defaultBackgroundSprite;
	[SerializeField] private Sprite tunnelBackgroundSprite;

	public IconManager IconManager;

	public bool canThreatsSpawn = false;
	public bool generated = false;
	public bool hasMonster = false;
	public bool hasTeleporter = false;
	public bool hasWell = false;
	public bool isHidden = true;
	public bool isTunnel = false;
	public int tunnelEntrancePathRef;

	private Vector3 gridPosition;
	private int[] paths = new int[4] { 0, 0, 0, 0 };
	public bool CanMonsterSpawn() { return !hasMonster && !hasTeleporter && !hasWell && !canThreatsSpawn && isHidden && !isTunnel; }
	public bool CanTeleporterSpawn() { return !hasMonster && !hasTeleporter && !hasWell && !canThreatsSpawn; }
	public bool CanWellSpawn() { return !hasMonster && !hasTeleporter && !hasWell && !canThreatsSpawn; }
	public bool IsCellSafeFromThreats() { return !hasMonster && !hasTeleporter && !hasWell; }

	public bool CanTunnelSpawn()
	{
		return IsCellSafeFromThreats() && HasAnglePath() && !canThreatsSpawn;
	}

	private void Awake()
	{
		IconManager = GetComponentInChildren<IconManager>(true);
		backgroundSprite.sprite = defaultBackgroundSprite;
	}

	public void OpenPath(Vector3 direction)
	{
		if (direction == Vector3.up)
			paths[0] = 1;
		if (direction == Vector3.right)
			paths[1] = 1;
		if (direction == Vector3.down)
			paths[2] = 1;
		if (direction == Vector3.left)
			paths[3] = 1;
		UpdateDoors();
	}

	public bool HasStraightPath()
	{
		if (GetEntranceCount() != 2)
			return false;
		if ((paths[0] == 1 && paths[2] == 1) || (paths[1] == 1 && paths[3] == 1))
			return true;
		return false;
	}
	public bool HasAnglePath()
	{
		if (GetEntranceCount() != 2)
			return false;
		return !HasStraightPath();
	}
	public int GetEntranceCount()
	{
		int pathCount = 0;
		for (int i = 0; i < paths.Length; i++)
			pathCount += paths[i];
		return pathCount;
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
		isHidden = false;
	}

	public void SetGridPosition(int x, int y)
	{
		gridPosition = new Vector3(x, y);
	}

	public Vector3 GetGridPosition()
	{
		return gridPosition;
	}
	public void SetEntityGUI(Color color)
	{
		entityIcon.color = color;
		entityIcon.gameObject.SetActive(true);
	}

	public void ClearEntyty()
	{
		entityIcon.gameObject.SetActive(false);
		IconManager.Clear();
	}

	public void OnPlayerEnter(Vector3 enteringFromDirection)
	{
		if (isTunnel)
			GameManager.Instance.grid.MovePlayer(GetTunnelExit(enteringFromDirection));
		else if (hasMonster)
			GameManager.Instance.player.EndPlayerGame(Player.ResultType.DeathByMonster);
		else if (hasTeleporter)
			GameManager.Instance.grid.TeleportPlayer();
		else if (hasWell)
			GameManager.Instance.player.EndPlayerGame(Player.ResultType.DeathByWell);
	}

	public void OnArrowEnter(Arrow arrow)
	{
		arrow.ApplyMovement();
		if (hasMonster)
			GameManager.Instance.player.EndPlayerGame(Player.ResultType.Win);
		else if (isTunnel)
		{
			arrow.Direction = GetTunnelExit(arrow.Direction * -1);
			GameManager.Instance.grid.UpdateArrowMove(arrow);
		}

		if (GameManager.Instance.grid.GetPlayerCurrentCell() == this)
			GameManager.Instance.player.EndPlayerGame(Player.ResultType.DeathByplayer);

	}

	public Vector3 GetTunnelExit(Vector3 enteringFromDirection)
	{
		if (isTunnel)
			foreach (Vector3 direction in Grid.AdiacentPositions)
				if (direction != enteringFromDirection && IsPathAvailable(direction))
					return direction;
		return Vector3.zero;
	}

	public void SetTunnelCell()
	{
		isTunnel = true;
		backgroundSprite.sprite = tunnelBackgroundSprite;

		//Calculate Direction for sprite rotation
		for (int i = 0; i < paths.Length; i++)
		{
			if (paths[i] == 1 && i == paths.Length - 1)
			{
				tunnelEntrancePathRef = i;
				break;
			}
			if (paths[i] == 1 && paths[i + 1] == 1)
			{
				tunnelEntrancePathRef = i;
				break;
			}
		}
		Vector3 direction = Grid.AdiacentPositions[tunnelEntrancePathRef];
		backgroundSprite.transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.right, direction, Vector3.forward));
	}
}

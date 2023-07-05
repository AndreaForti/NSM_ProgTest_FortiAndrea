using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using static IconManager;

public class Grid : MonoBehaviour
{
	[Header("Grid Data")]
	[SerializeField] private int gridSize;
	[SerializeField] public int cellSize;

	[Header("World Gen Values")]
	[Range(0f, 1f)]
	[SerializeField] private float ExtraPathInCellChance;
	[SerializeField] public Vector3 spawnPointGrid;
	[SerializeField] private int MonstersCount;
	[SerializeField] private int TeleporterCount;
	[SerializeField] private int WellCount;

	[Header("References")]
	[SerializeField] private Cell cellPrefab;
	[SerializeField] private Player player;
	[SerializeField] private Camera gameCamera;

	private int generationDepth = 0;
	public static List<Vector3> AdiacentPositions = new List<Vector3>() { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
	private Cell[,] cells;
	private List<Cell> cellList;


	private void Start()
	{
		ResetGame();
		gameCamera.orthographicSize *= cellSize;
	}

	public void ResetGame()
	{
		WorldGen();
		PlayerSpawn();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
			MovePlayer(Vector3.up);
		if (Input.GetKeyDown(KeyCode.S))
			MovePlayer(Vector3.down);
		if (Input.GetKeyDown(KeyCode.D))
			MovePlayer(Vector3.right);
		if (Input.GetKeyDown(KeyCode.A))
			MovePlayer(Vector3.left);

		if (Input.GetKeyDown(KeyCode.R))
			ResetGame();

		if (Input.GetKeyDown(KeyCode.I))
			player.Shoot(Vector3.up, this);
		if (Input.GetKeyDown(KeyCode.J))
			player.Shoot(Vector3.left, this);
		if (Input.GetKeyDown(KeyCode.K))
			player.Shoot(Vector3.down, this);
		if (Input.GetKeyDown(KeyCode.L))
			player.Shoot(Vector3.right, this);
	}

	#region PLAYER
	private void MovePlayer(Vector3 direction)
	{
		if (CheckValidPlayerMovement(direction))
		{
			player.transform.position += direction * cellSize;
			Cell destinationCell = GetPlayerCellRelative(Vector3.zero);
			SetPlayerPositionToCell(destinationCell);
		}
	}

	public bool CheckValidPlayerMovement(Vector3 direction)
	{
		Vector3 testingNewPosition = player.transform.position + direction * cellSize;

		//out of grid check
		if (Mathf.Abs(testingNewPosition.x) > gridSize * cellSize / 2 || Mathf.Abs(testingNewPosition.y) > gridSize * cellSize / 2)
			return false;

		if (!GetPlayerCellRelative(Vector3.zero).IsPathAvailable(direction))
			return false;

		return true;
	}

	public Cell GetPlayerCellRelative(Vector3 relative)
	{
		Vector2Int cellGridPos = GetCellGridPositionFromWorldPos(player.transform.position + relative * cellSize);
		//int x = (((int)player.transform.position.x / cellSize) + gridSize / 2) + (int)relative.x;
		//int y = (((int)player.transform.position.y / cellSize) + gridSize / 2) + (int)relative.y;

		if (Mathf.Abs(cellGridPos.x) >= gridSize || Mathf.Abs(cellGridPos.y) >= gridSize)
			return null;
		return cells[cellGridPos.x, cellGridPos.y];
	}

	public Cell getCellFromWorldPosition(Vector3 position)
	{
		Vector2Int cellGridPos = GetCellGridPositionFromWorldPos(position);
		return cells[cellGridPos.x, cellGridPos.y];
	}

	public Vector2Int GetCellGridPositionFromWorldPos(Vector3 position)
	{
		return new Vector2Int((int)((position.x / cellSize) + gridSize / 2), (int)((position.y / cellSize) + gridSize / 2));
	}

	public void TeleportPlayer()
	{
		Cell randomizedCell = cellList.Where(x => x.IsCellSafeFromThreats()).ToList()[Random.Range(0, cellList.Where(x => x.IsCellSafeFromThreats()).Count())];
		SetPlayerPositionToCell(randomizedCell);
	}

	public void KillPlayer()
	{
		Debug.Log("YOU DIED");
	}

	public void PlayerWin()
	{
		Debug.Log("YOU WIN");
	}
	public void SetPlayerPositionToCell(Cell cell)
	{
		player.transform.position = new Vector3(cell.transform.position.x, cell.transform.position.y, player.transform.position.z);
		cell.HideFogOfWar();
		cell.OnPlayerEnter(this);
	}
	#endregion

	#region ACCESS DATA

	public Cell GetAdiancentCell(Cell currentCell, Vector3 relative)
	{
		Vector3 adiacentCellPosition = currentCell.GetGridPosition() + relative;
		if (adiacentCellPosition.x < 0 || adiacentCellPosition.x >= gridSize || adiacentCellPosition.y >= gridSize || adiacentCellPosition.y < 0)
			return null;
		//Debug.Log($"GetAdiancentCell: {(int)adiacentCellPosition.x} | {(int)adiacentCellPosition.y}");
		return cells[(int)adiacentCellPosition.x, (int)adiacentCellPosition.y];
	}

	public List<Cell> GetAdiacentCells(Cell currentCell)
	{
		List<Cell> cells = new List<Cell>();

		foreach (Vector3 relativePosition in AdiacentPositions)
		{
			Cell tempCell = GetAdiancentCell(currentCell, relativePosition);
			if (tempCell != null)
				cells.Add(tempCell);
		}
		return cells;
	}

	private Vector3 GetRelativePositionBetweenCells(Cell origin, Cell destination)
	{
		return destination.GetGridPosition() - origin.GetGridPosition();
	}
	#endregion

	#region WORLDGEN
	public void WorldGen()
	{
		WorldCellsInGridInstantiate();
		WorldCellsPathingCreation();
		MonsterSpawn();
		TeleporterSpawn();
		WellSpawn();

		UpdateGUI();
	}

	public void WorldCellsInGridInstantiate()
	{
		cells = new Cell[gridSize, gridSize];

		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		for (int row = 0; row < gridSize; row++)
		{
			for (int column = 0; column < gridSize; column++)
			{
				Cell tempCell = Instantiate(cellPrefab, transform.position + new Vector3(row - gridSize / 2, column - gridSize / 2, 0f) * cellSize, Quaternion.identity, transform);
				tempCell.gameObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
				tempCell.SetGridPosition(row, column);
				cells[row, column] = tempCell;
			}
		}

		cells[(int)spawnPointGrid.x, (int)spawnPointGrid.y].canThreatsSpawn = true;

		cellList = cells.Cast<Cell>().ToList();
	}


	public void PlayerSpawn()
	{
		Vector3 cellPosition = cells[(int)spawnPointGrid.x, (int)spawnPointGrid.y].transform.position;
		player.transform.position = cellPosition;
		player.transform.localScale = new Vector3(cellSize, cellSize, player.transform.localScale.z);
		GetPlayerCellRelative(Vector3.zero).HideFogOfWar();
	}
	private void WorldCellsPathingCreation()
	{
		RecoursiveWorldCellsPathingCreation(cells[10, 10]);
		Debug.Log($"UnGeneratedCells: {cellList.Where(x => !x.generated).Count()}");
	}

	private bool RecoursiveWorldCellsPathingCreation(Cell currentCell)
	{
		Debug.Log($"[{generationDepth}] Start recoursiveWorldGen {currentCell.GetGridPosition()}");
		List<Cell> adiacentCells = GetAdiacentCells(currentCell);
		Debug.Log($"[{generationDepth}] adiacentCells count: {adiacentCells.Count}");


		Cell nextCell = CellSpawn(currentCell);
		generationDepth++;

		if (nextCell == null)
			return false;
		else
		{
			bool result = RecoursiveWorldCellsPathingCreation(nextCell);
			if (!result)
			{
				return RecoursiveWorldCellsPathingCreation(currentCell);
			}
			return result;
		}
	}
	private Cell CellSpawn(Cell cell)
	{
		Cell selectedRandomUngeneratedAdiancentCell = null;
		List<Cell> adiacentCells = GetAdiacentCells(cell);
		List<Cell> ungeneratedAdiacentCells = adiacentCells.Where(x => !x.generated).ToList();
		if (ungeneratedAdiacentCells.Count > 0)
		{
			selectedRandomUngeneratedAdiancentCell = ungeneratedAdiacentCells[Random.Range(0, ungeneratedAdiacentCells.Count)];
			Vector3 direction = GetRelativePositionBetweenCells(selectedRandomUngeneratedAdiancentCell, cell);
			selectedRandomUngeneratedAdiancentCell.SetPath(direction, 1);
			cell.SetPath(direction * -1, 1);
			Debug.Log($"[{generationDepth}] Opened new Path between cells {cell.GetGridPosition()} | {selectedRandomUngeneratedAdiancentCell.GetGridPosition()}");

			//spawn extra path in cell
			if (Random.Range(0f, 1f) <= ExtraPathInCellChance)
			{
				Cell selectedRandomAdiancentCell = adiacentCells[Random.Range(0, adiacentCells.Count)];
				Vector3 direction2 = GetRelativePositionBetweenCells(selectedRandomAdiancentCell, cell);
				selectedRandomAdiancentCell.SetPath(direction2, 1);
				cell.SetPath(direction2 * -1, 1);
			}
		}
		cell.generated = true;
		return selectedRandomUngeneratedAdiancentCell;
	}

	private void MonsterSpawn()
	{
		for (int i = 0; i < MonstersCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanMonsterSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanMonsterSpawn()).Count())];
			randomizedCell.hasMonster = true;
			Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}

	private void TeleporterSpawn()
	{
		for (int i = 0; i < TeleporterCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanTeleporterSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanTeleporterSpawn()).Count())];
			randomizedCell.hasTeleporter = true;
			Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}
	private void WellSpawn()
	{
		for (int i = 0; i < WellCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanWellSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanWellSpawn()).Count())];
			randomizedCell.hasWell = true;
			Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}

	public void UpdateGUI()
	{
		cellList.ForEach(x => x.ClearEntyty());
		foreach (Cell cell in cellList.Where(x => x.hasMonster).ToList())
		{
			cell.SetEntity(Color.red);
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Monster);
			}
		}
		foreach (Cell cell in cellList.Where(x => x.hasTeleporter).ToList())
		{
			cell.SetEntity(Color.cyan);
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Telporter);
			}
		}
		foreach (Cell cell in cellList.Where(x => x.hasWell).ToList())
		{
			cell.SetEntity(Color.green);
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Well);
			}
		}
	}

	public void UpdateArrowMove(Arrow arrow)
	{
		Cell currentArrowCell = getCellFromWorldPosition(arrow.transform.position);

		if (currentArrowCell.IsPathAvailable(arrow.direction))
		{
			Cell destinationArrowCell = GetAdiancentCell(currentArrowCell, arrow.direction);
			if (destinationArrowCell != null)
				destinationArrowCell.OnArrowEnter(this, arrow);
			else
				arrow.DestroyArrow();
		}
		else
			arrow.DestroyArrow();
	}

	public void RepositionMonster()
	{
		foreach (Cell monsterCell in cellList.Where(x => x.hasMonster).ToList())
			monsterCell.hasMonster = false;
		MonsterSpawn();
		UpdateGUI();
	}

	#endregion

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.down) * gridSize / 2 * cellSize, transform.position + (Vector3.right + Vector3.down) * gridSize / 2 * cellSize);
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.up) * gridSize / 2 * cellSize, transform.position + (Vector3.right + Vector3.up) * gridSize / 2 * cellSize);
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.down) * gridSize / 2 * cellSize, transform.position + (Vector3.left + Vector3.up) * gridSize / 2 * cellSize);
		Gizmos.DrawLine(transform.position + (Vector3.right + Vector3.down) * gridSize / 2 * cellSize, transform.position + (Vector3.right + Vector3.up) * gridSize / 2 * cellSize);

	}
}

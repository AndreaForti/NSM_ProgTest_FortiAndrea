using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
	[Header("Grid Data")]
	[SerializeField] private int gridSize;
	[SerializeField] private int cellSize;

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
	}

	public void ResetGame()
	{
		WorldGen();
		PlayerSpawn();
	}
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
				tempCell.SetGridPosition(row, column);
				cells[row, column] = tempCell;
			}
		}

		cells[(int)spawnPointGrid.x, (int)spawnPointGrid.y].safeCell = true;

		cellList = cells.Cast<Cell>().ToList();
	}


	public void PlayerSpawn()
	{
		player.transform.position = transform.position + Vector3.forward * -1;
		GetPlayerCellRelative(Vector3.zero).HideFogOfWar();
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
	}


	private void MovePlayer(Vector3 direction)
	{
		if (CheckValidPlayerMovement(direction))
		{
			player.transform.position += direction * cellSize;
			GetPlayerCellRelative(Vector3.zero).HideFogOfWar();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.down) * gridSize / 2, transform.position + (Vector3.right + Vector3.down) * gridSize / 2);
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.up) * gridSize / 2, transform.position + (Vector3.right + Vector3.up) * gridSize / 2);
		Gizmos.DrawLine(transform.position + (Vector3.left + Vector3.down) * gridSize / 2, transform.position + (Vector3.left + Vector3.up) * gridSize / 2);
		Gizmos.DrawLine(transform.position + (Vector3.right + Vector3.down) * gridSize / 2, transform.position + (Vector3.right + Vector3.up) * gridSize / 2);

	}

	public bool CheckValidPlayerMovement(Vector3 direction)
	{
		Vector3 testingNewPosition = player.transform.position + direction * cellSize;

		//out of grid check
		if (Mathf.Abs(testingNewPosition.x) > gridSize / 2 || Mathf.Abs(testingNewPosition.y) > gridSize / 2)
			return false;

		if (!GetPlayerCellRelative(Vector3.zero).IsPathAvailable(direction))
			return false;

		return true;
	}

	public Cell GetPlayerCellRelative(Vector3 relative)
	{
		int x = (((int)player.transform.position.x / cellSize) + gridSize / 2) + (int)relative.x;
		int y = (((int)player.transform.position.y / cellSize) + gridSize / 2) + (int)relative.y;

		if (Mathf.Abs(x) >= gridSize || Mathf.Abs(y) >= gridSize)
			return null;
		return cells[x, y];
	}

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

	private Vector3 GetRelativePositionBetweenCells(Cell origin, Cell destination)
	{
		return destination.GetGridPosition() - origin.GetGridPosition();
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
		cellList.Where(x => x.hasMonster).ToList().ForEach(x => x.SetEntity(Color.yellow));
		cellList.Where(x => x.hasTeleporter).ToList().ForEach(x => x.SetEntity(Color.cyan));
		cellList.Where(x => x.hasWell).ToList().ForEach(x => x.SetEntity(Color.blue));
	}

	public void UpdateCellListReference()
	{

	}

}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
	[SerializeField] private int gridSize;
	[SerializeField] private int cellSize;

	[SerializeField] private Cell cellPrefab;
	[SerializeField] private Player player;
	[SerializeField] private Camera gameCamera;

	[SerializeField] private float chanceForSecondPath;

	private int generationDepth = 0;

	public static List<Vector3> AdiacentPositions = new List<Vector3>() { Vector3.up, Vector3.right, Vector3.down, Vector3.left };

	private Cell[,] cells;
	private void Start()
	{
		cells = new Cell[gridSize, gridSize];
		WorldGen();
		PlayerSpawn();
	}

	public void WorldGen()
	{
		//instantiate cells
		for (int row = 0; row < gridSize; row++)
		{
			for (int column = 0; column < gridSize; column++)
			{
				Cell tempCell = Instantiate(cellPrefab, transform.position + new Vector3(row - gridSize / 2, column - gridSize / 2, 0f) * cellSize, Quaternion.identity, transform);
				tempCell.SetGridPosition(row, column);
				//tempCell.SetPaths(true, true, true, true);
				//tempCell.RandomizePaths();
				cells[row, column] = tempCell;
			}
		}

		RunWorldGeneration();
	}

	public void PlayerSpawn()
	{
		player.transform.position = transform.position + Vector3.forward * -1;
		GetPlayerCellRelative(Vector3.zero).HideFogOfWar();
		//camera.transform.position += Vector3.right * player.transform.position.x;
		//camera.transform.position += Vector3.up * player.transform.position.y;
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
		//out of grid
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
		//Debug.Log($"x: {x}   y:{y}");

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

	private void RunWorldGeneration()
	{
		//cells[10, 10].SetPaths(right: 1);
		//cells[10, 10].generated = true;
		recoursiveWorldGen(cells[10, 10]);
	}

	private bool recoursiveWorldGen(Cell currentCell)
	{
		Debug.Log($"[{generationDepth}] Start recoursiveWorldGen {currentCell.GetGridPosition()}");
		List<Cell> adiacentCells = GetAdiacentCells(currentCell);
		Debug.Log($"[{generationDepth}] adiacentCells count: {adiacentCells.Count}");


		Cell nextCell = OpenNewPathInCell(currentCell);
		generationDepth++;

		if (nextCell == null)
			return false;
		else
		{
			bool result = recoursiveWorldGen(nextCell);
			if (!result)
			{
				return recoursiveWorldGen(currentCell);
			}
			return result;
		}

		//align adiancent cells with current cell doors open
		//foreach (Cell adiacentCell in adiacentCells)
		//{
		//	Vector3 direction = GetRelativePositionBetweenCells(currentCell, adiacentCell);
		//	if (currentCell.IsPathAvailable(direction))
		//	{
		//		adiacentCell.SetPath(direction * -1, 1);
		//		Debug.Log($"[{generationDepth}] Cell {adiacentCell.GetGridPosition()} opened backward path {direction * -1}");
		//	}
		//}
		//List<Cell> ungeneratedAdiacentCells = adiacentCells.Where(x => !x.generated).ToList();
		//if (ungeneratedAdiacentCells.Count > 0)
		//{
		//	Cell randomUngeneratedAdiancentCell = ungeneratedAdiacentCells[Random.Range(0, ungeneratedAdiacentCells.Count)];
		//	Debug.Log($"[{generationDepth}] Picked cell {randomUngeneratedAdiancentCell.GetGridPosition()}");
		//	OpenNewPathInCell(randomUngeneratedAdiancentCell);
		//	generationDepth++;
		//	recoursiveWorldGen(randomUngeneratedAdiancentCell);
		//}
	}

	private Vector3 GetRelativePositionBetweenCells(Cell origin, Cell destination)
	{
		return destination.GetGridPosition() - origin.GetGridPosition();
	}

	private Cell OpenNewPathInCell(Cell cell)
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

			//extra path
			if(Random.Range(0f,1f) <= chanceForSecondPath)
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
}

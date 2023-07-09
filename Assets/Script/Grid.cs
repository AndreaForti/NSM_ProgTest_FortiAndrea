using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using static IconManager;
using static UnityEngine.Rendering.DebugUI.Table;

public class Grid : MonoBehaviour
{
	[Header("Grid Configuration")]
	[SerializeField] private int gridSize;
	[SerializeField] public int cellSize;
	[SerializeField] private Vector3 spawnPointGrid;

	[Header("World Gen Values")]
	[Range(0f, 1f)]
	[SerializeField] private float extraPathInCellChance;
	[SerializeField] private int monsterSpawnCount;
	[SerializeField] private int teleporterSpawnCount;
	[SerializeField] private int wellSpawnCount;
	[SerializeField] private int tunnelSpawnCount;

	[Header("References")]
	[SerializeField] private Cell cellPrefab;

	private Cell[,] cells;
	private List<Cell> cellList;

	private Stack<Cell> cellsStack = new Stack<Cell>();
	private int generatedCells;


	public static List<Vector3> AdiacentPositions = new List<Vector3>() { Vector3.up, Vector3.right, Vector3.down, Vector3.left };



	private void Start()
	{
		ResetGame();
	}

	public void ResetGame()
	{
		WorldGen();
		PlayerSpawn();
	}

	#region PLAYER
	public void MovePlayer(Vector3 direction)
	{
		if (CheckValidPlayerMovement(direction))
		{
			GameManager.Instance.player.transform.position += direction * cellSize;
			Cell destinationCell = GetPlayerCurrentCell();
			SetPlayerPositionToCell(destinationCell, direction * -1);
		}
	}

	public bool CheckValidPlayerMovement(Vector3 direction)
	{
		Vector3 testingNewGridCoordinate = GetCellGridCoordinatesFromWorldPos(GameManager.Instance.player.transform.position + direction * cellSize);
		if (!IsGridCoordinateInsideGrid(testingNewGridCoordinate))
			return false;

		if (!GetPlayerCurrentCell().IsPathAvailable(direction))
			return false;

		return true;
	}

	/// <summary>
	/// Returns the Cell next to the player in the chosen direction.
	/// </summary>
	/// <param name="relativePosition">Normalized Vector indicating the direction to pick the Cell from (player position is the origin)</param>
	/// <returns>Cell next to the player in the chosen direction</returns>
	public Cell GetPlayerAdiacentCell(Vector3 relativePosition)
	{
		Vector3 cellGridPos = GetCellFromWorldPosition(GameManager.Instance.player.transform.position + relativePosition * cellSize).GetGridPosition();

		if (Mathf.Abs(cellGridPos.x) >= gridSize || Mathf.Abs(cellGridPos.y) >= gridSize)
			return null;
		return GetCellFromGridPosition(cellGridPos);
	}

	public Cell GetPlayerCurrentCell()
	{
		return GetPlayerAdiacentCell(Vector3.zero);
	}


	/// <summary>
	/// Set the Player position to a new random available safe location.
	/// </summary>
	public void TeleportPlayer()
	{
		List<Cell> availableCells = cellList.Where(x => x.IsCellSafeFromThreats() && !x.isTunnel).ToList();
		Cell randomizedCell = availableCells[Random.Range(0, availableCells.Count())];
		SetPlayerPositionToCell(randomizedCell, Vector3.zero);
	}

	/// <summary>
	/// Set the Player position to a specific Cell.
	/// </summary>
	/// <param name="cell">Destination Cell</param>
	/// <param name="enteringFromDirection">Direction which the player is entering the new Cell from</param>
	public void SetPlayerPositionToCell(Cell cell, Vector3 enteringFromDirection)
	{
		GameManager.Instance.player.transform.position = cell.transform.position;
		cell.HideFogOfWar();
		cell.OnPlayerEnter(enteringFromDirection);
	}
	#endregion

	#region ACCESS DATA


	/// <summary>
	/// Centralized function to get data from Cell matrix converting Vector3 to Vector3Int.
	/// </summary>
	/// <param name="GridCoordinates">Coordinates for the cell inside the grid matrix</param>
	/// <returns>Selected Cell</returns>
	public Cell GetCellFromGridPosition(Vector3 GridCoordinates)
	{
		if (IsGridCoordinateInsideGrid(GridCoordinates))
		{
			Vector3Int convertedGridPosition = Vector3Int.FloorToInt(GridCoordinates);
			return cells[convertedGridPosition.x, convertedGridPosition.y];
		}
		return null;
	}

	/// <summary>
	/// Convert a World Position to the corresponding Cell inside the grid.
	/// </summary>
	/// <param name="position">Position in World</param>
	/// <returns>Cell inside the grid</returns>
	public Cell GetCellFromWorldPosition(Vector3 position)
	{
		Vector3 cellGridPos = GetCellGridCoordinatesFromWorldPos(position);
		return GetCellFromGridPosition(cellGridPos);
	}

	/// <summary>
	/// Convert a World Position to the corresponding Coordinates inside the grid.
	/// </summary>
	/// <param name="position">Position in World Space</param>
	/// <returns>Cell coordinates inside the grid</returns>
	public Vector3 GetCellGridCoordinatesFromWorldPos(Vector3 position)
	{
		return new Vector3((int)((position.x / cellSize) + gridSize / 2), (int)((position.y / cellSize) + gridSize / 2));
	}

	/// <summary>
	/// Returns the Cell next to the chosen Cell in the chosen Direction.
	/// </summary>
	/// <param name="currentCell">Reference Cell</param
	/// <param name="relative">Normalized Vector indicating the direction to pick the Cell from (player position is the origin)</param>
	/// <returns>Cell next to the chosen Cell in the chosen Direction</returns>
	public Cell GetAdiancentCell(Cell currentCell, Vector3 relative)
	{
		Vector3 adiacentCellPosition = currentCell.GetGridPosition() + relative;
		if (adiacentCellPosition.x < 0 || adiacentCellPosition.x >= gridSize || adiacentCellPosition.y >= gridSize || adiacentCellPosition.y < 0)
			return null;
		return cells[(int)adiacentCellPosition.x, (int)adiacentCellPosition.y];
	}

	/// <summary>
	/// Returns all the Cells next to the chosen Cell, no pathing is calculated in this operation.
	/// </summary>
	/// <param name="currentCell">Reference Cell</param>
	/// <returns>Cells next to the chosen Cell</returns>
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

	/// <summary>
	///  Returns the direction required to move from origin Cell to destination Cell.
	/// </summary>
	/// <param name="origin">Cell to move from</param>
	/// <param name="destination">Cell to move to</param>
	/// <returns>Direction required to move from origin Cell to destination Cell</returns>
	private Vector3 GetRelativePositionBetweenCells(Cell origin, Cell destination)
	{
		return destination.GetGridPosition() - origin.GetGridPosition();
	}

	public bool IsGridCoordinateInsideGrid(Vector3 gridCoordinate)
	{
		return gridCoordinate.x >= 0 && gridCoordinate.x < gridSize && gridCoordinate.y >= 0 && gridCoordinate.y < gridSize;
	}

	public Vector3 GridCoordinatesToWorldPos(Vector3 gridCoordinate)
	{
		return transform.position + new Vector3(gridCoordinate.x - gridSize / 2, gridCoordinate.y - gridSize / 2, 0f) * cellSize;
	}

	#endregion

	#region WORLDGEN
	public void WorldGen()
	{
		WorldCellsInGridInstantiate();
		RunWorldGenerationCoroutine();
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
				Cell tempCell = Instantiate(cellPrefab, GridCoordinatesToWorldPos(new Vector3(row, column)), Quaternion.identity, transform);
				tempCell.gameObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
				tempCell.SetGridPosition(row, column);
				tempCell.gameObject.name = $"Cell_{row}_{column}";
				cells[row, column] = tempCell;
			}
		}

		cells[(int)spawnPointGrid.x, (int)spawnPointGrid.y].canThreatsSpawn = true;

		cellList = cells.Cast<Cell>().ToList();
	}


	public void PlayerSpawn()
	{
		Vector3 cellPosition = GetCellFromGridPosition(spawnPointGrid).transform.position;
		GameManager.Instance.player.transform.position = cellPosition;
		GameManager.Instance.player.transform.localScale = new Vector3(cellSize, cellSize, GameManager.Instance.player.transform.localScale.z);
		GetPlayerCurrentCell().HideFogOfWar();
	}



	public void RunWorldGenerationCoroutine()
	{
		generatedCells = 0;
		Cell returningCell = RecoursiveWorldCellsPathingCreation(cells[10, 10]);
		cellsStack.Push(returningCell);

		StartCoroutine(WorldGenerationCoroutine());
	}

	IEnumerator WorldGenerationCoroutine()
	{
		while (cellsStack.Count() > 0)
		{
			GameManager.Instance.loadingMenu.UpdateLoadingPercentage((float)generatedCells * 100 / (gridSize * gridSize));
			RunNextCell();
			yield return new WaitForSeconds(0);
		}
		Debug.Log($"GENERATION COMPLETED: {generatedCells}/{cellList.Where(x => x.generated).Count()}");

		MonsterSpawn();
		TeleporterSpawn();
		WellSpawn();
		TunnelSpawn();
		UpdateGUI();
	}

	public void RunNextCell()
	{
		Cell currentCell = cellsStack.Pop();
		Cell NextCell = RecoursiveWorldCellsPathingCreation(currentCell);
		if (NextCell != null)
		{
			//generatedCells++;
			cellsStack.Push(currentCell);
			cellsStack.Push(NextCell);
		}
	}

	private Cell RecoursiveWorldCellsPathingCreation(Cell currentCell)
	{
		Cell nextCell = SpawnNewCell(currentCell);
		return nextCell;
	}
	private Cell SpawnNewCell(Cell lastGeneratedCell)
	{
		Cell selectedRandomUngeneratedAdiancentCell = null;
		List<Cell> adiacentCells = GetAdiacentCells(lastGeneratedCell);
		List<Cell> ungeneratedAdiacentCells = adiacentCells.Where(x => !x.generated).ToList();

		//pick random ungenerated adiacent cell and open path with lastGeneratedCell
		if (ungeneratedAdiacentCells.Count > 0)
		{
			selectedRandomUngeneratedAdiancentCell = ungeneratedAdiacentCells[Random.Range(0, ungeneratedAdiacentCells.Count)];
			Vector3 direction = GetRelativePositionBetweenCells(selectedRandomUngeneratedAdiancentCell, lastGeneratedCell);
			selectedRandomUngeneratedAdiancentCell.OpenPath(direction);
			lastGeneratedCell.OpenPath(direction * -1);

			selectedRandomUngeneratedAdiancentCell.generated = true;
			generatedCells++;

			//spawn extra path in cell to another random adiacent cell, doesnt't count in generation
			if (Random.Range(0f, 1f) <= extraPathInCellChance)
			{
				Cell selectedRandomExtraAdiancentCell = adiacentCells[Random.Range(0, adiacentCells.Count)];
				Vector3 direction2 = GetRelativePositionBetweenCells(selectedRandomExtraAdiancentCell, lastGeneratedCell);
				selectedRandomExtraAdiancentCell.OpenPath(direction2);
				lastGeneratedCell.OpenPath(direction2 * -1);
			}
		}

		return selectedRandomUngeneratedAdiancentCell;
	}

	private void MonsterSpawn()
	{
		List<Cell> cellPullList = cellList.Where(x => x.CanMonsterSpawn()).ToList();
		for (int i = 0; i < monsterSpawnCount; i++)
		{
			if (cellPullList.Count == 0)
				break;
			Cell randomizedCell = cellPullList[Random.Range(0, cellPullList.Count())];
			randomizedCell.hasMonster = true;
			cellPullList.Remove(randomizedCell);
		}
	}

	private void TeleporterSpawn()
	{
		List<Cell> cellPullList = cellList.Where(x => x.CanTeleporterSpawn()).ToList();

		for (int i = 0; i < teleporterSpawnCount; i++)
		{
			if (cellPullList.Count == 0)
				break;
			Cell randomizedCell = cellPullList[Random.Range(0, cellPullList.Count())];
			randomizedCell.hasTeleporter = true;
			cellPullList.Remove(randomizedCell);
		}
	}
	private void WellSpawn()
	{
		List<Cell> cellPullList = cellList.Where(x => x.CanWellSpawn()).ToList();

		for (int i = 0; i < wellSpawnCount; i++)
		{
			if (cellPullList.Count == 0)
				break;
			Cell randomizedCell = cellPullList[Random.Range(0, cellPullList.Count())];
			randomizedCell.hasWell = true;
			cellPullList.Remove(randomizedCell);
		}
	}

	private void TunnelSpawn()
	{
		List<Cell> cellPullList = cellList.Where(x => x.CanTunnelSpawn()).ToList();
		//Debug.Log($"AvailableCells for Tunnel Spawn: {availableCells.Count()}");
		for (int i = 0; i < tunnelSpawnCount; i++)
		{
			if (cellPullList.Count == 0)
				break;
			Cell selectedCell = cellPullList[Random.Range(0, cellPullList.Count())];
			selectedCell.SetTunnelCell();
			cellPullList.Remove(selectedCell);
		}
	}

	public void UpdateGUI()
	{
		cellList.ForEach(x => x.ClearEntyty());
		foreach (Cell cell in cellList.Where(x => x.hasMonster).ToList())
		{
			cell.SetEntityGUI(new Color32(255, 50, 50, 255));
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Monster);
			}
		}
		foreach (Cell cell in cellList.Where(x => x.hasTeleporter).ToList())
		{
			cell.SetEntityGUI(new Color32(50, 130, 255, 255));
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Telporter);
			}
		}
		foreach (Cell cell in cellList.Where(x => x.hasWell).ToList())
		{
			cell.SetEntityGUI(new Color32(65, 150, 25, 255));
			foreach (Cell adiacentCell in GetAdiacentCells(cell))
			{
				adiacentCell.IconManager.AddIcon(IconType.Well);
			}
		}
	}

	public void UpdateArrowMove(Arrow arrow)
	{
		Cell currentArrowCell = GetCellFromWorldPosition(arrow.transform.position);

		if (currentArrowCell.IsPathAvailable(arrow.Direction))
		{
			Cell destinationArrowCell = GetAdiancentCell(currentArrowCell, arrow.Direction);
			if (destinationArrowCell != null)
				destinationArrowCell.OnArrowEnter(arrow);
			else
				arrow.DestroyArrow();
		}
		else
			arrow.DestroyArrow();
	}

	public void RepositionMonster()
	{
		if (GameManager.Instance.player.flyingArrowCount == 0 && GameManager.Instance.player.arrowCount == 0)
			GameManager.Instance.player.EndPlayerGame(Player.ResultType.OutOfAmmo);
		foreach (Cell monsterCell in cellList.Where(x => x.hasMonster).ToList())
			monsterCell.hasMonster = false;
		MonsterSpawn();
		UpdateGUI();
	}

	#endregion

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;

		Gizmos.DrawLine(GridCoordinatesToWorldPos(new Vector3(0, 0, 0)), GridCoordinatesToWorldPos(new Vector3(0, gridSize - 1, 0)));
		Gizmos.DrawLine(GridCoordinatesToWorldPos(new Vector3(0, 0, 0)), GridCoordinatesToWorldPos(new Vector3(gridSize - 1, 0, 0)));

		Gizmos.DrawLine(GridCoordinatesToWorldPos(new Vector3(gridSize - 1, gridSize - 1, 0)), GridCoordinatesToWorldPos(new Vector3(0, gridSize - 1, 0)));
		Gizmos.DrawLine(GridCoordinatesToWorldPos(new Vector3(gridSize - 1, gridSize - 1, 0)), GridCoordinatesToWorldPos(new Vector3(gridSize - 1, 0, 0)));
	}
}

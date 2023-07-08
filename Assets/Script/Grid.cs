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

	private int generationDepth = 0;
	private Cell[,] cells;
	private List<Cell> cellList;

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

	void Update()
	{

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
	/// Returns the Cell next to the player in the chosen direction
	/// </summary>
	/// <param name="relativePosition">Normalized Vector indicating the direction to pick the Cell from (player position is the origin).</param>
	/// <returns></returns>
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
	/// Set the Player position to a specific Cell
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
	/// Centralized function to get data from Cell matrix converting Vector3 to Vector3Int
	/// </summary>
	/// <param name="GridPosition"></param>
	/// <returns></returns>
	public Cell GetCellFromGridPosition(Vector3 GridPosition)
	{
		Vector3Int convertedGridPosition = Vector3Int.FloorToInt(GridPosition);
		return cells[convertedGridPosition.x, convertedGridPosition.y];
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
	/// <returns></returns>
	public Cell GetAdiancentCell(Cell currentCell, Vector3 relative)
	{
		Vector3 adiacentCellPosition = currentCell.GetGridPosition() + relative;
		if (adiacentCellPosition.x < 0 || adiacentCellPosition.x >= gridSize || adiacentCellPosition.y >= gridSize || adiacentCellPosition.y < 0)
			return null;
		return cells[(int)adiacentCellPosition.x, (int)adiacentCellPosition.y];
	}

	/// <summary>
	/// Returns all the Cell next to the chosen Cell, no pathing is calculated in this operation
	/// </summary>
	/// <param name="currentCell">Reference Cell</param>
	/// <returns></returns>
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
	///  Returns the direction required to move from origin Cell to destination Cell
	/// </summary>
	/// <param name="origin">Cell to move from</param>
	/// <param name="destination">Cell to move to</param>
	/// <returns></returns>
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

	public Stack<Cell> cellsStack = new Stack<Cell>();
	public int generatedCells;

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
		Debug.Log($"[{generationDepth}] Start recoursiveWorldGen {currentCell.GetGridPosition()}");
		List<Cell> adiacentCells = GetAdiacentCells(currentCell);
		Debug.Log($"[{generationDepth}] adiacentCells count: {adiacentCells.Count}");

		Cell nextCell = SpawnNewCell(currentCell);
		generationDepth++;

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
			selectedRandomUngeneratedAdiancentCell.OpenPath(direction, 1);
			lastGeneratedCell.OpenPath(direction * -1, 1);
			Debug.Log($"[{generationDepth}] Opened new Path between cells {lastGeneratedCell.GetGridPosition()} | {selectedRandomUngeneratedAdiancentCell.GetGridPosition()}");

			selectedRandomUngeneratedAdiancentCell.generated = true;
			generatedCells++;

			//spawn extra path in cell to another random adiacent cell, doesnt't count in generation
			if (Random.Range(0f, 1f) <= extraPathInCellChance)
			{
				Cell selectedRandomExtraAdiancentCell = adiacentCells[Random.Range(0, adiacentCells.Count)];
				Vector3 direction2 = GetRelativePositionBetweenCells(selectedRandomExtraAdiancentCell, lastGeneratedCell);
				selectedRandomExtraAdiancentCell.OpenPath(direction2, 1);
				lastGeneratedCell.OpenPath(direction2 * -1, 1);
			}
		}

		return selectedRandomUngeneratedAdiancentCell;
	}

	private void MonsterSpawn()
	{
		for (int i = 0; i < monsterSpawnCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanMonsterSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanMonsterSpawn()).Count())];
			randomizedCell.hasMonster = true;
			//Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}

	private void TeleporterSpawn()
	{
		for (int i = 0; i < teleporterSpawnCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanTeleporterSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanTeleporterSpawn()).Count())];
			randomizedCell.hasTeleporter = true;
			//Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}
	private void WellSpawn()
	{
		for (int i = 0; i < wellSpawnCount; i++)
		{
			Cell randomizedCell = cellList.Where(x => x.CanWellSpawn()).ToList()[Random.Range(0, cellList.Where(x => x.CanWellSpawn()).Count())];
			randomizedCell.hasWell = true;
			//Debug.Log($"Monster spawned in {randomizedCell.GetGridPosition()}");
		}
	}

	private void TunnelSpawn()
	{
		List<Cell> availableCells = cellList.Where(x => x.CanTunnelSpawn()).ToList();
		//Debug.Log($"AvailableCells for Tunnel Spawn: {availableCells.Count()}");
		for (int i = 0; i < tunnelSpawnCount; i++)
		{
			Cell selectedCell = availableCells[Random.Range(0, availableCells.Where(x => x.CanTunnelSpawn()).Count())];
			selectedCell.SetTunnelCell();
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
		Cell currentArrowCell = GetCellFromWorldPosition(arrow.transform.position);

		if (currentArrowCell.IsPathAvailable(arrow.direction))
		{
			Cell destinationArrowCell = GetAdiancentCell(currentArrowCell, arrow.direction);
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

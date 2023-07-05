using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
	[SerializeField]
	private Tilemap map;
	[SerializeField]
	private Transform player;
	[SerializeField]
	private List<TileData> tileDataList;


	private Dictionary<TileBase, TileData> dataFromTiles;

	private void Awake()
	{
		dataFromTiles = new Dictionary<TileBase, TileData>();

		foreach (TileData tileData in tileDataList)
		{
			foreach (TileBase tile in tileData.tiles)
			{
				dataFromTiles.Add(tile, tileData);
			}
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
			if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position))].topMove)
				if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position + Vector3.up))].bottomMove)
					player.transform.position += Vector3.up;
		if (Input.GetKeyDown(KeyCode.S))
			if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position))].bottomMove)
				if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position + Vector3.down))].topMove)
					player.transform.position += Vector3.down;
		if (Input.GetKeyDown(KeyCode.D))
			if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position))].rightMove)
				if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position + Vector3.right))].leftMove)
					player.transform.position += Vector3.right;
		if (Input.GetKeyDown(KeyCode.A))
			if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position))].leftMove)
				if (dataFromTiles[map.GetTile(map.WorldToCell(player.transform.position + Vector3.left))].rightMove)
					player.transform.position += Vector3.left;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
	public TileBase[] tiles;
	public bool topMove;
	public bool bottomMove;
	public bool rightMove;
	public bool leftMove;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelDataSO :ScriptableObject
{
    public int totalTiles;

    public List<Vector2Int> activeBlocksPosList;

    public List<Vector2Int> obstacleBlocksPosList;
}

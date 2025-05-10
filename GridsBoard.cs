using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridsBoard : MonoBehaviour
{
    public static GridsBoard Instance { get; private set; }

    [SerializeField] private Transform GridPanel;
    [SerializeField] private Sprite[] iconSpriteArray;

    private TileData tileSelected;
    
    private TileData[,] tilesBoardArray;

    // 临时列表
    private List<TileData> pathPointsListTemporary;
    private Dictionary<int, List<TileData>> pathLineTilesDicTemporary;

    // 正式列表
    private List<TileData> pathPointsList;
    private Dictionary<int, List<TileData>> pathLineTilesDic;

    private Dictionary<int,List<TileData>> remainingTilesListDictionary;

    private int xLines;
    private int yLines;
    private int gameLevel;

    private void Awake()
    {
        if (Instance  == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        xLines = GridPanel.GetChild(0).childCount;
        yLines = GridPanel.childCount;

        tilesBoardArray = new TileData[xLines, yLines];

        pathPointsListTemporary = new List<TileData>();
        pathLineTilesDicTemporary = new Dictionary<int, List<TileData>>();

        pathPointsList = new();
        pathLineTilesDic = new();

        remainingTilesListDictionary = new Dictionary<int, List<TileData>>();  
    }



    public void ClearGridPanel()
    {
        for (int y = 0; y < yLines; y++)
        {
            for (int x = 0; x < xLines; x++)
            {
                TileData tile = GridPanel.GetChild(y).GetChild(x).GetComponent<TileData>();
                tile.SetTileData(0, x, y, false, null, false);
                tile.IsSelected(false);
                tilesBoardArray[x, y] = tile;
            }
        }
        remainingTilesListDictionary.Clear();
    }

    
    public void TileSelected(TileData _tileSelect)
    {
        if (tileSelected != null)
        {
            if (tileSelected == _tileSelect)
            {                
                return;
            }
            tileSelected.IsSelected(false);

            if (HasValidPath(tileSelected, _tileSelect))
            {// Tile Match Check Available Path
                TemporaryListToAvaliableList();

                SoundManager.Instance.SelectSound();
                
                ShowAvaliablePath();
                OnTilesMatch(tileSelected, _tileSelect);
                tileSelected = null;
                return;
            }
            //SoundManager.Instance.ErrorSound();
        }

        SoundManager.Instance.SelectSound();
        tileSelected = _tileSelect;
        tileSelected.IsSelected(true);
    }

    // Test 
    public void SetGameMap(int mapNum, LevelDataSO levelData)
    {        
        gameLevel = mapNum;        
        MiniGameController.Instance.ShowGameLevel(mapNum);

        int totalTiles = levelData.totalTiles;
        List<int> tileIDList = new();

        MiniGameController.Instance.SetRemainTile(totalTiles);        

        for (int i = 0; i < (totalTiles / 2); i++)
        {
            int randomID;
            if (i < iconSpriteArray.Count())
            {
                randomID = i;
            }
            else if (i >= (iconSpriteArray.Count() * 2))
            {
                randomID = Random.Range(0, iconSpriteArray.Count());
            }
            else
            {
                randomID = i - iconSpriteArray.Count();
            }
            tileIDList.Add(randomID);
            tileIDList.Add(randomID);
        }        

        tileIDList = tileIDList.OrderBy(x => Random.value).ToList();

        int idIndex = 0;
        foreach (Vector2Int pos in levelData.activeBlocksPosList)
        {
            SetActiveTileOnBlock(pos.x, pos.y, tileIDList[idIndex++]);
        }
        foreach (Vector2Int pos in levelData.obstacleBlocksPosList)
        {
            SetObstacleBlock(pos.x, pos.y);
        }
        //保存可点击方块字典
        UpdateRemainingTileListDic();
        //确认开局不死局
        CheckRemainingTilePairs();
    }

    //检测游戏是否结束
    public void CheckRemainingTilePairs()
    {
        if (remainingTilesListDictionary.Values.Any(list => list.Count > 0))
        {
            if (IsDeadlock())
            {                
                ShuffleRemainingTile();
                return;
            }
            else
            {
                return;
            }

        }

        if (MiniGameController.Instance.IsOnExtraLevel())
        {
            if (gameLevel >= 9)
            {
                MiniGameController.Instance.EndGame("EXTRA COMPLETED!!!");
                return;
            }
            StartCoroutine(MiniGameController.Instance.LoadGameMapCoroutine(gameLevel + 1));
            return;
        }

        Debug.Log("LevelCompleted");

        LevelController.Instance.OnLevelCompleted(gameLevel);

        MiniGameController.Instance.EndGame("GAME CLEAR");
    }

    // 方块成功连接时
    private void OnTilesMatch(TileData tile1, TileData tile2)
    {
        SoundManager.Instance.TileMatchSound();
        tile1.TileMatchEffect();
        tile2.TileMatchEffect();

        remainingTilesListDictionary[tile1.ID].Remove(tile1);
        remainingTilesListDictionary[tile2.ID].Remove(tile2);

        int remainingTiles = remainingTilesListDictionary.Values.Sum(list => list.Count);
        MiniGameController.Instance.SetRemainTile(remainingTiles);
        CheckRemainingTilePairs();
    }

    private void SetObstacleBlock(int _xLine, int _yLine)
    {
        TileData newObstacle = tilesBoardArray[_xLine, _yLine];
        newObstacle.SetTileData(0, _xLine, _yLine, false, null, true);
    }
    private void SetActiveTileOnBlock(int _xLine, int _yLine, int id)
    {
        TileData newTile = tilesBoardArray[_xLine, _yLine];
        newTile.SetTileData(id, _xLine, _yLine, true, iconSpriteArray[id], false);       
    }

    
    private void UpdateRemainingTileListDic()
    {
        remainingTilesListDictionary.Clear();
        for (int x = 0; x < xLines; x++)
        {
            for (int y = 0; y < yLines; y++)
            {
                TileData tile = tilesBoardArray[x, y];
                if (tile!=null && tile.INTERACTBLE)
                {
                    if (!remainingTilesListDictionary.ContainsKey(tile.ID))
                    {
                        remainingTilesListDictionary[tile.ID] = new List<TileData>();
                    }

                    remainingTilesListDictionary[tile.ID].Add(tile);
                }
            }
        }
    }


    //检测死局
    private bool IsDeadlock()
    {        
        foreach (int key in remainingTilesListDictionary.Keys)
        {
            List<TileData> tileList = remainingTilesListDictionary[key];
            for (int i = 0; i < tileList.Count; i++)
            {
                for (int j = i + 1; j < tileList.Count; j++)
                {
                    if (HasValidPath(tileList[i], tileList[j]))
                    {
                        pathPointsListTemporary.Clear();
                        pathLineTilesDicTemporary.Clear();
                        return false;
                    }
                }
            }
        }
        Debug.Log("DeadLock");
        return true;
    }
    //死局重组
    public void ShuffleRemainingTile()
    {//遍历保存剩余Tile的ID和位置
        List<Vector2Int> remainingTilePos = new();
        List<int> remainingTileID = new();
        foreach (List<TileData> tileDataList in remainingTilesListDictionary.Values)
        {
            foreach (TileData tile in tileDataList)
            {                
                remainingTilePos.Add(new Vector2Int(tile.XPOS,tile.YPOS));
                remainingTileID.Add(tile.ID);
            }
        }
        //打乱ID 根据位置重新分配
        remainingTileID = remainingTileID.OrderBy(x => Random.value).ToList();

        int idIndex = 0;
        foreach (Vector2Int pos in remainingTilePos)
        {
            SetActiveTileOnBlock(pos.x, pos.y, remainingTileID[idIndex++]);
        }
        UpdateRemainingTileListDic();
        Debug.Log("Shuffle");
    }

    //路径可视化
    #region
    private void ShowAvaliablePath()
    {
        //Point Effect
        ActivePathCornerPoint(pathPointsList.Count);

        //Path Line Effect   
        ActivePathLineDictionaryWithCorners(pathPointsList.Count);

        pathPointsList.Clear();
        pathLineTilesDic.Clear();
    }
    //将临时Point,Line列表传入显示列表    
    private void TemporaryListToAvaliableList()
    {   //列表交换属性
        pathPointsList.Clear();
        pathPointsList.AddRange(pathPointsListTemporary);

        pathLineTilesDic.Clear();
        foreach (var kvp in pathLineTilesDicTemporary)
        {
            pathLineTilesDic[kvp.Key] = kvp.Value;
        }

        //清空临时列表
        pathPointsListTemporary.Clear();
        pathLineTilesDicTemporary.Clear();
    }
    private void ActivePathCornerPoint(int pointListCount)
    {
        int startTile = 0;
        int endTile = pointListCount - 1;

        if (pointListCount == 2)
        {// tile match effect
            return;
        }

        if (pointListCount >= 3)
        {
            for (int corner = 0; corner <= endTile; corner++)
            {
                if (corner == startTile || corner == endTile)
                {// Tile match effect
                    continue;
                }

                CalculateCornerDiretion(pathPointsList[corner - 1], pathPointsList[corner + 1],
                    pathPointsList[corner]);
            }
        }

    }

    private void CalculateCornerDiretion(TileData first, TileData end, TileData corner)
    {//计算CornerTile并显示
        Vector2 direction = new(0, 0);

        if (first.XPOS == corner.XPOS)
        {
            direction.y = first.YPOS - corner.YPOS;
            direction.x = end.XPOS - corner.XPOS;
        }
        else if (first.YPOS == corner.YPOS)
        {
            direction.x = first.XPOS - corner.XPOS;
            direction.y = end.YPOS - corner.YPOS;
        }

        corner.SetCornerSpriteWithDirection(direction);

    }
    //遍历TileListDictionary显示消除路径（无拐点）
    private void ActivePathLineDictionaryWithCorners(int CornersCount)
    {
        List<TileData> tileList = new List<TileData>();

        for (int i = 0; i < CornersCount - 1; i++)
        {
            if (!pathLineTilesDic.TryGetValue(i, out tileList))
            {//如果目标点与拐点相连，无需连线
                continue;
            }
            if (pathPointsList[i].XPOS == pathPointsList[i + 1].XPOS)
            {
                for (int j = 0; j < tileList.Count; j++)
                {
                    tileList[j].SetPathImageEffect(1);
                }
            }
            else
            {
                for (int j = 0; j < tileList.Count; j++)
                {
                    tileList[j].SetPathImageEffect(0);
                }
            }
        }

    }

    #endregion

    //路径查询
    #region
    private bool HasValidPath(TileData tile1, TileData tile2)
    {
        if (tile1.ID != tile2.ID || !tile1.INTERACTBLE || !tile2.INTERACTBLE)
            return false;

        if (CheckStraightLine(tile1, tile2, 0))
        {
            pathPointsListTemporary.Add(tile1);
            pathPointsListTemporary.Add(tile2);
            return true;
        }

        if (CheckOneCorner(tile1, tile2))
            return true;

        if (CheckTwoCorner(tile1, tile2))
            return true;
        
        return false;
    }

    private bool CheckTwoCorner(TileData start, TileData end)
    {
        for (int x = 0; x < xLines; x++)
        {//水平方向搜索
            TileData mid1 = tilesBoardArray[x, start.YPOS];
            TileData mid2 = tilesBoardArray[x, end.YPOS];

            if (mid1.IsEmpty() && mid2.IsEmpty() &&
                CheckStraightLine(start, mid1,0) &&
                CheckStraightLine(mid1, mid2,1) &&
                CheckStraightLine(mid2, end,2))
            {
                pathPointsListTemporary.AddRange(new[] { start, mid1, mid2, end });
                return true;
            }
            //False则清理路径列表字典
            pathLineTilesDicTemporary.Clear();

        }

        for (int y = 0; y < yLines; y++)
        {//垂直方向搜索
            TileData mid1 = tilesBoardArray[start.XPOS, y];
            TileData mid2 = tilesBoardArray[end.XPOS, y];

            if (mid1.IsEmpty() && mid2.IsEmpty() &&
                CheckStraightLine(start, mid1,0) &&
                CheckStraightLine(mid1, mid2,1) &&
                CheckStraightLine(mid2, end,2))
            {                
                pathPointsListTemporary.AddRange(new[] { start, mid1, mid2, end });
                return true;
            }
            //False则清理路径列表字典
            pathLineTilesDicTemporary.Clear();

        }

        return false;
    }
    private bool CheckOneCorner(TileData tile1, TileData tile2)
    {
        TileData corner1 = tilesBoardArray[tile1.XPOS, tile2.YPOS];
        if (corner1.IsEmpty() &&
            CheckStraightLine(tile1, corner1,0) && 
            CheckStraightLine(corner1, tile2,1))
        {
            pathPointsListTemporary.Add(tile1);
            pathPointsListTemporary.Add(corner1);
            pathPointsListTemporary.Add(tile2);
            return true;
        }

        pathLineTilesDicTemporary.Clear();

        TileData corner2 = tilesBoardArray[tile2.XPOS, tile1.YPOS];
        if (corner2.IsEmpty() && 
            CheckStraightLine(tile1, corner2,0) && 
            CheckStraightLine(corner2, tile2,1))
        {
            pathPointsListTemporary.Add(tile1);
            pathPointsListTemporary.Add(corner2);
            pathPointsListTemporary.Add(tile2);
            return true;
        }
        //False则清理路径列表字典
        pathLineTilesDicTemporary.Clear();
        return false;
    }
    private bool CheckStraightLine(TileData tile1, TileData tile2,int corners)
    {   //临时列表保存可能的连接路线存入字典
        List<TileData> templePathLineList = new List<TileData>();

        //同列检查
        if (tile1.XPOS == tile2.XPOS)
        {
            int minY = Mathf.Min(tile1.YPOS, tile2.YPOS);
            int maxY = Mathf.Max(tile1.YPOS, tile2.YPOS);

            if (maxY - minY == 1)
            {
                return true;
            }

            for (int y = minY + 1; y < maxY; y++)
            {
                if (!tilesBoardArray[tile1.XPOS, y].IsEmpty())
                {                    
                    return false;
                }
                templePathLineList.Add(tilesBoardArray[tile1.XPOS, y]);
            }
            
            //Corner为Key，添加TileList到字典
            pathLineTilesDicTemporary.Add(corners, templePathLineList);
            return true;
        }

        //同行检查
        if (tile1.YPOS == tile2.YPOS)
        {
            int maxX = Mathf.Max(tile1.XPOS, tile2.XPOS);
            int minX = Mathf.Min(tile1.XPOS, tile2.XPOS);

            if (maxX-minX ==1)
            {
                return true;
            }
            for (int x = minX + 1; x < maxX; x++)
            {
                if (!tilesBoardArray[x, tile1.YPOS].IsEmpty())
                {
                    return false;
                }
                templePathLineList.Add(tilesBoardArray[x, tile1.YPOS]);

            }

            //Corner为Key，添加TileList到字典
            pathLineTilesDicTemporary.Add(corners, templePathLineList);
            return true;
        }

        return false;
    }
    #endregion
    
    
}

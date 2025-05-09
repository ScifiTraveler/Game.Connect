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
    
    private TileData[,] tilesArray;
    private List<TileData> pathPointsList;
    private Dictionary<int, List<TileData>> pathLineTilesDictionary;
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

        tilesArray = new TileData[xLines, yLines];
        pathPointsList = new List<TileData>();
        pathLineTilesDictionary = new Dictionary<int, List<TileData>>();
        remainingTilesListDictionary = new Dictionary<int, List<TileData>>();  
    }



    public void ClearGridPanel()
    {
        for (int y = 0; y < yLines; y++)
        {
            for (int x = 0; x < xLines; x++)
            {
                TileData tile = GridPanel.GetChild(y).GetChild(x).GetComponent<TileData>();
                tile.SetTileData(0, x, y, false, null);
                tile.IsSelected(false);
                tilesArray[x, y] = tile;                      
            }
        }
        remainingTilesListDictionary.Clear();
    }

    private void TilesMatch(TileData tile1, TileData tile2)
    {
        SoundManager.Instance.TileMatchSound();
        tile1.TileMatchEffect();
        tile2.TileMatchEffect();
        MiniGameController.Instance.ShowRemainTileText(-2);

        remainingTilesListDictionary[tile1.ID].Remove(tile1);
        remainingTilesListDictionary[tile2.ID].Remove(tile2);

        CheckRemainingTilePairs();
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
                SoundManager.Instance.SelectSound();
                
                ShowAvaliablePath();
                TilesMatch(tileSelected, _tileSelect);
                tileSelected = null;
                return;
            }            
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
        MiniGameController.Instance.ShowRemainTileText(0);

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
            SetRandomTileOnBlock(pos.x, pos.y, tileIDList[idIndex++]);
        }       
    }

    private void SetRandomTileOnBlock(int _xLine, int _yLine, int id)
    {
        TileData newTile = tilesArray[_xLine, _yLine];
        newTile.SetTileData(id, _xLine, _yLine, true, iconSpriteArray[id]);
        AddValidTileOnListDictionary(id, newTile);
    }
    private void AddValidTileOnListDictionary(int ID, TileData newTile)
    {
        if (remainingTilesListDictionary.TryGetValue(ID, out List<TileData> tileList))
        {
            tileList.Add(newTile);
        }
        else
        {
            remainingTilesListDictionary[ID] = new List<TileData> { newTile };
        }
    }

    public void CheckRemainingTilePairs()
    {        
        if (remainingTilesListDictionary.Values.Any(list => list.Count > 0))
        {
            return;
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

    //路径可视化
    #region
    private void ShowAvaliablePath()
    {
        //Point Effect
        ActivePathCornerPoint(pathPointsList.Count);

        //Path Line Effect   
        ActivePathLineDictionaryWithCorners(pathPointsList.Count);   

        pathPointsList.Clear();
        pathLineTilesDictionary.Clear();
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
            if(! pathLineTilesDictionary.TryGetValue(i, out tileList))
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
    private void AddPathLineOnTileListDictionary(List<TileData> tileList, int corners)
    {//Corner为Key，添加TileList到字典
        pathLineTilesDictionary.Add(corners, tileList);
    }

    

    
    #endregion

    //路径查询
    #region
    private bool HasValidPath(TileData tile1, TileData tile2)
    {
        if (tile1.ID != tile2.ID || !tile1.INTERACTBLE || !tile2.INTERACTBLE)
            return false;

        if (CheckStraightLine(tile1, tile2,0))
        {
            pathPointsList.Add(tile1);
            pathPointsList.Add(tile2);
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
            TileData mid1 = tilesArray[x, start.YPOS];
            TileData mid2 = tilesArray[x, end.YPOS];

            if (!mid1.INTERACTBLE && !mid2.INTERACTBLE &&
                CheckStraightLine(start, mid1,0) &&
                CheckStraightLine(mid1, mid2,1) &&
                CheckStraightLine(mid2, end,2))
            {
                pathPointsList.Add(start);
                pathPointsList.Add(mid1);
                pathPointsList.Add(mid2);
                pathPointsList.Add(end);
                return true;
            }
            //False则清理路径列表字典
            pathLineTilesDictionary.Clear();

        }

        for (int y = 0; y < yLines; y++)
        {//垂直方向搜索
            TileData mid1 = tilesArray[start.XPOS, y];
            TileData mid2 = tilesArray[end.XPOS, y];

            if (!mid1.INTERACTBLE && !mid2.INTERACTBLE &&
                CheckStraightLine(start, mid1,0) &&
                CheckStraightLine(mid1, mid2,1) &&
                CheckStraightLine(mid2, end,2))
            {
                pathPointsList.Add(start);
                pathPointsList.Add(mid1);
                pathPointsList.Add(mid2);
                pathPointsList.Add(end);
                return true;
            }
            //False则清理路径列表字典
            pathLineTilesDictionary.Clear();

        }

        return false;
    }
    private bool CheckOneCorner(TileData tile1, TileData tile2)
    {
        TileData corner1 = tilesArray[tile1.XPOS, tile2.YPOS];
        if (!corner1.INTERACTBLE &&
            CheckStraightLine(tile1, corner1,0) && 
            CheckStraightLine(corner1, tile2,1))
        {
            pathPointsList.Add(tile1);
            pathPointsList.Add(corner1);
            pathPointsList.Add(tile2);
            return true;
        }

        pathLineTilesDictionary.Clear();

        TileData corner2 = tilesArray[tile2.XPOS, tile1.YPOS];
        if (!corner2.INTERACTBLE && 
            CheckStraightLine(tile1, corner2,0) && 
            CheckStraightLine(corner2, tile2,1))
        {
            pathPointsList.Add(tile1);
            pathPointsList.Add(corner2);
            pathPointsList.Add(tile2);
            return true;
        }
        //False则清理路径列表字典
        pathLineTilesDictionary.Clear();
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
                if (tilesArray[tile1.XPOS, y].INTERACTBLE)
                {                    
                    return false;
                }
                templePathLineList.Add(tilesArray[tile1.XPOS, y]);
            }

            AddPathLineOnTileListDictionary(templePathLineList, corners);
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
                if (tilesArray[x, tile1.YPOS].INTERACTBLE)
                {
                    return false;
                }
                templePathLineList.Add(tilesArray[x, tile1.YPOS]);

            }

            AddPathLineOnTileListDictionary(templePathLineList, corners);
            return true;
        }

        return false;
    }
    #endregion
    
    
}

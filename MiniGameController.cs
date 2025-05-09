using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameController : MonoBehaviour
{
    public static MiniGameController Instance { get; private set; }

    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject settingPanel;
    
    [SerializeField] private Text endGameTxt; 
    [SerializeField] private Text levelTxt; 
    [SerializeField] private Text remainTileTxt;
    [SerializeField] private Text countdownTxt;

    [SerializeField] private List<LevelDataSO> levelDatasList;
   
    private int remainTile;
    private int extraLevelTime = 600;
    private float timer;
    
    private bool onExtraLevel;
    private bool onNormalLevel;
    private bool isPaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        IniciaScene();
    }

    private void Update()
    {
        if (onExtraLevel && !isPaused)
        {            
            timer -= Time.deltaTime;
            CountDownTimeUIUpdate(timer);
            if (timer <= 0)
            {
                EndGame("Challenge Faliled");
            }
        }

        if (onNormalLevel && !isPaused)
        {
            timer += Time.deltaTime;
            CountDownTimeUIUpdate(timer);
        }
    }

    // Change Scenes
    public void IniciaScene()
    {
        CanvasFadeIn(startPanel);

        gamePanel.transform.localScale = Vector3.zero;
        endPanel.transform.localScale = Vector3.zero;
        levelPanel.transform.localScale = Vector3.zero;
        settingPanel.transform.localScale = Vector3.zero;
        Debug.Log("Hide:GamePanel EndPanel LevelPanel");
    }
    public void SelectLevelScene()
    {
        CanvasFadeOut(startPanel);
        CanvasFadeOut(endPanel);
        CanvasFadeOut(gamePanel);

        CanvasFadeIn(levelPanel);
        LevelController.Instance.CheckLevelBtns();
    }
    public void EndGame(string context)
    {
        CanvasFadeIn(endPanel);
        endGameTxt.text = context;
        onExtraLevel = false;
        onNormalLevel = false;
    }


    // ChangeUI
    public int GetLevelListCount()
    {
        return levelDatasList.Count;
    }
    public void ShowGameLevel(int gameLevel)
    {
        StartCoroutine(LevelTextEffectCoroutine("LEVEL " + (gameLevel + 1)));
    }
    public void SetRemainTile(int Tilesnum)
    {
        remainTile = Tilesnum;
    }
    public void ShowRemainTileText(int value)
    {
        remainTileTxt.text = (remainTile += value).ToString();
    }
    private void CountDownTimeUIUpdate(float time)
    {
        time = Mathf.Max(0, time); // 防止负数,取较大值
        int minutes = Mathf.FloorToInt(time / 60f); //向下取整为整数，去掉小数部分（比如 9.9 → 9）
        int seconds = Mathf.FloorToInt(time % 60f);

        string formatted = string.Format("{0:00}:{1:00}", minutes, seconds); //{0:00}表示第{0}个参数始终显示两位数
        countdownTxt.text = formatted;
    }

    // Get Variable
    public bool IsOnExtraLevel()
    {
        return onExtraLevel;
    }

    // Setting Panel Effect
    public void ActiveSettingPanel(bool active)
    {
        if (active)
        {
            settingPanel.transform.DOScale(Vector3Int.one, .5f).SetEase(Ease.OutBack);
            isPaused = true;
        }
        else
        {
            settingPanel.transform.DOScale(Vector3Int.zero, .5f).SetEase(Ease.OutBack);
            isPaused = false;
        }
    }


    // Coroutines
    public IEnumerator LoadGameMapCoroutine(int mapNum)
    {
        GridsBoard.Instance.ClearGridPanel();
        CanvasFadeOut(levelPanel);
        yield return new WaitForSeconds(1f);
        CanvasFadeIn(gamePanel);

        if (mapNum == 10)
        {
            onExtraLevel = true;
            timer = extraLevelTime;
            mapNum = 0;
        }

        GridsBoard.Instance.SetGameMap(mapNum, levelDatasList[mapNum]);

        if (onExtraLevel)
        {
            yield break;
        }
        timer = 0;
        onNormalLevel = true;
    }

    private IEnumerator LevelTextEffectCoroutine(string context)
    {
        levelTxt.enabled = true;
        levelTxt.text = context;
        yield return new WaitForSeconds(.8f);

        levelTxt.GetComponent<CanvasGroup>().DOFade(0, 1f);
        yield return new WaitForSeconds(1f);
        levelTxt.enabled = false;
        levelTxt.GetComponent<CanvasGroup>().alpha = 1;        
    }
    

    // Fade Methoods
    private void CanvasFadeIn(GameObject gameObj)
    {
        gameObj.transform.localScale = Vector3.one;
        gameObj.GetComponent<CanvasGroup>().DOFade(1, 1);        
        Debug.Log("Show" + gameObj.name);
    }
    private void CanvasFadeOut(GameObject gameObj)
    {
        Debug.Log("Hide" + gameObj.name);
        gameObj.GetComponent<CanvasGroup>().DOFade(0, 1).OnComplete(() =>
        {
            gameObj.transform.localScale = Vector3.zero;
        });
    }

}

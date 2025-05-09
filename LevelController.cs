using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    [SerializeField] private Button[] levelButtonsArray;
    [SerializeField] private Button extraButton;

    private int unlokedLevel;
    private string unlockLevelKey = "UnLockedLevel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        CheckLevelBtns();
    }

    public void CheckLevelBtns()
    {
        unlokedLevel = PlayerPrefs.GetInt(unlockLevelKey, 0); //默认解锁第一关

        for (int i = 0; i < levelButtonsArray.Length; i++)
        {
            if (i <= unlokedLevel)
            {
                SetLevelBtnUnlock(levelButtonsArray[i]);
            }
            else
            {
                SetBtnLock(levelButtonsArray[i]);
            }
        }

        if (unlokedLevel > 9)
        {
            extraButton.transform.localScale = Vector3.one;
        }
        else
        {
            extraButton.transform.localScale = Vector3.zero;
        }
    }

    public void OnLevelCompleted(int completedLevel)
    {
        int currentUnlocked = PlayerPrefs.GetInt(unlockLevelKey, 0);
        if (completedLevel >= currentUnlocked && completedLevel <= 9)
        {
            PlayerPrefs.SetInt(unlockLevelKey, completedLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public void LoadLevel(int level)
    {        
        StartCoroutine(MiniGameController.Instance.LoadGameMapCoroutine(level));
    }

    // Test
    public void ClearLeveL()
    {
        PlayerPrefs.DeleteKey(unlockLevelKey);
    }


    private void SetLevelBtnUnlock(Button levelBtn)
    {
        levelBtn.interactable = true;
        levelBtn.transform.GetChild(0).gameObject.SetActive(false);
        levelBtn.transform.GetChild(1).gameObject.SetActive(true);
                
    }
    private void SetBtnLock(Button levelBtn)
    {
        levelBtn.interactable = false;
        levelBtn.transform.GetChild(0).gameObject.SetActive(true);
        levelBtn.transform.GetChild(1).gameObject.SetActive(false);
    }
    
    
}

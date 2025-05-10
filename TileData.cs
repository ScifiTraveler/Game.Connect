using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileData : MonoBehaviour
{
    [SerializeField] private GameObject interactBtn;
    [SerializeField] private GameObject selectMask;
    [SerializeField] private GameObject isObstacle;
    [SerializeField] private Image tilePathImg;
    [SerializeField] private Sprite[] pathLineSprits;
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup interactCanvasGroup;
    [SerializeField] private CanvasGroup pathLineCanvasGroup;

    private int id;
    private int xPos;
    private int yPos;
    private bool interactable;
    private bool obstacle;
   
    private Coroutine pathLineEffectCoroutine;

    public int ID { get { return id; } set { id = value; } }
    public int XPOS { get { return xPos; } set { xPos = value; } }
    public int YPOS { get { return yPos; } set { yPos = value; } }
    
    public bool INTERACTBLE
    {
        get { return interactable; }
        set
        {
            interactable = value;
            interactBtn.SetActive(value);            
        }
    }
    public bool OBSTACLE
    {
        get { return obstacle; }
        set
        {
            obstacle = value;
            isObstacle.SetActive(value);
        }
    }

    public void SetTileData(int _id, int _xPos, int _yPos, bool _interacatble,
        Sprite _icon,bool _obstacle)
    {
        ID = _id;
        XPOS = _xPos;
        YPOS = _yPos;
        INTERACTBLE = _interacatble;
        OBSTACLE = _obstacle;
       
        iconImage.sprite = _icon;
    }
    
    public bool IsEmpty()
    {
        return !INTERACTBLE && !OBSTACLE;
    }
    public void OnClick()
    {        
        GridsBoard.Instance.TileSelected(this);
    }

    public void IsSelected(bool isSelect)
    {
        selectMask.SetActive(isSelect);
    }

    public void SetCornerSpriteWithDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            if (direction.y > 0)
            {
                SetPathImageEffect(4);
            }
            else if (direction.y < 0)
            {
                SetPathImageEffect(2);
            }
        }
        else if (direction.x < 0)
        {
            if (direction.y > 0)
            {
                SetPathImageEffect(5);
            }
            else if (direction.y < 0)
            {
                SetPathImageEffect(3);
            }
        }
    }
    public void SetPathImageEffect(int spritePos)
    {
        tilePathImg.enabled = true;
        tilePathImg.sprite = pathLineSprits[spritePos];

        pathLineCanvasGroup.DOKill();
        pathLineCanvasGroup.alpha = 1;

        if (pathLineEffectCoroutine != null)
        {
            StopCoroutine(pathLineEffectCoroutine);
        }
        pathLineEffectCoroutine = StartCoroutine(PathImgEffectCorout());
    }
    public void TileMatchEffect()
    {
        StartCoroutine(ImageFadeOut());
    }

    private IEnumerator ImageFadeOut()
    {
        interactCanvasGroup.DOFade(0f, .8f);
        yield return new WaitForSeconds(.8f);
        interactCanvasGroup.alpha = 1;        
        INTERACTBLE = false;
        
    }
    private IEnumerator PathImgEffectCorout()
    {  
        yield return pathLineCanvasGroup.DOFade(0f, .8f).WaitForCompletion();     

        tilePathImg.enabled = false;
        
    }
}

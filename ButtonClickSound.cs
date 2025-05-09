using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [SerializeField] private Button[] buttons;

    private void Start()
    {    
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => ClickSound());
        }
    }
    private void ClickSound()
    {
        if (audioSource != null || clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}

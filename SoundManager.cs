using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Dropdown musicDropdown;

    [SerializeField] private AudioSource soundEffectAudioSource;
    [SerializeField] private AudioSource backGroudMusicAudioSource;

    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip[] musicArray;


    private string backgroudMusicKey = "BackgroundMusic";
    private string volumeKey = "Volume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // 从 PlayerPrefs 中加载音量设置
        float savedVolume = PlayerPrefs.GetFloat(volumeKey, .5f); // 默认音量为.5
        volumeSlider.value = savedVolume;

        // 从 PlayerPrefs 中加载背景音乐设置
        int savedMusicIndex = PlayerPrefs.GetInt(backgroudMusicKey, 0); // 默认播放第一个音乐
        musicDropdown.value = savedMusicIndex;

        // 添加监听器
        musicDropdown.onValueChanged.AddListener(OnMusicChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // 播放默认背景音乐
        backGroudMusicAudioSource.clip = musicArray[savedMusicIndex];
        backGroudMusicAudioSource.Play();
    }
    
    public void SelectSound()
    {
        soundEffectAudioSource.PlayOneShot(selectSound);
    }
    public void TileMatchSound()
    {
        soundEffectAudioSource.PlayOneShot(matchSound);
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;

        PlayerPrefs.SetFloat(volumeKey, value);
    }
    private void OnMusicChanged(int index)
    {
        backGroudMusicAudioSource.clip = musicArray[index];
        backGroudMusicAudioSource.Play();

        PlayerPrefs.SetInt(backgroudMusicKey, index);
    }
}

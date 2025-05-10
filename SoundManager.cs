using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private Slider SoundEffectSlider;
    [SerializeField] private Slider BGMVolumeSlider;
    [SerializeField] private Dropdown musicDropdown;

    [SerializeField] private AudioSource soundEffectAudioSource;
    [SerializeField] private AudioSource backGroudMusicAudioSource;

    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip[] musicArray;


    private string backgroudMusicKey = "BackgroundMusic";
    private string soundEffectKey = "SoundEffect";
    private string BGMVolumeKey = "BGMVolume";

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
        float soundEffectVolume = PlayerPrefs.GetFloat(soundEffectKey, .5f); // 默认音量为.5
        SoundEffectSlider.value = soundEffectVolume;
        float bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, .5f);
        BGMVolumeSlider.value = bgmVolume;

        // 从 PlayerPrefs 中加载背景音乐设置
        int savedMusicIndex = PlayerPrefs.GetInt(backgroudMusicKey, 0); // 默认播放第一个音乐
        musicDropdown.value = savedMusicIndex;

        // 添加监听器
        musicDropdown.onValueChanged.AddListener(OnMusicChanged);
        SoundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
        BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

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
    public void ErrorSound()
    {
        soundEffectAudioSource.PlayOneShot(errorSound);
    }

    private void OnBGMVolumeChanged(float value)
    {
        backGroudMusicAudioSource.volume = value;

        PlayerPrefs.SetFloat(BGMVolumeKey, value);
    }
    private void OnSoundEffectVolumeChanged(float value)
    {
        soundEffectAudioSource.volume = value;

        PlayerPrefs.SetFloat(soundEffectKey, value);
    }
    private void OnMusicChanged(int index)
    {
        backGroudMusicAudioSource.clip = musicArray[index];
        backGroudMusicAudioSource.Play();

        PlayerPrefs.SetInt(backgroudMusicKey, index);
    }
}

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
        // �� PlayerPrefs �м�����������
        float soundEffectVolume = PlayerPrefs.GetFloat(soundEffectKey, .5f); // Ĭ������Ϊ.5
        SoundEffectSlider.value = soundEffectVolume;
        float bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, .5f);
        BGMVolumeSlider.value = bgmVolume;

        // �� PlayerPrefs �м��ر�����������
        int savedMusicIndex = PlayerPrefs.GetInt(backgroudMusicKey, 0); // Ĭ�ϲ��ŵ�һ������
        musicDropdown.value = savedMusicIndex;

        // ��Ӽ�����
        musicDropdown.onValueChanged.AddListener(OnMusicChanged);
        SoundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
        BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        // ����Ĭ�ϱ�������
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

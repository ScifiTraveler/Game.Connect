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
        // �� PlayerPrefs �м�����������
        float savedVolume = PlayerPrefs.GetFloat(volumeKey, .5f); // Ĭ������Ϊ.5
        volumeSlider.value = savedVolume;

        // �� PlayerPrefs �м��ر�����������
        int savedMusicIndex = PlayerPrefs.GetInt(backgroudMusicKey, 0); // Ĭ�ϲ��ŵ�һ������
        musicDropdown.value = savedMusicIndex;

        // ��Ӽ�����
        musicDropdown.onValueChanged.AddListener(OnMusicChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

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

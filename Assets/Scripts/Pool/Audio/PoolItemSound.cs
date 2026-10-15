using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZZZ.SoundAssets;



public enum AudioType
{
    ATK,
    BLOCK,
    HIT,
    WALK,
    RUN,
    DASH
}

public class PoolItemSound : PoolItembase
{
    private AudioSource audioSource;
    [SerializeField] private AudioType audioType;
    [SerializeField] private AudioSoundSO audioSoundSO;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();  //获取ATK池子的音频播放器
    }
    public override void Spawn()    //声音一激活，那么就开始播放
    {
        PlayAudio();
    }

    private void PlayAudio()
    {
        var sound = audioSoundSO;
        audioSource.clip = audioSoundSO.GetAudioClips(audioType);   //让音频播放器获得声音的片段
        audioSource.Play();
        StartRecycle();
    }


    //回收的时间
    private void StartRecycle()
    {
        GameTimerManager.MainInstance.TryGetOneTimer(0.3f, DisableSelf);
    }


    private void DisableSelf()
    {
        audioSource.Stop();
        this.gameObject.SetActive(false);
    }
}

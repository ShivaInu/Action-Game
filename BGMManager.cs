//ゲームのBGMをシーンによって切り替えるスクリプト。
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance = null;
    public AudioSource bgmPlayer;
    public AudioClip mainMenuAudio,level1Audio,bossAudio,creditsAudio;

    void Start()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        switch(scene.name){
            case "MainMenu":
                bgmPlayer.clip = mainMenuAudio;
                bgmPlayer.Play();
                break;
            case "Level1" or "Level2" or "Level3":
                if(bgmPlayer.clip!=level1Audio){
                bgmPlayer.clip = level1Audio;
                bgmPlayer.Play();
                }
                break;
            case "Level4":
                bgmPlayer.clip = bossAudio;
                bgmPlayer.Play();
                break;
            case "Ending1":
                bgmPlayer.clip = null;
                break;
            case "Credits":
                bgmPlayer.clip = creditsAudio;
                bgmPlayer.Play();
                break;
        }
    }
}

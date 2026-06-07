//プレイヤーの効果音を管理するクラス。
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public AudioClip attack_l;
    public AudioClip attack_m;
    public AudioClip attack_h;
    public AudioClip attack1;
    public AudioClip attack2;
    public AudioClip attack3;
    public AudioClip hit_l;
    public AudioClip hit_h;
    public AudioClip landing;
    public AudioClip dodgeSuccess;
    public AudioClip backdashSound;
    public AudioSource playerSounds;
    public AudioSource hitSounds; //か、オーディオチャネル2
    public AudioSource voiceSounds;
    public void PlayEnemyHurtSound(AudioClip clip)
    {
        hitSounds.clip = clip;
        hitSounds.Play();
    }
    public void PlayPlayerSound(AudioClip clip)
    {
        playerSounds.clip = clip;
        playerSounds.Play();
    }
    public void PlayVoiceSound(AudioClip clip)
    {
        voiceSounds.clip = clip;
        voiceSounds.Play();
    }
    public void playLight()
    {
        playerSounds.clip = attack_l;
        playerSounds.Play();
    }
    public void playMedium()
    {
        playerSounds.clip = attack_m;
        playerSounds.Play();
    }
    public void playHeavy()
    {
        playerSounds.clip = attack_h;
        playerSounds.Play();
    }

    public void playAttack1()
    {
        voiceSounds.clip = attack1;
        voiceSounds.Play();
    }

    public void playAttack2()
    {
        voiceSounds.clip = attack2;
        voiceSounds.Play();
    }

    public void playAttack3()
    {
        voiceSounds.clip = attack3;
        voiceSounds.Play();
    }
    public void playLanding()
    {
        playerSounds.clip = landing;
        playerSounds.Play();
    }
}

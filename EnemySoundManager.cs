/*
敵が出す音を関するスクリプト。
*/
using UnityEngine;

public class EnemySoundManager : MonoBehaviour
{
    public AudioClip walk;
    public AudioClip hitSound;
    public AudioClip damageSound_l;
    public AudioClip damageSound_h;
    public AudioSource enemySoundSource;
    public AudioSource playerHitSoundSource;
    public AudioSource enemyHurtSoundSource;

    public void playClip(AudioClip clip)
    {
        enemySoundSource.clip = clip;
        enemySoundSource.Play();
    }

    public void playHurtSound(AudioClip clip)
    {
        enemyHurtSoundSource.clip = clip;
        enemyHurtSoundSource.Play();
    }
    public void playWalk()
    {
        playClip(walk);
    }

    public void playDamageSoundL()
    {
        playClip(damageSound_l);
    }

    public void playDamageSoundH()
    {
        playClip(damageSound_h);
    }

    public void playHitSound()
    {
        playerHitSoundSource.clip = hitSound;
        playerHitSoundSource.Play();
    }
}

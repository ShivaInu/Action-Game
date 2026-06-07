//中ボスの効果音を管理するクラス。
using UnityEngine;

public class NightborneSoundManager : EnemySoundManager
{
    public AudioClip slashSound;
    public AudioClip deathSound;

    public void playSlashSound()
    {
        playClip(slashSound);
    }

    public void playDeathSound()
    {
        playClip(deathSound);
    }

}

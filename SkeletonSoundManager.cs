//骸骨の効果音を管理するクラス。
using UnityEngine;

public class SkeletonSoundManager : EnemySoundManager
{
    public AudioClip slashSound;
    public AudioClip blockSound;

    public void playSlashSound()
    {
        playClip(slashSound);
    }

    public void playBlockSound()
    {
        playClip(blockSound);
    }

}

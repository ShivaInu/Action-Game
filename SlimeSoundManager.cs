//スライムの効果音を管理するクラス。
using UnityEngine;

public class SlimeSoundManager : EnemySoundManager
{
    public AudioClip crashSound;
    public AudioClip spinSound;
    public AudioClip chompSound;
    public AudioClip jumpSound;
    public AudioClip diveSound;

    public void PlaySpin()
    {
        playClip(spinSound);
    }

    public void PlayCrash()
    {
        playClip(crashSound);
    }

    public void PlayChomp()
    {
        playClip(chompSound);
    }

    public void PlayDive()
    {
        playClip(diveSound);
    }

    public void PlayJump()
    {
        playClip(jumpSound);
    }
}

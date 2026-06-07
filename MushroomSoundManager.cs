//キノコの敵の効果音を管理するクラス。
using UnityEngine;

public class MushroomSoundManager : EnemySoundManager
{
    public AudioClip walk2;
    public AudioClip uku;
    public AudioClip slam;

    public void playWalk2()
    {
        playClip(walk2);
    }

    public void playUku()
    {
        playClip(uku);
    }

    public void playSlam()
    {
        playClip(slam);
    }
}

//ヒットストップを行うクラス。
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class HitStopper : MonoBehaviour
{
    /*
    ヒットストップを開始するメソッド。
    duration - ヒットストップの長さ(秒単位)
    timeScale - 時間の流れのスピードを何パーセントにする ([0.0,1.0])
    */
    public void Execute(float duration, float timeScale)
    {
        StartCoroutine(DoHitstop(duration,timeScale));
    }

    IEnumerator DoHitstop(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }
}

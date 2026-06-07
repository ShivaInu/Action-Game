/*
各シーン遷移の演出を管理するスクリプト。フェードイン・フェードアウト用のイメージに付ける。
*/
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class FadeManager : MonoBehaviour
{
    [SerializeField] private float fadeInDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FadeIn(fadeInDuration);
    }


    public void FadeOut(float duration,string nextScene)
    {
            Color tmp = GetComponent<Image>().color;
            tmp.a = 1.0f;
            GetComponent<Image>().DOColor(tmp,duration).OnComplete(()=>
            {
                if (nextScene!=null)
                {
                    GameManager.instance.ChangeSceneTo(nextScene);
                }
            });
            
    }

    public void FadeIn(float duration)
    {
            Color tmp = GetComponent<Image>().color;
            tmp.a = 0.0f;
            GetComponent<Image>().DOColor(tmp,duration);
    }

}

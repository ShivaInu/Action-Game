/*
プレイヤーのHPバーのアニメーション処理を行うスクリプト。
HPバーの要素が三つ：
1.黒色の背景部分
2.赤色の真ん中にある部分
3.緑色の一番上の部分
*/
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthImage;
    [SerializeField] private Image redBar;
    public float duration;
    public float currentProportion;
    void Awake()
    {
        currentProportion = 1.0f;
    }
    /*HPに変更があった時の演出を担当するメソッド。
    引数：targetProportion - このメソッドの処理が終わった時のHPの割合。
    */
    public void UpdateHealthBar(float targetProportion)
    {
        healthImage.DOFillAmount(targetProportion,duration).OnComplete(() =>
        {
           redBar.DOFillAmount(targetProportion,duration*0.5f); 
        });
        currentProportion = targetProportion;
    }
}

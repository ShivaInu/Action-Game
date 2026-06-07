/*
敵のHPバーのアニメーション処理を行うスクリプト。
HPバーの要素が三つ：
1.黒色の背景部分
2.赤色の真ん中にある部分
3.緑色の一番上の部分
*/
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HealthBar : MonoBehaviour
{
    
    [SerializeField,Header("緑色の部分")] 
    private Image healthImage;
    [SerializeField,Header("赤色の部分")] 
    private Image redBar;
    [SerializeField,Header("赤色の部分がなくなるまでの時間")]
    public float duration;
    [SerializeField,Header("最大HPと現在HPの割合")]
    public float currentProportion;
    [SerializeField,Header("シーンのメインカメラ")] 
    private Transform cameraTransform;

    void Awake()
    {
        currentProportion = 1.0f;
        cameraTransform = GameObject.FindWithTag("MainCamera").transform;
    }
    //敵の向きがどうであろうとHPバーの表記が変わらない。
    void Update()
    {
        transform.rotation = cameraTransform.rotation;
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

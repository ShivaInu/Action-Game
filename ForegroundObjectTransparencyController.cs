/*
前景にある物を透明化するスクリプト。視界の邪魔になりそうな前景の物にこれとコライダーを付ける。
*/
using UnityEngine;
using DG.Tweening;

public class ForegroundObjectTransparencyController : MonoBehaviour
{
    private int touchCount=0;

    //プレイヤーか敵が前景の物の後ろに入ったら透明にする。
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            Color tmp = GetComponent<SpriteRenderer>().color;
            tmp.a = 0.5f;
            GetComponent<SpriteRenderer>().DOColor(tmp,0.5f);
            touchCount++;
        }
    }

    //プレイヤーも敵も後ろにいない場合、普通のアルファに戻す。
    public void OnTriggerExit2D(Collider2D other)
    {
        
        if(other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            touchCount--;
            if(touchCount == 0)
            {
                Color tmp = GetComponent<SpriteRenderer>().color;
                tmp.a = 1.0f;
                GetComponent<SpriteRenderer>().DOColor(tmp,0.5f);
            }
        }
    }
}

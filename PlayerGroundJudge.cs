//プレイヤーが地面に接しているかどうかを判断し、適切な処理を行うクラス。
using UnityEngine;
using System.Collections;
public class PlayerGroundJudge : MonoBehaviour
{
    [SerializeField] private PlayerSousa player;
    [SerializeField] private float coyoteTimeDuration;
    [SerializeField] private bool waitingOnCoyoteTime;
    [SerializeField] private Coroutine coyoteTimer;

    void Awake()
    {
        waitingOnCoyoteTime = false;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Floor")
        {
            if(waitingOnCoyoteTime){
            StopCoroutine(coyoteTimer);
            }
            player.midAir = false;
            player.airComboEnd = false;
            player.rb.gravityScale = player.defaultGrav;
        }
    }
    //地面から離れた時の処理
    public void OnTriggerExit2D(Collider2D other)
    {
        
        if(other.gameObject.tag == "Floor")
        {
                coyoteTimer = StartCoroutine(CoyoteTime());
        }
    }
    //地面から離れているのにまだジャンプ入力を受け付ける猶予期間
    IEnumerator CoyoteTime()
    {
        waitingOnCoyoteTime = true;
        yield return new WaitForSeconds(coyoteTimeDuration);

        waitingOnCoyoteTime = false;
        player.midAir = true;
    }
}

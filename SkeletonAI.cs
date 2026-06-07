//骸骨の行動パターンを変える子クラス。骸骨の一番大きい特徴は、正面からのダメージを全部防御する事。
using UnityEngine;
using System.Collections;
public class SkeletonAI : EnemyAI
{
    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        StartCoroutine(ComeToLife());
        staggerState = false;
        isFacingRight = false;
        isAttacking = false;
    }

    //主に中ボスの骸骨を召喚する時用のアニメーションを再生する
    public IEnumerator ComeToLife()
    {
        currentState = State.Dying; //このステートにしておいたら動かない
        animator.Play("Ressurection");
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        currentState = State.Idle;
    }
    //ダメージが正面から来ているかどうかを判断するメソッド。
    public override bool BlockJudge()
    {
        return (isFacingRight && (transform.position.x - player.position.x < 0)) || 
        (!isFacingRight && (transform.position.x - player.position.x > 0));
    }

    //防御するアニメーションを再生するメソッド。
    public override void BlockStart()
    {
        if (isAttacking)
            {
                StopCoroutine(exclusiveAction);
            }
        exclusiveAction = StartCoroutine(Block());
    }
    //防御ルーチン
    public IEnumerator Block()
    {
        isAttacking = true;
        animator.Play("Block");
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        isAttacking = false;
        ChangeState(State.Chase);
    }
}

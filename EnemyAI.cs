//敵の行動を司るステートマシンを定義するクラス。これの子クラスを作る事で特徴的な行動ルーチンを作れる。
using UnityEngine;
using System.Collections;
using DG.Tweening;
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Hurt, Dying, Midair}
    public State currentState = State.Chase;

    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float detectRange = 5f;
    [SerializeField] private float pauseTime;
    public bool isAttacking;
    public bool staggerState;
    protected Transform player;
    protected Coroutine exclusiveAction;
    public Animator animator;
    public bool isFacingRight;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        currentState = State.Idle;
        staggerState = false;
        isFacingRight = false;
        isAttacking = false;
    }

    void FixedUpdate()
    {
        if(!isAttacking){
        switch (currentState) //現在の状態に基づいて行動を決める
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Hurt: UpdateHurt(); break;
            case State.Dying: break;
            case State.Midair: break;
        }
        }
    }
    //待機状態のアップデート。
    public virtual void UpdateIdle()
    {
        //プレイヤーが近ければ追いかける
        if(DistanceToPlayer() < detectRange)
        {
            ChangeState(State.Chase);
        }
    }
    //追跡状態のアップデート。
    public virtual void UpdateChase()
    {
        FacePlayer();
        //プレイヤーが攻撃範囲なら攻撃
        if(DistanceToPlayer() < attackRange)
        {
            ChangeState(State.Attack);
            return;
        }
        //プレイヤーを追いかけ続ける
        float step = moveSpeed * Time.deltaTime;
        Vector3 pathToPlayer = Vector3.MoveTowards(transform.position,player.position,step);
        pathToPlayer.y = transform.position.y;
        transform.position = pathToPlayer;
    }

    //プレイヤーの方向に向く。
    protected void FacePlayer()
    {
        float xdist = transform.position.x - player.position.x;
        bool shouldFaceRight = xdist <= 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0,180,0);
        }
    }
    //攻撃状態のアップデート。
    protected void UpdateAttack()
    {
        //プレイヤーが攻撃範囲から抜き出したならまた追いかける
        if (DistanceToPlayer() > attackRange)
        {
            ChangeState(State.Chase);
        }
        //プレイヤーがまだ攻撃範囲内なら攻撃し続ける
        else
        {
            FacePlayer();
            ChangeState(State.Attack);
        }
    }
    //怯み状態のアップデート。
    protected IEnumerator UpdateHurt()
    {
        staggerState = true;
        animator.Play("Hurt",0,0.0f);
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength + 0.5f);
        staggerState = false;
        ChangeState(State.Chase);
    }
    //状態を変える時の処理を管理するメソッド。
    protected virtual void ChangeState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
                case State.Idle: animator.Play("Idle"); break;
                case State.Chase: animator.Play("Run"); break;
                case State.Attack: 
                    exclusiveAction = StartCoroutine(Attack());
                    break;
                case State.Hurt:
                if (staggerState || isAttacking)
                {
                    isAttacking = false;
                    StopCoroutine(exclusiveAction); //怯みアニメーションか攻撃アニメーション中ならダメージ受けたらリセット
                    exclusiveAction = StartCoroutine(UpdateHurt());
                }
                else
                {
                exclusiveAction = StartCoroutine(UpdateHurt()); 
                }
                break;
                //死んだら他の行動をキャンセルする
                case State.Dying: if (staggerState || isAttacking){StopCoroutine(exclusiveAction);} break;
                case State.Midair: break;
        }
    }
    //攻撃ルーチン
    public virtual IEnumerator Attack()
    {
        isAttacking = true;
        animator.Play("Attack");
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        animator.Play("Idle");
        yield return new WaitForSeconds(pauseTime);
        isAttacking = false;


    }
    //被ダメージ時の処理
    public virtual void OnDamaged()
    {
        ChangeState(State.Hurt);
    }
    //死ぬ時の処理を始めるメソッド。
    public void OnDeath()
    {
        ChangeState(State.Dying);
        StartCoroutine(Die());
    }
    //死ぬ時の処理
    public virtual IEnumerator Die()
    {
        animator.Play("Death");
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.a = 0.0f;
        //敵の死体がフェードアウトする演出
        GetComponent<SpriteRenderer>().DOColor(tmp,0.5f).OnComplete(()=>
        {
            GameManager.DecrementCounter();
            Debug.Log($"{GameManager.GetCounter()} enemies remaining.");
            Destroy(gameObject);
        });
    }

    //プレイヤーとの距離を計算する
    protected float DistanceToPlayer()
    {
        return Mathf.Abs(transform.position.x - player.position.x);
        
    }
    //このクラスでは効果がないけど、防御ができる敵の子クラスに書き直される処理
    public virtual void BlockStart()
    {
        return;
    }
    public virtual bool BlockJudge()
    {
        return false;
    }
}

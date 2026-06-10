//ボスの行動を担当するクラス。
using UnityEngine;
using System.Collections;
using Cinemachine;
public class SlimeAi : EnemyAI
{
    [SerializeField] private float waitTime;
    [SerializeField] private float chaseTime;
    [SerializeField] private bool isChasing;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float chompForce;
    [SerializeField] private Vector3 jumpDirection;
    [SerializeField] private float jumpForce;
    [SerializeField] private float diveForce;
    [SerializeField] private float landingPause;
    [SerializeField] private float diveOffset;
    [SerializeField] private bool isDiving = false;
    [SerializeField] private float spinForce;
    [SerializeField] private float spinFriction;
    [SerializeField] private float spinBounciness;
    [SerializeField] private float spinChargeTimer;
    [SerializeField] private PhysicsMaterial2D spinMaterial;
    [SerializeField] private PhysicsMaterial2D defaultMaterial;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private FadeManager fade;
    [SerializeField] private float deathDuration;
    [SerializeField] private SlimeSoundManager soundManager;
    [SerializeField] private int previousChoice;
    [SerializeField] private bool isFirstAttack;
    private float defaultGrav;
    private float defaultBounciness;
    private float defaultFriction;
    private bool isWaiting;
    public bool desperationMode = false;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        currentState = State.Chase;
        staggerState = false;
        isFacingRight = false;
        isAttacking = false;
        defaultGrav = rb.gravityScale;
        isFirstAttack = true;
    }
    void FixedUpdate()
    {
        if (!isAttacking)
        {
        //現在の状態によって次の行動を決める
        switch (currentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Hurt: UpdateHurt(); break;
            case State.Dying: break;
            case State.Midair: UpdateMidair(); break;
        }
        }
    }
    protected override void ChangeState(State newState)
    {
        if(newState != State.Hurt){
        currentState = newState;
        }
        switch (newState)
        {
            case State.Idle: 
            animator.Play("Idle");
            break;
            case State.Chase: 
                animator.Play("Run"); 
                break;
            case State.Attack: 
                exclusiveAction = StartCoroutine(Attack());
                break;
            case State.Hurt: break;
            case State.Dying: if(isAttacking || isChasing || isWaiting) {StopCoroutine(exclusiveAction);} break;
            case State.Midair: animator.Play("Jump"); break;

        }
    }
    //待機状態のアップデートメソッド。攻撃の後、少し待機するように作られている。
    public override void UpdateIdle()
    {
        if(!isWaiting){
        exclusiveAction = StartCoroutine(IdleRoutine());
        }
    }
    //攻撃後の待機状態のタイマーを開始するルーチン。これが終わったら次の攻撃が始まる。
    public IEnumerator IdleRoutine(){
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        isWaiting = false;
        ChangeState(State.Chase);
    }
    //追跡状態のアップデート。
    public override void UpdateChase()
    {
        //追跡状態のタイマーを開始する。時間が経ったら、追跡を諦めて攻撃する。
        if(!isChasing){
        exclusiveAction = StartCoroutine(ChaseTimer());
        }
        float xdist = transform.position.x - player.position.x;
        bool shouldFaceRight = xdist <= 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0,180,0);
        }
        //プレイヤーが攻撃範囲内なら追跡を諦めて攻撃する
        if(DistanceToPlayer() < attackRange)
        {
            StopCoroutine(exclusiveAction);
            isChasing = false;
            ChangeState(State.Attack);
            return;
        }
        float step = moveSpeed * Time.deltaTime;
        Vector3 pathToPlayer = Vector3.MoveTowards(transform.position,player.position,step);
        pathToPlayer.y = transform.position.y;
        transform.position = pathToPlayer;
    }
    //追跡のタイマーを管理するルーチン。
    public IEnumerator ChaseTimer()
    {
        isChasing = true;
        yield return new WaitForSeconds(chaseTime);
        isChasing = false;
        ChangeState(State.Attack);

    }
    //空中状態のアップデート。
    public void UpdateMidair()
    {
        //ジャンプの頂点に着いたら飛び込み攻撃を開始する。
        if(rb.linearVelocity.y < -0.1f && !isDiving)
        {
            Debug.Log("Dive start");
            exclusiveAction = StartCoroutine(DiveAttack());
        }
    }
    //飛び込み攻撃の処理を担当するルーチン。
    public IEnumerator DiveAttack()
    {
        //接触ダメージをONにする。
        isDiving = true;
        attackHitbox.SetActive(true);
        //プレイヤーの位置を保存する
        rb.linearVelocity = Vector2.zero;
        LayerMask floorMask = LayerMask.GetMask("Default");
        RaycastHit2D targetPoint;
        //float defGrav = rb.gravityScale;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(0.5f);

        float xdist = transform.position.x - player.position.x;
        bool shouldFaceRight = xdist <= 0;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0,180,0);
        }
        //プレイヤーの位置に照準を当てる
        targetPoint = Physics2D.Raycast(player.position, -Vector2.up, Mathf.Infinity, floorMask);
        Vector2 target = targetPoint.point;
        Debug.Log($"{target.x}, {target.y}");
        //プレイヤーの方向に飛び込む
        Vector3 diveDirection = new Vector3(target.x + (isFacingRight ? diveOffset : -diveOffset), target.y, transform.position.z) - transform.position;
        rb.AddForce(diveDirection.normalized * diveForce, ForceMode2D.Impulse);
        soundManager.PlayDive();
        //rb.gravityScale = defGrav;
    }
    //地面に着いたら飛び込み攻撃を終了する
    public void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Floor")
        {
            if(isDiving){
            exclusiveAction = StartCoroutine(Landing());
            }
        }
    }
    //着陸アニメーションを再生するルーチン
    public IEnumerator Landing()
    {
        rb.gravityScale = defaultGrav;
        animator.Play("Jump_Landing");
        yield return new WaitForSeconds(landingPause);
        isAttacking = false;
        isDiving = false;
        ChangeState(State.Idle);
    }
    /*
    攻撃パターンを管理するルーチン。攻撃が三つある：
    1. 噛みつき: プレイヤーの方向に少し移動しながら近距離攻撃。
    2. コマ攻撃: ぐるぐる回りながら高スピードで体当たりする攻撃。
    3. 飛び込み攻撃：空中に上昇して、溜めの後プレイヤー方向に飛ぶ攻撃。
    この三つの攻撃からランダムに一つが選ばれて、同じ攻撃を二回連続選ばれない。
    */
    public override IEnumerator Attack()
    {
        isAttacking = true;
        animator.Play("Attack_Windup");
        yield return null;
        //攻撃を選ぶ
        int selection = Random.Range(0,3);
        //
        if (!isFirstAttack)
        {
            if(selection == previousChoice)
            {
                selection = (selection+1) % 3;
            }
        }
        else
        {
            isFirstAttack = false;
        }
        previousChoice = selection;
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        switch (selection)
        {
            case 0:
                rb.AddForce(-transform.right * chompForce,ForceMode2D.Impulse);
                animator.Play("Chomp");
                break;
            case 1: 
                animator.Play("Jump_Start");
                rb.AddForce(transform.up*jumpForce,ForceMode2D.Impulse);
                yield return null;

                clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(clipLength);

                attackHitbox.SetActive(false);
                ChangeState(State.Midair);
                isAttacking = false;
                yield break;
            case 2: 
            animator.Play("Spin");
            exclusiveAction = StartCoroutine(SpinAttack());
            yield break;
        }
        yield return null;
        clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        isAttacking = false;
        ChangeState(State.Idle);
    }
    //コマ攻撃を行うルーチン。
    private IEnumerator SpinAttack()
    {
        attackHitbox.SetActive(false);
        yield return new WaitForSeconds(spinChargeTimer);
        
        attackHitbox.SetActive(true);
        rb.sharedMaterial = spinMaterial; //摩擦の少ないマテリアルに変換
        rb.AddForce(-transform.right * spinForce,ForceMode2D.Impulse);
        soundManager.PlaySpin();
        yield return new WaitForSeconds(0.2f);
        while(Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            yield return null;
        }
 
        rb.sharedMaterial = defaultMaterial;
        isAttacking = false;
        ChangeState(State.Idle);
    }

    //ボスが倒された際の演出の処理。エンディングシーンに遷移する。
    public override IEnumerator Die()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0.0f;
        attackHitbox.SetActive(false);
        animator.Play("Death");
        yield return new WaitForSeconds(0.5f);
        fade.FadeOut(deathDuration,"Ending1");
        yield return null;
    }
}

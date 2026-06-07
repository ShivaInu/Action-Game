using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
プレイヤーの動き、攻撃、性能、アニメーションの一部、と被弾時の処理を管理するスクリプト。
*/
public class PlayerSousa : MonoBehaviour
{
    [SerializeField, Header("移動速度")]
    private float moveSpeed;
    [SerializeField, Header("攻撃中の減速度")] 
    private float attackBrakeFactor;
    [SerializeField, Header("死の際に呼び出す用")]
    private FadeManager fadeManager;
    [SerializeField, Header("ジャンプ速度")]
    private float jumpSpeed;
    [SerializeField, Header("回避速度")]
    private float backDashForce;
    [SerializeField, Header("地面から離れた時のジャンプ猶予時間")]
    public float coyoteTimeDuration;
    //　ジャンプ猶予の時間中かどうか
    private bool waitingOnCoyoteTime;
    [SerializeField, Header("加速度")]
    public float acceleration;
    [SerializeField, Header("アニメーション中の動きをなぞるオブジェクト")]
    public GameObject spritePos;
    [SerializeField, Header("移動中止時の減速度")]
    public float deceleration;
    [SerializeField, Header("右を向いている")]
    public bool facingRight;
    [SerializeField,Header("怯み中")] 
    private bool isStaggered;
    [SerializeField,Header("方向を変えている")]
    public bool isTurning;
    [SerializeField,Header("空中")]
    public bool midAir;
    [SerializeField,Header("左スティックや矢印キーの向き")]
    public Vector2 inputDirection;
    [SerializeField,Header("プレイヤーのRigidBody2D")]
    public Rigidbody2D rb;
    //プレイヤーのAnimator
    private Animator animator;
    //向きを変えている時のコルーチンの参照
    private Coroutine turnCoroutine;
    [SerializeField,Header("プレイヤーに与えたジャンプ猶予時間")]
    public Coroutine coyoteTimer;
    //プレイヤーの振り返るアニメーションをキャンセル時用
    private bool turnFinished;
    //攻撃中で色んな入力処理を省く用
    private bool isAttacking;
    //攻撃をキャンセルできるかどうか。アニメーションイベントで呼び出し
    public bool inComboWindow;
    //メインコンボのマックスヒット数
    private int mainComboHits;
    [SerializeField,Header("コンボの段階")]
    public int comboStep = 0;
    //攻撃のコルーチンの参照
    private Coroutine attackCoroutine;
    //プレイヤーのRigidBody2Dのデフォルト重力
    public float defaultGrav;
    //プレイヤーの空中コンボが終わったかどうか
    public bool airComboEnd;
    //プレイヤーのヒット判定がアクティブかどうか。非アクティブならヒット判定の処理を省く。
    private bool hitboxActive;
    [SerializeField,Header("ゲームパッドの振動の強さ設定")]
    public float leftMotorSpeed;
    public float rightMotorSpeed;
    [SerializeField,Header("プレイヤーのヒット判定コライダー")]
    private BoxCollider2D attackHitbox;
    //もうダメージを与えたコライダーのリスト
    private List<Collider2D> damagedColliders;
    [SerializeField,Header("ヒット時の演出")]
    GameObject hitEffect;
    [SerializeField,Header("回避成功時の演出")]
    GameObject dodgeEffect;
    [SerializeField,Header("回避のスピード感を出す演出")]
    GameObject backdashEffect;
    [SerializeField,Header("プレイヤーの音源を処理するコンポーネント")]
    PlayerAudioController audioController;
    [SerializeField,Header("プレイヤーの最大HP")]
    public int maxHP;
    [SerializeField,Header("プレイヤーの現在HP")]
    public int currentHP;
    [SerializeField,Header("無敵状態")]
    public bool isInvuln;
    [SerializeField,Header("プレイヤーのHPバー")]
    public PlayerHealthBar healthBar;
    [SerializeField,Header("ヒットストップを管理するゲームオブジェクト")]
    public HitStopper hitStopper;
    [SerializeField,Header("死の処理が行っているかどうか")]
    public bool isDying;
    //public Collider2D groundCollider;
    [SerializeField,Header("回避のクールタイム")]
    public float backDashCooldown;
    [SerializeField,Header("回避可能かどうか")]
    private bool canBackDash;
    [SerializeField,Header("左スティックのx要素の最小値(これより小さかったら無視される)")]
    public float xMoveTol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        midAir = false;
        facingRight = true;
        turnFinished = false;
        isAttacking = false;
        inComboWindow = false;
        mainComboHits = 3;
        defaultGrav = rb.gravityScale;
        damagedColliders = new List<Collider2D>();
        audioController = GetComponent<PlayerAudioController>();
        isDying = false;
        canBackDash = true;
        healthBar = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<PlayerHealthBar>();
        hitStopper = GameObject.FindGameObjectWithTag("Hitstopper").GetComponent<HitStopper>();
    }

    void FixedUpdate()
    {
        //移動処理を怯みや攻撃状態で停止される
        if(!isAttacking && !isStaggered){
        Move();
        }
        //攻撃のヒット判定がアクティブであれば判定チェックを行う
        else
        {
            if (attackHitbox.isActiveAndEnabled)
            {
                CheckHit();
            }
        }
        //プレイヤーの動きのアニメーションを処理する
        MoveAnimation();
        JumpAnimation();
    }
    //プレイヤーの動きの処理を行うメソッド
    private void Move()
    {   
        //入力でプレイヤーが右に向かっているかどうかを推測する
        bool goingRight = inputDirection.x > 0;
        //誤差入力を無視する
        if(Mathf.Abs(inputDirection.x) > xMoveTol)
        {
            inputDirection.x = goingRight ? inputDirection.magnitude : -inputDirection.magnitude;
            inputDirection.y = 0.0f;
        }
        else
        {
            inputDirection.x = 0.0f;
        }
        float targetSpeed = inputDirection.x * moveSpeed;
        
        //プレイヤーの向きを変えないといけなかったら
        if(goingRight != facingRight && !isTurning && targetSpeed != 0)
        {
            //空中であれば振り返りのアニメーションの処理を省く
            if(!midAir){
                turnCoroutine = StartCoroutine(TurnAround(inputDirection.x));
            }
            else
            {
                ChangeDirection();
            }
        }

        //プレイヤーが加速しているか減速しているか判断する
        float speedDif = targetSpeed - rb.linearVelocity.x;
        bool tryingToMove = Mathf.Abs(targetSpeed) > 0.01f;
        //加速度か減速度の値
        float accelRate = tryingToMove ? acceleration : deceleration;

        
        float movement = speedDif * accelRate;
        animator.SetBool("HasMoveImpulse",tryingToMove);
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);


    }
    IEnumerator TurnAround(float newDirection)
    {
        isTurning = true;
        turnFinished = false;
        animator.Play("run_turn");


        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        isTurning = false;
    }
    private void MoveAnimation()
    {
        animator.SetFloat("MoveHorizontal",Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("MoveMagnitude",Mathf.Abs(inputDirection.x));
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }
    public void OnBackdash(InputAction.CallbackContext context)
    {
        if (isStaggered){
            return;
        }
        else
        {
            if (context.performed)
            {
                if(canBackDash){
                    if (isAttacking)
                    {
                        posSync();
                        StopCoroutine(attackCoroutine);
                    }
                comboStep = 2;
                inComboWindow = false;
                StartCoroutine(Backdash());
                }
            }
        }
    }
    IEnumerator Backdash()
    {
        animator.Play("Backdash");
        audioController.PlayPlayerSound(audioController.backdashSound);
        Vector3 currentPos = transform.position;
        currentPos = new Vector3(currentPos.x,currentPos.y+1.0f,currentPos.z);
        GameObject.Instantiate(backdashEffect,currentPos,facingRight ? Quaternion.identity : Quaternion.Euler(0,180,0));
        isAttacking = true;
        canBackDash = false;
        turnCheck();
        yield return null;

        rb.linearVelocity = Vector2.zero;
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        rb.AddForce(facingRight ? transform.right * backDashForce : -transform.right * backDashForce, ForceMode2D.Impulse);
        rb.gravityScale = 0;
        yield return new WaitForSeconds(clipLength / 4);

        rb.gravityScale = defaultGrav;
        yield return new WaitForSeconds(3 * clipLength / 4);
        isAttacking = false;
        canBackDash = true;
        comboStep = 0;
    }
    private void CheckHit()
    {
        Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int colliderCount = Physics2D.OverlapCollider(attackHitbox, filter, collidersToDamage);
        for(int i = 0; i < colliderCount; i++)
        {
            Collider2D enemyCollider = collidersToDamage[i];
            if (!damagedColliders.Contains(enemyCollider) && (enemyCollider.tag == "Enemy" || enemyCollider.tag == "Boss"))
            {
                Debug.Log("Hit!");
                if(!enemyCollider.gameObject.GetComponent<Enemy>().isDying)
                {
                Vector3 knockback;
                if(!(enemyCollider.tag == "Boss")){
                knockback = new Vector3(facingRight ? .7f : -.7f,.3f,0f)
                *(comboStep == 2 ? 5.0f : 2.0f);
                }
                else
                {
                    knockback = Vector3.zero;
                }
                hitStopper.Execute(comboStep == 2 ? 0.10f : 0.05f,0f);
                StartCoroutine(HitFeedback(0.15f));
                GameObject.Instantiate(hitEffect,enemyCollider.transform.position,Quaternion.identity);
                
                enemyCollider.GetComponent<Enemy>().TakeDamage(comboStep == 2 ? 10 : 5,knockback);
                if(enemyCollider.GetComponent<EnemyAI>().BlockJudge())
                    {
                        Debug.Log("Attack blocked.");
                        enemyCollider.GetComponent<EnemySoundManager>().playHurtSound(enemyCollider.GetComponent<Enemy>().blockSound);
                    }
                else{
                if(comboStep == 2)
                    {
                        enemyCollider.GetComponent<EnemySoundManager>().playHurtSound(enemyCollider.GetComponent<Enemy>().hurtSoundH);
                    }
                    else
                    {
                        enemyCollider.GetComponent<EnemySoundManager>().playHurtSound(enemyCollider.GetComponent<Enemy>().hurtSoundL);
                    }
                }
                }
                damagedColliders.Add(enemyCollider);
            }
        }
    }
    IEnumerator HitFeedback(float duration)
    {
        Gamepad.current.SetMotorSpeeds(leftMotorSpeed,rightMotorSpeed);
        yield return new WaitForSeconds(duration);
        Gamepad.current.ResetHaptics();
    }
    private void ChangeDirection()
    {
        turnFinished = true;
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            if(context.canceled && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,rb.linearVelocity.y*0.5f);
            }
            return;
        }
        if(!midAir && !isAttacking && !isStaggered){
            StartCoroutine(JumpRoutine());
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isStaggered)
        {
            return;
        }
        if (context.performed)
        {
            if(!(midAir && airComboEnd)){
                if (!isAttacking)
                {
                    turnCheck();
                    attackCoroutine = StartCoroutine(Attack());
                }
                else if (inComboWindow && comboStep < mainComboHits - 1)
                {
                    StopCoroutine(attackCoroutine);
                    comboStep++;
                    inComboWindow = false;
                    attackCoroutine = StartCoroutine(Attack());
                }
            }
        }
    }

    /*アニメーションイベントで呼び出す用。攻撃のヒット判定をONに*/
    public void OpenComboWindow()
    {
        inComboWindow = true;
    }
    /*アニメーションイベントで呼び出す用。攻撃のヒット判定をOFFに*/
    public void CloseComboWindow()
    {
        inComboWindow = false;
    }
    /*メイン攻撃の処理を行うコルーチン*/
    IEnumerator Attack()
    { 
        isAttacking = true;
        damagedColliders.Clear();

        if(midAir){
        rb.linearVelocity = new Vector2(0,0);
        rb.gravityScale = 0;
        }
        else
        {
            rb.linearVelocity = new Vector2(attackBrakeFactor * rb.linearVelocity.x,0);
        }

        string animationName = "Attack " + (comboStep + 1);
        animator.Play(animationName);
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);
        if(midAir)  airComboEnd = true;
        rb.gravityScale = defaultGrav;
        comboStep = 0;
        isAttacking = false;
        inComboWindow = false;
    }
    /*キャラクターがあっちこっち動いているアニメーションでも終了・キャンセルの時のTransformをスプライトと一致させるメソッド*/
    public void posSync()
    {
        transform.position = spritePos.transform.position;
    }
    
    /*被ダメージ時の処理*/
    public void OnHurt(int damage)
    {
        if(!isInvuln){
        currentHP -= damage;
        healthBar.UpdateHealthBar((float)currentHP / (float) maxHP);
        if(currentHP <= 0)
            {
                StartCoroutine(Die());
            }
        else
            {
                StartCoroutine(OnDamaged());
            }
        }
        else if (!canBackDash)
        {
            Vector3 currentPos = transform.position;
            currentPos = new Vector3(currentPos.x,currentPos.y+1.0f,currentPos.z);
            GameObject.Instantiate(dodgeEffect,currentPos,Quaternion.identity);
            audioController.PlayEnemyHurtSound(audioController.dodgeSuccess);
            hitStopper.Execute(.05f,0.0f);
        }
    }
    //無敵期間を開始する
    private void StartInvuln()
    {
        isInvuln = true;
    }
    //無敵期間を終了する
    private void EndInvuln()
    {
        isInvuln = false;
    }
    //被ダメージ時の無敵と怯み処理を担当するルーチン
    IEnumerator OnDamaged()
    {
        animator.Play("Hurt");
        isStaggered = true;
        isInvuln = true;
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        isStaggered = false;
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.a = 0.7f;
        GetComponent<SpriteRenderer>().color = tmp;
        yield return new WaitForSeconds(1.0f);

        tmp.a = 1.0f;
        GetComponent<SpriteRenderer>().color = tmp;
        isInvuln = false;


    }
    //プレイヤーの向きを変えるアニメーション中に行動でキャンセルする場合の処理。
    private void turnCheck()
    {
        if(isTurning)
        {
            //プレイヤーが向きを変えている途中ならそのアニメーションを停止し、一瞬で向きを変える
            isTurning = false;
            StopCoroutine(turnCoroutine);
            if(!turnFinished)
            {
                ChangeDirection();
                turnFinished = false;
            }
        }
        else
        {
            turnFinished = false;
        }
    }
    //空中時の処理
    private void JumpAnimation()
    {
        animator.SetFloat("MoveVertical",rb.linearVelocity.y);
        animator.SetBool("Midair",midAir);
        animator.SetBool("isAttacking",isAttacking);
        //落下時の重力を強くする
        if(midAir && rb.linearVelocity.y < -0.1)
        {
            rb.gravityScale = defaultGrav * 1.5f;
        }
    }
    //プレイヤーが死ぬときの処理
    IEnumerator Die()
    {
        isDying = true;
        isStaggered = true;
        animator.Play("Die");
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);
        //アニメーションが終わったら現在のシーンを再起動して
        fadeManager.FadeOut(1.0f,SceneManager.GetActiveScene().name);
    }

    //プレイヤーのジャンプアニメーションを呼び出す
    IEnumerator JumpRoutine()
    {
        midAir = true;
        turnCheck();
        animator.Play("jump start");
        yield return null;

        //プレイヤーが立っているところの法線ベクトルに沿って力を入れる
        rb.linearVelocity = new Vector3(rb.linearVelocity.x,0);
        LayerMask floorMask = LayerMask.GetMask("Default");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up,floorMask);
        rb.AddForce(hit.normal * jumpSpeed, ForceMode2D.Impulse);
    }
}

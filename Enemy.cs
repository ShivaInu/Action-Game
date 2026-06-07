//敵のHP管理、攻撃とダメージを受ける際の処理を行うスクリプト。これを継承したら敵の特徴的な処理を実装できます。
using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
    [SerializeField] protected int maxHP = 30;
    [SerializeField] HealthBar healthBar;
    [SerializeField] Canvas canvas;
    [SerializeField] public EnemyAI ai;
    [SerializeField] protected bool playerHit;
    [SerializeField] protected PlayerSousa player;
    [SerializeField] protected Rigidbody2D playerRb;
    [SerializeField] protected Rigidbody2D rb;

    [SerializeField] GameObject hitEffect;
    [SerializeField] private BoxCollider2D attackHitbox;
    [SerializeField] private int attackDamage;
    [SerializeField] public bool isDying;
    [SerializeField] float knockbackForce;
    public AudioClip hurtSoundL;
    public AudioClip hurtSoundH;
    public AudioClip blockSound;
    public EnemySoundManager enemySounds;
    protected int currentHP;
    protected bool successfulHit;

    void Awake()
    {
        currentHP = maxHP;
        healthBar = GetComponentInChildren<HealthBar>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerSousa>();
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        canvas.enabled = false;
        isDying = false;
    }
    void FixedUpdate()
    {
        //敵の攻撃判定コライダーがアクティブならプレイヤーとの衝突をチェック
        if (attackHitbox.isActiveAndEnabled)
        {
            CheckHit();
        }
    }

    //ダメージを受ける際の処理
    public virtual void TakeDamage(int damage,Vector3 knockbackForce)
    {   if(!isDying) //死んでいればこの処理を飛ばす
        {
            rb.AddForce(knockbackForce,ForceMode2D.Impulse);
            currentHP -= damage;
            if (!canvas.isActiveAndEnabled)
            {
                canvas.enabled = true; //HPゲージを表示する
            }
            healthBar.UpdateHealthBar((float) currentHP/(float) maxHP);
            if (currentHP <= 0) //HPがなくなったら死ぬ処理を始める
            {
                isDying = true;
                ai.OnDeath();
            }
            else
            {
                ai.OnDamaged(); //AIスクリプト側の被ダメージ処理を始める
            }
        }
    }
    //攻撃がプレイヤーと当たているか確認し、当たていればダメージとノックバックを与える処理を行う
    public virtual void CheckHit()
    {
        Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int colliderCount = Physics2D.OverlapCollider(attackHitbox, filter, collidersToDamage);
        //当たっているコライダーを確認し、プレビューの判定コライダーならダメージ処理
        for(int i = 0; i < colliderCount; i++)
        {
            if (!playerHit && collidersToDamage[i].tag == "Player")
            {
                if(!player.isDying)
                { 
                if(!player.isInvuln){
                successfulHit = true;
                //プレイヤーのスピードを０にして、ノックバック力を与える
                playerRb.linearVelocity = Vector2.zero;
                playerRb.AddForce(new Vector3(ai.isFacingRight ? .7f : -.7f,.3f,0f)*knockbackForce,ForceMode2D.Impulse);
                enemySounds.playHitSound(); //プレイヤーにダメージを与える時の音を再生する

                //ヒット効果を当たったところに生成する
                Vector3 targetPos = collidersToDamage[i].transform.position;
                targetPos = new Vector3(targetPos.x,targetPos.y+1.0f,targetPos.z);
                GameObject.Instantiate(hitEffect,targetPos,Quaternion.identity);
                }
                player.OnHurt(attackDamage); //プレイヤー側の被ダメージ処理をする
                }
                playerHit = true;
                StartCoroutine(hitTimer());
            }
        }
    }
    //攻撃が一回しか当たらないようにクールタイムのメソッド。
    private IEnumerator hitTimer()
    {
        yield return new WaitForSeconds(0.5f);
        AttackStart();
    }
    //以上のクールタイムをリセットするメソッド。
    public virtual void AttackStart()
    {
        playerHit = false;
        successfulHit = false;
    }
}

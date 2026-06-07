//中ボスの攻撃ルーチンを定義するスクリプト。
using UnityEngine;

public class Enemy_Nightborne : Enemy
{
    [SerializeField,Header("敵の数の制限")]
    public int enemyMax;
    public GameObject skeleton;
    public bool skeletonSpawned;

    void Awake()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerSousa>();
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        isDying = false;
        rb = GetComponent<Rigidbody2D>();
        skeletonSpawned = false;
    }
    public override void CheckHit()
    {
        base.CheckHit();
        //プレイヤーを攻撃した場合、骸骨を生成する。敵の数に制限があり、それ以上の敵を同時に生成できない。
        if (successfulHit)
        {
            int enemiesActive = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemiesActive < enemyMax)
            {
                if(!skeletonSpawned){
                    Vector3 spawnPos = new Vector3(transform.position.x + 1,transform.position.y,transform.position.z);
                    GameObject.Instantiate(skeleton,spawnPos,Quaternion.identity,transform.parent);
                    GameManager.IncrementCounter();
                    skeletonSpawned = true;
                }
            }
        }
    }

    public override void AttackStart()
    {
        playerHit = false;
        skeletonSpawned = false;
        successfulHit = false;
    }
}

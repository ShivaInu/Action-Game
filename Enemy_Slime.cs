//ボス敵の管理スクリプト。Awakeの処理が違うため、Enemyを継承してAwakeだけを改造している。
using UnityEngine;
using System.Collections;
public class Enemy_Slime : Enemy
{
        void Awake()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerSousa>();
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        isDying = false;
    }
}

//中ボスの行動パターンを定義するクラス。デフォルト行動パターンと大体同じだがひるまない。
using UnityEngine;

public class NightborneAI : EnemyAI
{
    //怯み状態に入らない
    public override void OnDamaged()
    {
        return;
    }
}

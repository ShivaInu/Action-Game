//骸骨の被ダメージ処理を変える子クラス。
using UnityEngine;

public class Skeleton : Enemy
{

    public override void TakeDamage(int damage,Vector3 knockback)
    {
        if (!isDying)
        {
        //正面からのダメージを防御する
        if (ai.BlockJudge())
        {
            ai.BlockStart();
            return;
        }
            else
            {
                base.TakeDamage(damage,knockback);
            }
        }
    }
}


//ヒット効果やバックダッシュの効果で使われているアニメーションが終わったらオブジェクトを消すスクリプト。
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] Animator anim;

    void Start()
    {   anim = GetComponent<Animator>();
        float wait = anim.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, wait);
    }
}

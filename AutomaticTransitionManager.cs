/*
5秒経ったらシーンを変えるだけのスクリプト。
*/
using UnityEngine;
using System.Collections;

public class AutomaticTransitionManager : MonoBehaviour
{
    [SerializeField,Header("フェード用のGameObject")] 
    private FadeManager fadeManager;
    void Start()
    {
        StartCoroutine(SceneTransition());
    }

    private IEnumerator SceneTransition(){
        yield return new WaitForSeconds(5.0f);
        fadeManager.FadeOut(1.0f,"Credits");
    }
}

/*
タイトル画面の演出とメニュー処理を行うスクリプト。
*/
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class MainMenuController : MonoBehaviour
{
    public Transform sceneCamera;
    public MainMenuCameraController cameraController;
    public Transform player;
    public Animator playerAnimator;
    public Image menuBackdrop;
    public float playerMovementSpeed;
    public float cameraTravelTime;
    public TextMeshProUGUI titleText;
    public GameObject[] menuElements;
    public FadeManager fadeManager;
    

    void Start()
    {
        //プレイヤーの歩きアニメーションを開始する。
        playerAnimator.Play("from idle");
        //カメラの焦点が空からプレイヤーに移る。
        sceneCamera.DOMoveY(cameraController.focalPoint.position.y,cameraTravelTime).OnComplete(() =>
        {
        //カメラの下の動きが終わったらプレイヤーの動きをなぞり、メニューを表示する。
        cameraController.isChasingPlayer = true;
        
        StartCoroutine(DisplayMenu());
        });
    }

    void FixedUpdate()
    {
        //原点から離れすぎると物理計算がバグるとの事でここまで来たら一旦リセット
        if(player.position.x > 9000.0f)
        {
            fadeManager.FadeOut(0.5f,"MainMenu");
        }
        //プレイヤーがタイトル画面の背景でずっと歩いている。
        player.position = new Vector3(player.position.x + playerMovementSpeed,player.position.y,player.position.z);
    }

    //メニューの表示演出を担当するメソッド。
    private IEnumerator DisplayMenu()
    {
        yield return new WaitForSeconds(1.0f);
        Color tmp = menuBackdrop.color;
        tmp.a = 0.7f;
        menuBackdrop.DOColor(tmp,2.0f).OnComplete(() =>
        {
            Color tmp = titleText.color;
            tmp.a = 1.0f;
            titleText.DOColor(tmp,2.0f).OnComplete(() =>
            {
            for(int i = 0; i < menuElements.Length; i++)
                {
                    menuElements[i].SetActive(true);
                }
            });
        });
    }

    //ゲーム開始がメニューから選ばれた時に呼び出される。
    public void SceneTransition()
    {
        fadeManager.FadeOut(0.5f,"Level1");
    }

    //ゲーム終了がメニューから選ばれた時に呼び出される。
    public void QuitGame()
    {
        Application.Quit();
    }
}

/*
敵の数を数、シーンの遷移、一時停止を管理するスクリプト。
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //残りの敵の数。
    public static int enemiesRemaining;
    [SerializeField,Header("一時停止のメニュー")]
    public GameObject pauseMenu;
    [SerializeField, Header("一時停止のメニューのボタン")]
    public GameObject resumeButton, quitButton;
    public bool isPaused;
    [SerializeField, Header("プレイヤーのオブジェクトについているPlayerInput")]
    public PlayerInput playerInput;
    

    //敵の数を数える
    void Start()
    {
        instance = this;
        enemiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length + GameObject.FindGameObjectsWithTag("Boss").Length;
        isPaused = false;
    }

    //敵の数を+1
    public static void IncrementCounter()
    {
        enemiesRemaining++;
    }
    //敵の数を-1
    public static void DecrementCounter()
    {
        enemiesRemaining--;
    }
    //現在の敵の数を呼び出しの元に返る。
    public static int GetCounter()
    {
        return enemiesRemaining;
    }

    //一時停止入力イベント処理
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //もう停止されているなら再開する
            if (isPaused)
            {
                Unpause();
            }
            else
            {
                //ゲームの時間を停止して、UIへの入力しか受けないようにする。一時停止メニューを表示する。
                isPaused = true;
                playerInput.SwitchCurrentActionMap("UI");
                Time.timeScale = 0.0f;
                pauseMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                
                EventSystem.current.SetSelectedGameObject(resumeButton);
            }
        }
    }
    //一時停止から再開するメソッド。
    public void Unpause()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        playerInput.SwitchCurrentActionMap("Player");
    }
    //シーン遷移。
    public void ChangeSceneTo(string sceneName)
    {
        //時間の流れを普通のスピードに戻す
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }
}

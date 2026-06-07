//タイトル画面にある入力ボタンアイコンを操作方法で切り替えるクラス。
//ゲームパッドとキーボード対応
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class MainMenuInputIconSwitcher : MonoBehaviour
{
    public Sprite arrowKeys, enterKey, dPad, gamePadButtons;
    public Image moveIcon, confirmIcon;

    //コントローラーが変わったらアイコンを切り替える
    public void OnControlsChanged(PlayerInput input)
    {
        if(input.currentControlScheme == "Gamepad")
        {
            moveIcon.sprite = dPad;
            moveIcon.transform.localScale *= 0.5f;
            confirmIcon.sprite = gamePadButtons;
        }
        else
        {
            moveIcon.sprite = arrowKeys;
            moveIcon.transform.localScale *= 2;
            confirmIcon.sprite = enterKey;
        }
    }

}

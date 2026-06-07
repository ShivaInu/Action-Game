//レベル1の操作説明のアイコンをコントローラーの種類に合わせるクラス。
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialWindowManager : MonoBehaviour
{
    public Sprite gamePadMove, gamePadAttack, gamePadDodge, gamePadJump, gamePadPause,
    keyboardMove, keyboardAttack, keyboardDodge, keyboardJump, keyboardPause;

    public Image move, jump, attack, dodge, dodge2, pause;

    public void OnControlsChanged(PlayerInput input)
    {
        if(input.currentControlScheme == "Gamepad")
        {
            move.sprite = gamePadMove;
            move.transform.localScale *= 0.75f;
            attack.sprite = gamePadAttack;
            dodge.sprite = gamePadDodge;
            dodge.transform.localScale *= 0.75f;
            dodge2.sprite = gamePadDodge;
            dodge2.transform.localScale *= 0.75f;
            jump.sprite = gamePadJump;
            jump.transform.localScale /= 1.5f;
            pause.sprite = gamePadPause;
            pause.transform.localScale *= 1.5f;
        }
        else
        {
            move.sprite = keyboardMove;
            move.transform.localScale /= 0.75f;
            attack.sprite = keyboardAttack;
            dodge.sprite = keyboardDodge;
            dodge.transform.localScale /= 0.75f;
            dodge2.sprite = keyboardDodge;
            dodge2.transform.localScale /= 0.75f;
            jump.sprite = keyboardJump;
            jump.transform.localScale *= 1.5f;
            pause.sprite = keyboardPause; 
            pause.transform.localScale /= 1.5f;
        }
    }
}

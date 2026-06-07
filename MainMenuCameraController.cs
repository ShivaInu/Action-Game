//タイトル画面のカメラ移動を管理するクラス。
using UnityEngine;

public class MainMenuCameraController : MonoBehaviour
{
    public bool isChasingPlayer;
    public Transform focalPoint;

    void LateUpdate()
    {
        transform.position = new Vector3(focalPoint.position.x, transform.position.y, transform.position.z);
    }
}

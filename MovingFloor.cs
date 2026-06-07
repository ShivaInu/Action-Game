using UnityEngine;

public class MovingFloor : MonoBehaviour
{
    public Transform player;
    void Update()
    {
        transform.position = new Vector3(player.position.x,transform.position.y,transform.position.z);
    }
}

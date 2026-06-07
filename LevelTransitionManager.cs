using UnityEngine;

public class LevelTransitionManager : MonoBehaviour
{
    [SerializeField] private string nextLevelName;

    void Update()
    {
        if(GameManager.GetCounter() == 0)
        {
        GetComponent<CapsuleCollider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player"){
            GameManager.instance.ChangeSceneTo(nextLevelName);
        }
    }
}

using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, moveY, 0).normalized;

        transform.position += move * moveSpeed * Time.deltaTime;

        // Lật nhân vật theo hướng trái/phải
        if (moveX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (moveX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}

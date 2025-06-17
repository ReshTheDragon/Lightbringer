using UnityEngine;

public class RuneBlink : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float blinkSpeed = 2f;
    private Transform player;
    private float maxDistance = 5f; // Khoảng cách tối đa để rune nhấp nháy

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false; // Ẩn rune khi bắt đầu
        player = GameObject.FindGameObjectWithTag("Player").transform; // Tìm player
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= maxDistance)
        {
            spriteRenderer.enabled = true; // Hiển thị rune khi trong khoảng cách
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        }
        else
        {
            spriteRenderer.enabled = false; // Ẩn rune khi ngoài khoảng cách
        }
    }
}
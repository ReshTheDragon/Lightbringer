using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayFire : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float timeDestroy = 0.5f;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] private int damage = 10;
    void Start()
    {
        Destroy(gameObject, timeDestroy);
    }

    // Update is called once per frame
    void Update()
    {
        MoveBullet();
    }
    void MoveBullet()
    {
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        Destroy(gameObject, timeDestroy);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu va chạm với Tilemap
        if (collision.GetComponent<TilemapCollider2D>() != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 0.1f);
            Destroy(gameObject);
            //Debug.Log("2Va chạm với: " + collision.name + " - tag: " + collision.tag + " - collider: " + collision.GetType());

            return;
        }


        if (collision.CompareTag("Player"))
        {
            PlayerControl enemy = collision.GetComponent<PlayerControl>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Assuming Enemy has a TakeDamage method
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 0.1f);

            }
            Destroy(gameObject);
        }

    }

}

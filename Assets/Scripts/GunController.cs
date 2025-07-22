using System;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private float rotateOffset = 180f; // Giữ nguyên offset này cho việc xoay súng
    [SerializeField] private Transform firePos;
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private float shotDelay = 0.15f;
    [SerializeField] private Transform player; // Reference to player transform
    [SerializeField] private float triggerRadius = 5f; // Bán kính vùng trigger
    [SerializeField] private float rotationSpeed = 5f; // Tốc độ xoay súng
    [SerializeField] private Animator dragonAnimator;

    private float nextShot;
    private bool playerInRange = false;
    private bool wasPlayerInRange = false; // Theo dõi trạng thái trước đó

    [SerializeField] private AudioManager audioManager;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        CheckPlayerInRange();

        // Kiểm tra khi player vào hoặc ra khỏi vùng trigger
        if (playerInRange != wasPlayerInRange)
        {
            HandlePlayerRangeChange();
        }

        if (playerInRange)
        {
            RotateGunToPlayer();
            HandleShooting();
        }

        wasPlayerInRange = playerInRange;
    }

    private void CheckPlayerInRange()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= triggerRadius;
    }

    // Xử lý khi player vào/ra khỏi vùng trigger
    private void HandlePlayerRangeChange()
    {
        if (dragonAnimator == null) return;

        if (playerInRange)
        {
            // Player vào vùng trigger - bắt đầu animation tấn công
            //Debug.Log("Player entered attack range");
            dragonAnimator.SetBool("IsAttack", true);
        }
        else
        {
            // Player ra khỏi vùng trigger - dừng animation tấn công
            //Debug.Log("Player left attack range");
            dragonAnimator.SetBool("IsAttack", false);
        }
    }

    // Xoay súng để nó luôn hướng về phía người chơi
    void RotateGunToPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotateOffset;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    void HandleShooting()
    {
        if (playerInRange && Time.time > nextShot)
        {
            nextShot = Time.time + shotDelay;

            // Tính toán hướng ngược lại so với người chơi
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion inverseRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Instantiate(bulletPrefabs, firePos.position, inverseRotation);
            audioManager.playShootSound();

        }
    }

    
}
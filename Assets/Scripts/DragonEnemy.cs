using UnityEngine;

public class DragonEnemy : EnemyFollow
{
    private bool isWalking = false;
  
    protected override void Start()
    {
        base.Start();

        speed = 1.5f;
        attackDamage = 15f;
        chaseRadius = 7f;
        maxHealth = 150f;

    }

    protected override void Update()
    {
        if (player == null || isDying) return;

        // Kiểm tra xem dragon có đang di chuyển không
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool shouldWalk = distanceToPlayer < chaseRadius && !isPlayerInAttackRange;

        // Cập nhật animation IsWalk
        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            animator.SetBool("IsWalk", isWalking);
        }

        // Gọi Update của class cha
        base.Update();


    }


    public override void TakeHit(Vector3 attackerPos, float force, float damage)
    {
        // Dragon chỉ nhận 80% damage từ skill
        float reducedDamage = damage * 0.8f;

        base.TakeHit(attackerPos, force * 0.5f, reducedDamage);

        Debug.Log($"🐉 Dragon bị trúng chiêu và nhận {reducedDamage} damage!");

        // Tùy chọn: Thêm hiệu ứng hoặc hành vi riêng
        StartCoroutine(StunBriefly());
    }

    private System.Collections.IEnumerator StunBriefly()
    {
        speed = 0f; // dừng di chuyển
        yield return new WaitForSeconds(1f);
        speed = 1.5f; // trở lại bình thường
    }





}
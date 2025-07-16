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


    // Override method TakeHit để Dragon có phản ứng khác
    public override void TakeHit(Vector3 attackerPos, float force, float damage)
    {
        // Dragon chỉ nhận 80% damage (có armor)
        float reducedDamage = damage * 0.8f;
        base.TakeHit(attackerPos, force * 0.5f, reducedDamage); // Cũng giảm knockback

        Debug.Log($"Dragon took reduced damage: {reducedDamage}");
    }

   


}
using System.Collections;
using UnityEngine;

public class EnemyNPC : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int patrolAreaSize = 6;
    public Transform[] patrolPoints;
    public float detectionRadius = 5f;
    public Transform attackPoint; // Reference to the attack point
    public float attackPointRadius = 1f; // Radius for attack point

    private Transform player;
    private int currentPatrolIndex;
    private bool isChasing;
    private bool isAttacking;
    private float attackCooldownTimer;
    private Vector3 initialPosition;

    public int damageAmount = 10; // Amount of damage dealt to the player
    private PlayerHealth playerHealth; // Reference to the PlayerHealth component
    public Animator animator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        currentPatrolIndex = 0;
        isChasing = false;
        isAttacking = false;
        attackCooldownTimer = 0f;
        initialPosition = transform.position;

        if (patrolPoints.Length == 0)
        {
            GeneratePatrolPoints();
        }

        PatrolToNextPoint();
    }

    void Update()
    {
        animator.SetFloat("speed", patrolSpeed);
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        if (isAttacking)
        {
            attackCooldownTimer += Time.deltaTime;
            if (attackCooldownTimer >= attackCooldown)
            {
                isAttacking = false;
                attackCooldownTimer = 0f;
            }
        }
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 0.5f)
        {
            PatrolToNextPoint();
        }
        else
        {
            MoveTowards(patrolPoints[currentPatrolIndex].position, patrolSpeed);
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            isChasing = true;
        }
    }

    void PatrolToNextPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void ChasePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) > detectionRadius)
        {
            isChasing = false;
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            MoveTowards(player.position, chaseSpeed);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        // Perform attack logic here (e.g., reduce player health)
        Debug.Log("Attacking the player!");
        animator.SetTrigger("Attack");

        // Check if the player is within the attack point radius during the attack
        yield return new WaitForSeconds(0.5f); // Wait for the attack animation to reach the hit frame

        float distanceToPlayer = Vector3.Distance(attackPoint.position, player.position);
        if (distanceToPlayer <= attackPointRadius)
        {
            // Apply damage to the player
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogError("PlayerHealth component not found!");
            }
        }

        yield return new WaitForSeconds(attackCooldown - 0.5f); // Wait for the rest of the cooldown

        isAttacking = false;
    }

    private void GeneratePatrolPoints()
    {
        patrolPoints = new Transform[4];
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            GameObject patrolPoint = new GameObject("PatrolPoint" + i);
            patrolPoint.transform.position = new Vector3(
                Random.Range(initialPosition.x - patrolAreaSize / 2, initialPosition.x + patrolAreaSize / 2),
                initialPosition.y,
                Random.Range(initialPosition.z - patrolAreaSize / 2, initialPosition.z + patrolAreaSize / 2)
            );
            patrolPoints[i] = patrolPoint.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackPointRadius);
        }
    }
}

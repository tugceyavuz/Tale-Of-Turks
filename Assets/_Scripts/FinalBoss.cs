using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public int damage = 20;
    public int numberOfWaypoints = 8;
    public float waypointRadius = 2f;
    public Transform attackPoint;
    public float attackPointRadius = 1f;

    private Transform player;
    private Vector3[] waypoints;
    private int currentWaypointIndex = 0;
    private float attackTimer;
    public Animator animator;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackTimer = attackCooldown;

        // Generate waypoints in a circular pattern around the boss's initial position
        waypoints = new Vector3[numberOfWaypoints];
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfWaypoints;
            float x = Mathf.Cos(angle) * waypointRadius;
            float z = Mathf.Sin(angle) * waypointRadius;
            waypoints[i] = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
        }
    }

    void Update()
    {
        animator.SetFloat("speed", patrolSpeed);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0)
            return;

        Vector3 targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetDirection = (targetWaypoint - transform.position).normalized;

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void ChasePlayer()
    {
        Vector3 targetDirection = (player.position - transform.position).normalized;
        if(targetDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 0); // Flip horizontally
        }
        else if (targetDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 0); // Reset scale
        }

        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    void Attack()
    {
        if (attackTimer >= attackCooldown)
        {
            // Trigger attack animation
            animator.SetTrigger("Attack");

            // Detect player in range of attackPoint
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackPointRadius);

            foreach (Collider hitPlayer in hitPlayers)
            {
                if (hitPlayer.CompareTag("Player"))
                {
                    // Apply damage to the player
                    hitPlayer.GetComponent<PlayerHealth>().TakeDamage(damage);
                }
            }

            attackTimer = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (var waypoint in waypoints)
            {
                Gizmos.DrawSphere(waypoint, 0.2f);
            }
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackPointRadius);
        }
    }
}

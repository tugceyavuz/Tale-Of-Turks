using System.Collections;
using UnityEngine;

public class CatNPC : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float minPauseTime = 1f;
    public float maxPauseTime = 3f;
    public float minWalkTime = 2f;
    public float maxWalkTime = 5f;

    public Animator animator;

    // Animation triggers
    private string[] animationTriggers = { "Stretc", "Itch", "Lick", "Sleep", "Lay" };

    private Rigidbody2D rb;
    private Vector2 walkDirection;
    private bool isPerformingAnimation = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(WalkAndPauseRoutine());
    }

    IEnumerator WalkAndPauseRoutine()
    {
        while (true)
        {
            if (!isPerformingAnimation)
            {
                // Random walk
                walkDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                float walkTime = Random.Range(minWalkTime, maxWalkTime);
                animator.SetFloat("Speed", walkSpeed);  // Update animator with walk speed

                // Flip the sprite based on the direction
                if (walkDirection.x < 0)
                {
                    transform.localScale = new Vector3(-(float)0.5, (float)0.5, (float)0.5);  // Flip left
                }
                else if (walkDirection.x > 0)
                {
                    transform.localScale = new Vector3((float)0.5, (float)0.5, (float)0.5);   // Flip right
                }

                yield return new WaitForSeconds(walkTime);

                // Pause and play random animation
                rb.velocity = Vector2.zero;
                animator.SetFloat("Speed", 0);  // Set speed to 0 to stop walk animation

                // Choose a random animation trigger
                string randomAnimation = animationTriggers[Random.Range(0, animationTriggers.Length)];
                StartCoroutine(PerformAnimation(randomAnimation));

                // Wait for the animation to complete
                yield return new WaitForSeconds(5f);
            }

            // Pause for a random time before starting next walk cycle
            float pauseTime = Random.Range(minPauseTime, maxPauseTime);
            yield return new WaitForSeconds(pauseTime);
        }
    }

    IEnumerator PerformAnimation(string animationTrigger)
    {
        isPerformingAnimation = true;
        animator.SetTrigger(animationTrigger);
        yield return new WaitForSeconds(5f);  // Assume each animation takes 5 seconds
        isPerformingAnimation = false;
    }

    void FixedUpdate()
    {
        if (!isPerformingAnimation && animator.GetFloat("Speed") > 0)
        {
            rb.velocity = walkDirection * walkSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required for checking UI focus
using TMPro; // Required for TextMeshPro

public class PlayerControl : MonoBehaviour
{
    public float movementSpeed;
    Vector2 movement;
    Rigidbody2D rb;
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if the input field is focused
        if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
        {
            // If focused, do not process movement input
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

        // Process movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Flip the player if moving left
        if (movement.x < 0)
        {
            transform.localScale = new Vector3(-4, 4, 0); // Flip horizontally
        }
        // Flip back if moving right
        else if (movement.x > 0)
        {
            transform.localScale = new Vector3(4, 4, 0); // Reset scale
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime / 2);
    }
}

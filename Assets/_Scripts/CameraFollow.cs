using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f; // Speed of camera movement
    public Vector3 offset; // Offset between the camera and the player

    private Transform target; // Reference to the player character's transform
    private bool playerFound = false; // Flag to track if player has been found

    void LateUpdate()
    {
        // If the player is not found, try to find it
        if (!playerFound)
        {
            FindPlayer();
        }

        // If the target is found, follow it
        if (target != null)
        {
            // Calculate the desired position for the camera
            Vector3 desiredPosition = target.position + offset;
            desiredPosition.z = transform.position.z; // Fix the z-position

            // Smoothly interpolate the camera position towards the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    void FindPlayer()
    {
        // Try to find the player character GameObject in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If the player character is found, assign its transform as the target
        if (player != null)
        {
            target = player.transform;
            playerFound = true; // Set flag to true to indicate player has been found
        }
    }

    public void ResetTheScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

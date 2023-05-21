using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float walkSpeed = 6f;
    public float runSpeed = 15f;
    public float crouchSpeed = 3f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;

    public float gravity = -9.81f;

    Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool isGrounded;
    bool isRunning;
    bool isCrouching;

    // Dash
    bool isDashing;
    float dashTimer;
    bool canDash = true;

    public float dashCooldown = 0.3f;

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        // Running
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        // Crouching
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.C) && !isDashing)
        {
            Dash();
        }

        // Movement speed
        float moveSpeed = walkSpeed;
        if (isRunning)
        {
            moveSpeed = runSpeed;
        }
        else if (isCrouching)
        {
            moveSpeed = crouchSpeed;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Dash movement
        if (isDashing)
        {
            DashMovement();
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    void Dash()
    {

        isDashing = true;
        dashTimer = dashDuration;

        if (!canDash)
        {
            // Dash is on cooldown
            return;
        }

        // Disable dashing during the cooldown period
        canDash = false;

        // Disable gravity during dash
        velocity.y = 0f;

        // Calculate dash direction based on player's input
        Vector3 dashDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            dashDirection += transform.forward;
        if (Input.GetKey(KeyCode.S))
            dashDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            dashDirection -= transform.right;
        if (Input.GetKey(KeyCode.D))
            dashDirection += transform.right;

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            // If no movement keys are pressed, dash backward
            dashDirection = -transform.forward;
        }

        // Normalize dash direction to ensure consistent speed regardless of the number of movement keys pressed
        dashDirection.Normalize();

        // Start the dash coroutine to smoothly move the player forward
        StartCoroutine(DashCoroutine(dashDirection));

        // Start dash timer coroutine
        StartCoroutine(DashTimer());
    }


    IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dashDirection * dashDistance;

        // Perform a raycast in the dash direction to check for obstacles
        RaycastHit hit;
        bool hitObstacle = Physics.Raycast(startPosition, dashDirection, out hit, dashDistance);

        // Adjust the target position if an obstacle is hit
        if (hitObstacle)
        {
            targetPosition = hit.point;
        }

        while (elapsedTime < dashDuration)
        {
            // Calculate the interpolation factor (0 to 1) based on the elapsed time and dash duration
            float t = elapsedTime / dashDuration;

            // Smoothly move the player towards the target position
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Increase the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the player reaches the target position exactly
        transform.position = targetPosition;

        // Dash complete, re-enable gravity
        velocity.y = gravity;
    }


    void DashMovement()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;

            // Re-enable gravity after dash
            velocity.y = gravity;
        }
    }

    IEnumerator DashTimer()
    {
        // Wait for the dash cooldown duration
        yield return new WaitForSeconds(dashCooldown);

        // Enable dashing again after the cooldown
        canDash = true;
    }
}

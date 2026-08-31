using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header("References")]
    public Transform cube;
    public Transform forwardReference;

    [Header("Movement Settings")]
    public float maxSpeed = 4f;
    public float acceleration = 2f;
    public float deceleration = 1.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 120f;

    private float currentSpeed = 0f;

    void Update()
    {
        HandleRotation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 forward = forwardReference.forward;
        forward.y = 0f;
        forward.Normalize();

        float input = 0f;

        // W = vooruit
        if (Input.GetKey(KeyCode.W))
        {
            input += 1f;
        }

        // S = achteruit
        if (Input.GetKey(KeyCode.S))
        {
            input -= 1f;
        }

        float targetSpeed = input * maxSpeed;

        float rate = Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed)
            ? acceleration
            : deceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        // Beweging
        cube.position += forward * currentSpeed * Time.fixedDeltaTime;
    }

    void HandleRotation()
    {
        // NIET draaien als je stilstaat
        if (Mathf.Abs(currentSpeed) < 0.05f)
            return;

        float rotationInput = 0f;

        // A = links
        if (Input.GetKey(KeyCode.A))
        {
            rotationInput -= 1f;
        }

        // D = rechts
        if (Input.GetKey(KeyCode.D))
        {
            rotationInput += 1f;
        }

        if (rotationInput != 0f)
        {
            float rotation = rotationInput * rotationSpeed * Time.deltaTime;

            cube.Rotate(0f, rotation, 0f);
        }
    }
}
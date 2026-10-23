
using UnityEngine;

public class SpaceFloat : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatSpeed = 0.5f;
    public float floatAmount = 0.5f;
    public float horizontalAmount = 0.2f;

    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(5f, 10f, 3f);

    [Header("Random Movement")]
    public bool randomMovement = true;
    public float randomSpeed = 0.3f;
    public float randomAmount = 0.2f;

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time;

        Vector3 offset = new Vector3(
            Mathf.Sin(time * floatSpeed) * horizontalAmount,
            Mathf.Sin(time * floatSpeed * 1.3f) * floatAmount,
            Mathf.Cos(time * floatSpeed * 0.7f) * horizontalAmount
        );

        if (randomMovement)
        {
            offset += new Vector3(
                Mathf.PerlinNoise(time * randomSpeed, randomOffset) - 0.5f,
                Mathf.PerlinNoise(randomOffset, time * randomSpeed) - 0.5f,
                Mathf.PerlinNoise(time * randomSpeed, randomOffset + 50f) - 0.5f
            ) * randomAmount;
        }

        transform.localPosition = startPosition + offset;

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}

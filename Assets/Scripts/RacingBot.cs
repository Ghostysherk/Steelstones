
using UnityEngine;

public class RacingBot : MonoBehaviour
{
    [Header("References")]
    public Rigidbody carRigidbody;
    public Transform forwardReference;
    public Transform racePath;

    [Header("Movement")]
    public float maxSpeed = 4f;
    public float acceleration = 2f;

    [Header("Rotation")]
    public float rotationSpeed = 180f;

    [Header("Path Settings")]
    public bool loopPath = true;

    [Tooltip("Hoe hoog de auto boven de waypoints rijdt.")]
    public float heightOffset = 0f;

    [Header("Debug")]
    public bool showPath = true;

    private Transform[] waypoints;
    private int currentWaypoint = 0;

    private float currentSpeed = 0f;
    private float segmentProgress = 0f;

    private Vector3 startPoint;
    private Vector3 endPoint;

    private bool finished = false;

    void Start()
    {
        if (carRigidbody == null || racePath == null)
        {
            Debug.LogError(
                "RacingBot: Rigidbody of RacePath ontbreekt!"
            );

            enabled = false;
            return;
        }

        if (racePath.childCount < 2)
        {
            Debug.LogError(
                "RacingBot: Minimaal 2 waypoints nodig!"
            );

            enabled = false;
            return;
        }

        waypoints = new Transform[racePath.childCount];

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = racePath.GetChild(i);
        }

        carRigidbody.isKinematic = true;
        carRigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        // Start bij het dichtstbijzijnde segment
        FindClosestSegment();
    }

    void FixedUpdate()
    {
        if (finished)
            return;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            maxSpeed,
            acceleration * Time.fixedDeltaTime
        );

        MoveAlongPath();
    }

    void FindClosestSegment()
    {
        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;
        float closestProgress = 0f;

        int segmentCount = loopPath
            ? waypoints.Length
            : waypoints.Length - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[
                (i + 1) % waypoints.Length
            ].position;

            Vector3 segment = b - a;

            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared < 0.0001f)
                continue;

            float t = Mathf.Clamp01(
                Vector3.Dot(
                    carRigidbody.position - a,
                    segment
                ) / lengthSquared
            );

            Vector3 closest = Vector3.Lerp(a, b, t);

            float distance = Vector3.SqrMagnitude(
                carRigidbody.position - closest
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
                closestProgress = t;
            }
        }

        currentWaypoint = closestIndex;
        segmentProgress = closestProgress;

        UpdateSegment();
    }

    void UpdateSegment()
    {
        startPoint = waypoints[currentWaypoint].position;

        int nextIndex =
            (currentWaypoint + 1) % waypoints.Length;

        endPoint = waypoints[nextIndex].position;
    }

    void MoveAlongPath()
    {
        float distanceToMove =
            currentSpeed * Time.fixedDeltaTime;

        int safety = 0;

        while (distanceToMove > 0f && safety < 1000)
        {
            safety++;

            float segmentLength =
                Vector3.Distance(startPoint, endPoint);

            if (segmentLength < 0.0001f)
            {
                if (!NextSegment())
                    break;

                continue;
            }

            float remainingDistance =
                segmentLength * (1f - segmentProgress);

            if (distanceToMove >= remainingDistance)
            {
                distanceToMove -= remainingDistance;

                segmentProgress = 1f;

                if (!NextSegment())
                    break;
            }
            else
            {
                segmentProgress +=
                    distanceToMove / segmentLength;

                distanceToMove = 0f;
            }
        }

        Vector3 targetPosition = Vector3.Lerp(
            startPoint,
            endPoint,
            segmentProgress
        );

        targetPosition += Vector3.up * heightOffset;

        carRigidbody.MovePosition(targetPosition);

        // Richt de auto naar de volgende waypoint
        Vector3 direction = endPoint - startPoint;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            Quaternion newRotation =
                Quaternion.RotateTowards(
                    carRigidbody.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );

            carRigidbody.MoveRotation(newRotation);
        }
    }

    bool NextSegment()
    {
        currentWaypoint++;

        if (currentWaypoint >= waypoints.Length)
        {
            if (loopPath)
            {
                currentWaypoint = 0;
            }
            else
            {
                currentWaypoint = waypoints.Length - 2;
                segmentProgress = 1f;
                finished = true;
                currentSpeed = 0f;
                UpdateSegment();
                return false;
            }
        }

        // Bij een open pad is het laatste punt het eindpunt
        if (!loopPath &&
            currentWaypoint >= waypoints.Length - 1)
        {
            currentWaypoint = waypoints.Length - 2;
            segmentProgress = 1f;
            finished = true;
            currentSpeed = 0f;
            UpdateSegment();
            return false;
        }

        segmentProgress = 0f;
        UpdateSegment();

        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (!showPath || racePath == null)
            return;

        int count = racePath.childCount;

        if (count < 2)
            return;

        Gizmos.color = Color.green;

        int segmentCount = loopPath
            ? count
            : count - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            Transform a = racePath.GetChild(i);
            Transform b = racePath.GetChild(
                (i + 1) % count
            );

            Gizmos.DrawSphere(a.position, 0.25f);
            Gizmos.DrawLine(a.position, b.position);
        }

        Gizmos.DrawSphere(
            racePath.GetChild(count - 1).position,
            0.25f
        );
    }
}

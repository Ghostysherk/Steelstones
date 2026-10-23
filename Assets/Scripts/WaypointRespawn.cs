
using UnityEngine;

public class WaypointRespawn : MonoBehaviour
{
    [Header("Waypoints (in race order)")]
    public Transform[] waypoints;

    [Header("Respawn Settings")]
    public float respawnHeight = 1f;
    public float respawnBackOffset = 2f;

    [Header("Car Settings")]
    public string carTag = "Main Car";

    private Transform car;
    private Rigidbody carRb;
    private int lastWaypointIndex = -1;
    private bool isRespawning;

    void Start()
    {
        GameObject carObject = GameObject.FindGameObjectWithTag(carTag);

        if (carObject == null)
        {
            Debug.LogError("No car found with tag: " + carTag);
            return;
        }

        car = carObject.transform;
        carRb = carObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (car == null || waypoints == null || waypoints.Length == 0)
            return;

        if (isRespawning)
            return;

        // Save closest waypoint while above the trigger
        if (car.position.y > transform.position.y)
        {
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    continue;

                float distance = Vector3.Distance(
                    car.position,
                    waypoints[i].position
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    lastWaypointIndex = i;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(carTag))
            return;

        if (lastWaypointIndex < 0 || isRespawning)
            return;

        RespawnCar();
    }

    void RespawnCar()
    {
        isRespawning = true;

        Transform currentWaypoint = waypoints[lastWaypointIndex];

        // Find next valid waypoint
        int nextIndex = (lastWaypointIndex + 1) % waypoints.Length;
        int attempts = 0;

        while (waypoints[nextIndex] == null &&
               attempts < waypoints.Length)
        {
            nextIndex = (nextIndex + 1) % waypoints.Length;
            attempts++;
        }

        Transform nextWaypoint = waypoints[nextIndex];

        // Direction towards next waypoint
        Vector3 direction = nextWaypoint.position - currentWaypoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = currentWaypoint.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.forward;

        direction.Normalize();

        // Respawn slightly behind waypoint
        Vector3 respawnPosition =
            currentWaypoint.position
            - direction * respawnBackOffset
            + Vector3.up * respawnHeight;

        Quaternion respawnRotation = Quaternion.LookRotation(
            direction,
            Vector3.up
        );

        // Reset physics and teleport
        if (carRb != null)
        {
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;

            carRb.position = respawnPosition;
            carRb.rotation = respawnRotation;
        }
        else
        {
            car.SetPositionAndRotation(
                respawnPosition,
                respawnRotation
            );
        }

        Physics.SyncTransforms();

        Debug.Log(
            "Respawned at " + currentWaypoint.name +
            " facing " + nextWaypoint.name
        );

        isRespawning = false;
    }
}

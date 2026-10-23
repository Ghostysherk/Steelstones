
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class RaceFinishManager : MonoBehaviour
{
    [Header("Racers")]
    public string playerTag = "Main Car";
    public string enemyTag = "Enemy";

    [Header("Race Settings")]
    [Min(1)] public int totalLaps = 3;
    [Min(0f)] public float finishCooldown = 30f;

    [Header("TextMeshPro")]
    public TMP_Text lapText;
    public TMP_Text timerText;
    public TMP_Text resultText;

    [Header("Finish Settings")]
    public string nextSceneName = "Menu";
    public float resultDisplayTime = 5f;

    [Header("Testing")]
    public KeyCode testKey = KeyCode.T;
    public bool testFirstPlace = true;
    public bool debugMessages = true;

    private class Racer
    {
        public Transform transform;
        public int laps;
        public float nextFinishTime;
        public Vector3 previousPosition;
        public bool wasInside;
    }

    private Racer player = new Racer();
    private Racer enemy = new Racer();

    private BoxCollider finishCollider;

    private bool raceStarted;
    private bool raceEnded;
    private float raceTime;

    void Awake()
    {
        finishCollider = GetComponent<BoxCollider>();
        finishCollider.isTrigger = true;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        GameObject e = GameObject.FindGameObjectWithTag(enemyTag);

        if (p != null)
            player.transform = p.transform;

        if (e != null)
            enemy.transform = e.transform;

        if (player.transform == null ||
            enemy.transform == null)
        {
            Debug.LogError(
                "RaceFinishManager: Main Car or Enemy missing!"
            );

            enabled = false;
            return;
        }

        totalLaps = Mathf.Max(1, totalLaps);

        player.previousPosition = player.transform.position;
        enemy.previousPosition = enemy.transform.position;

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        UpdateDisplays();
    }

    void Update()
    {
        // Test finish screen
        if (!raceEnded && Input.GetKeyDown(testKey))
        {
            EndRace(testFirstPlace);
            return;
        }

        if (raceEnded)
            return;

        // Wait for countdown to finish
        if (!raceStarted)
        {
            if (Time.timeScale <= 0f)
                return;

            raceStarted = true;
            raceTime = 0f;

            player.nextFinishTime = finishCooldown;
            enemy.nextFinishTime = finishCooldown;

            ResetDetection(player);
            ResetDetection(enemy);

            if (debugMessages)
                Debug.Log("Race started! Finish unlocks in " +
                    finishCooldown + " seconds.");
        }

        raceTime += Time.deltaTime;

        CheckRacer(player, true);

        if (!raceEnded)
            CheckRacer(enemy, false);

        UpdateDisplays();
    }

    void CheckRacer(Racer racer, bool isPlayer)
    {
        if (racer.transform == null)
            return;

        Vector3 current = racer.transform.position;

        bool inside = IsInsideFinish(current);

        bool crossed = SegmentCrossesFinish(
            racer.previousPosition,
            current
        );

        bool entered = (inside && !racer.wasInside) || crossed;

        if (entered)
        {
            string racerName = isPlayer ? "PLAYER" : "ENEMY";

            if (raceTime >= racer.nextFinishTime)
            {
                racer.laps++;

                if (debugMessages)
                    Debug.Log(
                        racerName + " completed lap " +
                        racer.laps + "/" + totalLaps
                    );

                // Start new 30-second cooldown
                racer.nextFinishTime =
                    raceTime + finishCooldown;

                if (racer.laps >= totalLaps)
                {
                    EndRace(isPlayer);
                }
            }
            else if (debugMessages)
            {
                Debug.Log(
                    racerName + " crossed too early! " +
                    Mathf.CeilToInt(
                        racer.nextFinishTime - raceTime
                    ) + " seconds remaining."
                );
            }
        }

        racer.previousPosition = current;
        racer.wasInside = inside;
    }

    void ResetDetection(Racer racer)
    {
        racer.previousPosition = racer.transform.position;
        racer.wasInside = IsInsideFinish(
            racer.transform.position
        );
    }

    bool IsInsideFinish(Vector3 worldPosition)
    {
        Vector3 local =
            finishCollider.transform.InverseTransformPoint(
                worldPosition
            ) - finishCollider.center;

        Vector3 half = finishCollider.size * 0.5f;

        return Mathf.Abs(local.x) <= half.x &&
               Mathf.Abs(local.y) <= half.y &&
               Mathf.Abs(local.z) <= half.z;
    }

    bool SegmentCrossesFinish(Vector3 start, Vector3 end)
    {
        Vector3 a =
            finishCollider.transform.InverseTransformPoint(start)
            - finishCollider.center;

        Vector3 b =
            finishCollider.transform.InverseTransformPoint(end)
            - finishCollider.center;

        Vector3 direction = b - a;
        Vector3 half = finishCollider.size * 0.5f;

        float tMin = 0f;
        float tMax = 1f;

        for (int axis = 0; axis < 3; axis++)
        {
            float origin = a[axis];
            float delta = direction[axis];
            float extent = half[axis];

            if (Mathf.Abs(delta) < 0.00001f)
            {
                if (origin < -extent || origin > extent)
                    return false;
            }
            else
            {
                float t1 = (-extent - origin) / delta;
                float t2 = (extent - origin) / delta;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);

                if (tMin > tMax)
                    return false;
            }
        }

        return true;
    }

    void UpdateDisplays()
    {
        if (lapText != null)
        {
            lapText.text = "LAP " +
                Mathf.Min(player.laps + 1, totalLaps) +
                "/" + totalLaps;
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(raceTime / 60f);
            int seconds = Mathf.FloorToInt(raceTime % 60f);
            int hundredths = Mathf.FloorToInt(
                (raceTime % 1f) * 100f
            );

            timerText.text = string.Format(
                "{0:00}:{1:00}.{2:00}",
                minutes,
                seconds,
                hundredths
            );
        }
    }

    void EndRace(bool playerWon)
    {
        if (raceEnded)
            return;

        raceEnded = true;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);

            resultText.text = playerWon
                ? "1ST PLACE!"
                : "2ND PLACE!";
        }

        if (debugMessages)
            Debug.Log(playerWon
                ? "PLAYER WON!"
                : "ENEMY WON!");

        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(
            resultDisplayTime
        );

        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }

    void OnDisable()
    {
        if (raceEnded)
            Time.timeScale = 1f;
    }
}

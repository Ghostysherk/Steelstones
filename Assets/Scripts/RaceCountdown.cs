
using UnityEngine;
using TMPro;
using System.Collections;

public class RaceCountdown : MonoBehaviour
{
    [Header("Countdown Settings")]
    public float countdownInterval = 1f;

    [Header("TextMeshPro")]
    public TMP_Text countdownText;

    [Header("Optional")]
    public bool startOnAwake = true;

    private Coroutine countdownRoutine;

    void Awake()
    {
        // Pause immediately when scene loads
        Time.timeScale = 0f;
    }

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (startOnAwake)
            StartCountdown();
    }

    public void StartCountdown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        // Keep game paused during countdown
        Time.timeScale = 0f;

        if (countdownText == null)
        {
            Debug.LogError("Countdown TextMeshPro is missing!");
            Time.timeScale = 1f;
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(countdownInterval);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(countdownInterval);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(countdownInterval);

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(countdownInterval);

        countdownText.gameObject.SetActive(false);

        // Start race
        Time.timeScale = 1f;

        countdownRoutine = null;
    }

    void OnDisable()
    {
        // Prevent leaving the game paused
        Time.timeScale = 1f;
    }
}

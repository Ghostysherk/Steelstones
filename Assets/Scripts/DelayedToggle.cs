
using UnityEngine;
using System.Collections;

public class DelayedToggle : MonoBehaviour
{
    [Header("Timer Settings")]
    public float delay = 5f;

    [Header("Objects To Enable")]
    public GameObject[] objectsToEnable;

    [Header("Objects To Disable")]
    public GameObject[] objectsToDisable;

    void Start()
    {
        StartCoroutine(ToggleObjects());
    }

    IEnumerator ToggleObjects()
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLauncher : MonoBehaviour
{
    public float launchingTime = 5.0f; // Temps en secondes avant de lancer la prochaine tâche
    public bool counting = false;

    void Start()
    {
        StartCoroutine(LaunchNextTaskAfterDelay());
    }

    IEnumerator LaunchNextTaskAfterDelay()
    {
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(counting);
    }
}

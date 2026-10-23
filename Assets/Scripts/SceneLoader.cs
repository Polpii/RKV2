using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class SceneLoader : MonoBehaviour
{
    public float secondes = 0;
    public bool notCounting;
    public void LoadSceneByName(string sceneName)
    {
        if (sceneName == "")
        {
            if (notCounting)
            {
                GameManager.Instance.currentTask --;
            }
            StartCoroutine(LaunchNextTaskAfterDelay());
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void Start()
    {
        if(secondes != 0)
        {
            LoadSceneByName("");
        }
    }

    IEnumerator LaunchNextTaskAfterDelay()
    {
        yield return new WaitForSeconds(secondes);
        GameManager.Instance.CompleteTaskAndLoadNext();
    }
}
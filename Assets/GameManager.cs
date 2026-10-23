using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<string> taskOrder;
    public int currentIndex = 0;
    public int currentTask = 0;
    private int nextDataColumn = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTasks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetTask()
    {
        return taskOrder[currentIndex - 1];
    }

    public int GetDataColumn()
    {
        nextDataColumn++;
        return nextDataColumn - 1;
    }

    private void LoadTasks()
    {
        string path = "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Order.txt";
        taskOrder = new List<string>();

        // Lire chaque ligne du fichier
        foreach (string line in File.ReadAllLines(path))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                taskOrder.Add(line.Trim()); // Ajoutez le nom de la scène à la liste
            }
        }
    }

    public void CompleteTaskAndLoadNext(bool counting = true)
    {
        StartCoroutine(LoadNext(counting));
    }

    IEnumerator LoadNext(bool counting = true)
    {
        if (currentIndex < taskOrder.Count)
        {
            GameObject audioController = GameObject.Find("AudioController");
            while (Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]) > 0.1)
            {
                yield return null;
            }
            if(audioController != null)
            {
                audioController.GetComponent<Manipulandum_data_aquired>().enabled = false;
            }
            SceneManager.LoadScene(taskOrder[currentIndex]);
        }
        else
        {
            // Toutes les tâches sont terminées
            Debug.Log("Toutes les tâches sont terminées");
        }
        if (counting)
        {
            currentTask++;
        }
        currentIndex++;
    }
}

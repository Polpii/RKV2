using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;

public class ReflexTestManager : MonoBehaviour
{
    public Camera mainCamera;
    public TMP_Text consigneText;
    public TMP_Text scoreText;

    private float reactionStartTime;
    private bool isTestRunning;
    private float Sensibility;
    private bool readyToStartTest;
    private bool greenScreen;
    private bool waitingForRelease;
    private bool waitingToStart;
    private bool loaded;

    private void Start()
    {
        loaded = false;
        isTestRunning = false;
        Sensibility = 0.5f; // Ajustez cette valeur selon vos besoins
        readyToStartTest = false;
        greenScreen = false;
        waitingForRelease = false; // Nouvelle étape d'attente
        waitingToStart = true;
        mainCamera.backgroundColor = new Color32(222, 119, 110, 255);
    }


    private void Update()
    {
        float currentForce = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
        if(waitingToStart)
        {
            if(currentForce <= Sensibility * 0.6)
            {
                waitingToStart = false;
            }
        }
        else if (!isTestRunning)
        {
            if (currentForce > Sensibility && !readyToStartTest)
            {
                readyToStartTest = true;
                consigneText.text = "Attendez que l'écran devienne vert...";
            }
            else if (currentForce < Sensibility * 0.8 && readyToStartTest)
            {
                StartTest();
            }
        }
        else if (isTestRunning && currentForce > Sensibility)
        {
            if (greenScreen)
            {
                RecordReactionTime();
                this.GetComponent<DataSave>().SaveDataTimeReaction(scoreText.text.Substring(0, scoreText.text.Length - 3));
                waitingForRelease = true; // Commence l'attente pour le relâchement
                Sensibility = 20;
            }
            else
            {
                scoreText.text = "NaN";
                this.GetComponent<DataSave>().SaveDataTimeReaction(scoreText.text);
                consigneText.text = "Vous avez appuyé trop tôt. Appuyez sur le piston pour continuer.";
                waitingForRelease = true;
                Sensibility = 20;
            }
        }
        else if (waitingForRelease && currentForce < 0.2)
        {
            // Attend que le joueur relâche le piston
            consigneText.text = "Regardez votre score, puis appuyez sur le piston pour continuer.";
            waitingForRelease = false;
        }
        else if (!waitingForRelease && currentForce > 0.5)
        {
            if (!loaded)
            {
                loaded = true;
                // L'utilisateur a appuyé sur le piston après avoir vu son score
                StartCoroutine(LaunchNextTaskAfterDelay(0.5f));
            }            
        }
    }

    private void StartTest()
    {
        isTestRunning = true;
        consigneText.text = "Prêt...";

        // Wait for a random time between 2 to 5 seconds, then change the screen to green
        Invoke("ChangeBackgroundToGreen", UnityEngine.Random.Range(2f, 5f));
    }

    private void ChangeBackgroundToGreen()
    {
        if (isTestRunning)
        {
            mainCamera.backgroundColor = new Color32(130, 217, 155, 255);
            reactionStartTime = Time.time;
            greenScreen = true;
        }
    }

    private void RecordReactionTime()
    {
        if (isTestRunning)
        {
            float reactionTime = Time.time - reactionStartTime;
            scoreText.text = (reactionTime * 1000).ToString("F2") + " ms";
            consigneText.text = "Appuyez sur le piston pour continuer.";
        }
    }
    IEnumerator LaunchNextTaskAfterDelay(float launchingTime)
    {
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(false);
    }

}

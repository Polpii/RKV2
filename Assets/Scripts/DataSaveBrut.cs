using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class DataSaveBrut : MonoBehaviour
{
    string rawFilePath;
    public float sensibility;
    public bool timeReaction;
    List<string> dataBrutBuffer = new List<string>();
    float timeStamp;
    string userName;
    bool datarecording = false;
    bool recording = false;
    int targetColumnIndex;
    string columnHeader;

    void Start()
    {
        InitializeVariables();
        columnHeader = GameManager.Instance.GetTask();
        targetColumnIndex = GameManager.Instance.GetDataColumn();
    }

    void InitializeVariables()
    {
        userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        rawFilePath = @"C:\Users\paul-\UnityProject\RK_V2\Assets\Results\" + userName + "\\" + userName + "_Brut.txt";
        dataBrutBuffer = new List<string>();
        sensibility = 0.66f;
        timeStamp = 0;
    }

    public void LaunchDataRecording()
    {
        datarecording = true;
    }

    void Update()
    {
        if (!timeReaction)
        {
            if ((datarecording && Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]) >= 0.1f))
            {
                recording = true;
            }
            if (recording)
            {
                float currentForce = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
                dataBrutBuffer.Add("[ " + timeStamp.ToString() + " ; " + currentForce.ToString() + " ]");
                timeStamp += Time.deltaTime;
            }
        }
    }

    public void SaveDataBrut()
    {
        // Assurer la création du répertoire si celui-ci n'existe pas
        string directory = Path.GetDirectoryName(rawFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Écrire le nouveau contenu dans le fichier texte en mode d'ajout
        using (StreamWriter sw = new StreamWriter(rawFilePath, true)) // true pour ajouter à la fin du fichier existant
        {
            // Construire la ligne avec le titre et les données
            string line = columnHeader + ": ";
            foreach (string data in dataBrutBuffer)
            {
                line += data + " "; // Ajouter chaque donnée à la ligne
            }

            // Écrire la ligne dans le fichier texte
            sw.WriteLine(line);

            // Ajouter une ligne vide à la fin
            sw.WriteLine();
        }

        // Vider le buffer après l'enregistrement
        dataBrutBuffer.Clear();
    }
}
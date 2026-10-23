using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

public class DataSave : MonoBehaviour
{
    string filePath;
    public float sensibility;
    bool DM_Pressed = false;
    List<string> dataBuffer;
    List<string> buttonBuffer;
    float timeStamp;
    string userName;
    bool datarecording = false;
    bool recording = false;
    int targetColumnIndex;
    string columnHeader;
    public bool timeReaction;
    public int doigt = 1;

    void Start()
    {
        InitializeVariables();
        columnHeader = GameManager.Instance.GetTask();
        //WriteColumnHeaders(); // Écrire l'en-tête de colonne au début
        targetColumnIndex = GameManager.Instance.GetDataColumn();
    }

    public void LaunchDataRecording()
    {
        datarecording = true;
        this.gameObject.GetComponent<DataSaveBrut>().LaunchDataRecording();
    }

    void InitializeVariables()
    {
        userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        //filePath = @"C:\Users\paul-\UnityProject\RK_V2\Assets\Results\" + userName + "\\" + userName + ".csv";
        filePath = System.IO.Path.Combine(Application.dataPath, "Results", userName, userName + ".csv");
        sensibility = 0.66f;
        DM_Pressed = false;
        dataBuffer = new List<string>();
        buttonBuffer = new List<string>();
        timeStamp = 0;
    }

    void Update()
    {
        if (!timeReaction)
        {
            if ((datarecording && Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) >= 0.1f))
            {
                recording = true;
            }
            if (recording)
            {
                float currentForce = Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]);
                timeStamp += Time.deltaTime;

                if (currentForce > sensibility && !DM_Pressed)
                {
                    // Enregistrez le timestamp du moment où l'utilisateur commence à exercer une force
                    dataBuffer.Add(timeStamp + ""); // + ";"
                    buttonBuffer.Add("D"); //;
                    DM_Pressed = true;
                }
                else if (currentForce < sensibility * 0.5 && DM_Pressed)
                {
                    // Enregistrez le timestamp du moment où l'utilisateur cesse d'exercer la force
                    dataBuffer.Add(timeStamp + "");//;
                    buttonBuffer.Add("U");
                    DM_Pressed = false;
                }
            }
        }
    }

    public void SaveDataTimeReaction(string score)
    {
        dataBuffer.Add(score.ToString());
        SaveData();
    }

    public void SaveData(int startRowIndex = 3)
    {
        // Lire le contenu existant du fichier CSV
        List<string[]> allData = new List<string[]>();
        if (File.Exists(filePath))
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    allData.Add(sr.ReadLine().Split(';'));
                }
            }
        }

        // Remplir les lignes précédant la ligne de départ avec des cellules vides si nécessaire
        while (allData.Count < startRowIndex)
        {
            allData.Add(new string[targetColumnIndex + 2]); // +2 pour inclure l'espace pour buttonBuffer
        }

        // Vérifier et mettre à jour l'en-tête pour toutes les colonnes jusqu'à targetColumnIndex
        if (allData.Count > 0)
        {
            if (allData[startRowIndex - 1].Length <= targetColumnIndex + 1) // +1 pour buttonBuffer
            {
                string[] expandedHeader = new string[targetColumnIndex + 2]; // +2 pour buttonBuffer
                allData[startRowIndex - 1].CopyTo(expandedHeader, 0);
                allData[startRowIndex - 1] = expandedHeader;
            }

            allData[startRowIndex - 1][targetColumnIndex] = columnHeader;
            allData[startRowIndex - 1][targetColumnIndex + 1] = "Button"; // Ajouter l'en-tête pour buttonBuffer
        }
        else
        {
            // Créer la ligne d'en-tête si elle n'existe pas
            string[] headerRow = new string[targetColumnIndex + 2]; // +2 pour buttonBuffer
            headerRow[targetColumnIndex] = columnHeader;
            headerRow[targetColumnIndex + 1] = "Button"; // Ajouter l'en-tête pour buttonBuffer
            allData.Insert(startRowIndex - 1, headerRow);
        }

        // Ajouter les données dans la colonne cible pour dataBuffer et buttonBuffer
        for (int i = 0; i < Math.Max(dataBuffer.Count, buttonBuffer.Count); i++)
        {
            int rowIndex = startRowIndex + i;

            while (allData.Count <= rowIndex)
            {
                allData.Add(new string[targetColumnIndex + 2]);
            }

            // Étendre la ligne si nécessaire
            if (allData[rowIndex].Length <= targetColumnIndex + 1)
            {
                string[] tempArray = allData[rowIndex];
                Array.Resize(ref tempArray, targetColumnIndex + 2);
                allData[rowIndex] = tempArray;
            }

            // Écrire les données de dataBuffer et buttonBuffer
            if (i < dataBuffer.Count)
            {
                allData[rowIndex][targetColumnIndex] = dataBuffer[i];
            }

            if (i < buttonBuffer.Count)
            {
                allData[rowIndex][targetColumnIndex + 1] = buttonBuffer[i];
            }
        }


        // Écrire le nouveau contenu dans le fichier CSV
        using (StreamWriter sw = new StreamWriter(filePath, false)) // false pour écraser le fichier existant
        {
            foreach (string[] lineData in allData)
            {
                sw.WriteLine(string.Join(";", lineData));
            }
        }

        // Vider le buffer après l'enregistrement
        dataBuffer.Clear();
        buttonBuffer.Clear();
    }
}

/*using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

public class DataSave : MonoBehaviour
{
    string filePath;
    public float sensibility;
    bool DM_Pressed = false;
    List<string> dataBuffer;
    float timeStamp;
    string userName;
    bool datarecording = false;
    bool recording = false;
    int targetColumnIndex;
    string columnHeader;
    public bool timeReaction;
    List<string> dataBrutBuffer = new List<string>();

    void Start()
    {
        InitializeVariables();
        columnHeader = GameManager.Instance.GetTask();
        //WriteColumnHeaders(); // Écrire l'en-tête de colonne au début
        targetColumnIndex = GameManager.Instance.GetDataColumn();
    }

    public void LaunchDataRecording()
    {
        datarecording = true;
        this.gameObject.GetComponent<DataSaveBrut>().LaunchDataRecording();
    }

    void InitializeVariables()
    {
        userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        filePath = @"C:\Users\paul-\UnityProject\RK_V2\Assets\Results\" + userName + "\\" + userName + ".csv";
        sensibility = 0.66f;
        DM_Pressed = false;
        dataBuffer = new List<string>();
        timeStamp = 0;
    }

    void Update()
    {
        if (!timeReaction)
        {
            if ((datarecording && Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]) >= 0.1f))
            {
                recording = true;
            }
            if(recording)
            {
                float currentForce = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
                timeStamp += Time.deltaTime;

                if (currentForce > sensibility && !DM_Pressed)
                {
                    // Enregistrez le timestamp du moment où l'utilisateur commence à exercer une force
                    dataBuffer.Add(timeStamp + ";");
                    DM_Pressed = true;
                }
                else if (currentForce < sensibility * 0.5 && DM_Pressed)
                {
                    // Enregistrez le timestamp du moment où l'utilisateur cesse d'exercer la force
                    dataBuffer.Add(timeStamp + ";");
                    DM_Pressed = false;
                }
            }
        }
    }

    public void SaveDataTimeReaction(string score)
    {
        dataBuffer.Add(score.ToString());
        SaveData();
    }

    public void SaveData(int startRowIndex = 3)
    {
        // Lire le contenu existant du fichier CSV
        List<string[]> allData = new List<string[]>();
        if (File.Exists(filePath))
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    allData.Add(sr.ReadLine().Split(';'));
                }
            }
        }

        // Remplir les lignes précédant la ligne de départ avec des cellules vides si nécessaire
        while (allData.Count < startRowIndex)
        {
            allData.Add(new string[targetColumnIndex]); // Remplir avec des cellules vides
        }

        // Vérifier et mettre à jour l'en-tête pour toutes les colonnes jusqu'à targetColumnIndex
        if (allData.Count > 0)
        {
            if (allData[startRowIndex-1].Length <= targetColumnIndex)
            {
                string[] expandedHeader = new string[targetColumnIndex + 1];
                allData[startRowIndex-1].CopyTo(expandedHeader, 0);
                allData[startRowIndex-1] = expandedHeader;
            }

            allData[startRowIndex - 1][targetColumnIndex] = columnHeader;
        }
        else
        {
            // Créer la ligne d'en-tête si elle n'existe pas
            string[] headerRow = new string[targetColumnIndex + 1];
            headerRow[targetColumnIndex] = columnHeader;
            allData.Insert(startRowIndex-1, headerRow);
        }

        // Ajouter les données dans la colonne cible
        for (int i = 0; i < dataBuffer.Count; i++)
        {
            int rowIndex = startRowIndex + i;

            while (allData.Count <= rowIndex)
            {
                allData.Add(new string[targetColumnIndex + 1]);
            }

            if (allData[rowIndex].Length <= targetColumnIndex)
            {
                string[] expandedRow = new string[targetColumnIndex + 1];
                allData[rowIndex].CopyTo(expandedRow, 0);
                allData[rowIndex] = expandedRow;
            }

            allData[rowIndex][targetColumnIndex] = dataBuffer[i];
        }

        // Écrire le nouveau contenu dans le fichier CSV
        using (StreamWriter sw = new StreamWriter(filePath, false)) // false pour écraser le fichier existant
        {
            for (int i = 0; i < allData.Count; i++)
            {
                string[] lineData = allData[i];

                // S'assurer que chaque ligne a la longueur adéquate
                if (lineData.Length < targetColumnIndex + 1)
                {
                    Array.Resize(ref lineData, targetColumnIndex + 1);
                    allData[i] = lineData;
                }

                // Remplir les cellules manquantes avec des cellules vides pour maintenir la structure
                for (int j = lineData.Length; j < targetColumnIndex + 1; j++)
                {
                    lineData[j] = "";
                }

                // Écrire la ligne dans le fichier CSV
                sw.WriteLine(string.Join(";", lineData));
            }
        }

        // Vider le buffer après l'enregistrement
        dataBuffer.Clear();
    }

}*/

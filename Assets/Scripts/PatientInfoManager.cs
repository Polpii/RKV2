using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PatientInfoManager : MonoBehaviour
{
    public TMP_InputField NomInput;
    public TMP_InputField PrenomInput;
    public TMP_Dropdown SexeInput;
    public TMP_InputField AgeInput;
    public TMP_Dropdown MainInput;
    public TMP_Dropdown MusicienInput;
    public TMP_InputField MusicienAnneesInput;
    public TMP_InputField MusicienHeuresInput;
    public TMP_Dropdown JeuxVideosInput;

    string filePath;
    List<string> dataBuffer;
    string userName;

    public void SavePatientInfo()
    {
        string nom = NomInput.text;
        string prenom = PrenomInput.text;
        string sexe = SexeInput.options[SexeInput.value].text;
        string age = AgeInput.text;
        string main = MainInput.options[MainInput.value].text;
        string musicien = MusicienInput.options[MusicienInput.value].text;
        string musicienAnnees = MusicienAnneesInput.text;
        string musicienHeures = MusicienHeuresInput.text;
        string jeuxVideo = JeuxVideosInput.options[JeuxVideosInput.value].text;

        // Sauvegarder ces informations dans PlayerPrefs
        PlayerPrefs.SetString("PatientNom", nom);
        PlayerPrefs.SetString("PatientPrenom", prenom);
        PlayerPrefs.SetString("PatientSexe", sexe);
        PlayerPrefs.SetString("PatientAge", age);
        PlayerPrefs.SetString("PatientMain", main);
        PlayerPrefs.SetString("PatientMusicien", musicien);
        PlayerPrefs.SetString("PatientMusicienAnnees", musicienAnnees);
        PlayerPrefs.SetString("PatientMusicienHeures", musicienHeures);
        PlayerPrefs.SetString("PatientJeuxVideo", jeuxVideo);

        InitializeVariables();
        InitializeCsv(filePath, new string[9] { "Nom", "Prenom", "Sexe", "Age", "Main_Dominante", "Musicien", "Nb_Annees", "Nb_Heures", "Jeux_videos" });
        AppendToCsv(filePath, new string[9]{
            PlayerPrefs.GetString("PatientNom"),
            PlayerPrefs.GetString("PatientPrenom"),
            PlayerPrefs.GetString("PatientSexe"),
            PlayerPrefs.GetString("PatientAge"),
            PlayerPrefs.GetString("PatientMain"),
            PlayerPrefs.GetString("PatientMusicien"),
            PlayerPrefs.GetString("PatientMusicienAnnees"),
            PlayerPrefs.GetString("PatientMusicienHeures"),
            PlayerPrefs.GetString("PatientJeuxVideo"),
        });

        StartCoroutine(LaunchNextTaskAfterDelay());
    }

    void InitializeCsv(string filePath, string[] headers)
    {
        if (!File.Exists(filePath)) // Vérifiez si le fichier n'existe pas déjà
        {
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                sw.WriteLine(string.Join(";", headers)); // Écrivez les en-têtes dans le fichier CSV
            }
        }
    }

    void InitializeVariables()
    {
        userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        var folder = Directory.CreateDirectory(Path.Combine(Application.dataPath, "Results", userName));
        filePath = Path.Combine(Application.dataPath, "Results", userName, userName + ".csv");
        /*sensibility = GameObject.Find("GameManager").GetComponent<RK_controller>().sensibility;*/
        dataBuffer = new List<string>();
    }

    public void AppendToCsv(string filePath, string[] data)
    {
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(string.Join(";", data)); // Convertit le tableau de données en chaîne formatée CSV
        }
    }

    IEnumerator LaunchNextTaskAfterDelay()
    {
        yield return new WaitForSeconds(0);
        GameManager.Instance.CompleteTaskAndLoadNext();
    }
}

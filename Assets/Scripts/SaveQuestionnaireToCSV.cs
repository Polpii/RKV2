using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.IO;

public class SaveQuestionnaireToCSV : MonoBehaviour
{
    public GameObject questionnaireContent; // Assigner le GameObject Content

    public void SaveToCSV()
    {
        StringBuilder csv = new StringBuilder();

        // Ajouter l'en-tête du fichier CSV
        csv.AppendLine("Part;Question;Response");

        GameObject part0 = questionnaireContent.transform.Find("Part0").gameObject;
        for (int questionIndex = 1; questionIndex <= 3; questionIndex++)
        {
            GameObject question = part0.transform.Find("Question" + questionIndex).gameObject;
            ToggleGroup toggleGroup = question.GetComponentInChildren<ToggleGroup>();

            int selectedToggleIndex = GetSelectedToggleIndex(toggleGroup);
            csv.AppendLine($"{0};{questionIndex};{selectedToggleIndex}");
        }

        // Parcourir chaque Part et chaque Question
        for (int partIndex = 1; partIndex <= 5; partIndex++)
        {
            GameObject part = questionnaireContent.transform.Find("Part" + partIndex).gameObject;
            for (int questionIndex = 1; questionIndex <= 5; questionIndex++)
            {
                GameObject question = part.transform.Find("Question" + questionIndex).gameObject;
                ToggleGroup toggleGroup = question.GetComponentInChildren<ToggleGroup>();

                int selectedToggleIndex = GetSelectedToggleIndex(toggleGroup);
                csv.AppendLine($"{partIndex};{questionIndex};{selectedToggleIndex}");
            }
        }

        // Écriture dans un fichier CSV
        string userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        //string filePath = @"C:\Users\paul-\UnityProject\RK_V2\Assets\Results\" + userName + "\\" + userName + "_Questionnary.csv";
        string filePath = System.IO.Path.Combine(Application.dataPath, "Results", userName, userName + "_Questionnary.csv");
        File.WriteAllText(filePath, csv.ToString());
        StartCoroutine(LaunchNextTaskAfterDelay());
    }

    public void SaveToCSV2()
    {
        StringBuilder csv = new StringBuilder();

        // Ajouter l'en-tête du fichier CSV
        csv.AppendLine("Part;Question;Response");

        GameObject part0 = questionnaireContent.transform.Find("Part0").gameObject;
        for (int questionIndex = 1; questionIndex <= 1; questionIndex++)
        {
            GameObject question = part0.transform.Find("Question" + questionIndex).gameObject;
            ToggleGroup toggleGroup = question.GetComponentInChildren<ToggleGroup>();

            int selectedToggleIndex = GetSelectedToggleIndex(toggleGroup);
            csv.AppendLine($"{0};{questionIndex};{selectedToggleIndex}");
        }

        // Parcourir chaque Part et chaque Question
        for (int partIndex = 1; partIndex <= 5; partIndex += 2)
        {
            GameObject part = questionnaireContent.transform.Find("Part" + partIndex).gameObject;
            for (int questionIndex = 1; questionIndex <= 5; questionIndex++)
            {
                GameObject question = part.transform.Find("Question" + questionIndex).gameObject;
                ToggleGroup toggleGroup = question.GetComponentInChildren<ToggleGroup>();

                int selectedToggleIndex = GetSelectedToggleIndex(toggleGroup);
                csv.AppendLine($"{partIndex};{questionIndex};{selectedToggleIndex}");
            }
        }

        // Écriture dans un fichier CSV
        string userName = PlayerPrefs.GetString("PatientNom") + "_" + PlayerPrefs.GetString("PatientPrenom")[0];
        //string filePath = @"C:\Users\paul-\UnityProject\RK_V2\Assets\Results\" + userName + "\\" + userName + "_Questionnary.csv";
        string filePath = System.IO.Path.Combine(Application.dataPath, "Results", userName, userName + "_Questionnary.csv");

        File.WriteAllText(filePath, csv.ToString());
        StartCoroutine(LaunchNextTaskAfterDelay());
    }

    private int GetSelectedToggleIndex(ToggleGroup group)
    {
        Toggle[] toggles = group.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
                return i + 1; // Retourner l'index du Toggle sélectionné
        }
        return -1; // Aucune sélection
    }

    IEnumerator LaunchNextTaskAfterDelay()
    {
        yield return new WaitForSeconds(0);
        GameManager.Instance.CompleteTaskAndLoadNext();
    }
}

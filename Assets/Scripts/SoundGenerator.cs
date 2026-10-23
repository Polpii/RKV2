using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SoundGenerator : MonoBehaviour
{
    public float[] timestamps; // Les timestamps où les notes doivent être jouées
    public AudioClip[] notes;  // Les notes que vous pouvez jouer (doit être pré-assignées dans l'inspecteur Unity)

    private AudioSource audioSource;
    public string audioName;
    public int[] partition;

    public float sensibility = 0.66f;
    public float startingTime; // Temps d'attente avant de lancer les notes
    public bool availableRestart;
    bool dataRecording = false;
    public bool training;
    public bool silence;
    public int doigt = 1;
    private bool dataSaved = false;

    void Start()
    {
        // Initialisation
        InitializeGenerator();
    }

    void InitializeGenerator()
    {
        audioSource = GetComponent<AudioSource>();
        timestamps = GetTimestamps();
        partition = GetPartition();
        StartCoroutine(StartPlayingAfterDelay());
    }
    private void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            StartCoroutine(LaunchNextTaskAfterDelay(0));
        }
    }

    IEnumerator StartPlayingAfterDelay()
    {
        yield return new WaitForSeconds(startingTime); // Attendre pendant le temps défini par startingTime
        StartCoroutine(PlayNotesCoroutine());
    }

    IEnumerator PlayNotesCoroutine()
    {
        float startTime = Time.time;
        int i = 0;
        while (i < partition.Length)
        {            
            float targetTime = startTime + timestamps[2 * i]; // Utilisez toujours 2 * i si vous souhaitez conserver un timestamp sur deux
            yield return new WaitUntil(() => Time.time >= targetTime);

            audioSource.Stop(); // Arrête toute note actuellement jouée

            AudioClip noteToPlay = notes[partition[i]];
            audioSource.PlayOneShot(noteToPlay);

            i++;
        }
        StartCoroutine(PlayWithManipulandum());
    }

    IEnumerator PlayWithManipulandum()
    {
        GameObject.Find("AudioController").GetComponent<Manipulandum_data_aquired>().enabled = true;
        yield return new WaitForSeconds(1f);
        GameObject.Find("Jauge").GetComponent<JaugeController>().Reset();
        GameObject.Find("Jauge").GetComponent<JaugeController>().playing = true;
        audioSource.Stop();
        if (silence)
        {
            audioSource.volume = 0;
        }
        else
        {
            audioSource.volume = 1;
        }
        if (!dataRecording && !training)
        {
            GameObject.Find("GamaManager").GetComponent<DataSave>().LaunchDataRecording();
            dataRecording = true;            
        }
        int noteIndex = 0;
        while (noteIndex < partition.Length)
        {
            // Attendre que la force dépasse le seuil pour jouer une note
            yield return new WaitUntil(() => Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) > sensibility);

            // Jouer la note
            AudioClip noteToPlay = notes[partition[noteIndex]];            
            audioSource.PlayOneShot(noteToPlay);
            noteIndex++;

            // Attendre que la force redescende avant de permettre la prochaine note
            yield return new WaitUntil(() => Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) < sensibility * 0.5);
        }
        if (availableRestart)
        {            
            StartCoroutine(PlayWithManipulandum());
        }
        else if (!dataSaved)
        {
            while(dataSaved == false)
            {
                if (Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) < sensibility * 0.5)
                {
                    StartCoroutine(LaunchNextTaskAfterDelay(1));
                    dataSaved = true;
                }
            }
            
        }
    }

    public void Restart()
    {
        startingTime = 0;
        StopAllCoroutines(); // Arrêter toutes les coroutines en cours
        InitializeGenerator(); // Réinitialiser et relancer le processus
    }

    public float[] GetTimestamps()
    {
        string filePath = Path.Combine(Application.dataPath, "Timestamps", audioName + ".txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError("Le fichier n'existe pas : " + filePath);
            return new float[0];
        }

        List<float> timestamps = new List<float>();
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (float.TryParse(line, out float value))
            {
                timestamps.Add(value);
            }
            else
            {
                Debug.LogWarning("Valeur non valide trouvée dans le fichier : " + line);
            }
        }

        return timestamps.ToArray();
    }
    public int[] GetPartition()
    {
        string filePath = Path.Combine(Application.dataPath, "Partitions", audioName + ".txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError("Le fichier n'existe pas : " + filePath);
            return new int[0];
        }

        List<int> timestamps = new List<int>();
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (int.TryParse(line, out int value))
            {
                timestamps.Add(value);
            }
            else
            {
                Debug.LogWarning("Valeur non valide trouvée dans le fichier : " + line);
            }
        }

        return timestamps.ToArray();
    }

    IEnumerator LaunchNextTaskAfterDelay(float launchingTime)
    {
        yield return new WaitForSeconds(0.2f);
        /*if (!dataSaved)
        {
            GameObject.Find("GamaManager").GetComponent<DataSave>().SaveData();
            GameObject.Find("GamaManager").GetComponent<DataSaveBrut>().SaveDataBrut();
            dataSaved = true;
        }*/
        GameObject.Find("GamaManager").GetComponent<DataSave>().SaveData();
        //GameObject.Find("GamaManager").GetComponent<DataSaveBrut>().SaveDataBrut();
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(!training);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSegmentPlayerSAVED : MonoBehaviour
{
    public float[] timestamps;
    public float thresholdLow = 0.33f;
    public float thresholdHigh = 0.66f;
    public float startingTime;
    public bool availableRestart;
    public string audioName;
    public bool training;
    public bool Silence;
    public int doigt = 4;

    private int currentSegmentIndex = 0;
    private float lastPressure = 0f;
    private FMOD.Studio.EventInstance playBackSpeed;
    private float playSpeed = 1f;
    private bool isPlaying = false;
    private int currentTime;
    AudioSource audioData;
    bool audioListening = true;
    bool coroutineStarted = false;
    bool dataRecording = false;
    bool dataSaved = false;

    private readonly float[] playSpeedValues = { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2.0f };

    private void Start()
    {
        timestamps = GetTimestamps();
        InitializePlaybackSpeed();
        audioData = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            StartCoroutine(LaunchNextTaskAfterDelay(0));
        }
        if (audioListening && !audioData.isPlaying)
        {
            if (!coroutineStarted)
            {
                StartCoroutine(StartPlayingAfterDelay());
                coroutineStarted = true;
            }
        }
        else if (!audioListening && !audioData.isPlaying)
        {
            GameObject.Find("AudioController").GetComponent<Manipulandum_data_aquired>().enabled = true;
            GameObject.Find("Jauge").GetComponent<JaugeController>().playing = true;

            if (!dataRecording && !training)
            {
                GameObject.Find("GamaManager").GetComponent<DataSave>().LaunchDataRecording();
                dataRecording = true;
            }
            if (currentSegmentIndex >= timestamps.Length - 2 && availableRestart)
            {
                RestartFmodPlayback();
                GameObject.Find("Jauge").GetComponent<JaugeController>().Reset(1);
                currentSegmentIndex = 0;
                playSpeed = 1f;
            }
            else if (currentSegmentIndex >= timestamps.Length - 2 && !availableRestart)
            {
                if (!dataSaved && Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) < thresholdLow)
                {
                    StartCoroutine(LaunchNextTaskAfterDelay(1));
                    dataSaved = true;
                }
            }
            ProcessAudioPlayback();
        }
    }

    IEnumerator StartPlayingAfterDelay()
    {
        yield return new WaitForSeconds(startingTime); // Attendre pendant le temps défini par startingTime
        audioListening = false;
        audioData.Play();
    }

    private void RestartFmodPlayback()
    {
        playBackSpeed.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        playBackSpeed.start();
        SetPlaybackParameter(1f); // Réinitialiser la vitesse de lecture
    }

    public float[] GetTimestamps()
    {
        string filePath = Path.Combine(Application.dataPath, "Timestamps", audioName + ".txt");

        if (!File.Exists(filePath))
        {
            //Debug.LogError("Le fichier n'existe pas : " + filePath);
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

    private void InitializePlaybackSpeed()
    {
        playBackSpeed = RuntimeManager.CreateInstance("event:/" + audioName);
        playBackSpeed.start();
        SetPlaybackParameter(playSpeed);
        playBackSpeed.setPaused(true);
    }

    public void Restart()
    {
        startingTime = 0;
        audioListening = true;
        currentSegmentIndex = 0;
        lastPressure = 0f;
        StopAllCoroutines();
        StartCoroutine(StartPlayingAfterDelay());
    }

    private void ProcessAudioPlayback()
    {
        bool isReversed = currentSegmentIndex % 2 != 0;
        float currentPressure = GetCurrentPressure(isReversed);

        if (currentPressure >= thresholdLow)
        {
            isPlaying = true;
        }
        if (isPlaying)
        {
            PlayAudioSegment(currentPressure, isReversed);
        }
    }

    private float GetCurrentPressure(bool isReversed)
    {
        // Remplacer par l'acquisition réelle de la pression
        float pressure = Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]);
        if ((isReversed && pressure > lastPressure) || (!isReversed && pressure < lastPressure))
        {
            pressure = lastPressure;
        }
        return pressure;
    }

    private void PlayAudioSegment(float currentPressure, bool isReversed)
    {
        if (Silence)
        {
            playBackSpeed.setVolume(0);
        }
        else
        {
            playBackSpeed.setVolume(1);
        }
        lastPressure = currentPressure;
        playBackSpeed.getTimelinePosition(out currentTime);
        float newTime = CalculateSegmentTime(currentPressure, isReversed) * 1000;
        float tolerance = 100;

        // Smoothly adjust the playback speed and select the closest value
        playSpeed = Mathf.Lerp(playSpeed, CalculatePlaybackSpeed(currentTime, newTime), Time.deltaTime * 5);
        playSpeed = GetClosestPlaySpeed(playSpeed);
        SetPlaybackParameter(playSpeed);

        if (Math.Abs(currentTime - newTime) < tolerance)
        {
            playBackSpeed.setPaused(true);
        }
        else
        {
            playBackSpeed.setPaused(false);
        }

        if ((isReversed && currentPressure <= thresholdLow) || (!isReversed && currentPressure >= thresholdHigh))
        {
            if (currentSegmentIndex < timestamps.Length - 2)
            {
                currentSegmentIndex++;
            }
            //currentSegmentIndex = (currentSegmentIndex + 1) % timestamps.Length;
        }
    }

    private float CalculateSegmentTime(float currentPressure, bool isReversed)
    {
        float segmentStart = timestamps[currentSegmentIndex];
        float segmentEnd = timestamps[currentSegmentIndex + 1];
        float normalizedPressure = (currentPressure - thresholdLow) / (thresholdHigh - thresholdLow);
        return isReversed ? segmentEnd - normalizedPressure * (segmentEnd - segmentStart)
                          : segmentStart + normalizedPressure * (segmentEnd - segmentStart);
    }

    private float CalculatePlaybackSpeed(float currentTime, float targetTime)
    {
        float timeDifference = targetTime - currentTime;
        float speedSensitivity = 0.001f;
        float speedAdjustment = speedSensitivity * timeDifference;
        return Mathf.Clamp(1.0f + speedAdjustment * 0.5f, 0.5f, 2f); // Clamp to ensure speed is within acceptable range
    }

    private float GetClosestPlaySpeed(float speed)
    {
        float closestSpeed = playSpeedValues[0];
        float minDifference = Mathf.Abs(playSpeedValues[0] - speed);

        foreach (float possibleSpeed in playSpeedValues)
        {
            float difference = Mathf.Abs(possibleSpeed - speed);
            if (difference < minDifference)
            {
                closestSpeed = possibleSpeed;
                minDifference = difference;
            }
        }

        return closestSpeed;
    }

    private void SetPlaybackParameter(float value)
    {
        playBackSpeed.setParameterByName("PlaybackSpeed", value);
    }

    IEnumerator LaunchNextTaskAfterDelay(float launchingTime)
    {
        if (!dataSaved)
        {
            GameObject.Find("GamaManager").GetComponent<DataSave>().SaveData();
            //GameObject.Find("GamaManager").GetComponent<DataSaveBrut>().SaveDataBrut();
            dataSaved = true;
        }
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(!training);
    }
}
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSegmentPlayerSAVED : MonoBehaviour
{
    public float[] timestamps;
    public float thresholdLow = 0.33f;
    public float thresholdHigh = 0.66f;
    public string audioName;

    private int currentSegmentIndex = 0;
    private AudioSource audioSource;
    private bool pairTimestamps = true;
    private int thresholdCrossings = 0;

    void Start()
    {
        timestamps = GetTimestamps();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        float currentPressure = GetCurrentPressure();
        CheckPressureThreshold(currentPressure);

        if (!audioSource.isPlaying && currentSegmentIndex < timestamps.Length - 1 && currentSegmentIndex < thresholdCrossings)
        {
            StartSegmentPlayback();
        }

        if (audioSource.isPlaying)
        {
            ManageAudioPlayback();
        }
    }

    float GetCurrentPressure()
    {
        // Cette méthode doit être connectée à votre système de mesure de pression
        return Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
    }

    void CheckPressureThreshold(float pressure)
    {
        if (pressure >= thresholdHigh && pairTimestamps)
        {
            pairTimestamps = false;
            thresholdCrossings++;
        }
        else if (pressure <= thresholdLow && !pairTimestamps)
        {
            pairTimestamps = true;
            thresholdCrossings++;
        }
    }

    void StartSegmentPlayback()
    {
        audioSource.time = timestamps[currentSegmentIndex];
        audioSource.Play();
    }

    void ManageAudioPlayback()
    {
        if (audioSource.time >= timestamps[currentSegmentIndex + 1])
        {
            audioSource.Pause(); // Pause au timestamp exact
            currentSegmentIndex++;
            if (currentSegmentIndex < thresholdCrossings && currentSegmentIndex < timestamps.Length - 1)
            {
                StartSegmentPlayback();  // Commence le prochain segment immédiatement si le seuil est déjà franchi
            }
        }
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
}

/*
Bon maintenant on va devoir réfléchir ensemble. Avant d'écrire un nouveau code je veux d'abord qu'on valide ensemble. Tu n'as pas le droit de me proposer un nouveau code avant que j'ai validé l'idée ok? 

Voici le problème: l'utilisateur doit appuyer sur le bouton pour recevoir un feedback audio de notre jeu. Mais vu qu'il appuie avant de recevoir le feedback audio et que le feedback audio se lance seulement quand l'utilisateur passe le seuil bas ou le seuil haut, il va y avoir un delais entre le mouvement de l'utilisateur et le feedback auditif.
Donc la solution serait de réussir à synchroniser la tête de lecture du curseur audio au mouvement de l'utilisateur. Le problème est qu'il ne faut pas perdre en qualité vocale ou en qualité audio.

Qu'est ce que tu proposes? 
Moi j'ai pensé à:

- rescale la pression appliquer par l'utilisateur entre 0% et 100%: seuil bas = 0% et seuil haut 100% et vice versa quand pairTimestamps == false. Ensuite il suffit de téléporter la tête de lecture audio au bon temps. On calculera le temps en considérent le timestamp n-1 à 0% et le timestamp n à 100% Le problème avec cette solution est que vu la tête de lecture va se téléporter cela va s'entendre à l'oreille. Ca risque de baisse la qualité audio non? Je pense qu'on va avoir un feedback auditif un peu robotisé.
- Changer en live le temps auquel l'audio doit s'arrêter. Donc on fait à peu près pareil que pour la première solution sauf qu'on essaie de faire en sorte que la tête de lecture suive de très prêt le mouvement de l'utilisateur. Mais vu que la tête de lecture ne doit pas dépasser un certain temps fixé, on va devoir faire tout le temps des pauses et pareil ça va baisser la qualité audio. non?
- Changer la vitesse de la tête de lecture en live et faire en sorte d'à peu près suivre le mouvement de l'utilisateur mais la vitesse va tout le temps varier et cela va aussi s'entendre à l'oreille. 

Bon que penses tu de mes idées? Laquelle est la meilleur? Est ce que tu penses qu'on peut les améliorer? Est ce que tu en as d'autres? Il faut vraiment qu'on réfléchisse en terme d'algorythme.
N'écrit aucun code, je veux qu'on discute ensemble.

*/


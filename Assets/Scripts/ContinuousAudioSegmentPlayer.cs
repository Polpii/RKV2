/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSegmentPlayer : MonoBehaviour
{
    public float[] timestamps;
    public float thresholdLow = 0.33f;
    public float thresholdHigh = 0.66f;
    public string audioName;
    public float startingTime;
    public bool availableRestart;
    public bool training;
    public bool Silence;

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
                if (!dataSaved && Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]) < thresholdLow)
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

    private void InitializePlaybackSpeed()
    {
        playBackSpeed = RuntimeManager.CreateInstance("event:/" + audioName);
        playBackSpeed.start();
        SetPlaybackParameter(1f); // Réinitialiser la vitesse de lecture
        playBackSpeed.setPaused(true);
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
        float pressure = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
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

        float segmentStart = timestamps[currentSegmentIndex];
        float segmentEnd = timestamps[currentSegmentIndex + 1];
        float normalizedPressure = (currentPressure - thresholdLow) / (thresholdHigh - thresholdLow);
        float targetTime = isReversed ? segmentEnd - normalizedPressure * (segmentEnd - segmentStart) : segmentStart + normalizedPressure * (segmentEnd - segmentStart);

        float tolerance = 100; // Adjust this value as necessary
        if (Math.Abs(currentTime - targetTime * 1000) > tolerance)
        {
            float timeDifference = (targetTime * 1000 - currentTime) / 1000f;
            float speedAdjustment = Mathf.Clamp(timeDifference, -0.5f, 1f); // Smooth adjustment
            playSpeed = Mathf.Clamp(1f + speedAdjustment, 0.5f, 2f);
            Debug.Log(playSpeed);
            SetPlaybackParameter(playSpeed);
        }
        else
        {
            SetPlaybackParameter(1f);
        }

        if (currentTime >= targetTime * 1000 + tolerance)
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
        }
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
            GameObject.Find("GamaManager").GetComponent<DataSaveBrut>().SaveDataBrut();
            dataSaved = true;
        }
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(!training);
    }
}
//TEST 2
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSegmentPlayer : MonoBehaviour
{
    public float[] timestamps;
    public float thresholdLow = 0.33f;
    public float thresholdHigh = 0.66f;
    public float startingTime;
    public bool availableRestart;
    public string audioName;
    public bool training;
    public bool Silence;

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
                if (!dataSaved && Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]) < thresholdLow)
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
        float pressure = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);
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
            GameObject.Find("GamaManager").GetComponent<DataSaveBrut>().SaveDataBrut();
            dataSaved = true;
        }
        yield return new WaitForSeconds(launchingTime);
        GameManager.Instance.CompleteTaskAndLoadNext(!training);
    }
}
*/

// LE VRAI CODE EST LE SUIVANT


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSegmentPlayer : MonoBehaviour
{
    public float[] timestamps;
    public float thresholdLow = 0.33f;
    public float thresholdHigh = 0.66f;
    public float startingTime;
    public bool availableRestart;
    public string audioName;
    public bool training;
    public bool Silence;
    public int doigt = 1;

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
        if (Math.Abs(currentTime - newTime) < tolerance)
        {
            SetPlaybackParameter(0.5f);
        }
        else
        {
            playSpeed = CalculatePlaybackSpeed(currentTime, newTime);
            SetPlaybackParameter(playSpeed);
        }

        if (currentTime >= newTime + tolerance)
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


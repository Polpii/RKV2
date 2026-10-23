using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;

[RequireComponent(typeof(AudioSource))]
public class save_continous_fader : MonoBehaviour
{
    public float[] timestamps; // Les timestamps qui délimitent les segments
    public float thresholdLow = 0.33f; // Seuil bas S1
    public float thresholdHigh = 0.66f; // Seuil haut S2

    private int currentSegmentIndex = 0; // Index du segment actuel
    private float lastPressure = 0f; // Dernière valeur de pression enregistrée

    private int currentTime;
    float playSpeed;
    bool isPlaying;

    private FMOD.Studio.EventInstance playBackSpeed;


    private void Start()
    {
        isPlaying = false;
        float[] Timestamps = new float[timestamps.Length + 1];
        Timestamps[0] = 0;
        for (int i = 0; i < timestamps.Length; i++)
        {
            Timestamps[i + 1] = timestamps[i];
        }
        timestamps = Timestamps;
        playSpeed = 1f;
        playBackSpeed = RuntimeManager.CreateInstance("event:/PlaybackSpeed");
        playBackSpeed.start();
        SetParameter(playSpeed);
        playBackSpeed.setPaused(true);
        playBackSpeed.getTimelinePosition(out currentTime);
    }

    private void Update()
    {
        // Détermine si nous devrions jouer le segment en avant ou à l'envers
        bool isReversed = currentSegmentIndex % 2 != 0;

        float currentPressure = GetCurrentPressure(isReversed);
        //float currentPressure = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]);

        if(currentPressure >= thresholdLow)
        {
            isPlaying = true;
        }
        if (isPlaying)
        {
            // Joue le segment audio en fonction de la pression actuelle
            PlayAudioSegmentBasedOnPressure(currentPressure, isReversed);
        }
    }

    public void SetParameter(float value)
    {
        playBackSpeed.setParameterByName("PlaybackSpeed", value); // Remplacez "ParameterName" par le nom de votre paramètre
    }

    private float GetCurrentPressure(bool isReversed)
    {
        // À remplacer par l'acquisition réelle de la pression de votre dispositif de mesure
        float pressure = Math.Abs((float)Manipulandum_data_aquired.Force_Data[1]); // Valeur simulée pour l'exemple
        if ((isReversed && pressure > lastPressure) || (!isReversed && pressure < lastPressure))
        {
            pressure = lastPressure;
        }
        return pressure;
    }

    private void PlayAudioSegmentBasedOnPressure(float currentPressure, bool isReversed)
    {

        lastPressure = currentPressure; // Mise à jour de la dernière pression enregistrée

        playBackSpeed.getTimelinePosition(out currentTime);
        // Calculer la nouvelle position de lecture basée sur la pression
        float newTime = GetSegmentTime(currentPressure, isReversed);        

        playSpeed = CalculatePlaybackSpeed(currentTime / 1000, newTime);
        SetParameter(playSpeed);

        if(playSpeed < 0.5f)
        {
            playBackSpeed.setPaused(true);
        }
        else
        {
            playBackSpeed.setPaused(false);
        }

        // Passer au prochain segment si nécessaire
        if ((isReversed && currentPressure <= thresholdLow) ||
            (!isReversed && currentPressure >= thresholdHigh))
        {
            currentSegmentIndex = (currentSegmentIndex + 1) % timestamps.Length;
            //Debug.Log(currentSegmentIndex);
        }
    }

    private float GetSegmentTime(float currentPressure, bool isReversed)
    {
        float segmentStart = timestamps[currentSegmentIndex];
        float segmentEnd = timestamps[currentSegmentIndex + 1];
        float normalizedPressure = (currentPressure - thresholdLow) / (thresholdHigh - thresholdLow);

        // Si le segment est à l'envers, inverser la progression
        if (isReversed)
        {
            return segmentEnd - normalizedPressure * (segmentEnd - segmentStart);
        }

        return segmentStart + normalizedPressure * (segmentEnd - segmentStart);
    }

    private float CalculatePlaybackSpeed(float currentTime, float targetTime)
    {
        // Différence de temps (en secondes)
        float timeDifference = targetTime - currentTime;

        // Sensibilité de la vitesse - Ajustez cette valeur selon les besoins
        float speedSensitivity = 1f;

        // Calculer la vitesse de lecture ajustée
        // La formule suivante peut être affinée pour répondre à vos exigences spécifiques
        float speedAdjustment = speedSensitivity * timeDifference;
        float playbackSpeed = 1.0f + speedAdjustment;

        //Debug.Log(playbackSpeed);
        // Assurer que la vitesse de lecture reste dans les limites acceptables
        return playbackSpeed;
    }

}

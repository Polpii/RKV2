using UnityEngine;
using UnityEngine.UI;
using System;
public class JaugeController : MonoBehaviour
{
    public Image jaugeImage; // Assignez cela dans l'inspecteur
    public int nombreTotalAppels; // Définissez cela dans l'inspecteur
    public bool playing = false;
    public int doigt = 1;

    private int nombreAppelsActuels = 0;

    private bool DM_Pressed = false;

    private void Update()
    {
        if (playing)
        {
            if (Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) > 0.66f && !DM_Pressed)
            {
                DM_Pressed = true;
                IncrementerJauge();
            }
            if (Math.Abs((float)Manipulandum_data_aquired.Force_Data[doigt]) < 0.33f && DM_Pressed)
            {
                DM_Pressed = false;
                //IncrementerJauge();
            }
        }        
    }
    public void IncrementerJauge()
    {
        if (nombreAppelsActuels < nombreTotalAppels)
        {
            nombreAppelsActuels++;
            jaugeImage.fillAmount = (float)nombreAppelsActuels / nombreTotalAppels;
        }
    }
    public void Reset(int nb_appels = 0)
    {
        nombreAppelsActuels = nb_appels;
        jaugeImage.fillAmount = (float)nombreAppelsActuels / nombreTotalAppels;
    }
}

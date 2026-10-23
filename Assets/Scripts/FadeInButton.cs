using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInButton : MonoBehaviour
{
    public Button buttonToFadeIn; // Assigner le bouton à faire apparaître graduellement
    public float appearingDuration = 3;
    public float appearingTime = 5;

    void Start()
    {
        // Démarrer la coroutine après un délai de 5 secondes
        StartCoroutine(FadeInAfterDelay(appearingTime, appearingDuration));
    }

    IEnumerator FadeInAfterDelay(float delaySeconds, float fadeInSeconds)
    {
        // Attendre le délai initial
        yield return new WaitForSeconds(delaySeconds);

        // Obtenir les composants Image et Text du bouton
        Image buttonImage = buttonToFadeIn.GetComponent<Image>();
        Text buttonText = buttonToFadeIn.GetComponentInChildren<Text>();

        // Initialiser la transparence à 0
        if (buttonImage != null) buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0);
        if (buttonText != null) buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0);

        float elapsedTime = 0;

        // Animation de l'opacité
        while (elapsedTime < fadeInSeconds)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInSeconds);

            if (buttonImage != null) buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, alpha);
            if (buttonText != null) buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, alpha);

            yield return null;
        }
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

public class AppearingText : MonoBehaviour
{
    private TextMeshPro textMeshProComponent;
    public float duration = 5f; // Durée totale de l'effet d'apparition
    public float startingTime = 2f; // Temps avant que le texte commence à apparaître
    public float endingTime = 7f; // Temps avant que le texte commence à disparaître
    private float fadeOutDuration = 1.5f; // Durée de l'effet de disparition

    void Start()
    {
        textMeshProComponent = GetComponent<TextMeshPro>();
        if (textMeshProComponent != null)
        {
            StartCoroutine(HandleTextVisibility());
        }
    }

    private IEnumerator HandleTextVisibility()
    {
        // Attendre avant de commencer l'apparition
        yield return new WaitForSeconds(startingTime);

        // Démarrer l'apparition
        yield return StartCoroutine(FadeTextToFullAlpha(duration, textMeshProComponent));

        if (endingTime > 0)
        {
            // Attendre avant de commencer la disparition
            yield return new WaitForSeconds(endingTime - startingTime - duration);

            // Démarrer la disparition
            yield return StartCoroutine(FadeTextToZeroAlpha(fadeOutDuration, textMeshProComponent));
        }
    }

    public IEnumerator FadeTextToFullAlpha(float t, TextMeshPro textComponent)
    {
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0);
        while (textComponent.color.a < 1.0f)
        {
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, textComponent.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    public IEnumerator FadeTextToZeroAlpha(float t, TextMeshPro textComponent)
    {
        while (textComponent.color.a > 0.0f)
        {
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, textComponent.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }
}

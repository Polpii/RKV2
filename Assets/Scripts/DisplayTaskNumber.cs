using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Assurez-vous d'inclure l'espace de noms pour TextMeshPro

public class DisplayTaskNumber : MonoBehaviour
{
    private TextMeshPro textComponent;

    void Start()
    {
        // Obtenez le composant TextMeshProUGUI attaché à cet objet
        textComponent = GetComponent<TextMeshPro>();

        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on the object.");
            return;
        }

        UpdateText();
    }

    void UpdateText()
    {
        // Mettez à jour le texte en utilisant l'index du GameManager
        textComponent.text = "Essai: " + ((GameManager.Instance.currentTask)) + " / 24";
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Random = System.Random;

public class OrderWriter_Study2 : MonoBehaviour
{
    private static string path;
    //private static readonly string path = "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Order.txt";
    private static readonly List<string> initialItems = new List<string>
    {
        "PatientInfo", "Consignes", "Consignes2", "Consignes3", "Conseils",
        "Training", "Training2", "Training3", "Lancement"
    };
    private static readonly List<string> poolItems = new List<string>
    {
        "Balou_B", "Balou_BN", "Balou_M", "Balou_MN", "Balou_S", "Balou_SN",
        "Balou_SM", "Balou_SMN"
    };

    private void Start()
    {
        path = System.IO.Path.Combine(Application.dataPath, "Order.txt");
        WriteOrderFile();
    }

    public static void WriteOrderFile()
    {
        using (StreamWriter sw = new StreamWriter(path, false))
        {
            // Écrire les éléments initiaux
            foreach (string item in initialItems)
            {
                sw.WriteLine(item);
            }

            // Écrire les pools
            for (int i = 0; i < 3; i++)
            {
                WritePool(sw); // Ajouter "Moitié" au milieu de la deuxième pool
                if (i == 2)
                {
                    sw.WriteLine("Questionnary");
                    sw.WriteLine("Fin");
                }
            }
        }
    }
    private static List<string> ShufflePoolItems(List<string> items)
    {
        Random rng = new Random();
        List<string> shuffled = new List<string>(items);
        int n = shuffled.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }

        // Vérifier les doublons consécutifs et les échanger si nécessaire
        for (int i = 0; i < shuffled.Count - 1; i++)
        {
            string currentBase = GetBaseSound(shuffled[i]);
            string nextBase = GetBaseSound(shuffled[i + 1]);

            if (currentBase == nextBase)
            {
                // Trouver un élément non-doublon à échanger
                for (int j = i + 2; j < shuffled.Count; j++)
                {
                    if (GetBaseSound(shuffled[j]) != currentBase)
                    {
                        string temp = shuffled[i + 1];
                        shuffled[i + 1] = shuffled[j];
                        shuffled[j] = temp;
                        break;
                    }
                }
            }
        }

        return shuffled;
    }

    private static string GetBaseSound(string item)
    {
        // Extraire la partie "base" du nom de l'élément (par exemple, "Balou" de "Balou_BN")
        return item.Split('_')[0];
    }

    private static void WritePool(StreamWriter sw)
    {
        List<string> shuffledPool = ShufflePoolItems(poolItems);
        for (int i = 0; i < shuffledPool.Count; i++)
        {
            sw.WriteLine(shuffledPool[i]);
        }
    }
}

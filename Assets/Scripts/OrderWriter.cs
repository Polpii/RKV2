using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Random = System.Random;

public class OrderWriter : MonoBehaviour
{
    private static string path => Path.Combine(Application.dataPath, "Order.txt");
    /*private static readonly List<string> initialItems = new List<string>
    {
        "PatientInfo", "Consignes", "Consignes2", "Consignes3", "Conseils",
        "Training", "Training2", "Training3", "ReflexTest", "ReflexTestNext",
        "ReflexTestNext", "Lancement"
    };
    private static readonly List<string> poolItems = new List<string>
    {
        "Balou_B", "Balou_BN", "Balou_M", "Balou_MN", "Balou_S", "Balou_SN",
        "Balou_SM", "Balou_SMN", "Papaoutai_B", "Papaoutai_BN", "Papaoutai_M",
        "Papaoutai_MN", "Papaoutai_S", "Papaoutai_SN", "Papaoutai_SM",
        "Papaoutai_SMN", "Piaf_B", "Piaf_BN", "Piaf_M", "Piaf_MN", "Piaf_S",
        "Piaf_SN", "Piaf_SM", "Piaf_SMN"
    };*/
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
                WritePool(sw, i == 1); // Ajouter "Moitié" au milieu de la deuxième pool
                if (i < 2)
                {
                    /*sw.WriteLine("ReflexTest");
                    sw.WriteLine("ReflexTestNext");
                    sw.WriteLine("ReflexTestNext");
                    sw.WriteLine("Pause");*/
                }
                else
                {
                    /*sw.WriteLine("ReflexTest");
                    sw.WriteLine("ReflexTestNext");
                    sw.WriteLine("ReflexTestNext");*/
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
                bool swapFound = false;
                for (int j = i + 2; j < shuffled.Count; j++)
                {
                    if (GetBaseSound(shuffled[j]) != currentBase)
                    {
                        string temp = shuffled[i + 1];
                        shuffled[i + 1] = shuffled[j];
                        shuffled[j] = temp;
                        swapFound = true;
                        break;
                    }
                }

                // Dans le cas rare où aucun échange n'est possible, on refait le mélange
                if (!swapFound)
                {
                    return ShufflePoolItems(items);
                }
            }
        }

        return shuffled;
    }

    private static string GetBaseSound(string item)
    {
        // Extraire la partie "base" du nom de l'élément (par exemple, "Balou" de "Balou_BN")
        Debug.Log(item);
        return item.Split('_')[0];
    }

    private static void WritePool(StreamWriter sw, bool addHalfway)
    {
        List<string> shuffledPool = ShufflePoolItems(poolItems);
        for (int i = 0; i < shuffledPool.Count; i++)
        {
            if (addHalfway && i == shuffledPool.Count / 2)
            {
                /*sw.WriteLine("Moitié");*/
            }
            sw.WriteLine(shuffledPool[i]);
        }
    }
}

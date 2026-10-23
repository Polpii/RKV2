using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class Manipulandum_data_aquired : MonoBehaviour
{
    [DllImport("DAQMANIPULANDUM", EntryPoint = "DaqAcq")]
    public static extern int DaqAcq();
    [DllImport("DAQMANIPULANDUM", EntryPoint = "DaqStart")]
    public static extern int DaqStart();
    [DllImport("DAQMANIPULANDUM", EntryPoint = "DaqStop")]
    public static extern int DaqStop();
    [DllImport("DAQMANIPULANDUM", EntryPoint = "DaqClear")]
    public static extern int DaqClear();
    [DllImport("DAQMANIPULANDUM", EntryPoint = "DaqRead")]
    public static extern int DaqRead();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpFileName);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    public static double[] Raw_Data = new double[5];
    private static double[] Zero_Data = new double[5];
    public static double[] Force_Data = new double[5];
    private static double[] Force_Data_old = new double[5];


    private double[] Force_max = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    private bool ChangementForceMax = false;
    public static int N_Force_Max = 1;
    //Manipulandum 6ers kits
    //private double[] Calibration_Data = { 1.6f, -1.6f, -1.6f, -1.6f, -1.6f };
    //Manipulandum proto
    private double[] Calibration_G1i = { 1.6f, 1.6f, 1.6f, 1.6f, 1.6f };
    private double[] Calibration_G2i = { 0.85f, 0.85f, 0.85f, 0.85f, 0.85f };
    private double[] Calibration_C2i = { 0.45f, 0.45f, 0.45f, 0.45f, 0.45f };
    private double[] Intersection_Xi = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

    //Détection
    private static float Threshold = 0.5f;
    public static bool[] rising_edge = new bool[5];
    public static bool rising_phase;
    public static bool[] falling_edge = new bool[5];
    public static bool falling_phase;
    private static int dec;
    private static int inc;
    public static bool tap_phase;

    //Déclaration des paramètres de suivi temporel
    public static float dtMean;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            if (!Directory.Exists(Application.persistentDataPath + "\\Config files\\"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "\\Config files\\");
            }
            if (File.Exists(Application.persistentDataPath + "\\Config files\\Manipulandum_calibration.txt"))
            {
                string[] dataArray = File.ReadAllLines(Application.persistentDataPath + "\\Config files\\Manipulandum_calibration.txt");
                string[] G1_line = dataArray[3].Split('\t');
                string[] G2_line = dataArray[4].Split('\t');
                string[] C2_line = dataArray[5].Split('\t');

                for (int i = 0; i < 5; i++)
                {
                    Calibration_G1i[i] = Convert.ToDouble(double.Parse(G1_line[i + 1], new CultureInfo("en-US")));
                    Calibration_G2i[i] = Convert.ToDouble(double.Parse(G2_line[i + 1], new CultureInfo("en-US")));
                    Calibration_C2i[i] = Convert.ToDouble(double.Parse(C2_line[i + 1], new CultureInfo("en-US")));
                    Intersection_Xi[i] = Convert.ToDouble(Calibration_C2i[i] / (Calibration_G1i[i] - Calibration_G2i[i]));
                }
            }
        }
        else
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Config files")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Config files"));
            }
            if (File.Exists(Path.Combine(Application.dataPath, "Config files", "Manipulandum_calibration.txt")))
            {
                string[] dataArray = File.ReadAllLines(Path.Combine(Application.dataPath, "Config files", "Manipulandum_calibration.txt"));
                string[] G1_line = dataArray[3].Split('\t');
                string[] G2_line = dataArray[4].Split('\t');
                string[] C2_line = dataArray[5].Split('\t');

                for (int i = 0; i < 5; i++)
                {
                    Calibration_G1i[i] = Convert.ToDouble(double.Parse(G1_line[i + 1], new CultureInfo("en-US")));
                    Calibration_G2i[i] = Convert.ToDouble(double.Parse(G2_line[i + 1], new CultureInfo("en-US")));
                    Calibration_C2i[i] = Convert.ToDouble(double.Parse(C2_line[i + 1], new CultureInfo("en-US")));
                    Intersection_Xi[i] = Convert.ToDouble(Calibration_C2i[i] / (Calibration_G1i[i] - Calibration_G2i[i]));
                }
            }
        }

        DaqAcq();
        read_data(0);
        Threshold = 0.25f + 0.25f * N_Force_Max;
        Force_Data_old = new double[5];
        rising_edge = new bool[5];
        falling_edge = new bool[5];
        rising_phase = false;
        falling_phase = false;

        dtMean = 0;

        ChangementForceMax = false;
    }

    void Update()
    {
        DaqAcq();
        read_data(1);
    }

    void read_data(int mode)
    {
        IntPtr hModule = Manipulandum_data_aquired.LoadLibrary("DAQMANIPULANDUM.dll");
        IntPtr hData = Manipulandum_data_aquired.GetProcAddress(hModule, "Data");

        double[] Data = new double[10];
        Marshal.Copy(hData, Data, 0, 10);

        if (mode == 1)
        {
            rising_phase = false;
            falling_phase = false;
            tap_phase = false;
            for (int i = 0; i < 5; i++)
            {
                Raw_Data[i] = Data[i] - Zero_Data[i];

                if (Mathf.Abs(Convert.ToSingle(Raw_Data[i])) > Mathf.Abs(Convert.ToSingle(Intersection_Xi[i])))
                {
                    Force_Data[i] = Calibration_G2i[i] * Raw_Data[i] + Calibration_C2i[i];//*10;
                }
                else
                {
                    Force_Data[i] = Calibration_G1i[i] * Raw_Data[i];//*10;
                }

                //Front montant
                if (Force_Data[i] > Threshold && Force_Data_old[i] < Threshold)
                {
                    rising_edge[i] = true;
                    rising_phase = true;
                }
                else rising_edge[i] = false;

                //Front descendant
                if (Force_Data[i] < Threshold && Force_Data_old[i] > Threshold)
                {
                    falling_edge[i] = true;
                    falling_phase = true;
                }

                //Tape
                if (rising_phase == true)
                {
                    dec = 1;
                }
                if (dec == 1 && falling_phase == true)
                {
                    inc = 1;
                }
                if (dec + inc == 2)
                {
                    tap_phase = true;
                    dec = 0;
                    inc = 0;
                }
                //Reinitialiser quand XXX_instruction.tSolution>1.0f ou quand dans Compare_TI.cs enabledFinger=0;
                //else falling_edge[i] = false;

                //Force i-1
                Force_Data_old[i] = Force_Data[i];

                //Display and change force
                if (ChangementForceMax == false)
                {

                    for (int a = 0; a < 5; a++)
                    {
                        Force_max[a] = N_Force_Max;
                    }
                    ChangementForceMax = true;
                    N_Force_Max = 1;
                }
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                Zero_Data[i] = Data[i];
            }
        }
    }
}

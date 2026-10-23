# Rhythm Karaoke (RK V2)

Unity project used for a study on rhythmic finger tapping with sound.

The participant presses a force sensor (the Dextrain Manipulandum) with their fingers. Each press plays the next piece of a song. The goal is to tap in time with the song. We compare different kinds of sound:

- **B**: a simple beep
- **M**: melody notes, no words
- **S**: spoken words, no melody
- **SM**: sung words (melody + words)

Each one exists with feedback and without feedback (suffix `N`). We use three songs: *Balou*, *Papaoutai* and a *Piaf* song. *Emmenez-moi* is used for training.

Paper: *A Novel Method for Synchronized Rhythmic Production of Finger Movements and the Effects of Auditory Stimuli and Feedback*, Paul-Peter Arslan, Xiao Xiao, Påvel G. Lindberg.

## Requirements

- Unity **2021.3.8f1**
- Windows (the sensor driver `DAQMANIPULANDUM.dll` is Windows only)
- The **NI-DAQmx** driver from National Instruments (the sensor DLL uses it)
- A Dextrain Manipulandum plugged in by USB
- FMOD for Unity (already in `Assets/Plugins/FMOD`)
- Python 3 with numpy, pandas, scipy, statsmodels, matplotlib, seaborn, pingouin (only for the analysis)

## How to run

1. Install the NI-DAQmx driver and plug in the Manipulandum.
2. Clone the repo and open the folder with Unity Hub (Unity 2021.3.8f1). The first opening takes a few minutes.
3. Open `Assets/Scenes/PageAcceuil.unity` and press Play.

All paths are relative to the `Assets` folder, so the project works wherever you clone it. Run it from the Unity editor: the files it reads and writes (`Order.txt`, `Results/`) are inside `Assets`.

FMOD may show a warning that it can't find the FMOD Studio project. You can ignore it, the songs are played with Unity audio.

## What happens in a session

The list of scenes to play comes from `Assets/Order.txt` (one scene name per line). A session usually goes like this:

1. Participant info (`PatientInfo`)
2. Instructions and tips (`Consignes`, `Conseils`)
3. Training (`Training`, `Training2`, `Training3`)
4. Reaction time test (`ReflexTest`), in the first study
5. The song tasks (`<Song>_<Condition>` scenes)
6. Questionnaire (`Questionnary`)
7. End (`Fin`)

`Order_Study1.txt` is the order used in the first study. `OrderWriter.cs` makes a new random order where the same song or the same condition never comes twice in a row.

Results are saved as CSV files in `Assets/Results/<participant>/`.

## Folders

| Folder | What is inside |
|---|---|
| `Assets/Scenes` | All scenes. Song scenes are named `<Song>_<Condition>` |
| `Assets/Scripts` | C# scripts |
| `Assets/GameManager.cs` | Loads the task order and goes from one scene to the next |
| `Assets/Audios` | Song audio files, piano notes and beeps |
| `Assets/Timestamps` | Time of each note/syllable for every song and condition |
| `Assets/Partitions` | Notes to play for each song |
| `Assets/FMODBanks` | FMOD sound banks |
| `Assets/AnalysisScripts` | Python scripts and notebook for the first analysis |
| `Assets/FolderAnalysis_RK_Healthy/codeDeposit/analysis` | Notebooks used for the paper (stats and questionnaire) |

## Main scripts

| Script | Role |
|---|---|
| `Manipulandum_data_aquired.cs` | Reads force from the sensor and detects press / release |
| `SoundGenerator.cs` | Reads the timestamps and plays the right sound at each tap |
| `ContinuousAudioSegmentPlayer.cs` | Plays the song piece by piece, following the speed of the taps |
| `AudioSegmentPlayer.cs` | Simpler version, one segment per tap |
| `FMOD_ContinuousAudioSegment.cs` | Same idea, using FMOD |
| `ReflexTestManager.cs` | Reaction time test |
| `DataSave.cs` / `DataSaveBrut.cs` | Save tap data / raw force data to CSV |
| `SaveQuestionnaireToCSV.cs` | Save questionnaire answers |
| `OrderWriter.cs` | Makes the random task order |

## Data

Participant data is **not** in this repo, for ethics reasons. You can ask the authors for it. Notebook outputs were also removed for the same reason, so the notebooks need the data to run.

## Contact

Paul-Peter Arslan, paul-peter.arslan@devinci.fr

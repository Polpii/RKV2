import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Configuration initiale
names = ["21_X"]
assets_folder = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
results_folder = os.path.join(assets_folder, "Results") + os.sep
timestamps_folder = os.path.join(assets_folder, "Timestamps") + os.sep
outlier_percent = 15

def read_reference_timestamps(task_name):
    """Lire les timestamps de référence pour une tâche donnée."""
    with open(f"{timestamps_folder}{task_name}.txt", 'r') as file:
        return [float(line.strip().replace(',', '.')) for line in file]

def calculate_errors(user_timestamps, reference_timestamps):
    """Calculer les erreurs absolues et relatives entre les timestamps utilisateur et référence."""
    min_length = min(len(user_timestamps), len(reference_timestamps))
    user_timestamps = user_timestamps[:min_length]
    reference_timestamps = reference_timestamps[:min_length]

    absolute_errors = np.abs(np.array(user_timestamps) - np.array(reference_timestamps))
    relative_errors = np.abs(np.diff(user_timestamps) - np.diff(reference_timestamps))

    return absolute_errors, relative_errors

def exclude_outliers(data):
    """Exclure les outliers pour le calcul des erreurs."""
    return np.array(sorted(data)[int(len(data) * outlier_percent / 100) : int(len(data) * (100 - outlier_percent) / 100)])

# Stockage des résultats
results = {}

# Traitement pour chaque patient
for name in names:
    patient_folder = f"{results_folder}{name}\\"
    for csv_file in os.listdir(patient_folder):
        if csv_file.endswith(".csv"):
            # Lire le fichier CSV
            df = pd.read_csv(f"{patient_folder}{csv_file}", sep=';', decimal=',', skiprows=2)

            # Créer un dictionnaire pour suivre les occurrences des tâches
            task_occurrences = {}
            for task in df.columns:
                if task not in task_occurrences:
                    task_occurrences[task] = 0
                else:
                    task_occurrences[task] += 1

                if task and 'Unnamed' not in task and task != 'ReflexTest':
                    # Générer le nom de fichier de référence en tenant compte des doublons
                    ref_task_name = f"{task}{('.' + str(task_occurrences[task])) if task_occurrences[task] > 0 else ''}"
                    user_timestamps = df[task].dropna().tolist()
                    try:
                        reference_timestamps = read_reference_timestamps(ref_task_name)
                    except FileNotFoundError:
                        continue  # Ignorer si le fichier de référence n'existe pas

                    # ... (reste du traitement)


                    abs_errors, rel_errors = calculate_errors(user_timestamps, reference_timestamps)
                    abs_errors_excluded = exclude_outliers(abs_errors)
                    rel_errors_excluded = exclude_outliers(rel_errors)

                    # Calculs des moyennes
                    mean_abs_error = np.mean(abs_errors)
                    mean_rel_error = np.mean(rel_errors)
                    mean_abs_error_excl = np.mean(abs_errors_excluded)
                    mean_rel_error_excl = np.mean(rel_errors_excluded)

                    # Stockage des résultats
                    if task not in results:
                        results[task] = {'abs_error': [], 'rel_error': [], 'abs_error_excl': [], 'rel_error_excl': []}
                    results[task]['abs_error'].append(mean_abs_error)
                    results[task]['rel_error'].append(mean_rel_error)
                    results[task]['abs_error_excl'].append(mean_abs_error_excl)
                    results[task]['rel_error_excl'].append(mean_rel_error_excl)

# Calcul des moyennes par groupe de tâches
grouped_results = {
    'mean_abs_error': [],
    'mean_rel_error': [],
    'mean_abs_error_excl': [],
    'mean_rel_error_excl': []
}
for task in results:
    grouped_results['mean_abs_error'].append(np.mean(results[task]['abs_error']))
    grouped_results['mean_rel_error'].append(np.mean(results[task]['rel_error']))
    grouped_results['mean_abs_error_excl'].append(np.mean(results[task]['abs_error_excl']))
    grouped_results['mean_rel_error_excl'].append(np.mean(results[task]['rel_error_excl']))

# Présentation des résultats sous forme de tableau
df_results = pd.DataFrame(grouped_results, index=results.keys())
print(df_results)

# Visualisation des résultats sous forme de graphique
fig, axes = plt.subplots(2, 2, figsize=(15, 10))
fig.suptitle('Analyse des Erreurs par Tâche')

axes[0, 0].bar(df_results.index, df_results['mean_abs_error'])
axes[0, 0].set_title('Erreur Absolue Moyenne')

axes[0, 1].bar(df_results.index, df_results['mean_rel_error'])
axes[0, 1].set_title('Erreur Relative Moyenne')

axes[1, 0].bar(df_results.index, df_results['mean_abs_error_excl'])
axes[1, 0].set_title('Erreur Absolue Moyenne (Excl. Outliers)')

axes[1, 1].bar(df_results.index, df_results['mean_rel_error_excl'])
axes[1, 1].set_title('Erreur Relative Moyenne (Excl. Outliers)')

plt.setp(axes, xticks=range(len(df_results.index)), xticklabels=df_results.index)
plt.xticks(rotation=45)
plt.tight_layout(rect=[0, 0.03, 1, 0.95])
plt.show()

"""
Je veux que tu écrives un script python qui satisfait toutes ces directive:"
J'ai réalisé une étude avec plusieurs patient. En faisant l'étude chaque patient génère 3 fichiers: un fichier csv, un fichier texte et un autre fichier csv qui sont les réponses à un questionnaire.
Je veux que tu m'aides à écrire un script python qui va analyser ces fichiers. A chaque fois qu'un patient fait l'étude un dossier à son nom est créé à l'emplacement de ce path: "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Results\\".
J'ai stockés tous les noms dans un tableau: "names = ["x_x", "Arslan_P"]". Dans ces dossiers il y a les 3 fichiers que je t'ai décrit mais pour l'instant on va se focaliser sur un seul de ces fichiers qui est nommé au nom du patient + ".csv". Par exemple, si le nom du patient est x_x alors le fichier se nomme "x_x.csv".
Les deux premières lignes du fichier donnent des informations sur l'utilisateur mais on va les ignorer pour l'instant.
La 3ème ligne est importante! Chaque cellule de la 3eme ligne est l'en-tête d'une colonne. En dessous de ces en-tête, sur toute la colonne, il y aura les résultats de la tâche au nom de l'en-tête. 
Dans de rare cas, l'entête sera "ReflexTest", dans ce cas là ignore la colonne. Il est également possible que l'entête soit vide, dans ce cas là ignore la ('Unnamed' not in task and task != 'ReflexTest'). Le reste des tâches sont des tâches rythmiques et les chiffres sur la colonne réprésentent les timestamps de l'utilisateur. Les timestamps parfaits que l'utilisateur avait à réaliser sont stockés dans le dossier à l'emplacement: "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Timestamps\\"
Dans le dossier Timestamps il y a des fichiers qui ont les exactes mêmes noms que les en-tête de la 3ème ligne. Ces fichiers sont des fichiers textes et quand tu les lis, chaque ligne représentent un timestamps. Par exemple "2,75" veut dire 2 secondes et 75 centièmes.
J'aimerais que tu compares les timestamps réalisé par l'utilisateur et les timestamps parfaits à faire. Tu remarqueras parfois que la longueur des timestamps fait par l'utilisateur et la longueur de timestamps parfaits ne sont pas égales. Dans ce cas, considères que les deux tableaux de données font la taille du plus petit des deux.
Voici un exemple de l'ordre des tâches qu'un patient va faire: "ReflexTest	Papaoutai_BN	Piaf_MN	Papaoutai_MN	Papaoutai_BN	Papaoutai_SM	Balou_SN	Balou_B	Papaoutai_S	Piaf_S	Piaf_MN	Papaoutai_MN	Balou_S	Balou_MN	Papaoutai_M	Papaoutai_B	Papaoutai_SM	Papaoutai_BN	Balou_S	Piaf_SN	Piaf_SN	Balou_MN	Papaoutai_B	ReflexTest	Piaf_B	Balou_B	Balou_SM	Balou_SM	Piaf_B	Papaoutai_SN	Piaf_SMN	Piaf_SMN	Piaf_SM	Balou_SMN	Papaoutai_SMN	Piaf_BN	Balou_BN	Balou_MN	Piaf_SM	Balou_BN	Balou_M	Piaf_SN	Balou_SMN	Balou_B	Piaf_M	Balou_SN	Papaoutai_SMN	ReflexTest	Papaoutai_SN	Papaoutai_SMN	Piaf_B	Papaoutai_SN	Piaf_SM	Balou_S	Balou_SM	Piaf_MN	Papaoutai_MN	Piaf_BN	Piaf_M	Papaoutai_SM	Balou_M	Piaf_BN	Piaf_S	Piaf_S	Balou_SN	Papaoutai_S	Papaoutai_M	Papaoutai_B	Balou_BN	Papaoutai_S	Piaf_SMN	Papaoutai_M	Balou_M	Balou_SMN	Piaf_M	ReflexTest"

Pour chaque tâche je veux que dans ton scripts python tu calcules plusieurs données: 
    - la moyenne de l'erreur (la différence absolue entre le timestamps parfait et le timestamp réalisé par l'utilisateur)
    - la moyenne de l'erreur relative (la différence les écarts des timestamps. Par exemple dans le tableau des timestamps parfaits, le premier ecart est égale au timestamps 2 moins le timestamps 1. Le deuxième ecart est égal au timestamps 3 moins le timestamps 2)
    - la moyenne de l'erreur avec les outlayers (il s'agit de la moyenne des erreurs sans prendre en compte les 15% plus petites erreurs et 15% plus grandes erreur. Je veux que 15% soit une varible que je peux changer)
    - la moyenne de l'erreur relative avec les outlayers (il s'agit de la moyenne des différences entre les ecarts des erreurs sans prendre en compte les 15% plus grands et plus petits. )

Pour stocké les données tu devrais utiliser un id qui correspond à l'ordre des tâches. La première tâche aura la clé 0, la deuxième aura l'id 1, etc!!! Donc à result[0] tu vas stocké un tableau qui comprend [taskName, moyenne absolue, moyenne relative, moyenne absolue outlayers, moyenne relative outlayers] 

Une fois que tu auras fait ça je veux que tu regroupes les types de tâches ensemble et que tu calcules:
    - la moyenne des moyennes de l'erreur
    - la moyenne des moyennes de l'erreur relative
    - la moyenne des moyennes de l'erreur avec les outlayers
    - la moyenne des moyennes de l'erreur relative avec les outlayers
Pour chacune de ces moyenne je veux que tu me proposes le résultat en prenant en compte 100% des tâches ET en enlevant les 15% plus petites et plus grandes (outlayers)
Trouve un moyen de me présenter tout ça sous forme de graphique ou de tableau."
User
Je ne demande pas de faire des calcules hypothétiques! je te demande de faire un script qui fait tous ces calcules!
Puis je lancerai le script moi même et cela me donnera les résultats.
Fait attention, le fichiers csv a des cellule séparé par des ";" et les float à l'intérieur ont des décimals séparés par des ",".
Je dois te dire qu'un même patient va rencontrer au moins 3 fois la même tâche lorsqu'il fait l'étude. Fait attention à ne pas overwrite les résultats quand tu tombe sur une tâche qui a le meme nom qu'une autre tache qui est deja sorti!
Quand tu analyses une tâche je veux que tu l'analyses de manière unique et que tu stockes les résultats appart. Un meme patient peut faire plusieurs fois la même tâche donc un liste avec une clé (name, task) n'est pas suffisant. Encore une fois, fait attention car tous les nombres décimaux sont écrit avec des , et pas des . Donc quand il y a écrit "2,27" cela veut dire "2.27"
"""
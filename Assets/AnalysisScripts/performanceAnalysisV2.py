import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Configuration initiale
names = ["ED001_E", "PavelTestP_P", "21_X", "AA002_A", "RT003_R", "QL004_Q", "IJ006_I"]
#names = ["IJ006_I"]
results_folder = "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Results\\"
timestamps_folder = "C:\\Users\\paul-\\UnityProject\\RK_V2\\Assets\\Timestamps\\"
outlier_percent = 15

def plot_first_task(df, name="Balou_SN"):
    done = False
    for task in df.columns:
        if task == name and not done:
            done = True
            user_timestamps = df[task].dropna().tolist()
            ref_timestamps = read_reference_timestamps(task)
            ref_timestamps = ref_timestamps[:-1]

            # Soustraction de la première valeur de la série de timestamps de l'utilisateur
            #user_timestamps = np.array(user_timestamps) - user_timestamps[0]
            user_timestamps[0] = 0
            #user_timestamps.append(ref_timestamps[len(ref_timestamps) - 1])

            # Réglage de la taille du graphique pour plus de largeur
            plt.figure(figsize=(15, 6))  # Augmentation de la largeur

            # Tracer les lignes pour les user_timestamps et ajouter des crochets aux extrémités
            for i in range(0, len(user_timestamps)-1, 2):
                plt.plot(user_timestamps[i:i+2], [1, 1], "r-", label='Lignes Utilisateur' if i == 0 else "")
                plt.plot(user_timestamps[i], 1, "r|", markersize=10)  # Marqueur pour le début du trait
                plt.plot(user_timestamps[i+1], 1, "r|", markersize=10)  # Marqueur pour la fin du trait

            # Tracer les lignes pour les ref_timestamps et ajouter des crochets aux extrémités
            for i in range(0, len(ref_timestamps)-1, 2):
                plt.plot(ref_timestamps[i:i+2], [1.005, 1.005], "g-", label='Lignes Parfaites' if i == 0 else "")
                plt.plot(ref_timestamps[i], 1.005, "g|", markersize=10)  # Marqueur pour le début du trait
                plt.plot(ref_timestamps[i+1], 1.005, "g|", markersize=10)  # Marqueur pour la fin du trait

            plt.title(f'Timestamps pour la tâche : {task}')
            plt.xlabel('Temps (secondes)')
            plt.yticks([1, 1.005], ['Timestamps Utilisateur', 'Timestamps Parfaites'])
            plt.ylim(0.98, 1.04)  # Ajustement des limites de l'axe des y pour resserrer l'affichage
            plt.legend()
            plt.show()

            break  # Arrête après la première tâche valide

def plot_first_task2(df, name="Piaf_S"):
    done = False
    for task in df.columns:
        if task == name and not done:
            done = True
            user_timestamps = df[task].dropna().tolist()
            ref_timestamps = read_reference_timestamps(task)
            ref_timestamps = ref_timestamps[:-1]

            # Soustraction de la première valeur de chaque série de timestamps
            user_timestamps = np.array(user_timestamps) - user_timestamps[0]

            plt.figure(figsize=(10, 4))

            # Tracer les lignes pour les user_timestamps
            for i in range(0, len(user_timestamps)-1, 2):
                plt.plot(user_timestamps[i:i+2], [1, 1], "r-", label='Lignes Utilisateur' if i == 0 else "")

            # Tracer les lignes pour les ref_timestamps très proches au-dessus
            for i in range(0, len(ref_timestamps)-1, 2):
                plt.plot(ref_timestamps[i:i+2], [1.05, 1.05], "g-", label='Lignes Parfaites' if i == 0 else "")

            plt.title(f'Timestamps pour la tâche : {task}')
            plt.xlabel('Temps (secondes)')
            plt.yticks([1, 1.05], ['Timestamps Utilisateur', 'Timestamps Parfaites'])
            plt.legend()
            plt.show()

            break  # Arrête après la première tâche valide

# Exemple d'utilisation
# df est le DataFrame contenant les données d'un patient
# reference_timestamps est un dictionnaire des timestamps de référence pour chaque tâche
# plot_first_task(df, reference_timestamps)

def read_reference_timestamps(task_name):
    """Lire les timestamps de référence pour une tâche donnée."""
    with open(f"{timestamps_folder}{task_name.split('.')[0]}.txt", 'r') as file:
        return [float(line.strip().replace(',', '.')) for line in file]

def calculate_percentage_errors(user_timestamps, reference_timestamps):
    # Calcul des gaps pour les timestamps de référence et de l'utilisateur
    ref_diffs = np.diff(reference_timestamps)
    print(ref_diffs)
    user_diffs = np.diff(user_timestamps)

    # Éviter la division par zéro
    #ref_diffs[ref_diffs == 0] = np.nan

    # Calcul des pourcentages : (user_diffs / ref_diffs) * 100
    percentage_diffs = (user_diffs / ref_diffs) * 100

    # Calcul de la différence absolue des pourcentages par rapport à 100%
    absolute_percentage_errors = np.abs(100 - percentage_diffs)

    return absolute_percentage_errors

def calculate_errors(user_timestamps, reference_timestamps, monophasic = True):
    """Calculer les erreurs absolues et relatives entre les timestamps utilisateur et référence.
    
    Si monophasic est True, considérer uniquement les valeurs à des positions multiples de 2.
    """

    # Suppression de la dernière valeur de reference_timestamps
    reference_timestamps = reference_timestamps[:-1]
    user_timestamps[0] = 0
    #user_timestamps.append(reference_timestamps[len(reference_timestamps) - 1])

    # Conversion des listes en tableaux Numpy pour permettre la soustraction
    user_timestamps = np.array(user_timestamps)
    reference_timestamps = np.array(reference_timestamps)

    # Si monophasic est True, ne prendre en compte que les éléments à des indices pairs
    if monophasic:
        user_timestamps = user_timestamps[::2]
        reference_timestamps = reference_timestamps[::2]
    
    # Réduction des listes à la même longueur si nécessaire
    if len(user_timestamps) != len(reference_timestamps):
        print("Reduction")
        min_length = min(len(user_timestamps), len(reference_timestamps))
        user_timestamps = user_timestamps[:min_length]
        reference_timestamps = reference_timestamps[:min_length]

    # Calcul des erreurs absolues
    absolute_errors = np.abs(user_timestamps - reference_timestamps)

    # Calcul des erreurs relatives
    relative_errors = calculate_percentage_errors(user_timestamps, reference_timestamps)

    return absolute_errors, relative_errors




def exclude_10percent(data):
    """Exclure les outliers pour le calcul des erreurs."""
    return np.array(sorted(data)[int(len(data) * outlier_percent / 100) : int(len(data) * (100 - outlier_percent) / 100)])

def exclude_outliers(data):
    # Calcul de la médiane
    median = np.median(data)
    
    # Calcul des quartiles
    Q1 = np.percentile(data, 25)
    Q3 = np.percentile(data, 75)
    
    # Calcul de l'IQR
    IQR = Q3 - Q1
    
    # Définition des bornes pour les outliers
    lower_bound = median - 1.5 * IQR
    upper_bound = median + 1.5 * IQR
    
    # Exclusion des outliers
    filtered_data = [x for x in data if lower_bound <= x <= upper_bound]
    
    return filtered_data


# Stockage des résultats
results = {}

# Traitement pour chaque patient
for name in names:
    patient_folder = f"{results_folder}{name}\\"
    for csv_file in os.listdir(patient_folder):
        if csv_file.endswith(name + ".csv"):
            # Lire le fichier CSV
            df = pd.read_csv(f"{patient_folder}{csv_file}", sep=';', decimal=',', skiprows=2)
            
            #plot_first_task(df)

            # Analyse de chaque tâche
            for task in df.columns:
                if task and 'Unnamed' not in task and 'ReflexTest' not in task and 'Button' not in task:
                    user_timestamps = df[task].dropna().tolist()
                    reference_timestamps = read_reference_timestamps(task)

                    abs_errors, rel_errors = calculate_errors(user_timestamps, reference_timestamps)
                    abs_errors_excluded = exclude_outliers(abs_errors)
                    rel_errors_excluded = exclude_outliers(rel_errors)

                    # Calculs des moyennes
                    mean_abs_error = np.mean(abs_errors)
                    mean_rel_error = np.mean(rel_errors)
                    mean_abs_error_excl = np.mean(abs_errors_excluded)
                    mean_rel_error_excl = np.mean(rel_errors_excluded)

                    # Stockage des résultats
                    if task.split('.')[0] not in results:
                        results[task.split('.')[0]] = {'abs_error': [], 'rel_error': [], 'abs_error_excl': [], 'rel_error_excl': []}
                    results[task.split('.')[0]]['abs_error'].append(mean_abs_error)
                    results[task.split('.')[0]]['rel_error'].append(mean_rel_error)
                    results[task.split('.')[0]]['abs_error_excl'].append(mean_abs_error_excl)
                    results[task.split('.')[0]]['rel_error_excl'].append(mean_rel_error_excl)

# ...

# Calcul des moyennes par groupe de tâches
grouped_results = {
    'mean_abs_error': [],
    'mean_rel_error': [],
    'mean_abs_error_excl': [],
    'mean_rel_error_excl': []
}

# Tri par ordre alphabétique des tâches
sorted_tasks = sorted(results.keys())
for task in sorted_tasks:
    grouped_results['mean_abs_error'].append(np.mean(results[task]['abs_error']))
    grouped_results['mean_rel_error'].append(np.mean(results[task]['rel_error']))
    grouped_results['mean_abs_error_excl'].append(np.mean(results[task]['abs_error_excl']))
    grouped_results['mean_rel_error_excl'].append(np.mean(results[task]['rel_error_excl']))

# Présentation des résultats sous forme de tableau
df_results = pd.DataFrame(grouped_results, index=sorted_tasks)
print(df_results)

# Visualisation des résultats sous forme de graphique
fig, axes = plt.subplots(2, 2, figsize=(15, 10))
fig.suptitle('Analyse des Erreurs par Tâche')

# Fonction pour ajuster les étiquettes
def adjust_labels(ax):
    plt.setp(ax.get_xticklabels(), rotation=45, ha="right", rotation_mode="anchor", fontsize='small')

# Création des graphiques
axes[0, 0].bar(df_results.index, df_results['mean_abs_error'])
axes[0, 0].set_title('Erreur Absolue Moyenne')
adjust_labels(axes[0, 0])

axes[0, 1].bar(df_results.index, df_results['mean_rel_error'])
axes[0, 1].set_title('Erreur Relative Moyenne')
adjust_labels(axes[0, 1])

axes[1, 0].bar(df_results.index, df_results['mean_abs_error_excl'])
axes[1, 0].set_title('Erreur Absolue Moyenne (Excl. Outliers)')
adjust_labels(axes[1, 0])

axes[1, 1].bar(df_results.index, df_results['mean_rel_error_excl'])
axes[1, 1].set_title('Erreur Relative Moyenne (Excl. Outliers)')
adjust_labels(axes[1, 1])

plt.tight_layout(rect=[0, 0.03, 1, 0.95])
plt.show()

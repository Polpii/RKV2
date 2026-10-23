import os

# Spécifiez le chemin du répertoire cible
directory_path = 'C:\\Users\\paul-\\Downloads\\25405__tedagame__88-piano-keys-long-reverb\\'

# Parcourir tous les fichiers dans le répertoire spécifié
for filename in os.listdir(directory_path):
    if len(filename) > 18:
        # Supprimer les 18 premiers caractères
        new_filename = filename[18:]
        # Chemin complet vers l'ancien et le nouveau fichier
        old_file = os.path.join(directory_path, filename)
        new_file = os.path.join(directory_path, new_filename)
        # Renommer le fichier
        os.rename(old_file, new_file)
    else:
        print(f"Skipped: {filename} (filename too short)")

# WindowsImageMetadataService

Service responsable de la gestion des métadonnées des images Windows, incluant le chargement, la sauvegarde et la gestion des collections de métadonnées avec persistance JSON.

## Responsabilités

- **Persistance des métadonnées** : Sauvegarde et chargement JSON
- **Gestion des collections** : Ajout, suppression, mise à jour d'images
- **Validation** : Vérification de l'existence des fichiers d'images
- **Configuration** : Gestion des chemins de données et de répertoires

## Méthodes principales

### GetImagesAsync

Charge toutes les images Windows disponibles de manière asynchrone.

```csharp
Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
```

### SaveImagesAsync

Sauvegarde les métadonnées des images Windows de manière asynchrone.

```csharp
Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)
```

### RemoveImageAsync

Supprime une image de la collection de métadonnées.

```csharp
Task RemoveImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
```

### UpdateImageAsync

Met à jour une image existante dans la collection de métadonnées.

```csharp
Task UpdateImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
```

## Configuration

Le service nécessite le chemin du répertoire des images lors de l'initialisation :

```csharp
var metadataService = new WindowsImageMetadataService(Constants.ImportedWIMsDirectoryPath);
```

## Format de persistance

Les métadonnées sont stockées dans un fichier `images.json` au format JSON avec indentation pour la lisibilité :

```json
[
  {
    "Id": "guid",
    "Name": "Windows 11 Pro",
    "FilePath": "C:\\path\\to\\image.wim",
    "SourceIsoPath": "C:\\path\\to\\source.iso",
    "ModifiedDate": "2025-06-25T00:00:00Z",
    "Indices": [...]
  }
]
```

## Validation automatique

- Vérifie l'existence des fichiers d'images au chargement
- Supprime automatiquement les entrées orphelines
- Met à jour la collection si des fichiers sont manquants

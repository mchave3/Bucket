# WindowsImageIsoService

Service responsable des opérations ISO incluant le montage, démontage et opérations d'importation. Gère toutes les fonctionnalités liées aux ISO pour la gestion des images Windows.

## Responsabilités

- **Montage/Démontage ISO** : Opérations PowerShell pour monter/démonter les ISO
- **Importation depuis ISO** : Extraction et importation d'images depuis les ISO
- **Importation depuis WIM** : Importation directe de fichiers WIM/ESD
- **Recherche d'images** : Localisation des fichiers d'images dans les ISO montés

## Méthodes principales

### ImportFromIsoAsync

Importe une image Windows depuis un fichier ISO.

```csharp
Task<WindowsImageInfo> ImportFromIsoAsync(
    StorageFile isoFile,
    string customName = "",
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

### ImportFromWimAsync

Importe une image Windows directement depuis un fichier WIM/ESD.

```csharp
Task<WindowsImageInfo> ImportFromWimAsync(
    StorageFile wimFile,
    string customName = "",
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

### MountIsoAsync

Monte un fichier ISO et retourne le chemin de montage.

```csharp
Task<string> MountIsoAsync(string isoPath, CancellationToken cancellationToken)
```

### DismountIsoAsync

Démonte un fichier ISO.

```csharp
Task DismountIsoAsync(string isoPath, CancellationToken cancellationToken)
```

### IsIsoMountedAsync

Vérifie si un fichier ISO est actuellement monté.

```csharp
Task<bool> IsIsoMountedAsync(string isoPath, CancellationToken cancellationToken)
```

## Processus d'importation ISO

1. **Montage** : Monte l'ISO sur une lettre de lecteur disponible
2. **Recherche** : Localise les fichiers WIM/ESD dans l'ISO monté
3. **Copie** : Copie le fichier WIM vers le répertoire géré
4. **Analyse** : Analyse l'image pour extraire les indices
5. **Métadonnées** : Crée l'objet WindowsImageInfo
6. **Nettoyage** : Démonte l'ISO automatiquement

## Gestion du montage

### Détection automatique

- Vérifie si l'ISO est déjà monté avant de tenter un nouveau montage
- Récupère la lettre de lecteur existante si disponible

### Démontage robuste

- Méthode principale avec timeout de 30 secondes
- Méthode alternative de secours en cas d'échec
- Gestion gracieuse des erreurs de démontage

### Chemins de recherche

Le service recherche les images Windows dans :

- `/sources/` (répertoire standard)
- `/` (racine de l'ISO)

### Fichiers recherchés

- `install.wim` : Image d'installation principale
- `install.esd` : Image d'installation compressée
- `boot.wim` : Image de démarrage

## Dépendances

Le service dépend de :

- `IWindowsImageFileService` : Pour les opérations sur fichiers
- `IWindowsImagePowerShellService` : Pour l'analyse des images

## Sécurité

- Échappement des caractères spéciaux pour PowerShell
- Gestion des timeouts pour éviter les blocages
- Nettoyage automatique en cas d'erreur
- Validation des chemins de fichiers

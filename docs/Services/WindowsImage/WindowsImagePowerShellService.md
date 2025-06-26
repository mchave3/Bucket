# WindowsImagePowerShellService

Service responsable des opérations PowerShell et de l'analyse JSON pour les images Windows. Gère l'interaction avec la cmdlet PowerShell Get-WindowsImage et l'analyse des résultats JSON.

## Responsabilités

- **Exécution PowerShell** : Interaction avec Get-WindowsImage cmdlet
- **Analyse JSON** : Parsing des résultats PowerShell en objets .NET
- **Validation des formats** : Vérification des formats d'images supportés
- **Extraction de métadonnées** : Récupération d'informations détaillées des images

## Méthodes principales

### AnalyzeImageAsync

Analyse un fichier WIM/ESD et extrait ses indices de manière asynchrone.

```csharp
Task<List<WindowsImageIndex>> AnalyzeImageAsync(
    string imagePath,
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

### GetImageIndexDetailsAsync

Récupère les informations détaillées pour un index d'image spécifique.

```csharp
Task<WindowsImageIndex> GetImageIndexDetailsAsync(
    string imagePath,
    int index,
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

### IsValidImageFormat

Valide si le format de fichier est supporté pour l'imagerie Windows.

```csharp
bool IsValidImageFormat(string extension)
```

## Formats supportés

- **.wim** : Windows Imaging Format
- **.esd** : Electronic Software Distribution
- **.swm** : Split Windows Imaging Format

## Gestion des erreurs

Le service gère spécifiquement plusieurs types d'erreurs :

- **Accès refusé** : Nécessite des privilèges administrateur
- **Cmdlet manquante** : Get-WindowsImage non disponible
- **Fichier inaccessible** : Fichier en cours d'utilisation ou corrompu
- **JSON invalide** : Sortie PowerShell malformée

## Parsing JSON avancé

### Gestion des dates Microsoft JSON.NET

Support des formats de date legacy `/Date(timestamp)/` :

```csharp
private static bool TryParseMicrosoftJsonDate(string dateString, out DateTime result)
```

### Traitement des architectures

Conversion des codes d'architecture en chaînes lisibles :

- 0 → x86
- 5 → ARM
- 6 → IA64
- 9 → x64
- 12 → ARM64

### Gestion des tableaux mixtes

Traitement des propriétés JSON pouvant être chaînes ou tableaux :

```csharp
private static string GetStringOrArray(JsonElement element)
```

## Prérequis

- PowerShell avec module Windows DISM
- Privilèges administrateur pour certaines opérations
- Get-WindowsImage cmdlet disponible

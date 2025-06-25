# WindowsImageFileService

Service responsable de toutes les opérations sur fichiers liées aux images Windows, incluant la validation, la sanitisation, la copie avec rapport de progression, et la génération de noms uniques.

## Responsabilités

- **Opérations sur fichiers** : Copie avec progression, validation des chemins
- **Gestion des noms** : Génération de noms uniques, sanitisation, validation
- **Sécurité** : Protection contre les attaques de traversée de répertoires

## Méthodes principales

### CopyImageToManagedDirectoryAsync
Copie une image vers le répertoire géré avec rapport de progression.

```csharp
Task<string> CopyImageToManagedDirectoryAsync(
    string sourcePath,
    string targetName,
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

### GenerateUniqueFileName
Génère un nom de fichier unique en résolvant les conflits avec un compteur.

```csharp
string GenerateUniqueFileName(string baseName, string extension)
```

### SanitizeFileName
Sanitise un nom de fichier en supprimant ou remplaçant les caractères invalides.

```csharp
string SanitizeFileName(string fileName)
```

### IsValidFilePath
Valide un chemin de fichier pour prévenir les attaques de traversée de répertoires.

```csharp
bool IsValidFilePath(string filePath)
```

## Configuration

Le service nécessite le chemin du répertoire des images lors de l'initialisation :

```csharp
var fileService = new WindowsImageFileService(Constants.ImportedWIMsDirectoryPath);
```

## Sécurité

- Validation stricte des noms de fichiers
- Protection contre les traversées de répertoires
- Gestion des noms réservés Windows
- Limitation de la longueur des noms de fichiers

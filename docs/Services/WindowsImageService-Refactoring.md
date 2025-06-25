# Refactoring WindowsImageService - Architecture des services

## Vue d'ensemble

Le `WindowsImageService` original a été refactorisé en une architecture basée sur des services spécialisés avec injection de dépendances, suivant le principe de responsabilité unique (SRP).

## Structure des services

### Répertoire Services/WindowsImage/

```text
src/Services/WindowsImage/
├── IWindowsImageFileService.cs
├── IWindowsImageMetadataService.cs
├── IWindowsImagePowerShellService.cs
├── IWindowsImageIsoService.cs
├── WindowsImageFileService.cs
├── WindowsImageMetadataService.cs
├── WindowsImagePowerShellService.cs
└── WindowsImageIsoService.cs
```

## Services spécialisés

### 1. WindowsImageFileService

**Responsabilité** : Opérations sur fichiers et gestion des noms

- Copie avec progression
- Génération de noms uniques
- Validation et sanitisation
- Sécurité des chemins

### 2. WindowsImageMetadataService

**Responsabilité** : Gestion des métadonnées

- Persistance JSON
- Collections d'images
- Validation de l'existence des fichiers
- CRUD des métadonnées

### 3. WindowsImagePowerShellService

**Responsabilité** : Opérations PowerShell et parsing

- Exécution Get-WindowsImage
- Parsing JSON avancé
- Validation des formats
- Gestion des erreurs PowerShell

### 4. WindowsImageIsoService

**Responsabilité** : Opérations ISO

- Montage/démontage
- Importation depuis ISO
- Recherche d'images dans ISO
- Gestion robuste des erreurs

### 5. WindowsImageService (Principal)

**Responsabilité** : Coordination et API unifiée

- Orchestration des services
- Injection de dépendances
- API publique cohérente
- Validation de haut niveau

## Avantages du refactoring

### ✅ Principe de responsabilité unique

Chaque service a une responsabilité clairement définie.

### ✅ Testabilité améliorée

Interfaces permettent le mocking et les tests unitaires isolés.

### ✅ Maintenance facilitée

Modifications localisées sans impact sur les autres composants.

### ✅ Réutilisabilité

Services utilisables indépendamment dans d'autres contextes.

### ✅ Injection de dépendances

Meilleure séparation des préoccupations et flexibilité.

### ✅ Documentation spécialisée

Chaque service a sa propre documentation détaillée.

## Configuration

```csharp
// Initialisation des services
var imagesDirectory = Constants.ImportedWIMsDirectoryPath;

var metadataService = new WindowsImageMetadataService(imagesDirectory);
var fileService = new WindowsImageFileService(imagesDirectory);
var powerShellService = new WindowsImagePowerShellService();
var isoService = new WindowsImageIsoService(fileService, powerShellService);

// Service principal avec injection de dépendances
var windowsImageService = new WindowsImageService(
    metadataService,
    fileService,
    powerShellService,
    isoService
);
```

## Documentation détaillée

Chaque service dispose de sa propre documentation :

- [WindowsImageFileService.md](WindowsImageFileService.md)
- [WindowsImageMetadataService.md](WindowsImageMetadataService.md)
- [WindowsImagePowerShellService.md](WindowsImagePowerShellService.md)
- [WindowsImageIsoService.md](WindowsImageIsoService.md)
- [WindowsImageService-Principal.md](WindowsImageService-Principal.md)

## Impact sur l'existant

L'API publique du `WindowsImageService` principal reste compatible avec l'ancienne version, garantissant que les composants existants (ViewModels, etc.) continuent de fonctionner sans modification.

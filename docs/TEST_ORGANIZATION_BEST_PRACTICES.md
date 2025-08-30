# Organisation des Tests - Meilleures Pratiques

## 🏗️ Structures d'Arborescence Recommandées

### Option 1 : Séparation par dossiers (Recommandée)
```
tests/
├── Unit/                           # Tests unitaires uniquement
│   ├── Bucket.Core.UnitTests/
│   │   ├── Helpers/
│   │   │   └── VersionHelperTests.cs
│   │   ├── Models/
│   │   │   └── SupportedLanguagesTests.cs
│   │   └── Services/
│   │       └── MockServices/
│   └── Bucket.App.UnitTests/
│       ├── Services/
│       │   ├── WindowsSystemLanguageDetectionServiceTests.cs
│       │   └── Mocks/
│       │       ├── MockSystemLanguageDetectionService.cs
│       │       └── MockWindowsSystemLanguageDetectionService.cs
│       └── Localization/
│           └── LanguageMappingTests.cs
├── Integration/                    # Tests d'intégration
│   ├── Bucket.Core.IntegrationTests/
│   └── Bucket.App.IntegrationTests/
│       └── Services/
│           └── WindowsSystemLanguageDetectionServiceIntegrationTests.cs
└── E2E/                           # Tests end-to-end (optionnel)
    └── Bucket.E2ETests/
```

### Option 2 : Suffixes dans les noms (Alternative)
```
tests/
├── Bucket.Core.UnitTests/          # Uniquement tests unitaires
├── Bucket.Core.IntegrationTests/   # Uniquement tests d'intégration
├── Bucket.App.UnitTests/
└── Bucket.App.IntegrationTests/
```

## 📝 Conventions de Nommage

### Fichiers de Test
```csharp
// Tests unitaires
VersionHelperTests.cs
SupportedLanguagesTests.cs
WindowsSystemLanguageDetectionServiceTests.cs

// Tests d'intégration
WindowsSystemLanguageDetectionServiceIntegrationTests.cs
LocalizationServiceIntegrationTests.cs

// Tests E2E
MainWindowE2ETests.cs
```

### Classes de Test
```csharp
// Tests unitaires
public class VersionHelperTests
public class SupportedLanguagesTests

// Tests d'intégration
public class WindowsSystemLanguageDetectionServiceIntegrationTests
public class DatabaseIntegrationTests

// Tests E2E
public class UserWorkflowE2ETests
```

## 🏷️ Catégorisation avec Attributs

```csharp
// Tests unitaires (par défaut)
[Fact]
public void GetSystemLanguageCode_ShouldReturnValidFormat()
{
    // Test avec mock
}

// Tests d'intégration
[Fact]
[Trait("Category", "Integration")]
public void GetSystemLanguageCode_RealAPI_ShouldWork()
{
    // Test avec vraie API Windows
}

// Tests lents
[Fact]
[Trait("Category", "Slow")]
[Trait("Category", "Integration")]
public void FullSystemTest_ShouldWork()
{
    // Test complet
}
```

## ⚙️ Configuration xUnit

### xunit.runner.json
```json
{
  "methodDisplay": "method",
  "methodDisplayOptions": "replaceUnderscoreWithSpace",
  "traits": {
    "Category": ["Unit", "Integration", "E2E", "Slow"]
  }
}
```

## 🔧 Filtres pour CI/CD

### GitHub Actions - Tests par type
```yaml
# Tests unitaires seulement (rapides)
- name: Run Unit Tests
  run: dotnet test --filter "TestCategory!=Integration&TestCategory!=E2E" --logger trx

# Tests d'intégration (plus lents)
- name: Run Integration Tests
  run: dotnet test --filter "TestCategory=Integration" --logger trx

# Tous les tests
- name: Run All Tests
  run: dotnet test --logger trx
```

### Scripts PowerShell
```powershell
# Tests unitaires seulement
dotnet test --filter "TestCategory!=Integration"

# Tests d'intégration seulement
dotnet test --filter "TestCategory=Integration"

# Tests par projet
dotnet test tests/Unit/ --logger trx
dotnet test tests/Integration/ --logger trx
```

## 🎯 Recommandation pour le Projet Bucket

### Structure Recommandée
```
tests/
├── Unit/                                    # ⚡ Rapides, mocks uniquement
│   ├── Bucket.Core.UnitTests/
│   │   ├── Bucket.Core.UnitTests.csproj
│   │   ├── Helpers/
│   │   │   └── VersionHelperTests.cs
│   │   └── Models/
│   │       └── SupportedLanguagesTests.cs
│   └── Bucket.App.UnitTests/
│       ├── Bucket.App.UnitTests.csproj
│       ├── Services/
│       │   ├── WindowsSystemLanguageDetectionServiceTests.cs
│       │   └── Mocks/
│       │       ├── MockSystemLanguageDetectionService.cs
│       │       └── MockWindowsSystemLanguageDetectionService.cs
│       └── Localization/
│           └── LanguageMappingTests.cs
└── Integration/                             # 🐌 Plus lents, vraies APIs
    └── Bucket.App.IntegrationTests/
        ├── Bucket.App.IntegrationTests.csproj
        └── Services/
            └── WindowsSystemLanguageDetectionServiceIntegrationTests.cs
```

### Avantages de cette approche
1. **Séparation claire** - Impossible de confondre les types
2. **CI/CD optimisé** - Tests unitaires rapides en priorité
3. **Maintenance facile** - Structure évidente
4. **Parallélisation** - Différents runners pour différents types

## 📋 Types de Tests - Définitions

### Tests Unitaires
- **Objectif** : Tester une unité de code isolée
- **Dépendances** : Toutes mockées
- **Vitesse** : Très rapides (< 1ms par test)
- **Fiabilité** : 100% déterministes
- **Utilisation** : Validation de la logique métier

### Tests d'Intégration
- **Objectif** : Tester l'interaction entre composants
- **Dépendances** : Vraies dépendances (API, DB, fichiers)
- **Vitesse** : Plus lents (quelques secondes)
- **Fiabilité** : Peuvent échouer selon l'environnement
- **Utilisation** : Validation des intégrations réelles

### Tests E2E (End-to-End)
- **Objectif** : Tester des scénarios utilisateur complets
- **Dépendances** : Système complet
- **Vitesse** : Très lents (minutes)
- **Fiabilité** : Plus fragiles
- **Utilisation** : Validation des workflows utilisateur

## 🚀 Migration depuis la Structure Actuelle

### Étapes Recommandées
1. Créer les nouveaux dossiers `Unit/` et `Integration/`
2. Déplacer les projets existants vers `Unit/`
3. Renommer les projets avec suffixe `.UnitTests`
4. Créer des projets d'intégration si nécessaire
5. Mettre à jour les références dans la solution
6. Adapter les workflows GitHub Actions

### Commandes de Migration
```powershell
# Créer la nouvelle structure
mkdir tests/Unit tests/Integration

# Déplacer les projets existants
mv tests/Bucket.App.Tests tests/Unit/Bucket.App.UnitTests
mv tests/Bucket.Core.Tests tests/Unit/Bucket.Core.UnitTests

# Mettre à jour la solution
dotnet sln remove tests/Bucket.App.Tests/Bucket.App.Tests.csproj
dotnet sln remove tests/Bucket.Core.Tests/Bucket.Core.Tests.csproj
dotnet sln add tests/Unit/Bucket.App.UnitTests/Bucket.App.UnitTests.csproj
dotnet sln add tests/Unit/Bucket.Core.UnitTests/Bucket.Core.UnitTests.csproj
```

## 💡 Bonnes Pratiques Générales

### Règles d'Or
1. **Un test = un comportement** à valider
2. **Tests unitaires** = Mocks obligatoires pour les dépendances
3. **Tests d'intégration** = Vraies dépendances, marqués avec `[Trait]`
4. **Isolation** = Chaque test doit pouvoir s'exécuter indépendamment
5. **Nommage** = `MethodName_Scenario_ExpectedResult`

### Performance CI/CD
- **Tests unitaires** : Toujours exécutés (rapides)
- **Tests d'intégration** : Selon les branches (plus lents)
- **Tests E2E** : Uniquement sur les releases (très lents)

---

**Créé le :** 30 août 2025
**Contexte :** Migration des tests pour résoudre les problèmes GitHub Actions
**Statut actuel :** Tous les tests sont déjà des tests unitaires avec mocks ✅

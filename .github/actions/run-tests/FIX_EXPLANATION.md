# Fix pour l'erreur Bucket.App.Tests.dll introuvable

## Problème identifié

L'erreur se produisait lors de l'exécution des tests pour `Bucket.App.Tests` :
```
The test source file "D:\a\Bucket\Bucket\tests\Bucket.App.Tests\bin\Release\net9.0-windows10.0.26100\Bucket.App.Tests.dll" provided was not found.
```

## Cause racine

La différence de configuration entre les deux projets de test causait une incompatibilité :

### Bucket.Core.Tests (✅ Fonctionnel)
- **Framework** : `net9.0` (standard .NET)
- **Plateformes** : Aucune spécifiée
- **Chemin de sortie** : `bin\Release\net9.0\`

### Bucket.App.Tests (❌ Problématique)
- **Framework** : `net9.0-windows10.0.26100` (Windows-specific)
- **Plateformes** : `x86;x64;ARM64`
- **Chemin de sortie attendu** : `bin\x64\Release\net9.0-windows10.0.26100\`

## Le conflit

1. **Avant** : Utilisation de `msbuild` pour la construction avec `/p:Platform=x64`
   - Place les DLLs dans : `bin\x64\Release\net9.0-windows10.0.26100\`
   
2. **Puis** : Utilisation de `dotnet test --no-build`
   - Cherche les DLLs dans : `bin\Release\net9.0-windows10.0.26100\` (sans le répertoire de plateforme)

## Solution appliquée

### Changements dans `.github/actions/run-tests/action.yml`

1. **Unification de la construction** :
   - Remplacé `msbuild` par `dotnet build` pour la cohérence
   - Ajout du paramètre `-p:Platform=$platform` à `dotnet build`

2. **Cohérence des tests** :
   - Ajout de `-p:Platform=$platform` à tous les appels `dotnet test`
   - Conservation de `--no-build` pour éviter les reconstructions inutiles
   - Les deux projets de test utilisent maintenant la même approche

### Code modifié

```powershell
# Construction unifiée
dotnet build Bucket.sln `
  --configuration $configuration `
  -p:Platform=$platform `
  --no-restore `
  --verbosity minimal

# Tests avec paramètre Platform cohérent
dotnet test $testProject `
  --configuration $configuration `
  --no-restore `
  --no-build `
  -p:Platform=$platform `
  --logger "trx;LogFileName=TestResults-$platform.trx" `
  --results-directory $testResultsPath
```

## Avantages de cette solution

1. **Cohérence** : Utilisation uniforme de `dotnet` CLI pour build et test
2. **Compatibilité** : Gestion correcte des projets Windows avec plateformes multiples
3. **Performance** : Conservation de `--no-build` pour éviter les reconstructions
4. **Maintenabilité** : Code plus simple et plus facile à comprendre

## Validation

Pour valider que la solution fonctionne :

1. Pousser les changements vers GitHub
2. Déclencher le workflow Release Build
3. Vérifier que les tests passent pour toutes les plateformes (x64, x86, ARM64)

## Notes supplémentaires

- La propriété `Platform` doit être explicitement passée pour les projets Windows avec des plateformes multiples
- `dotnet test` peut gérer les projets Windows tant que les paramètres MSBuild sont correctement fournis
- Cette approche évite la complexité d'utiliser directement `msbuild` avec le target `VSTest`

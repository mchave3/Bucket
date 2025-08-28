# Configuration des Mises à Jour NuGet

# Packages à exclure des mises à jour automatiques
# Format: nom_du_package|raison
EXCLUDED_PACKAGES=(
    # Exemples de packages à exclure :
    # "Microsoft.WindowsAppSDK|Version critique pour la compatibilité"
    # "Microsoft.Windows.SDK.BuildTools|Nécessite validation manuelle"
)

# Packages avec versions maximales autorisées
# Format: nom_du_package|version_max
MAX_VERSIONS=(
    # Exemples :
    # "Serilog|4.0.0|Version stable recommandée"
)

# Projets à exclure des mises à jour automatiques
EXCLUDED_PROJECTS=(
    # Exemples :
    # "tests/Bucket.App.Tests"
)

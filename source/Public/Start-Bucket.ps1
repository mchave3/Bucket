Clear-Host

$OutputEncoding = [console]::InputEncoding = [console]::OutputEncoding = New-Object System.Text.UTF8Encoding

Write-SpectreFigletText -Text ":bucket: Bucket"
Write-SpectreHost -Message ":bucket: Bucket"

# Layout simple pour le menu principal
$layout = New-SpectreLayout -Name "root" -Rows @(
    # En-tête
    (New-SpectreLayout -Name "header" -Ratio 1 -Data ("Bienvenue dans Bucket" | Format-SpectrePanel -Header "Menu Principal" -Expand -Color Cyan1)),
    # Corps avec 2 colonnes
    (New-SpectreLayout -Name "body" -Ratio 3 -Columns @(
        # Colonne gauche - Options
        (New-SpectreLayout -Name "options" -Ratio 1 -Data (@(
            "1. Option 1",
            "2. Option 2", 
            "3. Option 3",
            "4. Quitter"
        ) -join "`n" | Format-SpectrePanel -Header "Options" -Expand -Color Green)),
        # Colonne droite - Informations
        (New-SpectreLayout -Name "info" -Ratio 1 -Data ("Informations système ou aide" | Format-SpectrePanel -Header "Info" -Expand -Color Yellow))
    ))
)

Clear-Host
$layout | Out-SpectreHost
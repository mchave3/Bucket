namespace Bucket.Core.Models
{
    /// <summary>
    /// Represents a language option with its code and display name
    /// </summary>
    /// <param name="Code">Language code (e.g., "en-US", "fr-FR")</param>
    /// <param name="DisplayName">Human-readable name (e.g., "English", "Français")</param>
    public record LanguageItem(string Code, string DisplayName);
}

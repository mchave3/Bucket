using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image index editing operations.
/// Handles modification of index metadata (name, description) in WIM files using native WIM APIs.
/// </summary>
public interface IWindowsImageIndexEditingService
{
    /// <summary>
    /// Updates the name of a specific Windows image index.
    /// </summary>
    /// <param name="wimFilePath">The path to the WIM file.</param>
    /// <param name="index">The index number to modify.</param>
    /// <param name="currentName">The current name of the index.</param>
    /// <param name="newName">The new name for the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    Task<bool> UpdateIndexNameAsync(string wimFilePath, int index, string currentName, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the description of a specific Windows image index.
    /// </summary>
    /// <param name="wimFilePath">The path to the WIM file.</param>
    /// <param name="index">The index number to modify.</param>
    /// <param name="currentDescription">The current description of the index.</param>
    /// <param name="newDescription">The new description for the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    Task<bool> UpdateIndexDescriptionAsync(string wimFilePath, int index, string currentDescription, string newDescription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates both name and description of a specific Windows image index in a single operation.
    /// </summary>
    /// <param name="wimFilePath">The path to the WIM file.</param>
    /// <param name="index">The index number to modify.</param>
    /// <param name="currentName">The current name of the index.</param>
    /// <param name="newName">The new name for the index.</param>
    /// <param name="currentDescription">The current description of the index.</param>
    /// <param name="newDescription">The new description for the index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    Task<bool> UpdateIndexMetadataAsync(string wimFilePath, int index, string currentName, string newName, string currentDescription, string newDescription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a WIM file is accessible and not in use by another process.
    /// </summary>
    /// <param name="wimFilePath">The path to the WIM file.</param>
    /// <returns>True if the file is accessible, false otherwise.</returns>
    bool IsWimFileAccessible(string wimFilePath);
}

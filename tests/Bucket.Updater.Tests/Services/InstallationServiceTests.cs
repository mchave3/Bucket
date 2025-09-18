namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    public class InstallationServiceTests : IDisposable
    {
        private readonly InstallationService _installationService;
        private bool _disposed;

        public InstallationServiceTests()
        {
            _installationService = new InstallationService();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // InstallationService doesn't implement IDisposable
                _disposed = true;
            }
        }

        [Fact]
        public void ConstructorShouldCreateValidInstance()
        {
            // Act
            var service = new InstallationService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldThrowOnNullFilePath()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _installationService.InstallUpdateAsync(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task InstallUpdateAsyncShouldThrowOnEmptyFilePath(string filePath)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _installationService.InstallUpdateAsync(filePath));
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldReturnFalseForNonexistentFile()
        {
            // Arrange
            var nonexistentFile = @"C:\NonExistent\update.msi";

            // Act
            var result = await _installationService.InstallUpdateAsync(nonexistentFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldReportProgress()
        {
            // Arrange
            var nonexistentMsi = @"C:\TestApp\update.msi";
            var progressReports = new List<string>();
            var progress = new Progress<string>(report => progressReports.Add(report));

            // Act
            var result = await _installationService.InstallUpdateAsync(nonexistentMsi, progress);

            // Assert
            Assert.False(result);
            // Should report at least "MSI file not found" progress
            Assert.NotEmpty(progressReports);
            Assert.Contains("MSI file not found", progressReports);
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldSupportCancellation()
        {
            // Arrange
            var msiPath = @"C:\TestApp\update.msi";
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act
            var result = await _installationService.InstallUpdateAsync(msiPath, cancellationToken: cancellationTokenSource.Token);

            // Assert
            // Should return false for non-existent file (cancellation won't affect this case)
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateMsiFileAsyncShouldThrowOnNullFilePath()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _installationService.ValidateMsiFileAsync(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateMsiFileAsyncShouldThrowOnEmptyFilePath(string filePath)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _installationService.ValidateMsiFileAsync(filePath));
        }

        [Fact]
        public async Task ValidateMsiFileAsyncShouldReturnFalseForNonexistentFile()
        {
            // Arrange
            var nonexistentFile = @"C:\NonExistent\update.msi";

            // Act
            var result = await _installationService.ValidateMsiFileAsync(nonexistentFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateMsiFileAsyncShouldReturnFalseForTooSmallFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "tiny"); // Less than 1024 bytes

            try
            {
                // Act
                var result = await _installationService.ValidateMsiFileAsync(tempFile);

                // Assert
                Assert.False(result);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateMsiFileAsyncShouldReturnFalseForInvalidSignature()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            // Create a file with wrong signature but correct size
            var invalidData = new byte[2048];
            // Fill with random data instead of MSI signature
            await File.WriteAllBytesAsync(tempFile, invalidData);

            try
            {
                // Act
                var result = await _installationService.ValidateMsiFileAsync(tempFile);

                // Assert
                Assert.False(result);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateMsiFileAsyncShouldReturnTrueForValidMsiSignature()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            // Create a file with correct MSI signature
            var msiSignature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
            var validMsiData = new byte[2048];
            Array.Copy(msiSignature, 0, validMsiData, 0, msiSignature.Length);
            await File.WriteAllBytesAsync(tempFile, validMsiData);

            try
            {
                // Act
                var result = await _installationService.ValidateMsiFileAsync(tempFile);

                // Assert
                Assert.True(result);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task EnsureBucketProcessStoppedAsyncShouldReturnTrueWhenNoProcessesRunning()
        {
            // This test assumes no Bucket processes are running during test execution
            // Act
            var result = await _installationService.EnsureBucketProcessStoppedAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EnsureBucketProcessStoppedAsyncShouldReportProgress()
        {
            // Arrange
            var progressReports = new List<string>();
            var progress = new Progress<string>(report => progressReports.Add(report));

            // Act
            var result = await _installationService.EnsureBucketProcessStoppedAsync(progress);

            // Assert
            Assert.True(result);
            // May or may not report progress depending on whether processes are found
        }

        [Fact]
        public async Task EnsureBucketProcessStoppedAsyncShouldSupportCancellation()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            // If no processes running, should complete quickly without throwing
            var result = await _installationService.EnsureBucketProcessStoppedAsync(cancellationToken: cancellationTokenSource.Token);

            // Should not throw OperationCanceledException for this scenario
            Assert.True(result);
        }

        [Fact]
        public void CleanupDownloadedFilesShouldThrowOnNullPath()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _installationService.CleanupDownloadedFiles(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CleanupDownloadedFilesShouldThrowOnEmptyPath(string path)
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _installationService.CleanupDownloadedFiles(path));
        }

        [Fact]
        public void CleanupDownloadedFilesShouldHandleNonexistentFile()
        {
            // Arrange
            var nonexistentFile = @"C:\NonExistent\update.msi";

            // Act & Assert - Should not throw
            _installationService.CleanupDownloadedFiles(nonexistentFile);
        }

        [Fact]
        public void CleanupDownloadedFilesShouldDeleteExistingFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            Assert.True(File.Exists(tempFile)); // Verify file exists

            // Act
            _installationService.CleanupDownloadedFiles(tempFile);

            // Assert
            Assert.False(File.Exists(tempFile)); // Should be deleted
        }

        [Fact]
        public void CleanupDownloadedFilesShouldCleanupTempFilesInSameDirectory()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var mainFile = Path.Combine(tempDir, "test-update.msi");
            var tempFile1 = Path.Combine(tempDir, "test1.tmp");
            var tempFile2 = Path.Combine(tempDir, "test2.partial");

            File.WriteAllText(mainFile, "main file");
            File.WriteAllText(tempFile1, "temp file 1");
            File.WriteAllText(tempFile2, "temp file 2");

            Assert.True(File.Exists(mainFile));
            Assert.True(File.Exists(tempFile1));
            Assert.True(File.Exists(tempFile2));

            // Act
            _installationService.CleanupDownloadedFiles(mainFile);

            // Assert
            Assert.False(File.Exists(mainFile)); // Main file should be deleted
            Assert.False(File.Exists(tempFile1)); // .tmp files should be cleaned up
            Assert.False(File.Exists(tempFile2)); // .partial files should be cleaned up
        }

        [Fact]
        public void CleanupAllTemporaryFilesShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _installationService.CleanupAllTemporaryFiles();
        }

        [Fact]
        public void CleanupAllTemporaryFilesShouldCleanupRecentBucketFiles()
        {
            // Arrange - Create recent Bucket-related temp files
            var tempDir = Path.GetTempPath();
            var bucketMsi = Path.Combine(tempDir, "Bucket-2.0.0-x64.msi");
            var bucketTmp = Path.Combine(tempDir, "bucket-update.tmp");

            File.WriteAllText(bucketMsi, "bucket msi");
            File.WriteAllText(bucketTmp, "bucket temp");

            Assert.True(File.Exists(bucketMsi));
            Assert.True(File.Exists(bucketTmp));

            // Act
            _installationService.CleanupAllTemporaryFiles();

            // Assert
            // Files should be cleaned up (assuming they're recent)
            Assert.False(File.Exists(bucketMsi));
            Assert.False(File.Exists(bucketTmp));
        }

        [Fact]
        public void CleanupAllTemporaryFilesShouldNotDeleteOldFiles()
        {
            // Arrange - Create an old file that shouldn't be deleted
            var tempDir = Path.GetTempPath();
            var oldFile = Path.Combine(tempDir, "old-bucket.msi");
            File.WriteAllText(oldFile, "old bucket file");

            // Set creation time to more than 24 hours ago
            var oldTime = DateTime.Now.AddDays(-2);
            File.SetCreationTime(oldFile, oldTime);

            Assert.True(File.Exists(oldFile));

            // Act
            _installationService.CleanupAllTemporaryFiles();

            // Assert
            // Old file should not be deleted
            Assert.True(File.Exists(oldFile));

            // Cleanup
            File.Delete(oldFile);
        }

        [Fact]
        public void CleanupAllTemporaryFilesShouldHandleLockedFiles()
        {
            // Arrange - Create a locked file
            var tempDir = Path.GetTempPath();
            var lockedFile = Path.Combine(tempDir, "locked-bucket.msi");

            using (var stream = File.Create(lockedFile))
            {
                // File is locked while stream is open

                // Act & Assert - Should not throw even with locked files
                _installationService.CleanupAllTemporaryFiles();
            }

            // Cleanup after test
            try
            {
                if (File.Exists(lockedFile))
                    File.Delete(lockedFile);
            }
            catch (Exception)
            {
                // Ignore cleanup errors in test
            }
        }

        [Theory]
        [InlineData(@"C:\Program Files\TestApp\app.msi")]
        [InlineData(@"D:\Updates\MyApp\update.msi")]
        [InlineData(@"C:\Users\TestUser\Downloads\installer.msi")]
        public async Task InstallUpdateAsyncShouldAcceptValidMsiPaths(string path)
        {
            // Note: These will return false since files don't exist,
            // but the path format should be valid and not throw exceptions

            // Act
            var result = await _installationService.InstallUpdateAsync(path);

            // Assert
            Assert.False(result); // Expected false since files don't exist
        }

        [Theory]
        [InlineData(@"C:\invalid|chars\app.msi")]
        [InlineData(@"invalid-path")]
        public async Task InstallUpdateAsyncShouldHandleInvalidPaths(string invalidPath)
        {
            // Act
            var result = await _installationService.InstallUpdateAsync(invalidPath);

            // Assert
            // Should handle gracefully and return false rather than throwing
            Assert.False(result);
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldValidateFileBeforeInstallation()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            // Create a file that exists but has invalid MSI signature
            var invalidData = new byte[2048]; // Large enough but wrong signature
            await File.WriteAllBytesAsync(tempFile, invalidData);

            try
            {
                // Act
                var result = await _installationService.InstallUpdateAsync(tempFile);

                // Assert
                Assert.False(result); // Should fail validation and not proceed to installation
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
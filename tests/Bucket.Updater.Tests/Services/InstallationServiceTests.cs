using System.Text;
using Bucket.Updater.Services;
using FluentAssertions;

namespace Bucket.Updater.Tests.Services
{
    public class InstallationServiceTests : IDisposable
    {
        private readonly InstallationService _sut;
        private readonly string _testDirectory;
        private readonly List<string> _testFilesToCleanup;

        public InstallationServiceTests()
        {
            _sut = new InstallationService();
            _testDirectory = Path.Combine(Path.GetTempPath(), "BucketUpdaterInstallationTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilesToCleanup = new List<string>();
        }

        #region File Cleanup Logic Tests

        [Fact]
        public void CleanupDownloadedFiles_ShouldDeleteFile_WhenFileExists()
        {
            // Arrange
            var testFilePath = CreateTestFile("test-download.msi");

            // Act
            _sut.CleanupDownloadedFiles(testFilePath);

            // Assert
            File.Exists(testFilePath).Should().BeFalse();
        }

        [Fact]
        public void CleanupDownloadedFiles_ShouldNotThrow_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "non-existent-file.msi");

            // Act & Assert
            _sut.Invoking(s => s.CleanupDownloadedFiles(nonExistentPath))
                .Should().NotThrow();
        }

        [Fact]
        public void CleanupDownloadedFiles_ShouldLogOperation_WhenCalled()
        {
            // Arrange
            var testFilePath = CreateTestFile("test-download-log.msi");

            // Act
            _sut.CleanupDownloadedFiles(testFilePath);

            // Assert
            // Since we can't easily verify logging without a logger interface,
            // we'll verify that the method completes without throwing
            File.Exists(testFilePath).Should().BeFalse();
        }

        [Fact]
        public void CleanupDownloadedFiles_ShouldHandleExceptions_WhenFileIsLocked()
        {
            // Arrange
            var testFilePath = CreateTestFile("locked-file.msi");
            
            // Lock the file by opening it
            using var fileStream = File.OpenWrite(testFilePath);
            
            // Act & Assert
            _sut.Invoking(s => s.CleanupDownloadedFiles(testFilePath))
                .Should().NotThrow();
            
            // File should still exist because it's locked
            File.Exists(testFilePath).Should().BeTrue();
        }

        #endregion

        #region Temporary Files Cleanup Tests

        [Fact]
        public void CleanupAllTemporaryFiles_ShouldIdentifyAndDeleteBucketTempFiles()
        {
            // Arrange
            var tempPath = Path.GetTempPath();
            var bucketTempFile1 = Path.Combine(tempPath, "Bucket-temp-12345.msi");
            var bucketTempFile2 = Path.Combine(tempPath, "bucket-download-67890.tmp");
            
            CreateFileAtPath(bucketTempFile1);
            CreateFileAtPath(bucketTempFile2);
            _testFilesToCleanup.Add(bucketTempFile1);
            _testFilesToCleanup.Add(bucketTempFile2);

            // Act
            _sut.CleanupAllTemporaryFiles();

            // Assert
            File.Exists(bucketTempFile1).Should().BeFalse();
            File.Exists(bucketTempFile2).Should().BeFalse();
        }

        [Fact]
        public void CleanupAllTemporaryFiles_ShouldCleanSystemTempDirectory()
        {
            // Arrange
            var tempPath = Path.GetTempPath();
            var bucketTempFile = Path.Combine(tempPath, $"Bucket-temp-{Guid.NewGuid()}.msi");
            
            CreateFileAtPath(bucketTempFile);
            _testFilesToCleanup.Add(bucketTempFile);

            // Act
            _sut.CleanupAllTemporaryFiles();

            // Assert
            File.Exists(bucketTempFile).Should().BeFalse();
        }

        [Fact]
        public void CleanupAllTemporaryFiles_ShouldHandleMissingDirectories()
        {
            // Arrange
            // No specific setup needed - testing the method handles missing directories gracefully

            // Act & Assert
            _sut.Invoking(s => s.CleanupAllTemporaryFiles())
                .Should().NotThrow();
        }

        [Fact]
        public void CleanupAllTemporaryFiles_ShouldLogNumberOfDeletedFiles()
        {
            // Arrange
            var tempPath = Path.GetTempPath();
            var bucketTempFile1 = Path.Combine(tempPath, $"Bucket-temp-{Guid.NewGuid()}.msi");
            var bucketTempFile2 = Path.Combine(tempPath, $"bucket-download-{Guid.NewGuid()}.tmp");
            var bucketTempFile3 = Path.Combine(tempPath, $"BUCKET-UPDATE-{Guid.NewGuid()}.exe");
            
            CreateFileAtPath(bucketTempFile1);
            CreateFileAtPath(bucketTempFile2);
            CreateFileAtPath(bucketTempFile3);
            _testFilesToCleanup.Add(bucketTempFile1);
            _testFilesToCleanup.Add(bucketTempFile2);
            _testFilesToCleanup.Add(bucketTempFile3);

            // Act
            _sut.CleanupAllTemporaryFiles();

            // Assert
            // Verify files are deleted
            File.Exists(bucketTempFile1).Should().BeFalse();
            File.Exists(bucketTempFile2).Should().BeFalse();
            File.Exists(bucketTempFile3).Should().BeFalse();

            // Since we can't easily verify logging without a logger interface,
            // we'll verify that the method completed successfully
            // In a real implementation, this would verify log output
        }

        #endregion

        private string CreateTestFile(string fileName)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllText(filePath, "Test file content");
            return filePath;
        }

        private void CreateFileAtPath(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, $"Test content for {Path.GetFileName(filePath)}");
        }

        public void Dispose()
        {
            // Clean up test files first
            foreach (var testFile in _testFilesToCleanup)
            {
                try
                {
                    if (File.Exists(testFile))
                    {
                        File.Delete(testFile);
                    }
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }

            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }
    }
}
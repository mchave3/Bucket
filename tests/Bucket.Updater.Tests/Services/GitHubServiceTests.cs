using System.Net;
using System.Text;
using Bucket.Updater.Models;
using Bucket.Updater.Models.GitHub;
using Bucket.Updater.Services;
using Bucket.Updater.Tests.Helpers;
using Bucket.Updater.Tests.TestData;
using FluentAssertions;
using Moq;

namespace Bucket.Updater.Tests.Services
{
    public class GitHubServiceTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubService _sut;
        private MockHttpMessageHandler _mockHandler;

        public GitHubServiceTests()
        {
            _mockHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHandler);
            _sut = new GitHubService();
            
            // Use reflection to set the private HttpClient field for testing
            var field = typeof(GitHubService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_sut, _httpClient);
        }

        #region Version Comparison Logic Tests

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnUpdate_WhenNewVersionIsGreater()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.1.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.2.0");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenNewVersionIsLower()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.3.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenVersionsAreEqual()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.2.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldHandleVersionsWithVPrefix()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "v1.1.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.2.0");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldIgnorePrereleasesSuffixes()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.1.0";
            
            var responseWithBeta = """
                [
                  {
                    "tag_name": "v1.2.0-beta",
                    "name": "Release 1.2.0 Beta",
                    "body": "Beta release",
                    "published_at": "2025-09-12T10:00:00Z",
                    "prerelease": false,
                    "assets": [
                      {
                        "name": "Bucket-1.2.0-beta-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0-beta/Bucket-1.2.0-beta-x64.msi",
                        "size": 50000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                responseWithBeta
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.2.0-beta");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenVersionParsingFails()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.1.0";
            
            var malformedVersionResponse = """
                [
                  {
                    "tag_name": "invalid.version.format",
                    "name": "Invalid Release",
                    "body": "Invalid version",
                    "published_at": "2025-09-12T10:00:00Z",
                    "prerelease": false,
                    "assets": [
                      {
                        "name": "Bucket-invalid-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/invalid/Bucket-invalid-x64.msi",
                        "size": 50000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                malformedVersionResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldHandleDifferentVersionFormats()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.2.3";
            
            var versionFormatResponse = """
                [
                  {
                    "tag_name": "v1.2.3.0",
                    "name": "Release 1.2.3.0",
                    "body": "Four-part version",
                    "published_at": "2025-09-12T10:00:00Z",
                    "prerelease": false,
                    "assets": [
                      {
                        "name": "Bucket-1.2.3.0-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.3.0/Bucket-1.2.3.0-x64.msi",
                        "size": 50000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                versionFormatResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert  
            // 1.2.3.0 should be considered equal to 1.2.3
            result.Should().BeNull();
        }

        #endregion

        #region Release Filtering Logic Tests

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenNoReleaseFoundForReleaseChannel()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.UpdateChannel = UpdateChannel.Release;
            configuration.CurrentVersion = "1.0.0";
            
            var prereleaseOnlyResponse = """
                [
                  {
                    "tag_name": "v1.2.0-beta",
                    "name": "Beta Release",
                    "body": "Beta only",
                    "published_at": "2025-09-12T10:00:00Z",
                    "prerelease": true,
                    "assets": [
                      {
                        "name": "Bucket-1.2.0-beta-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0-beta/Bucket-1.2.0-beta-x64.msi",
                        "size": 50000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                prereleaseOnlyResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenNoReleaseFoundForNightlyChannel()
        {
            // Arrange
            var configuration = TestFixtures.CreateNightlyConfiguration();
            configuration.CurrentVersion = "1.0.0";
            
            var stableOnlyResponse = """
                [
                  {
                    "tag_name": "v1.2.0",
                    "name": "Stable Release",
                    "body": "Stable only",
                    "published_at": "2025-09-12T10:00:00Z",
                    "prerelease": false,
                    "assets": [
                      {
                        "name": "Bucket-1.2.0-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi",
                        "size": 50000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                stableOnlyResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldFilterReleasesForReleaseChannel_WhenNonPrereleaseOnly()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.UpdateChannel = UpdateChannel.Release;
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.2.0"); // Should get the stable release, not the nightly
            result.IsPrerelease.Should().BeFalse();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldFilterReleasesForNightlyChannel_WhenPrereleaseWithNightly()
        {
            // Arrange
            var configuration = TestFixtures.CreateNightlyConfiguration();
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.3.0-nightly");
            result.IsPrerelease.Should().BeTrue();
        }

        #endregion

        #region Asset Selection Logic Tests

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenNoMsiAssetFoundForArchitecture()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.Architecture = SystemArchitecture.ARM64; // No ARM64 asset in basic response
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ValidReleaseResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenX86Architecture()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.Architecture = SystemArchitecture.X86;
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.MultiArchitectureResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Assets.Should().ContainSingle()
                .Which.Name.Should().Contain("x86");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenX64Architecture()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.Architecture = SystemArchitecture.X64;
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.MultiArchitectureResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Assets.Should().ContainSingle()
                .Which.Name.Should().Contain("x64");
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldSelectCorrectMsiAsset_WhenARM64Architecture()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.Architecture = SystemArchitecture.ARM64;
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.MultiArchitectureResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Assets.Should().ContainSingle()
                .Which.Name.Should().Contain("arm64");
        }

        #endregion

        #region Data Mapping and Error Handling Tests

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnCorrectUpdateInfo_WhenUpdateAvailable()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ValidReleaseResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.2.0");
            result.Body.Should().Be("Release notes for version 1.2.0");
            result.PublishedAt.Should().Be(new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc));
            result.IsPrerelease.Should().BeFalse();
            result.DownloadUrl.Should().Be("https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi");
            result.FileSize.Should().Be(50000000);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldMapGitHubDataCorrectly_WhenMappingToUpdateInfo()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            configuration.CurrentVersion = "1.0.0";
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.PrereleaseResponse
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.3.0-beta");
            result.Body.Should().Be("Beta release with new features");
            result.IsPrerelease.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.InternalServerError,
                "Server Error"
            );

            // Act
            var result = await _sut.CheckForUpdatesAsync(configuration);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetReleasesAsync Tests

        [Fact]
        public async Task GetReleasesAsync_ShouldReturnEmptyList_WhenApiCallFails()
        {
            // Arrange
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.NotFound,
                "Not Found"
            );

            // Act
            var result = await _sut.GetReleasesAsync("mchave3", "Bucket", true);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReleasesAsync_ShouldFilterPrereleases_WhenIncludePrereleaseIsFalse()
        {
            // Arrange
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.GetReleasesAsync("mchave3", "Bucket", false);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => !r.Prerelease);
        }

        [Fact]
        public async Task GetReleasesAsync_ShouldIncludeAllReleases_WhenIncludePrereleaseIsTrue()
        {
            // Arrange
            _mockHandler.SetupGet(
                "https://api.github.com/repos/mchave3/Bucket/releases",
                HttpStatusCode.OK,
                TestFixtures.GitHubApiResponses.ReleasesListResponse
            );

            // Act
            var result = await _sut.GetReleasesAsync("mchave3", "Bucket", true);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(r => r.Prerelease);
            result.Should().Contain(r => !r.Prerelease);
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
            _sut?.Dispose();
        }
    }
}
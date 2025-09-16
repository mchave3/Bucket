namespace Bucket.Updater.Tests.Models.GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bucket.Updater.Models.GitHub;
    using Xunit;

    public class GitHubReleaseTests
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            // Act
            var gitHubRelease = new GitHubRelease();

            // Assert
            Assert.Equal(0, gitHubRelease.Id);
            Assert.Equal(string.Empty, gitHubRelease.TagName);
            Assert.Equal(string.Empty, gitHubRelease.Name);
            Assert.Equal(string.Empty, gitHubRelease.Body);
            Assert.Equal(DateTime.MinValue, gitHubRelease.PublishedAt);
            Assert.False(gitHubRelease.Prerelease);
            Assert.NotNull(gitHubRelease.Assets);
            Assert.Empty(gitHubRelease.Assets);
        }

        [Fact]
        public void PropertiesShouldAllowGetAndSet()
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();
            var testId = 123456789L;
            var testTagName = "v24.1.15";
            var testName = "Release 24.1.15";
            var testBody = "Bug fixes and improvements";
            var testPublishedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var testAssets = new List<GitHubAsset>
            {
                new GitHubAsset { Name = "Bucket--x64.msi", Size = 52428800 }
            };

            // Act
            gitHubRelease.Id = testId;
            gitHubRelease.TagName = testTagName;
            gitHubRelease.Name = testName;
            gitHubRelease.Body = testBody;
            gitHubRelease.PublishedAt = testPublishedAt;
            gitHubRelease.Prerelease = true;
            gitHubRelease.Assets = testAssets;

            // Assert
            Assert.Equal(testId, gitHubRelease.Id);
            Assert.Equal(testTagName, gitHubRelease.TagName);
            Assert.Equal(testName, gitHubRelease.Name);
            Assert.Equal(testBody, gitHubRelease.Body);
            Assert.Equal(testPublishedAt, gitHubRelease.PublishedAt);
            Assert.True(gitHubRelease.Prerelease);
            Assert.Same(testAssets, gitHubRelease.Assets);
        }

        [Theory]
        [InlineData("v24.1.15")]
        [InlineData("v24.1.15.1")]
        [InlineData("v24.1.15-Nightly")]
        [InlineData("24.1.15")]
        public void TagNameShouldAcceptValidVersionTags(string tagName)
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();

            // Act
            gitHubRelease.TagName = tagName;

            // Assert
            Assert.Equal(tagName, gitHubRelease.TagName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PrereleaseShouldSupportBothValues(bool isPrerelease)
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();

            // Act
            gitHubRelease.Prerelease = isPrerelease;

            // Assert
            Assert.Equal(isPrerelease, gitHubRelease.Prerelease);
        }

        [Fact]
        public void AssetsShouldMaintainListReference()
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();
            var originalAssets = gitHubRelease.Assets;
            var newAsset = new GitHubAsset { Name = "NewAsset.msi" };

            // Act
            gitHubRelease.Assets.Add(newAsset);

            // Assert
            Assert.Same(originalAssets, gitHubRelease.Assets);
            Assert.Single(gitHubRelease.Assets);
            Assert.Equal("NewAsset.msi", gitHubRelease.Assets.First().Name);
        }

        [Fact]
        public void GitHubReleaseShouldCreateCompleteStableRelease()
        {
            // Arrange & Act
            var gitHubRelease = new GitHubRelease
            {
                Id = 123456789,
                TagName = "v24.1.15",
                Name = "Bucket Release 24.1.15",
                Body = "# What's Changed\n- Fixed critical bug\n- Performance improvements\n\n**Full Changelog**: https://github.com/mchave3/Bucket/compare/v24.1.14...v24.1.15",
                PublishedAt = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                Prerelease = false,
                Assets = new List<GitHubAsset>
                {
                    new GitHubAsset { Id = 111, Name = "Bucket--x64.msi", Size = 52428800, ContentType = "application/x-msi" },
                    new GitHubAsset { Id = 222, Name = "Bucket--x86.msi", Size = 48234496, ContentType = "application/x-msi" },
                    new GitHubAsset { Id = 333, Name = "Bucket--arm64.msi", Size = 51380224, ContentType = "application/x-msi" }
                }
            };

            // Assert
            Assert.Equal(123456789, gitHubRelease.Id);
            Assert.Equal("v24.1.15", gitHubRelease.TagName);
            Assert.StartsWith("v", gitHubRelease.TagName, StringComparison.Ordinal);
            Assert.Equal("Bucket Release 24.1.15", gitHubRelease.Name);
            Assert.Contains("Fixed critical bug", gitHubRelease.Body, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Full Changelog", gitHubRelease.Body, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc), gitHubRelease.PublishedAt);
            Assert.False(gitHubRelease.Prerelease);
            Assert.Equal(3, gitHubRelease.Assets.Count);
            Assert.All(gitHubRelease.Assets, asset => Assert.EndsWith(".msi", asset.Name, StringComparison.OrdinalIgnoreCase));
            Assert.Contains(gitHubRelease.Assets, asset => asset.Name.Contains("x64", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(gitHubRelease.Assets, asset => asset.Name.Contains("x86", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(gitHubRelease.Assets, asset => asset.Name.Contains("arm64", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GitHubReleaseShouldCreateCompleteNightlyRelease()
        {
            // Arrange & Act
            var gitHubRelease = new GitHubRelease
            {
                Id = 987654321,
                TagName = "v24.1.15.1-Nightly",
                Name = "Nightly Build 24.1.15.1",
                Body = "Automated nightly build from dev branch\n\nChanges since last release:\n- Feature improvements\n- Bug fixes",
                PublishedAt = DateTime.UtcNow,
                Prerelease = true,
                Assets = new List<GitHubAsset>
                {
                    new GitHubAsset { Id = 444, Name = "Bucket--x64.msi", Size = 52428800, ContentType = "application/x-msi" },
                    new GitHubAsset { Id = 555, Name = "Bucket--x86.msi", Size = 48234496, ContentType = "application/x-msi" },
                    new GitHubAsset { Id = 666, Name = "Bucket--arm64.msi", Size = 51380224, ContentType = "application/x-msi" }
                }
            };

            // Assert
            Assert.Equal(987654321, gitHubRelease.Id);
            Assert.EndsWith("-Nightly", gitHubRelease.TagName, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Nightly Build", gitHubRelease.Name, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Automated nightly build", gitHubRelease.Body, StringComparison.OrdinalIgnoreCase);
            Assert.True(gitHubRelease.Prerelease);
            Assert.Equal(3, gitHubRelease.Assets.Count);
        }

        [Fact]
        public void GitHubReleaseShouldHandleEmptyAssetsList()
        {
            // Arrange & Act
            var gitHubRelease = new GitHubRelease
            {
                Id = 123,
                TagName = "v1.0.0",
                Name = "Empty Release",
                Body = "No assets",
                PublishedAt = DateTime.UtcNow,
                Prerelease = false,
                Assets = new List<GitHubAsset>()
            };

            // Assert
            Assert.NotNull(gitHubRelease.Assets);
            Assert.Empty(gitHubRelease.Assets);
        }

        [Theory]
        [InlineData("v24.1.15", false, false)]
        [InlineData("v24.1.15.1-Nightly", true, true)]
        [InlineData("v24.1.15-alpha", true, true)]
        [InlineData("v24.1.15-beta", true, true)]
        public void ReleaseShouldCorrectlyIdentifyPrereleases(string tagName, bool isPrerelease, bool expectedPrerelease)
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();

            // Act
            gitHubRelease.TagName = tagName;
            gitHubRelease.Prerelease = isPrerelease;

            // Assert
            Assert.Equal(expectedPrerelease, gitHubRelease.Prerelease);
            if (expectedPrerelease)
            {
                Assert.True(gitHubRelease.TagName.Contains('-', StringComparison.Ordinal) ||
                           gitHubRelease.TagName.Contains("Nightly", StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void PublishedAtShouldSupportUtcDateTime()
        {
            // Arrange
            var gitHubRelease = new GitHubRelease();
            var utcTime = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc);

            // Act
            gitHubRelease.PublishedAt = utcTime;

            // Assert
            Assert.Equal(utcTime, gitHubRelease.PublishedAt);
            Assert.Equal(DateTimeKind.Utc, gitHubRelease.PublishedAt.Kind);
        }

        [Fact]
        public void GitHubReleaseShouldSupportLongReleaseNotes()
        {
            // Arrange
            var longReleaseNotes = string.Join("\n", Enumerable.Range(1, 100).Select(i => $"Line {i} of release notes"));
            var gitHubRelease = new GitHubRelease();

            // Act
            gitHubRelease.Body = longReleaseNotes;

            // Assert
            Assert.Equal(longReleaseNotes, gitHubRelease.Body);
            Assert.Contains("Line 1 of release notes", gitHubRelease.Body, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Line 100 of release notes", gitHubRelease.Body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
using Bucket.Updater.Models;
using Bucket.Updater.Models.GitHub;

namespace Bucket.Updater.Tests.TestData
{
    public static class TestFixtures
    {
        public static class GitHubApiResponses
        {
            public const string ValidReleaseResponse = """
                {
                  "tag_name": "v1.2.0",
                  "name": "Release 1.2.0", 
                  "body": "Release notes for version 1.2.0",
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
                """;

            public const string PrereleaseResponse = """
                {
                  "tag_name": "v1.3.0-beta",
                  "name": "Release 1.3.0 Beta",
                  "body": "Beta release with new features",
                  "published_at": "2025-09-13T10:00:00Z",
                  "prerelease": true,
                  "assets": [
                    {
                      "name": "Bucket-1.3.0-nightly-x64.msi",
                      "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.3.0-beta/Bucket-1.3.0-nightly-x64.msi",
                      "size": 52000000,
                      "content_type": "application/octet-stream"
                    }
                  ]
                }
                """;

            public const string MultiArchitectureResponse = """
                {
                  "tag_name": "v1.2.0",
                  "name": "Release 1.2.0",
                  "body": "Multi-architecture release",
                  "published_at": "2025-09-12T10:00:00Z",
                  "prerelease": false,
                  "assets": [
                    {
                      "name": "Bucket-1.2.0-x86.msi",
                      "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x86.msi",
                      "size": 45000000,
                      "content_type": "application/octet-stream"
                    },
                    {
                      "name": "Bucket-1.2.0-x64.msi",
                      "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi",
                      "size": 50000000,
                      "content_type": "application/octet-stream"
                    },
                    {
                      "name": "Bucket-1.2.0-arm64.msi",
                      "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-arm64.msi",
                      "size": 48000000,
                      "content_type": "application/octet-stream"
                    }
                  ]
                }
                """;

            public const string ReleasesListResponse = """
                [
                  {
                    "tag_name": "v1.2.0",
                    "name": "Release 1.2.0",
                    "body": "Latest stable release",
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
                  },
                  {
                    "tag_name": "v1.1.0",
                    "name": "Release 1.1.0",
                    "body": "Previous stable release",
                    "published_at": "2025-09-01T10:00:00Z",
                    "prerelease": false,
                    "assets": [
                      {
                        "name": "Bucket-1.1.0-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.1.0/Bucket-1.1.0-x64.msi",
                        "size": 48000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  },
                  {
                    "tag_name": "v1.3.0-nightly",
                    "name": "Nightly Build 1.3.0",
                    "body": "Nightly development build",
                    "published_at": "2025-09-15T02:00:00Z",
                    "prerelease": true,
                    "assets": [
                      {
                        "name": "Bucket-1.3.0-nightly-x64.msi",
                        "browser_download_url": "https://github.com/mchave3/Bucket/releases/download/v1.3.0-nightly/Bucket-1.3.0-nightly-x64.msi",
                        "size": 52000000,
                        "content_type": "application/octet-stream"
                      }
                    ]
                  }
                ]
                """;
        }

        public static class AppConfigurations
        {
            public const string ValidConfiguration = """
                {
                  "UpdateChannel": "Release",
                  "Architecture": "X64",
                  "GitHubOwner": "mchave3",
                  "GitHubRepository": "Bucket",
                  "CurrentVersion": "1.0.0.0",
                  "LastUpdateCheck": "2025-09-12T10:00:00Z"
                }
                """;

            public const string NightlyConfiguration = """
                {
                  "UpdateChannel": "Nightly",
                  "Architecture": "X86",
                  "GitHubOwner": "mchave3",
                  "GitHubRepository": "Bucket",
                  "CurrentVersion": "1.0.0.0",
                  "LastUpdateCheck": "2025-09-10T15:30:00Z"
                }
                """;

            public const string MinimalConfiguration = """
                {
                  "CurrentVersion": "1.1.0.0"
                }
                """;

            public const string MalformedJson = """
                {
                  "UpdateChannel": "Release",
                  "Architecture": "X64"
                  "GitHubOwner": "mchave3"
                }
                """;

            public const string InvalidEnumValues = """
                {
                  "UpdateChannel": "InvalidChannel",
                  "Architecture": "InvalidArch",
                  "CurrentVersion": "1.0.0.0"
                }
                """;
        }

        public static class VersionTestData
        {
            public static readonly (string newer, string older)[] VersionComparisons = {
                ("1.2.0", "1.1.0"),
                ("v1.2.0", "v1.1.0"),
                ("1.2.0-beta", "1.1.0"),
                ("2.0.0", "1.9.9"),
                ("1.10.0", "1.9.0"),
                ("1.2.3", "1.2.2")
            };

            public static readonly string[] MalformedVersions = {
                "invalid.version",
                "1.2.x",
                "",
                "v",
                "1.2.3.4.5",
                "abc.def.ghi"
            };

            public static readonly (string version1, string version2)[] EqualVersions = {
                ("1.2.0", "1.2.0"),
                ("v1.2.0", "v1.2.0"),
                ("1.2.3", "1.2.3.0")
            };
        }

        public static UpdaterConfiguration CreateDefaultConfiguration()
        {
            return new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                CurrentVersion = "1.0.0.0",
                LastUpdateCheck = new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc)
            };
        }

        public static UpdaterConfiguration CreateNightlyConfiguration()
        {
            return new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.X86,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                CurrentVersion = "1.0.0.0",
                LastUpdateCheck = new DateTime(2025, 9, 10, 15, 30, 0, DateTimeKind.Utc)
            };
        }

        public static GitHubRelease CreateValidRelease()
        {
            return new GitHubRelease
            {
                TagName = "v1.2.0",
                Name = "Release 1.2.0",
                Body = "Release notes for version 1.2.0",
                PublishedAt = new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc),
                Prerelease = false,
                Assets = new List<GitHubAsset>
                {
                    new GitHubAsset
                    {
                        Name = "Bucket-1.2.0-x64.msi",
                        BrowserDownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi",
                        Size = 50000000,
                        ContentType = "application/octet-stream"
                    }
                }
            };
        }

        public static UpdateInfo CreateValidUpdateInfo()
        {
            return new UpdateInfo
            {
                Version = "1.2.0",
                Body = "Release notes for version 1.2.0",
                PublishedAt = new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc),
                IsPrerelease = false,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi",
                FileSize = 50000000,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket-1.2.0-x64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v1.2.0/Bucket-1.2.0-x64.msi",
                        Size = 50000000
                    }
                }
            };
        }
    }
}
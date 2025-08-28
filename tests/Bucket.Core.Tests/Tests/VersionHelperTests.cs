using System.Reflection;
using Bucket.Core.Helpers;

namespace Bucket.Core.Tests
{
    [TestClass]
    public class VersionHelperTests
    {
        [TestMethod]
        public void GetAppVersion_ShouldReturnValidVersion()
        {
            // Act
            var result = VersionHelper.GetAppVersion();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreNotEqual("Unknown", result);
        }

        [TestMethod]
        public void GetAppVersion_WithSpecificAssembly_ShouldReturnVersionFromAssembly()
        {
            // Arrange
            var testAssembly = Assembly.GetExecutingAssembly();

            // Act
            var result = VersionHelper.GetAppVersion(testAssembly);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetAppVersionWithPrefix_ShouldReturnVersionWithDefaultPrefix()
        {
            // Act
            var result = VersionHelper.GetAppVersionWithPrefix();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("v"));
            Assert.IsTrue(result.Length > 1); // Should have more than just the prefix
        }

        [TestMethod]
        public void GetAppVersionWithPrefix_WithCustomPrefix_ShouldReturnVersionWithCustomPrefix()
        {
            // Arrange
            var customPrefix = "version-";

            // Act
            var result = VersionHelper.GetAppVersionWithPrefix(prefix: customPrefix);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith(customPrefix));
            Assert.IsTrue(result.Length > customPrefix.Length);
        }

        [TestMethod]
        public void GetAppVersionWithPrefix_WithEmptyPrefix_ShouldReturnVersionWithoutPrefix()
        {
            // Act
            var result = VersionHelper.GetAppVersionWithPrefix(prefix: "");
            var versionOnly = VersionHelper.GetAppVersion();

            // Assert
            Assert.AreEqual(versionOnly, result);
        }

        [TestMethod]
        public void GetAppVersion_WithNullAssembly_ShouldUseDefaultAssembly()
        {
            // Act
            var result = VersionHelper.GetAppVersion(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetAppVersionWithPrefix_ConsistentWithGetAppVersion()
        {
            // Arrange
            var prefix = "test-";

            // Act
            var versionOnly = VersionHelper.GetAppVersion();
            var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);

            // Assert
            Assert.AreEqual($"{prefix}{versionOnly}", versionWithPrefix);
        }
    }
}

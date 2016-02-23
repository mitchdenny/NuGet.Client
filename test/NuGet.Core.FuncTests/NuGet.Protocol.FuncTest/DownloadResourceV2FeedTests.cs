﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Logging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;
using NuGet.Protocol.Core.v3;

namespace NuGet.Protocol.FuncTest
{
    public class DownloadResourceV2FeedTests
    {
        [Fact]
        public async Task DownloadResourceFromUrl()
        {
            // Arrange
            var repo = Repository.Factory.GetCoreV3("https://www.nuget.org/api/v2/");

            var downloadResource = await repo.GetResourceAsync<DownloadResource>();

            var package = new SourcePackageDependencyInfo("WindowsAzure.Storage", new NuGetVersion("6.2.0"), null, true, repo, new Uri("https://www.nuget.org/api/v2/package/WindowsAzure.Storage/6.2.0"), "");

            // Act & Assert
            using (var downloadResult = await downloadResource.GetDownloadResourceResultAsync(package,
                                                              NullSettings.Instance,
                                                              NullLogger.Instance,
                                                              CancellationToken.None))
            {
                var packageReader = downloadResult.PackageReader;
                var files = packageReader.GetFiles();

                Assert.Equal(11, files.Count());
            }
        }

        [Fact]
        public async Task DownloadResourceFromIdentity()
        {
            // Arrange
            var repo = Repository.Factory.GetCoreV3("https://www.nuget.org/api/v2/");

            var downloadResource = await repo.GetResourceAsync<DownloadResource>();

            var package = new PackageIdentity("WindowsAzure.Storage", new NuGetVersion("6.2.0"));

            // Act & Assert
            using (var downloadResult = await downloadResource.GetDownloadResourceResultAsync(package,
                                                              NullSettings.Instance,
                                                              NullLogger.Instance,
                                                              CancellationToken.None))
            {
                var packageReader = downloadResult.PackageReader;
                var files = packageReader.GetFiles();

                Assert.Equal(11, files.Count());
            }
        }

        [Fact]
        public async Task DownloadResourceFromInvalidIdInUrl()
        {
            // Arrange
            var repo = Repository.Factory.GetCoreV3("https://www.nuget.org/api/v2/");

            var downloadResource = await repo.GetResourceAsync<DownloadResource>();

            var package = new SourcePackageDependencyInfo("not-found", new NuGetVersion("6.2.0"), null, true, repo, new Uri("https://www.nuget.org/api/v2/package/not-found/6.2.0"), "");

            // Act 
            var actual = await downloadResource.GetDownloadResourceResultAsync(
                package,
                NullSettings.Instance,
                NullLogger.Instance,
                CancellationToken.None);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(DownloadResourceResultStatus.NotFound, actual.Status);
        }

        [Fact]
        public async Task DownloadResourceFromInvalidUrl()
        {
            // Arrange
            var randomName = Guid.NewGuid().ToString();
            var repo = Repository.Factory.GetCoreV3($"https://www.{randomName}.org/api/v2");

            var downloadResource = await repo.GetResourceAsync<DownloadResource>();

            var package = new SourcePackageDependencyInfo("not-found", new NuGetVersion("6.2.0"), null, true, repo, new Uri($"https://www.{randomName}.org/api/v2/package/not-found/6.2.0"), "");

            // Act 
            Exception ex = await Assert.ThrowsAsync<FatalProtocolException>(async () => await downloadResource.GetDownloadResourceResultAsync(package,
                                                              NullSettings.Instance,
                                                              NullLogger.Instance,
                                                              CancellationToken.None));

            // Assert
            Assert.NotNull(ex);
            Assert.Equal($"Error downloading 'not-found.6.2.0' from 'https://www.{randomName}.org/api/v2/package/not-found/6.2.0'.", ex.Message);
        }

        [Fact]
        public async Task DownloadResourceFromIdentityInvalidSource()
        {
            // Arrange
            var randomName = Guid.NewGuid().ToString();
            var repo = Repository.Factory.GetCoreV3($"https://www.{randomName}.org/api/v2/");

            var downloadResource = await repo.GetResourceAsync<DownloadResource>();

            var package = new PackageIdentity("not-found", new NuGetVersion("6.2.0"));

            // Act & Assert
            // Act 
            Exception ex = await Assert.ThrowsAsync<FatalProtocolException>(async () => await downloadResource.GetDownloadResourceResultAsync(package,
                                                              NullSettings.Instance,
                                                              NullLogger.Instance,
                                                              CancellationToken.None));

            // Assert
            Assert.NotNull(ex);
            Assert.Equal($"Error downloading 'not-found.6.2.0' from 'https://www.{randomName}.org/api/v2/'.", ex.Message);
        }
    }
}

using FluentAssertions;
using Monorepo.Core;
using NuGet.Versioning;
using System;
using Xunit;

namespace Monorepo.Tests
{
    public class SemanticVersionExtensionsTests
    {
        [Theory]
        [InlineData("1.2.3", Release.Major, "", "2.0.0")]
        [InlineData("1.2.3", Release.Minor, "", "1.3.0")]
        [InlineData("1.2.3", Release.Patch, "", "1.2.4")]
        [InlineData("1.2.3-tag", Release.Major, "", "2.0.0")]
        [InlineData("1.2.0-0", Release.Patch, "", "1.2.0")]
        [InlineData("1.2.3-4", Release.Major, "", "2.0.0")]
        [InlineData("1.2.3-4", Release.Minor, "", "1.3.0")]
        [InlineData("1.2.3-4", Release.Patch, "", "1.2.3")]
        [InlineData("1.2.3-alpha.0.beta", Release.Major, "", "2.0.0")]
        [InlineData("1.2.3-alpha.0.beta", Release.Minor, "", "1.3.0")]
        [InlineData("1.2.3-alpha.0.beta", Release.Patch, "", "1.2.3")]
        [InlineData("1.2.4", Release.Prerelease, "", "1.2.5-0")]
        [InlineData("1.2.3-0", Release.Prerelease, "", "1.2.3-1")]
        [InlineData("1.2.3-alpha.0", Release.Prerelease, "", "1.2.3-alpha.1")]
        [InlineData("1.2.3-alpha.1", Release.Prerelease, "", "1.2.3-alpha.2")]
        [InlineData("1.2.3-alpha.2", Release.Prerelease, "", "1.2.3-alpha.3")]
        [InlineData("1.2.3-alpha.0.beta", Release.Prerelease, "", "1.2.3-alpha.1.beta")]
        [InlineData("1.2.3-alpha.1.beta", Release.Prerelease, "", "1.2.3-alpha.2.beta")]
        [InlineData("1.2.3-alpha.2.beta", Release.Prerelease, "", "1.2.3-alpha.3.beta")]
        [InlineData("1.2.3-alpha.10.0.beta", Release.Prerelease, "", "1.2.3-alpha.10.1.beta")]
        [InlineData("1.2.3-alpha.10.1.beta", Release.Prerelease, "", "1.2.3-alpha.10.2.beta")]
        [InlineData("1.2.3-alpha.10.2.beta", Release.Prerelease, "", "1.2.3-alpha.10.3.beta")]
        [InlineData("1.2.3-alpha.10.beta.0", Release.Prerelease, "", "1.2.3-alpha.10.beta.1")]
        [InlineData("1.2.3-alpha.10.beta.1", Release.Prerelease, "", "1.2.3-alpha.10.beta.2")]
        [InlineData("1.2.3-alpha.10.beta.2", Release.Prerelease, "", "1.2.3-alpha.10.beta.3")]
        [InlineData("1.2.3-alpha.9.beta", Release.Prerelease, "", "1.2.3-alpha.10.beta")]
        [InlineData("1.2.3-alpha.10.beta", Release.Prerelease, "", "1.2.3-alpha.11.beta")]
        [InlineData("1.2.3-alpha.11.beta", Release.Prerelease, "", "1.2.3-alpha.12.beta")]
        [InlineData("1.2.0", Release.Prepatch, "", "1.2.1-0")]
        [InlineData("1.2.0-1", Release.Prepatch, "", "1.2.1-0")]
        [InlineData("1.2.0", Release.Preminor, "", "1.3.0-0")]
        [InlineData("1.2.3-1", Release.Preminor, "", "1.3.0-0")]
        [InlineData("1.2.0", Release.Premajor, "", "2.0.0-0")]
        [InlineData("1.2.3-1", Release.Premajor, "", "2.0.0-0")]
        [InlineData("1.2.0-1", Release.Minor, "", "1.2.0")]
        [InlineData("1.0.0-1", Release.Major, "", "1.0.0")]
        [InlineData("1.2.3", Release.Major, "dev", "2.0.0")]
        [InlineData("1.2.3", Release.Minor, "dev", "1.3.0")]
        [InlineData("1.2.3", Release.Patch, "dev", "1.2.4")]
        [InlineData("1.2.3-tag", Release.Major, "dev", "2.0.0")]
        [InlineData("1.2.0-0", Release.Patch, "dev", "1.2.0")]
        [InlineData("1.2.3-4", Release.Major, "dev", "2.0.0")]
        [InlineData("1.2.3-4", Release.Minor, "dev", "1.3.0")]
        [InlineData("1.2.3-4", Release.Patch, "dev", "1.2.3")]
        [InlineData("1.2.3-alpha.0.beta", Release.Major, "dev", "2.0.0")]
        [InlineData("1.2.3-alpha.0.beta", Release.Minor, "dev", "1.3.0")]
        [InlineData("1.2.3-alpha.0.beta", Release.Patch, "dev", "1.2.3")]
        [InlineData("1.2.4", Release.Prerelease, "dev", "1.2.5-dev.0")]
        [InlineData("1.2.3-0", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.0", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.0", Release.Prerelease, "alpha", "1.2.3-alpha.1")]
        [InlineData("1.2.3-alpha.0.beta", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.0.beta", Release.Prerelease, "alpha", "1.2.3-alpha.1.beta")]
        [InlineData("1.2.3-alpha.10.0.beta", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.10.0.beta", Release.Prerelease, "alpha", "1.2.3-alpha.10.1.beta")]
        [InlineData("1.2.3-alpha.10.1.beta", Release.Prerelease, "alpha", "1.2.3-alpha.10.2.beta")]
        [InlineData("1.2.3-alpha.10.2.beta", Release.Prerelease, "alpha", "1.2.3-alpha.10.3.beta")]
        [InlineData("1.2.3-alpha.10.beta.0", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.10.beta.0", Release.Prerelease, "alpha", "1.2.3-alpha.10.beta.1")]
        [InlineData("1.2.3-alpha.10.beta.1", Release.Prerelease, "alpha", "1.2.3-alpha.10.beta.2")]
        [InlineData("1.2.3-alpha.10.beta.2", Release.Prerelease, "alpha", "1.2.3-alpha.10.beta.3")]
        [InlineData("1.2.3-alpha.9.beta", Release.Prerelease, "dev", "1.2.3-dev.0")]
        [InlineData("1.2.3-alpha.9.beta", Release.Prerelease, "alpha", "1.2.3-alpha.10.beta")]
        [InlineData("1.2.3-alpha.10.beta", Release.Prerelease, "alpha", "1.2.3-alpha.11.beta")]
        [InlineData("1.2.3-alpha.11.beta", Release.Prerelease, "alpha", "1.2.3-alpha.12.beta")]
        [InlineData("1.2.0", Release.Prepatch, "dev", "1.2.1-dev.0")]
        [InlineData("1.2.0-1", Release.Prepatch, "dev", "1.2.1-dev.0")]
        [InlineData("1.2.0", Release.Preminor, "dev", "1.3.0-dev.0")]
        [InlineData("1.2.3-1", Release.Preminor, "dev", "1.3.0-dev.0")]
        [InlineData("1.2.0", Release.Premajor, "dev", "2.0.0-dev.0")]
        [InlineData("1.2.3-1", Release.Premajor, "dev", "2.0.0-dev.0")]
        [InlineData("1.2.0-1", Release.Minor, "dev", "1.2.0")]
        [InlineData("1.0.0-1", Release.Major, "dev", "1.0.0")]
        [InlineData("1.2.3-dev.bar", Release.Prerelease, "dev", "1.2.3-dev.0")]
        public void Increment(string version, Release release, string prereleaseIdentifier, string expectedResult)
        {
            var semanticVersion = SemanticVersion.Parse(version);

            var result = semanticVersion.Increment(release, prereleaseIdentifier).ToString();

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Increment_ThrowsException_WhenReleaseIsInvalid(int release)
        {
            var semanticVersion = SemanticVersion.Parse("1.2.3");

            Action action = () => semanticVersion.Increment((Release)release);

            var ex = action.Should().Throw<ArgumentOutOfRangeException>().Which;
            ex.ParamName!.Should().Be("release");
            ex.ActualValue!.Should().Be(release);
        }
    }
}

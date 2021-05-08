using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Monorepo.IntegrationTests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            _testOutputHelper.WriteLine(Directory.GetCurrentDirectory());
            var repoPath = "repos/repo1";
            //Directory.CreateDirectory(path);
            Directory.Delete(repoPath, recursive: true);
            var absoluteRepoPath = Repository.Init(repoPath);

            var packagesPath = $"{absoluteRepoPath}/packages";
            CreateProjectFile(packagesPath, packageId: "Core", version: "0.0.1", referencedPackageIds: Array.Empty<string>());
            CreateProjectFile(packagesPath, packageId: "Lib1", version: "0.0.1", referencedPackageIds: new [] { "Core" });
            CreateProjectFile(packagesPath, packageId: "Lib4", version: "0.0.1", referencedPackageIds: new [] { "Core", "Lib1" });


            using var repo = new Repository(absoluteRepoPath);
        }

        private static void CreateProjectFile(string packagesPath, string packageId, string version, IEnumerable<string> referencedPackageIds)
        {
            var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <PackageId>{packageId}</PackageId>
    <Version>{version}</Version>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
{string.Join("\n", referencedPackageIds.Select(id => $@"     <ProjectReference Include=""..\{id}\{id}.csproj"" />"))}
  </ItemGroup>
</Project>
";
        }
    }
}

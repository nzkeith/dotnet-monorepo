using LibGit2Sharp;
using Monorepo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Monorepo.IntegrationTests
{
    public class VersionTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public VersionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Version()
        {
            _testOutputHelper.WriteLine(Directory.GetCurrentDirectory());

            var repoPath = "repos/repo1";

            DeleteDirectory(repoPath);

            var dotGitPath = Repository.Init(repoPath);
            using var git = new Git(dotGitPath);

            var absoluteRepoPath = git.SystemPath(".");
            var packagesPath = Path.GetFullPath("packages", absoluteRepoPath);

            ProjectSpec[] projectSpecs =
            {
                new("Core", "0.0.1", Array.Empty<string>()),
                new("Lib1", "0.0.1", new [] { "Core" }),
                new("Lib4", "0.0.1", new [] { "Core", "Lib1" }),
            };

            using var repo = new Repository(dotGitPath);

            foreach (var projectSpec in projectSpecs)
            {
                var csprojFilePath = CreateProjectFile(packagesPath, projectSpec);
                var csprojRelativePath = git.RelativePath(csprojFilePath);
                repo.Index.Add(csprojRelativePath);
            }

            repo.Index.Write();

            repo.Commit("message",
                new Signature("author", "author@example.com", DateTimeOffset.Now),
                new Signature("committer", "commiter@example.com", DateTimeOffset.Now));
        }

        private static void DeleteDirectory(string path)
        {
            if (File.Exists(path))
            {
                throw new Exception($"{nameof(path)} is a file: {path}");
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        private record ProjectSpec(string PackageId, string Version, IEnumerable<string> ReferencedPackageIds);

        /// <summary>Returns the .csproj file path</summary>
        private static string CreateProjectFile(string packagesPath, ProjectSpec projectSpec)
        {
            var projectPath = Path.GetFullPath(projectSpec.PackageId, packagesPath);
            var csprojFilePath = Path.GetFullPath($"{projectSpec.PackageId}.csproj", projectPath);

            Directory.CreateDirectory(projectPath);

            File.WriteAllText(csprojFilePath, $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <PackageId>{projectSpec.PackageId}</PackageId>
    <Version>{projectSpec.Version}</Version>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
{string.Join("\n", projectSpec.ReferencedPackageIds.Select(id => $@"     <ProjectReference Include=""..\{id}\{id}.csproj"" />"))}
  </ItemGroup>
</Project>
");

            return csprojFilePath;
        }
    }
}

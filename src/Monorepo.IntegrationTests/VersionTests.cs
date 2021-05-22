using CliWrap;
using LibGit2Sharp;
using Monorepo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task Version()
        {
            _testOutputHelper.WriteLine(Directory.GetCurrentDirectory());

            var repoPath = "repos/working/repo1";
            var originRepoPath = "repos/origin/repo1.git";

            DeleteDirectory(repoPath);
            DeleteDirectory(originRepoPath);

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

            void CreateAndStageProject(ProjectSpec projectSpec)
            {
                var csprojFilePath = CreateProjectFile(packagesPath, projectSpec);
                var csprojRelativePath = git.RelativePath(csprojFilePath);
                repo.Index.Add(csprojRelativePath);
            }

            void CreateAndStageFile(string relativePath, string contents)
            {
                File.WriteAllText(git.SystemPath(relativePath), contents);
                repo.Index.Add(relativePath);
            }

            foreach (var projectSpec in projectSpecs)
            {
                CreateAndStageProject(projectSpec);
            }
            repo.Index.Write();

            var committer = new Signature("committer", "committer@example.com", DateTimeOffset.Now);
            repo.Commit("commit message 1", author: committer, committer: committer);
            repo.ApplyTag("0.0.1", tagger: committer, "tag message");

            CreateAndStageFile("packages/Lib1/Class1.cs", contents: "");
            repo.Index.Write();
            repo.Commit("commit message 2", author: committer, committer: committer);

            var originAbsolutePath = git.Clone(originRepoPath, new CloneOptions
            {
                IsBare = true
            });
            git.SetRemoteUrl("origin", originAbsolutePath);
            git.SetUpstreamBranch("master", "origin", "refs/heads/master");

            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();
            var result = await Cli.Wrap("MonoRepo.Tool.exe")
                .WithArguments("version patch")
                .WithWorkingDirectory(repoPath)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            var stdout = stdOutBuffer.ToString();
            var stderr = stdErrBuffer.ToString();

            if (!string.IsNullOrEmpty(stdout))
            {
                _testOutputHelper.WriteLine("stdout: " + stdout);
            }

            if (!string.IsNullOrEmpty(stderr))
            {
                _testOutputHelper.WriteLine("stderr: " + stderr);
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

        private static void DeleteDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 10)
            {
                throw new Exception($"Refusing to delete dangerously short directory '{path}'");
            }

            if (File.Exists(path))
            {
                throw new Exception($"{nameof(path)} is a file: {path}");
            }

            if (!Directory.Exists(path))
            {
                return;
            }

            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(recursive: true);
        }
    }
}

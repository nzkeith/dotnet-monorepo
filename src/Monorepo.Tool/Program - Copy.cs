using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing;
using Monorepo.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Monorepo.Tool
{
    public class Program1
    {
        //public static void Main()
        //{
        //    try
        //    {
        //        IncrementTests();

        //        //LernaVersion();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        private static void IncrementTests()
        {
            var increments = new[]
            {
                ("1.2.3", Release.Major, "2.0.0", ""),
                ("1.2.3", Release.Minor, "1.3.0", ""),
                ("1.2.3", Release.Patch, "1.2.4", ""),
                ("1.2.3-tag", Release.Major, "2.0.0", ""),
                ("1.2.0-0", Release.Patch, "1.2.0", ""),
                // ("fake", Release.Major, null, ""),
                ("1.2.3-4", Release.Major, "2.0.0", ""),
                ("1.2.3-4", Release.Minor, "1.3.0", ""),
                ("1.2.3-4", Release.Patch, "1.2.3", ""),
                ("1.2.3-alpha.0.beta", Release.Major, "2.0.0", ""),
                ("1.2.3-alpha.0.beta", Release.Minor, "1.3.0", ""),
                ("1.2.3-alpha.0.beta", Release.Patch, "1.2.3", ""),
                ("1.2.4", Release.Prerelease, "1.2.5-0", ""),
                ("1.2.3-0", Release.Prerelease, "1.2.3-1", ""),
                ("1.2.3-alpha.0", Release.Prerelease, "1.2.3-alpha.1", ""),
                ("1.2.3-alpha.1", Release.Prerelease, "1.2.3-alpha.2", ""),
                ("1.2.3-alpha.2", Release.Prerelease, "1.2.3-alpha.3", ""),
                ("1.2.3-alpha.0.beta", Release.Prerelease, "1.2.3-alpha.1.beta", ""),
                ("1.2.3-alpha.1.beta", Release.Prerelease, "1.2.3-alpha.2.beta", ""),
                ("1.2.3-alpha.2.beta", Release.Prerelease, "1.2.3-alpha.3.beta", ""),
                ("1.2.3-alpha.10.0.beta", Release.Prerelease, "1.2.3-alpha.10.1.beta", ""),
                ("1.2.3-alpha.10.1.beta", Release.Prerelease, "1.2.3-alpha.10.2.beta", ""),
                ("1.2.3-alpha.10.2.beta", Release.Prerelease, "1.2.3-alpha.10.3.beta", ""),
                ("1.2.3-alpha.10.beta.0", Release.Prerelease, "1.2.3-alpha.10.beta.1", ""),
                ("1.2.3-alpha.10.beta.1", Release.Prerelease, "1.2.3-alpha.10.beta.2", ""),
                ("1.2.3-alpha.10.beta.2", Release.Prerelease, "1.2.3-alpha.10.beta.3", ""),
                ("1.2.3-alpha.9.beta", Release.Prerelease, "1.2.3-alpha.10.beta", ""),
                ("1.2.3-alpha.10.beta", Release.Prerelease, "1.2.3-alpha.11.beta", ""),
                ("1.2.3-alpha.11.beta", Release.Prerelease, "1.2.3-alpha.12.beta", ""),
                ("1.2.0", Release.Prepatch, "1.2.1-0", ""),
                ("1.2.0-1", Release.Prepatch, "1.2.1-0", ""),
                ("1.2.0", Release.Preminor, "1.3.0-0", ""),
                ("1.2.3-1", Release.Preminor, "1.3.0-0", ""),
                ("1.2.0", Release.Premajor, "2.0.0-0", ""),
                ("1.2.3-1", Release.Premajor, "2.0.0-0", ""),
                ("1.2.0-1", Release.Minor, "1.2.0", ""),
                ("1.0.0-1", Release.Major, "1.0.0", ""),

                ("1.2.3", Release.Major, "2.0.0", "dev"),
                ("1.2.3", Release.Minor, "1.3.0", "dev"),
                ("1.2.3", Release.Patch, "1.2.4", "dev"),
                // ("1.2.3tag", Release.Major, "2.0.0", "dev"),
                ("1.2.3-tag", Release.Major, "2.0.0", "dev"),
                // ("1.2.3", "fake", null, "dev"),
                ("1.2.0-0", Release.Patch, "1.2.0", "dev"),
                // ("fake", Release.Major, null, "dev"),
                ("1.2.3-4", Release.Major, "2.0.0", "dev"),
                ("1.2.3-4", Release.Minor, "1.3.0", "dev"),
                ("1.2.3-4", Release.Patch, "1.2.3", "dev"),
                ("1.2.3-alpha.0.beta", Release.Major, "2.0.0", "dev"),
                ("1.2.3-alpha.0.beta", Release.Minor, "1.3.0", "dev"),
                ("1.2.3-alpha.0.beta", Release.Patch, "1.2.3", "dev"),
                ("1.2.4", Release.Prerelease, "1.2.5-dev.0", "dev"),
                ("1.2.3-0", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.0", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.0", Release.Prerelease, "1.2.3-alpha.1", "alpha"),
                ("1.2.3-alpha.0.beta", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.0.beta", Release.Prerelease, "1.2.3-alpha.1.beta", "alpha"),
                ("1.2.3-alpha.10.0.beta", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.10.0.beta", Release.Prerelease, "1.2.3-alpha.10.1.beta", "alpha"),
                ("1.2.3-alpha.10.1.beta", Release.Prerelease, "1.2.3-alpha.10.2.beta", "alpha"),
                ("1.2.3-alpha.10.2.beta", Release.Prerelease, "1.2.3-alpha.10.3.beta", "alpha"),
                ("1.2.3-alpha.10.beta.0", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.10.beta.0", Release.Prerelease, "1.2.3-alpha.10.beta.1", "alpha"),
                ("1.2.3-alpha.10.beta.1", Release.Prerelease, "1.2.3-alpha.10.beta.2", "alpha"),
                ("1.2.3-alpha.10.beta.2", Release.Prerelease, "1.2.3-alpha.10.beta.3", "alpha"),
                ("1.2.3-alpha.9.beta", Release.Prerelease, "1.2.3-dev.0", "dev"),
                ("1.2.3-alpha.9.beta", Release.Prerelease, "1.2.3-alpha.10.beta", "alpha"),
                ("1.2.3-alpha.10.beta", Release.Prerelease, "1.2.3-alpha.11.beta", "alpha"),
                ("1.2.3-alpha.11.beta", Release.Prerelease, "1.2.3-alpha.12.beta", "alpha"),
                ("1.2.0", Release.Prepatch, "1.2.1-dev.0", "dev"),
                ("1.2.0-1", Release.Prepatch, "1.2.1-dev.0", "dev"),
                ("1.2.0", Release.Preminor, "1.3.0-dev.0", "dev"),
                ("1.2.3-1", Release.Preminor, "1.3.0-dev.0", "dev"),
                ("1.2.0", Release.Premajor, "2.0.0-dev.0", "dev"),
                ("1.2.3-1", Release.Premajor, "2.0.0-dev.0", "dev"),
                ("1.2.0-1", Release.Minor, "1.2.0", "dev"),
                ("1.0.0-1", Release.Major, "1.0.0", "dev"),
                ("1.2.3-dev.bar", Release.Prerelease, "1.2.3-dev.0", "dev"),
            };


            var allPassed = true;
            foreach (var (version, release, expectedResult, prereleaseIdentifier) in increments)
            {
                var semanticVersion = SemanticVersion.Parse(version);
                var result = semanticVersion.Increment(release, prereleaseIdentifier).ToString();
                var pass = result == expectedResult;
                if (!pass)
                {
                    allPassed = false;
                }
                Console.WriteLine($"{version} {release} -> {result} {(pass ? "PASS" : $"FAIL (expected {expectedResult})")}");
            }
            Console.WriteLine($"Summary: {(allPassed ? "PASS" : "FAIL")}");
        }

        private static void LernaVersion()
        {
            Directory.SetCurrentDirectory("../../../..");
            Console.WriteLine($"Working directory: {Environment.CurrentDirectory}");
            Console.WriteLine();

            var gitDescribeResult = GitDescribe(".");
            Console.WriteLine($"git describe: {gitDescribeResult}");
            Console.WriteLine();

            var projects = LoadProjects("libs");

            foreach (var project in projects)
            {
                Console.WriteLine(project.ProjFilePath);
                Console.WriteLine($"PackageId: {project.PackageId}");
                Console.WriteLine($"Version: {project.Version}");
                Console.WriteLine("ProjectReferences: ");
                Console.WriteLine(string.Join("\n", project.ProjectReferences));
            }

            var dependentProjects = GetChangedAndDependentProjects(projects, gitDescribeResult.LastTagName);

            Console.WriteLine();
            Console.WriteLine("Changed projects and their dependents:");
            foreach (var project in dependentProjects)
            {
                Console.WriteLine($"{project.PackageId} {project.Version}");
            }

            var release = Release.Major;

            Console.WriteLine();
            Console.WriteLine("New project versions for release:");
            foreach (var project in dependentProjects)
            {
                var oldVersion = SemanticVersion.Parse(project.Version);
                var newVersion = oldVersion.Increment(release);
                Console.WriteLine($"{project.PackageId} {project.Version} -> {newVersion}");
            }
        }

        private static IList<Project> LoadProjects(string rootDirectory)
        {
            var matcher = new Matcher();
            matcher.AddInclude("**/*.csproj");
            var csprojPaths = matcher.GetResultsInFullPath(rootDirectory);

            var csprojByPath = csprojPaths.ToDictionary(
                path => path,
                path => XElement.Parse(File.ReadAllText(path)));

            var projects = new List<Project>();
            foreach (var (projFilePath, csproj) in csprojByPath)
            {
                var projFileGitPath = GitPath(projFilePath);

                var basePath = Path.GetDirectoryName(projFilePath);
                var baseGitPath = GitPath(basePath);

                var packageId = csproj.XPathSelectElement("./PropertyGroup/PackageId")?.Value;
                var version = csproj.XPathSelectElement("./PropertyGroup/Version")?.Value;
                var projectReferences = csproj
                    .XPathSelectElements("./ItemGroup/ProjectReference")
                    .Select(el => el.Attribute("Include")?.Value)
                    .Where(value => value != null)
                    .Select(relativePath => Path.GetFullPath(relativePath, basePath!))
                    .ToList();

                projects.Add(new Project(
                    ProjFilePath: projFilePath,
                    ProjFileGitPath: projFileGitPath,
                    BasePath: basePath,
                    BaseGitPath: baseGitPath,
                    PackageId: packageId,
                    Version: version,
                    ProjectReferences: projectReferences
                ));
            }

            return projects;
        }

        private static IList<Project> DetectProjectsChangedSince(IList<Project> projects, string lastTagName)
        {
            return projects.Where(p => ProjectHasChanged(p, lastTagName)).ToList();
        }

        private static bool ProjectHasChanged(Project project, string lastTagName)
        {
            var treeChanges = GitDiff(lastTagName, project.BaseGitPath);
            return treeChanges.Any();
        }

        private static IList<Project> GetChangedAndDependentProjects(IList<Project> projects, string lastTagName)
        {
            var projectGraphBuilder = new ProjectGraphBuilder(projects);
            var projectGraph = projectGraphBuilder.Build();

            var changedProjects = DetectProjectsChangedSince(projects, lastTagName);
            var dependentProjects = projectGraph.CollectDependentProjects(changedProjects);
            return dependentProjects;
        }

        private static GitDescribeResult GitDescribe(string path)
        {
            using var repo = new Repository(Repository.Discover(path));

            var commit = repo.Head.Tip;
            var gitDescribeOutput = repo.Describe(commit, new DescribeOptions
            {
                AlwaysRenderLongFormat = true,
            });

            var match = Regex.Match(gitDescribeOutput, @"^((?:.*@)?(.*))-(\d+)-g([0-9a-f]+)$");
            if (!match.Success)
            {
                throw new InvalidOperationException($"Failed to parse git describe output '{gitDescribeOutput}'");
            }

            return new GitDescribeResult(
                LastTagName: match.Groups[2].Value,
                RefCount: int.Parse(match.Groups[3].Value),
                Sha: match.Groups[4].Value);
        }

        public record GitDescribeResult(string LastTagName, int RefCount, string Sha);

        private static TreeChanges GitDiff(string tagName, string gitPath)
        {
            using var repo = new Repository(Repository.Discover(".")); // TODO
            var headTree = repo.Head.Tip.Tree;
            var tag = repo.Tags[tagName];
            var tagCommit = repo.Lookup<Commit>(tag.Target.Sha);
            var tagTree = tagCommit.Tree;

            var treeChanges = repo.Diff.Compare<TreeChanges>(tagTree, headTree, new[] { gitPath });
            return treeChanges;
        }

        private static string GitPath(string systemPath)
        {
            using var repo = new Repository(Repository.Discover(".")); // TODO
            var relativePath = Path.GetRelativePath(repo.Info.WorkingDirectory, systemPath);
            if (Path.DirectorySeparatorChar == '\\')
            {
                relativePath = relativePath.Replace('\\', '/');
            }
            return relativePath;
        }
    }
}

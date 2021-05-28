//using LibGit2Sharp;
//using Microsoft.Extensions.FileSystemGlobbing;
//using Monorepo.Core;
//using NuGet.Versioning;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Xml.Linq;
//using System.Xml.XPath;

//namespace Monorepo.Tool
//{
//    public class Program1
//    {
//        private static void LernaVersion()
//        {
//            Directory.SetCurrentDirectory("../../../..");
//            Console.WriteLine($"Working directory: {Environment.CurrentDirectory}");
//            Console.WriteLine();

//            var gitDescribeResult = GitDescribe(".");
//            Console.WriteLine($"git describe: {gitDescribeResult}");
//            Console.WriteLine();

//            var projects = LoadProjects("libs");

//            foreach (var project in projects)
//            {
//                Console.WriteLine(project.ProjFilePath);
//                Console.WriteLine($"PackageId: {project.PackageId}");
//                Console.WriteLine($"Version: {project.Version}");
//                Console.WriteLine("ProjectReferences: ");
//                Console.WriteLine(string.Join("\n", project.ProjectReferences));
//            }

//            var dependentProjects = GetChangedAndDependentProjects(projects, gitDescribeResult.LastTagName);

//            Console.WriteLine();
//            Console.WriteLine("Changed projects and their dependents:");
//            foreach (var project in dependentProjects)
//            {
//                Console.WriteLine($"{project.PackageId} {project.Version}");
//            }

//            var release = Release.Major;

//            Console.WriteLine();
//            Console.WriteLine("New project versions for release:");
//            foreach (var project in dependentProjects)
//            {
//                var oldVersion = SemanticVersion.Parse(project.Version);
//                var newVersion = oldVersion.Increment(release);
//                Console.WriteLine($"{project.PackageId} {project.Version} -> {newVersion}");
//            }
//        }

//        private static IList<Project> LoadProjects(string rootDirectory)
//        {
//            var matcher = new Matcher();
//            matcher.AddInclude("**/*.csproj");
//            var csprojPaths = matcher.GetResultsInFullPath(rootDirectory);

//            var csprojByPath = csprojPaths.ToDictionary(
//                path => path,
//                path => XElement.Parse(File.ReadAllText(path)));

//            var projects = new List<Project>();
//            foreach (var (projFilePath, csproj) in csprojByPath)
//            {
//                var projFileGitPath = GitPath(projFilePath);

//                var basePath = Path.GetDirectoryName(projFilePath);
//                var baseGitPath = GitPath(basePath);

//                var packageId = csproj.XPathSelectElement("./PropertyGroup/PackageId")?.Value;
//                var version = csproj.XPathSelectElement("./PropertyGroup/Version")?.Value;
//                var projectReferences = csproj
//                    .XPathSelectElements("./ItemGroup/ProjectReference")
//                    .Select(el => el.Attribute("Include")?.Value)
//                    .Where(value => value != null)
//                    .Select(relativePath => Path.GetFullPath(relativePath, basePath!))
//                    .ToList();

//                projects.Add(new Project(
//                    ProjFilePath: projFilePath,
//                    ProjFileGitPath: projFileGitPath,
//                    BasePath: basePath,
//                    BaseGitPath: baseGitPath,
//                    PackageId: packageId,
//                    Version: version,
//                    ProjectReferences: projectReferences
//                ));
//            }

//            return projects;
//        }

//        private static IList<Project> DetectProjectsChangedSince(IList<Project> projects, string lastTagName)
//        {
//            return projects.Where(p => ProjectHasChanged(p, lastTagName)).ToList();
//        }

//        private static bool ProjectHasChanged(Project project, string lastTagName)
//        {
//            var treeChanges = GitDiff(lastTagName, project.BaseGitPath);
//            return treeChanges.Any();
//        }

//        private static IList<Project> GetChangedAndDependentProjects(IList<Project> projects, string lastTagName)
//        {
//            var projectGraphBuilder = new ProjectGraphBuilder(projects);
//            var projectGraph = projectGraphBuilder.Build();

//            var changedProjects = DetectProjectsChangedSince(projects, lastTagName);
//            var dependentProjects = projectGraph.CollectDependentProjects(changedProjects);
//            return dependentProjects;
//        }

//        private static GitDescribeResult GitDescribe(string path)
//        {
//            using var repo = new Repository(Repository.Discover(path));

//            var commit = repo.Head.Tip;
//            var gitDescribeOutput = repo.Describe(commit, new DescribeOptions
//            {
//                AlwaysRenderLongFormat = true,
//            });

//            var match = Regex.Match(gitDescribeOutput, @"^((?:.*@)?(.*))-(\d+)-g([0-9a-f]+)$");
//            if (!match.Success)
//            {
//                throw new InvalidOperationException($"Failed to parse git describe output '{gitDescribeOutput}'");
//            }

//            return new GitDescribeResult(
//                LastTagName: match.Groups[2].Value,
//                RefCount: int.Parse(match.Groups[3].Value),
//                Sha: match.Groups[4].Value);
//        }

//        public record GitDescribeResult(string LastTagName, int RefCount, string Sha);

//        private static TreeChanges GitDiff(string tagName, string gitPath)
//        {
//            using var repo = new Repository(Repository.Discover(".")); // TODO
//            var headTree = repo.Head.Tip.Tree;
//            var tag = repo.Tags[tagName];
//            var tagCommit = repo.Lookup<Commit>(tag.Target.Sha);
//            var tagTree = tagCommit.Tree;

//            var treeChanges = repo.Diff.Compare<TreeChanges>(tagTree, headTree, new[] { gitPath });
//            return treeChanges;
//        }

//        private static string GitPath(string systemPath)
//        {
//            using var repo = new Repository(Repository.Discover(".")); // TODO
//            var relativePath = Path.GetRelativePath(repo.Info.WorkingDirectory, systemPath);
//            if (Path.DirectorySeparatorChar == '\\')
//            {
//                relativePath = relativePath.Replace('\\', '/');
//            }
//            return relativePath;
//        }
//    }
//}

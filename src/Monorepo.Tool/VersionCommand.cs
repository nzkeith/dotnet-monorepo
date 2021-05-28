using McMaster.Extensions.CommandLineUtils;
using Monorepo.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monorepo.Tool
{
    internal class VersionCommand
    {
        internal static void Configure(CommandLineApplication command)
        {
            command.Description = "Bump version of packages changed since the last release";

            var release = command
                .Argument("release", "Controls the semantic version(s) generated for this release")
                .IsRequired()
                .Accepts(v => v.Enum<Release>(ignoreCase: true));

            command.OnExecute(() =>
            {
                Execute(Enum.Parse<Release>(release.Value!, ignoreCase: true));
            });
        }

        private static void Execute(Release release)
        {
            using var git = new Git(".");
            var projectLoader = new ProjectLoader(git);
            Console.WriteLine(Directory.GetCurrentDirectory());
            var projects = projectLoader.LoadProjects("../../../..");

            Console.WriteLine($"Executing release type '{release}'");
            Console.WriteLine();

            foreach (var project in projects)
            {
                Console.WriteLine(project.ProjFilePath);
                Console.WriteLine($"PackageId: {project.PackageId}");
                Console.WriteLine($"Version: {project.Version}");
                Console.WriteLine("ProjectReferences: ");
                Console.WriteLine(string.Join("\n", project.ProjectReferences));
            }

            var dependentProjects = GetChangedAndDependentProjects(git, projects, git.Describe().LastTagName);

            Console.WriteLine();
            Console.WriteLine("Changed projects and their dependents:");
            foreach (var project in dependentProjects)
            {
                Console.WriteLine($"{project.PackageId} {project.Version}");
            }

            // TODO: Exit early if nothing to do

            Console.WriteLine();
            Console.WriteLine("New project versions for release:");
            var tagNames = new List<string>();
            foreach (var project in dependentProjects)
            {
                var oldVersion = SemanticVersion.Parse(project.Version);
                var newVersion = oldVersion.Increment(release);
                Console.WriteLine($"{project.PackageId} {project.Version} -> {newVersion}");

                // Update project versions
                var projectEditor = new ProjectEditor(project.ProjFilePath);
                projectEditor.SetVersion(newVersion.ToString());
                git.StageFile(project.ProjFileGitPath);
                tagNames.Add($"{project.PackageId}@{newVersion}");
            }

            git.Commit("Updated versions of changed projects");

            foreach (var tagName in tagNames)
            {
                git.Tag(tagName, message: tagName);
            }

            // Push commit and tags
        }

        private static IList<Project> GetChangedAndDependentProjects(Git git, IList<Project> projects, string lastTagName)
        {
            var projectGraphBuilder = new ProjectGraphBuilder(projects);
            var projectGraph = projectGraphBuilder.Build();

            var changedProjects = DetectProjectsChangedSince(git, projects, lastTagName);
            var dependentProjects = projectGraph.CollectDependentProjects(changedProjects);
            return dependentProjects;
        }

        private static IList<Project> DetectProjectsChangedSince(Git git, IEnumerable<Project> projects, string lastTagName)
        {
            return projects.Where(p => ProjectHasChanged(git, p, lastTagName)).ToList();
        }

        private static bool ProjectHasChanged(Git git, Project project, string lastTagName)
        {
            var treeChanges = git.Diff(lastTagName, project.BaseGitPath);
            return treeChanges.Any();
        }
    }
}

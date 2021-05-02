using McMaster.Extensions.CommandLineUtils;
using Monorepo.Core;
using System;
using System.IO;

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
            var git = new Git(".");
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
        }
    }
}

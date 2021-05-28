using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Monorepo.Core
{
    public class ProjectLoader
    {
        private readonly Git _git;

        public ProjectLoader(Git git)
        {
            _git = git;
        }

        public IList<Project> LoadProjects(string rootDirectory)
        {
            var matcher = new Matcher();
            matcher.AddInclude("**/*.csproj");
            var csprojPaths = matcher.GetResultsInFullPath(rootDirectory).Select(path => new SystemPath(path));

            var csprojByPath = csprojPaths.ToDictionary(
                path => path,
                path => XElement.Parse(File.ReadAllText(path)));

            var projects = new List<Project>();
            foreach (var (projFilePath, csproj) in csprojByPath)
            {
                var projFileGitPath = _git.GitPath(projFilePath);

                var basePath = Path.GetDirectoryName(projFilePath);
                if (basePath == null)
                {
                    throw new MonorepoException($"Directory name for {nameof(projFilePath)} was unexpectedly null: '{projFilePath}'");
                }

                var baseGitPath = _git.GitPath(basePath);

                var packageId = csproj.XPathSelectElement("./PropertyGroup/PackageId")?.Value;
                var version = csproj.XPathSelectElement("./PropertyGroup/Version")?.Value;
                var projectReferences = csproj
                    .XPathSelectElements("./ItemGroup/ProjectReference")
                    .Select(el => el.Attribute("Include")?.Value)
                    .Where(value => value != null)
                    .Select(relativePath => new SystemPath(Path.GetFullPath(relativePath!, basePath)))
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
    }
}

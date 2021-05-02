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
            var csprojPaths = matcher.GetResultsInFullPath(rootDirectory);

            var csprojByPath = csprojPaths.ToDictionary(
                path => path,
                path => XElement.Parse(File.ReadAllText(path)));

            var projects = new List<Project>();
            foreach (var (projFilePath, csproj) in csprojByPath)
            {
                var projFileGitPath = _git.RelativePath(projFilePath);

                var basePath = Path.GetDirectoryName(projFilePath);
                var baseGitPath = _git.RelativePath(basePath);

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
    }
}

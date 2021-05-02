using System.Collections.Generic;
using System.Linq;

namespace Monorepo.Core
{
    public class ProjectGraphBuilder
    {
        private readonly IList<Project> _projects;
        private readonly ProjectNodesByPath _projectNodesByPath;

        public ProjectGraphBuilder(IList<Project> projects)
        {
            _projects = projects;
            _projectNodesByPath = new ProjectNodesByPath();
        }

        public ProjectGraph Build()
        {
            foreach (var targetProject in _projects)
            {
                var targetProjectNode = ProjectNodeFor(targetProject);

                foreach (var sourceProject in _projects.Where(project => project != targetProject))
                {
                    if (sourceProject.ProjectReferences.Contains(targetProject.ProjFilePath))
                    {
                        var sourceProjectNode = ProjectNodeFor(sourceProject);
                        targetProjectNode.Dependents.Add(sourceProjectNode);
                    }
                }
            }

            return new ProjectGraph
            {
                ProjectNodesByPath = _projectNodesByPath
            };
        }

        private ProjectNode ProjectNodeFor(Project project)
        {
            if (_projectNodesByPath.TryGetValue(project.ProjFilePath, out var projectNode))
            {
                return projectNode;
            }

            projectNode = new ProjectNode
            {
                Project = project,
                Dependents = new List<ProjectNode>()
            };

            _projectNodesByPath[project.ProjFilePath] = projectNode;

            return projectNode;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Monorepo.Core
{
    public class ProjectGraph
    {
        public ProjectNodesByPath ProjectNodesByPath { get; init; }

        public IList<Project> CollectDependentProjects(IEnumerable<Project> projects)
        {
            return CollectDependentProjectNodes(projects.Select(project => ProjectNodesByPath[project.ProjFilePath]))
                .Select(node => node.Project)
                .ToList();
        }

        private static IEnumerable<ProjectNode> CollectDependentProjectNodes(IEnumerable<ProjectNode> projectNodes)
        {
            var nodes = projectNodes.ToList();
            return nodes
                .Concat(nodes.SelectMany(projectNode => CollectDependentProjectNodes(projectNode.Dependents)))
                .Distinct();
        }
    }
}

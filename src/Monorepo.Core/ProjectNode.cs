using System.Collections.Generic;
using System.Linq;

namespace Monorepo.Core
{
    public class ProjectNode
    {
        public Project Project { get; init; }

        public IList<ProjectNode> Dependents { get; init; }

        public override string ToString() => $"{Project}: [{string.Join(", ", Dependents.Select(d => d.Project))}]";
    }
}

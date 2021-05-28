using System.Collections.Generic;

namespace Monorepo.Core
{
    public record Project(
        SystemPath ProjFilePath,
        GitPath ProjFileGitPath,
        SystemPath BasePath,
        GitPath BaseGitPath,
        string PackageId,
        string Version,
        IList<SystemPath> ProjectReferences);
}

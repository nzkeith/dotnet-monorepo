using System.Collections.Generic;

namespace Monorepo.Core
{
    public record Project(
        string ProjFilePath,
        string ProjFileGitPath,
        string BasePath,
        string BaseGitPath,
        string PackageId,
        string Version,
        IList<string> ProjectReferences);
}

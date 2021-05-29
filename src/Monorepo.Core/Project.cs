using System.Collections.Generic;

namespace Monorepo.Core
{
    public record Project(
        StringLike<SystemPath> ProjFilePath,
        StringLike<GitPath> ProjFileGitPath,
        StringLike<SystemPath> BasePath,
        StringLike<GitPath> BaseGitPath,
        string PackageId,
        string Version,
        IList<StringLike<SystemPath>> ProjectReferences);
}

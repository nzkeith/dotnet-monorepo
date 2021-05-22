using LibGit2Sharp;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Monorepo.Core
{
    public class Git : IDisposable
    {
        private readonly Repository _repo;

        public Git(string repositoryPath)
        {
            _repo = new Repository(Repository.Discover(repositoryPath));
        }

        public record DescribeResult(string LastTagName, int RefCount, string Sha);

        public DescribeResult Describe()
        {
            var commit = _repo.Head.Tip;
            var gitOutput = _repo.Describe(commit, new DescribeOptions
            {
                AlwaysRenderLongFormat = true,
            });

            var match = Regex.Match(gitOutput, @"^((?:.*@)?(.*))-(\d+)-g([0-9a-f]+)$");
            if (!match.Success)
            {
                throw new MonorepoException($"Failed to parse git describe output '{gitOutput}'");
            }

            return new DescribeResult(
                LastTagName: match.Groups[2].Value,
                RefCount: int.Parse(match.Groups[3].Value),
                Sha: match.Groups[4].Value);
        }

        public TreeChanges Diff(string tagName, string gitPath)
        {
            var headTree = _repo.Head.Tip.Tree;
            var tag = _repo.Tags[tagName];
            var tagCommit = _repo.Lookup<Commit>(tag.Target.Sha);
            var tagTree = tagCommit.Tree;

            return _repo.Diff.Compare<TreeChanges>(tagTree, headTree, new[] { gitPath });
        }

        public string Clone(string path, CloneOptions? options = null)
        {
            return Repository.Clone(_repo.Info.Path, path, options);
        }

        public void SetRemoteUrl(string remoteName, string url)
        {
            _repo.Network.Remotes.Update(remoteName, updater => updater.Url = url);
        }

        public void SetUpstreamBranch(string branchName, string remoteName, string upstreamBranchName)
        {
            var branch = _repo.Branches[branchName];
            _repo.Branches.Update(branch,
                b => b.Remote = remoteName,
                b => b.UpstreamBranch = upstreamBranchName);
        }

        public string RelativePath(string systemPath)
        {
            var relativePath = Path.GetRelativePath(_repo.Info.WorkingDirectory, systemPath);
            if (Path.DirectorySeparatorChar == '\\')
            {
                relativePath = relativePath.Replace('\\', '/');
            }
            return relativePath;
        }

        public string SystemPath(string relativePath)
        {
            return Path.GetFullPath(relativePath, _repo.Info.WorkingDirectory);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo?.Dispose();
            }
        }
        #endregion
    }
}

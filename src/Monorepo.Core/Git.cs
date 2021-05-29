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

        public string Clone(string path, CloneOptions? options = null)
        {
            return Repository.Clone(_repo.Info.Path, path, options);
        }

        public void Commit(string message)
        {
            var committer = new Signature("committer", "committer@example.com", DateTimeOffset.Now); // TODO: Infer actual committer
            _repo.Commit(message, author: committer, committer: committer);
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

        public void Tag(string tagName, string message)
        {
            var tagger = new Signature("tagger", "tagger@example.com", DateTimeOffset.Now); // TODO: Infer actual tagger
            _repo.ApplyTag(tagName, tagger, message);
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

        public void StageFile(StringLike<GitPath> gitPath)
        {
            _repo.Index.Add(gitPath);
            _repo.Index.Write();
        }

        public StringLike<GitPath> GitPath(StringLike<SystemPath> systemPath)
        {
            return Path.GetRelativePath(_repo.Info.WorkingDirectory, systemPath);
        }

        public StringLike<SystemPath> SystemPath(StringLike<GitPath> gitPath)
        {
            return Path.GetFullPath(gitPath, _repo.Info.WorkingDirectory);
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

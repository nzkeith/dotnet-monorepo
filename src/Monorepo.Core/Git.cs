using LibGit2Sharp;
using System;
using System.IO;

namespace Monorepo.Core
{
    public class Git : IDisposable
    {
        private readonly Repository _repo;

        public Git(string repositoryPath)
        {
            _repo = new Repository(Repository.Discover(repositoryPath));
        }

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
    }
}

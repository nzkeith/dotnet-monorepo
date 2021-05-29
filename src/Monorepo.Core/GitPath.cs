using System;
using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents a path relative to a git repo root, e.g. relative/path/to/file.txt</summary>
    public class GitPath : StringLike
    {
        public GitPath(string value) : base(Prepare(value))
        { }

        private static string Prepare(string value)
        {
            if (Path.IsPathRooted(value))
            {
                throw new ArgumentException($"{nameof(value)} cannot be an absolute path: '{value}'", nameof(value));
            }

            if (Path.DirectorySeparatorChar != '/')
            {
                value = value.Replace(Path.DirectorySeparatorChar, '/');
            }

            return value;
        }

        public static implicit operator string(GitPath systemPath) => systemPath.Value;

        public static implicit operator GitPath(string value) => new(value);
    }
}

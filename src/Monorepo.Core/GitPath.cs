using System;
using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents a path relative to a git repo root, e.g. relative/path/to/file.txt</summary>
    public class GitPath
    {
        private readonly string _value;

        public GitPath(string value)
        {
            if (Path.IsPathRooted(value))
            {
                throw new ArgumentException($"{nameof(value)} cannot be an absolute path: '{value}'", nameof(value));
            }

            if (Path.DirectorySeparatorChar != '/')
            {
                value = value.Replace(Path.DirectorySeparatorChar, '/');
            }

            _value = value;
        }

        public static implicit operator string(GitPath gitPath) => gitPath._value;

        public static implicit operator GitPath(string value) => new(value);

        public override string ToString() => _value;

        public override bool Equals(object? obj) => obj is GitPath gitPath && _value.Equals(gitPath._value);

        public override int GetHashCode() => _value.GetHashCode();
    }
}

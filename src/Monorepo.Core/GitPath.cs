using System;
using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents a path relative to a git repo root, e.g. relative/path/to/file.txt</summary>
    public class GitPath : IStringLike
    {
        public string FromString(string value)
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
    }
}

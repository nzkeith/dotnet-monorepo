using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents an absolute system path, e.g. C:/absolute/path/to/file.txt</summary>
    public class SystemPath : StringLike
    {
        public SystemPath(string value) : base(Prepare(value))
        { }

        private static string Prepare(string value)
        {
            var path = Path.GetFullPath(value);

            if (Path.DirectorySeparatorChar != '/')
            {
                path = path.Replace(Path.DirectorySeparatorChar, '/');
            }

            return path;
        }

        public static implicit operator string(SystemPath systemPath) => (StringLike)systemPath;

        public static implicit operator SystemPath(string value) => new(value);
    }
}

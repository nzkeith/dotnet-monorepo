using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents an absolute system path, e.g. C:/absolute/path/to/file.txt</summary>
    public class SystemPath : IStringLike
    {
        public string FromString(string value)
        {
            var path = Path.GetFullPath(value);

            if (Path.DirectorySeparatorChar != '/')
            {
                path = path.Replace(Path.DirectorySeparatorChar, '/');
            }

            return path;
        }
    }
}

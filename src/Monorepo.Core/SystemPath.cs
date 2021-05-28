using System.IO;

namespace Monorepo.Core
{
    /// <summary>Represents an absolute system path, e.g. C:/absolute/path/to/file.txt</summary>
    public class SystemPath
    {
        private readonly string _value;

        public SystemPath(string value)
        {
            var path = Path.GetFullPath(value);

            if (Path.DirectorySeparatorChar != '/')
            {
                path = path.Replace(Path.DirectorySeparatorChar, '/');
            }

            _value = path;
        }

        public static implicit operator string(SystemPath systemPath) => systemPath._value;

        public static implicit operator SystemPath(string value) => new(value);

        public override string ToString() => _value;

        public override bool Equals(object? obj) => obj is SystemPath systemPath && _value.Equals(systemPath._value);

        public override int GetHashCode() => _value.GetHashCode();
    }
}

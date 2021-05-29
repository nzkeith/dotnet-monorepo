using System;

namespace Monorepo.Core
{
    public interface IStringLike
    {
        string FromString(string value) => value;
    }

    public class StringLike<T> where T : IStringLike, new()
    {
        private static readonly T _T = new();

        private readonly string _value;

        public StringLike(string value)
        {
            _value = FromString(value);
        }

        private static string FromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _T.FromString(value);
        }

        public static implicit operator string(StringLike<T> systemPath) => systemPath._value;

        public static implicit operator StringLike<T>(string value) => new(value);

        public override string ToString() => _value;

        public override bool Equals(object? obj) => obj switch
        {
            (StringLike<T> other) => _value.Equals(other._value),
            (string other) => _value.Equals(FromString(other)),
            _ => false
        };

        public override int GetHashCode() => _value.GetHashCode();
    }
}

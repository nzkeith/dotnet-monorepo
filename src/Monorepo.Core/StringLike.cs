using System;

namespace Monorepo.Core
{
    public interface IStringLike
    {
        string Create(string value) => value;
    }

    public class StringLike<T> where T : IStringLike, new()
    {
        private static readonly T _T = new();

        private readonly string _value;

        public StringLike(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _value = _T.Create(value);
        }

        public static implicit operator string(StringLike<T> systemPath) => systemPath._value;

        public static implicit operator StringLike<T>(string value) => new(value);

        public override string ToString() => _value;

        public override bool Equals(object? obj) => obj is StringLike<T> other && _value.Equals(other._value);

        public override int GetHashCode() => _value.GetHashCode();
    }
}

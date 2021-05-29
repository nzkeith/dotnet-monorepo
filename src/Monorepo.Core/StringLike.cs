using System;

namespace Monorepo.Core
{
    public class StringLike
    {
        protected string Value { get; }

        public StringLike(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static implicit operator string(StringLike systemPath) => systemPath.Value;

        public static implicit operator StringLike(string value) => new(value);

        public override string ToString() => Value;

        public override bool Equals(object? obj) => obj is StringLike systemPath && Value.Equals(systemPath.Value);

        public override int GetHashCode() => Value.GetHashCode();
    }
}

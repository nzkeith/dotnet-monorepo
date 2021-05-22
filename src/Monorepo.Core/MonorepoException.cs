using System;

namespace Monorepo.Core
{
    public class MonorepoException : Exception
    {
        public MonorepoException()
        {
        }

        public MonorepoException(string message)
            : base(message)
        {
        }

        public MonorepoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

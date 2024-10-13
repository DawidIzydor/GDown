using System;

namespace GHent.GHentai
{
    /// <inheritdoc />
    public class TransferExceededException : Exception
    {
        /// <inheritdoc />
        public TransferExceededException()
        {
        }

        /// <inheritdoc />
        public TransferExceededException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public TransferExceededException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <inheritdoc />
        public TransferExceededException(Exception inner) : base("See inner exception for more details", inner)
        {
        }
    }
}
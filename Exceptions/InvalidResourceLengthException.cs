using System;

namespace RenwordDigital.StringSearchEngine.Exceptions {
    public class InvalidResourceLengthException : Exception {
        public InvalidResourceLengthException(string message) : base(message) {
        }

        public InvalidResourceLengthException(string message, Exception innerException) : 
            base(message, innerException) {
        }
    }
}
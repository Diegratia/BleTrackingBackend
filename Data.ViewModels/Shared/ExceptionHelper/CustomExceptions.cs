using System;
using System.Collections.Generic;

namespace DataView  // ✅ Pastikan namespace-nya DataView
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BusinessException : Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string message, string errorCode = null) : base(message)
        {
            ErrorCode = errorCode;
        }
        
        public BusinessException(string message, Exception innerException, string errorCode = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }
        
        public ValidationException(Dictionary<string, string[]> errors) 
            : base("Validation failed")
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
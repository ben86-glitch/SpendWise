using System;

namespace SpendWise.Exceptions;

public class InsufficientDataException : Exception
{
    public InsufficientDataException(string message) : base(message) { }
}

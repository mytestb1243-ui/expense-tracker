namespace First_Console_shafi;

public class InvalidExpenseException : Exception
{
    public InvalidExpenseException(string message) : base(message) { }
}
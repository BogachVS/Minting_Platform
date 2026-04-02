namespace PiggyGame.Common.Exceptions;

public class EntityWasNotFoundException : Exception
{
    public EntityWasNotFoundException(string message)
        : base(message) { }
    
    public EntityWasNotFoundException(string entityName, string param, object value) 
        : base($"Entity \"{entityName}\" with \"{param}\": \"{value}\" was not found") { }
}

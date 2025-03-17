namespace Domain.Shared;

public abstract class Command
{   
    public abstract bool IsValid();   
}
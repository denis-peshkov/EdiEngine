namespace EdiEngine.Validation;

public interface IValidatedEntity
{
    List<ValidationError> ValidationErrors { get; }
}
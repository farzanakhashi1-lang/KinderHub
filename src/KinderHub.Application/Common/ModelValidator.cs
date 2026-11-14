using System.ComponentModel.DataAnnotations;

namespace KinderHub.Application.Common;

// Runs the same validation rules as the forms ([Required], [Range], IValidatableObject...)
// on the SERVER, so bad data can never reach the database, even if the UI is bypassed.
public static class ModelValidator
{
    public static Result Validate(object model)
    {
        var errors = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true);

        return isValid
            ? Result.Success()
            : Result.Failure(string.Join(" ", errors.Select(e => e.ErrorMessage)));
    }
}

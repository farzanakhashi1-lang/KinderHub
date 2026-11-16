using System.ComponentModel.DataAnnotations;

namespace KinderHub.Application.DTOs;

public class ChildDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public int GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public string ParentName { get; init; } = string.Empty;
    public string ParentEmail { get; init; } = string.Empty;
    public string ParentPhone { get; init; } = string.Empty;
    public string? Notes { get; init; }

    public string FullName => $"{FirstName} {LastName}";

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }
}

// Search + filter + paging options for the children list
public class ChildQuery
{
    public string? Search { get; set; }
    public int? GroupId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ChildEditModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    public DateOnly? DateOfBirth { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a group.")]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Parent name is required.")]
    [StringLength(100)]
    public string ParentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parent email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email.")]
    [StringLength(150)]
    public string ParentEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parent phone is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string ParentPhone { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Extra rule: a kindergarten child must be between 1 and 8 years old
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth is { } dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (dob > today)
                yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(DateOfBirth) });
            else if (dob > today.AddYears(-1))
                yield return new ValidationResult("The child must be at least 1 year old.", new[] { nameof(DateOfBirth) });
            else if (dob < today.AddYears(-8))
                yield return new ValidationResult("The child must be younger than 8 years.", new[] { nameof(DateOfBirth) });
        }
    }

    public static ChildEditModel FromDto(ChildDto c) => new()
    {
        Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, DateOfBirth = c.DateOfBirth,
        GroupId = c.GroupId, ParentName = c.ParentName, ParentEmail = c.ParentEmail,
        ParentPhone = c.ParentPhone, Notes = c.Notes
    };
}

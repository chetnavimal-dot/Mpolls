namespace MPolls.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
}

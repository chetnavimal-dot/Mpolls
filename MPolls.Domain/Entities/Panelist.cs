using System;

namespace MPolls.Domain.Entities;

public class Panelist
{
    public Guid Id { get; set; }

    public string Ulid { get; set; } = string.Empty;

    public string FirebaseId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public bool Verified { get; set; }

    public int? CountryCode { get; set; }

    public int? Gender { get; set; }

    public int? Age { get; set; }

    public DateTime? ConsentCollectedOn { get; set; }

    public bool Onboarded { get; set; }
}

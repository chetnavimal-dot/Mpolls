using System;
using MPolls.Domain.Enums;

namespace MPolls.Domain.Entities;

public class UserReward
{
    public Guid RewardId { get; set; }

    public string PanelistUlid { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public int Points { get; set; }

    public RewardTransactionType TransactionType { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public Panelist? Panelist { get; set; }
}

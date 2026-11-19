namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §5.</summary>
public enum JobAdvertisementStatus : byte
{
    Draft = 0,
    SubmittedForReview = 1,
    UnderReview = 2,
    Rejected = 3,
    ApprovedAwaitingPayment = 4,
    PaymentPending = 5,
    Paid = 6,
    Published = 7,
    Paused = 8,
    Closed = 9,
    Expired = 10,
    Cancelled = 11,
}

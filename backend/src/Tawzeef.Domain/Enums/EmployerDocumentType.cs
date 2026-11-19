namespace Tawzeef.Domain.Enums;

/// <summary>WORKFLOWS_AND_STATE_MACHINES.md §3 — the 3 required legal documents, gating
/// VerificationStatus == Approved exactly like the job seeker's 3 (§1).</summary>
public enum EmployerDocumentType : byte
{
    CommercialRegister = 0,
    TaxCard = 1,
    OwnerNationalId = 2,
}

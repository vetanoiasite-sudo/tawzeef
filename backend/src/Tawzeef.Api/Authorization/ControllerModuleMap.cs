namespace Tawzeef.Api.Authorization;

/// <summary>
/// Controller name → permission-catalogue module (RBAC_PERMISSIONS.md §2). Populate as feature
/// controllers are added — an unlisted controller fails the startup audit rather than being
/// silently allowed.
/// </summary>
public static class ControllerModuleMap
{
    public static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Offices"] = "offices",
        ["JobSeekers"] = "jobseekers",
        ["OfficeJobSeekers"] = "jobseekers",
        ["Employers"] = "employers",
        ["OfficeEmployers"] = "employers",
        ["Complaints"] = "complaints",
        ["OfficeComplaints"] = "complaints",
        ["OfficeUsers"] = "officeusers",
        ["EmployerJobAdvertisements"] = "jobadvertisements",
        ["OfficeJobAdvertisements"] = "jobadvertisements",
        ["JobSeekerApplications"] = "jobapplications",
        ["EmployerApplications"] = "jobapplications",
        ["Suggestions"] = "suggestions",
        ["OfficeSuggestions"] = "suggestions",
    };
}

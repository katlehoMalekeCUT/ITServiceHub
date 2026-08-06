namespace ServiceHub_IT.Utilities;

public static class RoleDashboardRoute
{
    public static string GetControllerName(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Employee";
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "admin" => "Admin",
            "technician" => "Technician",
            _ => "Employee"
        };
    }
}

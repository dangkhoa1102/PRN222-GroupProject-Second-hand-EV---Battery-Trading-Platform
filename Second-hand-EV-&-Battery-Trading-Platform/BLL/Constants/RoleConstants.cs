namespace BLL.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static bool TryNormalize(string? inputRole, out string normalizedRole)
    {
        if (string.Equals(inputRole, Admin, StringComparison.OrdinalIgnoreCase))
        {
            normalizedRole = Admin;
            return true;
        }

        if (string.Equals(inputRole, Customer, StringComparison.OrdinalIgnoreCase))
        {
            normalizedRole = Customer;
            return true;
        }

        normalizedRole = string.Empty;
        return false;
    }
}


namespace RaceDay.Web.Helpers;

// Names of the values kept in the MVC session after login.
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string FullName = "FullName";
    public const string Role = "Role";
}

// The two roles in the RaceDay system.
public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
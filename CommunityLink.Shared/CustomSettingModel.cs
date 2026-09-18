namespace CommunityLink.Shared;

public sealed class CustomSettingModel
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public SmtpSettings SmtpSettings { get; set; } = new();
}

public sealed class ConnectionStringsSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
}

public sealed class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CommunityLink.Api";
    public string Audience { get; set; } = "CommunityLink.Clients";
    public int ExpiryMinutes { get; set; } = 480;
}

public sealed class SecuritySettings
{
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int PasswordHistoryLimit { get; set; } = 5;
    public int PasswordExpiryDays { get; set; } = 90;
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Community Link";
}
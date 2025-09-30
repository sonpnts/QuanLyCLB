namespace QuanLyCLB.Infrastructure.Settings;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 25;

    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string? FromName { get; set; }
}

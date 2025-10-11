namespace LabsQueueBot.Core.Settings;

public class PostgreSqlSettings
{
    public required string Host { get; init; }
    public required string Port { get; init; }
    public required string Database { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
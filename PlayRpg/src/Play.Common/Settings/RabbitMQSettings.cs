namespace Play.Common.Settings
{
    public record RabbitMQSettings
    {
        public string Host { get; init; } = string.Empty;
        public string User { get; init; } = string.Empty;
        public string Pass { get; init; } = string.Empty;
    }
}
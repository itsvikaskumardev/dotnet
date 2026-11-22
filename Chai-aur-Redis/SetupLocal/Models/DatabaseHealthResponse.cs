namespace Chai_aur_Redis.SetupLocal.Models
{
    public class DatabaseHealthResponse
    {
        public string Status { get; set; } = string.Empty;

        public string? Database { get; set; }

        public string? Response { get; set; }
    }
}

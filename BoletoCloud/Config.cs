namespace BoletoCloud
{
    public static class Config
    {
        public static string APIKey { get; set; }
        public static bool? isSandBox { get; set; }
        public static string UrlProxy { get; set; } //Example: "http://127.0.0.1:8888"
    }

    internal class ConfigURIS
    {
        static string SandboxURI => "https://sandbox.boletocloud.com/api/v1";
        static string ProductionURI => "https://app.boletocloud.com/api/v1";
        public static string URI()
        {
            if (!Config.isSandBox.HasValue) return "";
            return Config.isSandBox.Value ? SandboxURI : ProductionURI;
        }
    }
}

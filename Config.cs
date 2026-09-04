using System.IO;
using System.Text.Json;

namespace DataPars;

public class Config
{
    public string Server { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Password { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public string DbPattern { get; set; } = "Metro_SKZ_Archive_%";
    public int DbParallelism { get; set; } = 2;
    public int SetupParallelism { get; set; } = 4;

    public string BuildConnectionString(string database) =>
        $"Server={Server};Database={database};User Id={UserId};Password={Password};TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;";

    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    public static Config Load()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaults = new Config
            {
                Server = "GYRDYMOV-NEW\\DREAM",
                UserId = "sa",
                Password = "Basepwd#0000",
                OutputPath = @"C:\Users\БеляевИА\Desktop\ADC_Exports",
                DbPattern = "Metro_SKZ_Archive_%",
                DbParallelism = 2,
                SetupParallelism = 4
            };

            var json = JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);

            Console.WriteLine($"⚠️ Создан файл конфигурации: {ConfigPath}");
            Console.WriteLine("   Отредактируйте его и запустите снова.");
            Environment.Exit(0);
        }

        var text = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<Config>(text) ?? new Config();
    }
}

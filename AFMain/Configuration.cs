using System.Globalization;
using System.Reflection;
using TShockAPI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AutoFish.AFMain;

/// <summary>
///     插件配置模型，负责序列化与默认值初始化。
/// </summary>
internal class Configuration
{
    /// <summary>配置目录。</summary>
    public static readonly string ConfigDirectory = Path.Combine(TShock.SavePath, "AutoFish");

    /// <summary>配置文件路径。</summary>
    public static readonly string FilePath = Path.Combine(ConfigDirectory, "config.yml");

    /// <summary>是否首次生成配置文件（仅本次运行内有效）。</summary>
    internal static bool IsFirstInstall { get; private set; }

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithIndentedSequences()
        .Build();

    /// <summary>插件总开关。</summary>
    public bool PluginEnabled { get; set; } = true;

    /// <summary>界面与输出语言，默认 zh-cn。</summary>
    public string Language { get; set; } = "zh-cn";

    /// <summary>启用自定义钓鱼规则。</summary>
    public bool EnableCustomFishRules { get; set; } = true;

    /// <summary>启用原版钓鱼规则。</summary>
    public bool EnableVanillaFishRules { get; set; } = true;

    /// <summary>自定义钓鱼规则列表。</summary>
    public List<CustomFishDropRule> CustomFishRules { get; set; } = new();

    /// <summary>
    ///     将当前配置写入磁盘。
    /// </summary>
    public void Write()
    {
        Directory.CreateDirectory(ConfigDirectory);
        var yaml = Serializer.Serialize(this);
        File.WriteAllText(FilePath, yaml);
    }

    /// <summary>
    ///     读取配置文件，若不存在则创建默认配置。
    /// </summary>
    public static Configuration Read()
    {
        EnsureConfigFileExists();

        var yamlContent = File.ReadAllText(FilePath);
        var config = Deserializer.Deserialize<Configuration>(yamlContent);
        config.Normalize();
        return config;
    }

    private void Normalize()
    {
        Language = string.IsNullOrWhiteSpace(Language) ? "zh-cn" : Language.ToLowerInvariant();
    }

    private static void EnsureConfigFileExists()
    {
        Directory.CreateDirectory(ConfigDirectory);

        if (File.Exists(FilePath))
        {
            Console.WriteLine("[AutoFish]配置文件成功找到并加载");
            return;
        }

        IsFirstInstall = true;

        var preferredCulture = ResolvePreferredConfigCulture();
        if (TryExportEmbeddedConfig(preferredCulture))
        {
            Console.WriteLine($"[AutoFish]导出 {preferredCulture} 默认配置成功");
            return;
        }

        if (!preferredCulture.Equals("en-us", StringComparison.OrdinalIgnoreCase) &&
            TryExportEmbeddedConfig("en-us"))
        {
            Console.WriteLine("[AutoFish]导出 en-us 默认配置成功");
            return;
        }

        if (!preferredCulture.Equals("zh-cn", StringComparison.OrdinalIgnoreCase) &&
            TryExportEmbeddedConfig("zh-cn"))
        {
            Console.WriteLine("[AutoFish]导出 zh-cn 默认配置成功");
            return;
        }

        Console.WriteLine($"[AutoFish]无法导出默认配置！！！！ {preferredCulture} ");
        var defaultConfig = new Configuration();
        defaultConfig.Write();
    }

    private static bool TryExportEmbeddedConfig(string culture)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($"{culture}.yml", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null) return false;

        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null) return false;

        using var reader = new StreamReader(resourceStream);
        var content = reader.ReadToEnd();
        File.WriteAllText(FilePath, content);
        return true;
    }

    private static string ResolvePreferredConfigCulture()
    {
        var uiCulture = CultureInfo.CurrentUICulture;
        var name = uiCulture.Name.ToLowerInvariant();

        if (name.StartsWith("zh")) return "zh-cn";

        if (name.StartsWith("en")) return "en-us";

        return "en-us";
    }
}

/// <summary>
///     自定义钓鱼掉落规则（用于配置文件）。
/// </summary>
public class CustomFishDropRule
{
    /// <summary>可能掉落的物品 ID 列表</summary>
    public List<int> PossibleItems { get; set; } = new();

    /// <summary>概率分子</summary>
    public int ChanceNumerator { get; set; } = 1;

    /// <summary>概率分母</summary>
    public int ChanceDenominator { get; set; } = 1;

    /// <summary>稀有度类型字符串</summary>
    public string? Rarity { get; set; }

    /// <summary>钓鱼条件类型列表</summary>
    public List<string> Conditions { get; set; } = new();
}
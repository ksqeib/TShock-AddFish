
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AddFish.AFMain;

[ApiVersion(2, 1)]
public partial class AddFish : TerrariaPlugin
{
    public const string PermissionPrefix = "addfish.";
    public const string AdminPermission = $"{PermissionPrefix}admin";
    public const string CommonPermission = $"{PermissionPrefix}common";
    public const string DenyPermissionPrefix = $"{PermissionPrefix}no.";

    /// <summary>DEBUG 模式开关。</summary>
    internal static bool DebugMode = false;

    /// <summary>全局配置实例。</summary>
    internal static Configuration Config = new();

    /// <summary>
    ///     创建插件实例。
    /// </summary>
    public AddFish(Main game) : base(game)
    {
    }

    /// <summary>插件名称。</summary>
    public override string Name => "AddFish";

    /// <summary>插件作者。</summary>
    public override string Author => "ksqeib";

    /// <summary>插件版本。</summary>
    public override Version Version => new(1, 0,0);

    /// <summary>插件描述。</summary>
    public override string Description => "青山常伴绿水，燕雀已是南飞";

    /// <summary>统一的权限检查，支持 admin 全覆盖、common 通用以及显式负权限。</summary>
    internal static bool HasFeaturePermission(TSPlayer? player, string featureKey, bool allowCommon = true)
    {
        if (player == null) return false;

        if (player.HasPermission(AdminPermission)) return true;

        var denyPermission = $"{DenyPermissionPrefix}{featureKey}";
        if (player.HasPermission(denyPermission)) return false;

        if (allowCommon && player.HasPermission(CommonPermission)) return true;

        var allowPermission = $"{PermissionPrefix}{featureKey}";
        return player.HasPermission(allowPermission);
    }

    /// <summary>
    ///     插件初始化，注册事件和命令。
    /// </summary>
    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
        TShockAPI.Commands.ChatCommands.Add(new Command(new List<string> { "addfish", CommonPermission },
            Commands.Afs, "adf", "addfish"));
        TShockAPI.Commands.ChatCommands.Add(new Command(AdminPermission, Commands.Afa, "adfa", "addfishadmin"));
        RegisterToFishDB();
    }

    /// <summary>
    ///     服务器初始化后显示插件状态信息。
    /// </summary>
    private void OnGamePostInitialize(EventArgs args)
    {
        if (!Configuration.IsFirstInstall) return;
        TShock.Log.ConsoleInfo("========================================");
        TShock.Log.ConsoleInfo($"[AddFish] 插件已成功加载 v{Version}");
        TShock.Log.ConsoleInfo("[AddFish] 当前状态：正常运行");
        TShock.Log.ConsoleInfo("========================================");
        TShock.Log.ConsoleInfo("[AddFish] 遇到 BUG 或问题？");
        TShock.Log.ConsoleInfo("[AddFish] 1. 请先查看 README 文档");
        TShock.Log.ConsoleInfo("[AddFish] 2. 无法解决再联系开发者");
        TShock.Log.ConsoleInfo("[AddFish] GitHub: https://github.com/ksqeib/TShock-AddFish");
        TShock.Log.ConsoleInfo("[AddFish] Star 很重要，是支持开发者持续开发的动力，欢迎点个 Star！");
        TShock.Log.ConsoleInfo("[AddFish] 本插件为免费开源插件，如有任何付费购买行为，说明您被骗了。");
        TShock.Log.ConsoleInfo("[AddFish] 联系方式：QQ 2388990095 (ksqeib)");
        TShock.Log.ConsoleInfo("[AddFish] 警告：请勿在群内艾特开发者！");
        TShock.Log.ConsoleInfo("========================================");
    }

    /// <summary>
    ///     释放插件，注销事件与命令。
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.Afs);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.Afa);
            UnRegisterToFishDB();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     处理 /reload 触发的配置重载。
    /// </summary>
    private static void ReloadConfig(ReloadEventArgs args)
    {
        LoadConfig();
        args.Player.SendInfoMessage(Lang.T("addfish.reload.done"));
    }

    /// <summary>
    ///     读取并落盘配置。
    /// </summary>
    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Lang.Load(Config.Language);
        LoadCustomFishRules();
    }

    /// <summary>
    ///     从配置加载自定义钓鱼规则。
    /// </summary>
    private static void LoadCustomFishRules()
    {
        // 清空现有的自定义规则列表
        CustomRuleList._rules.Clear();

        if (Config.CustomFishRules == null || Config.CustomFishRules.Count == 0)
        {
            return;
        }

        try
        {
            var loadedCount = Enums.FishDropRuleExporter.LoadCustomRulesToList(
                Config.CustomFishRules,
                CustomRuleList);

            if (loadedCount > 0)
            {
                TShock.Log.ConsoleInfo($"[AddFish] 已加载 {loadedCount} 条自定义钓鱼规则");
            }
        }
        catch (Exception ex)
        {
            TShock.Log.Error($"[AddFish] 加载自定义钓鱼规则失败: {ex.Message}");
        }
    }
}
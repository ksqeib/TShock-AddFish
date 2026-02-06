using TShockAPI;

namespace AutoFish.AFMain;

/// <summary>
///     自动钓鱼插件的聊天命令处理入口与通用逻辑。
/// </summary>
public partial class Commands
{
    /// <summary>
    ///     处理 /af 相关指令的入口。
    /// </summary>
    public static void Afs(CommandArgs args)
    {
        var player = args.Player;
        var isConsole = !player.RealPlayer;

        if (!AutoFish.Config.PluginEnabled) return;

        if (isConsole)
        {
            player.SendInfoMessage(Lang.T("help.consoleUseAfa"));
            // 不再直接return，允许继续执行
        }
        if (args.Parameters.Count == 0)
        {
            HelpCmd(args.Player);
            return;
        }

        if (HandlePlayerCommand(args))
            return;

        HelpCmd(args.Player);
    }

    /// <summary>
    ///     处理 /afa（管理员）指令入口。
    /// </summary>
    public static void Afa(CommandArgs args)
    {
        if (!AutoFish.Config.PluginEnabled) return;

        var caller = args.Player ?? TSPlayer.Server;
        if (!caller.HasPermission(AutoFish.AdminPermission))
        {
            caller.SendErrorMessage(Lang.T("error.noPermission.admin"));
            return;
        }

        if (args.Parameters.Count == 0)
        {
            SendAdminHelpOnly(caller);
            return;
        }

        if (HandleAdminCommand(args)) return;

        SendAdminHelpOnly(caller);
    }
}
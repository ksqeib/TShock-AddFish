using TShockAPI;

namespace AddFish.AFMain;

/// <summary>
///     自动钓鱼插件的聊天命令处理入口与通用逻辑。
/// </summary>
public partial class Commands
{
    /// <summary>
    ///     处理 /adf 相关指令的入口。
    /// </summary>
    public static void Afs(CommandArgs args)
    {
        var player = args.Player;
        var isConsole = !player.RealPlayer;

        if (!AddFish.Config.PluginEnabled) return;

        if (isConsole)
        {
            player.SendInfoMessage(Lang.T("addfish.help.consoleUseAfa"));
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
    ///     处理 /adfa（管理员）指令入口。
    /// </summary>
    public static void Afa(CommandArgs args)
    {
        if (!AddFish.Config.PluginEnabled) return;

        var caller = args.Player ?? TSPlayer.Server;
        if (!caller.HasPermission(AddFish.AdminPermission))
        {
            caller.SendErrorMessage(Lang.T("addfish.error.noPermission.admin"));
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
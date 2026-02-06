using System.Text;
using AddFish.AFMain.Enums;
using TShockAPI;

namespace AddFish.AFMain;

/// <summary>
///     管理员侧指令处理�?
/// </summary>
public partial class Commands
{
    private static void AppendAdminHelp(TSPlayer player, StringBuilder helpMessage)
    {
        helpMessage.Append('\n').Append(Lang.T("addfish.help.admin.export"));
        helpMessage.Append('\n').Append(Lang.T("addfish.help.admin.exportstats"));
    }

    private static bool HandleAdminCommand(CommandArgs args)
    {
        var caller = args.Player ?? TSPlayer.Server;
        if (!caller.HasPermission(AddFish.AdminPermission)) return false;

        var sub = args.Parameters[0].ToLower();

        if (args.Parameters.Count == 1)
            switch (sub)
            {
                case "exportstats":
                    ExportStatsCommand(caller);
                    return true;
                case "export":
                    ExportToFileCommand(caller);
                    return true;
                case "debug":
                    AddFish.DebugMode = !AddFish.DebugMode;
                    var debugStatus = AddFish.DebugMode ? "[c/00FF00:已开启]" : "[c/FF0000:已关闭]";
                    caller.SendSuccessMessage($"DEBUG 模式 {debugStatus}");
                    if (AddFish.DebugMode)
                    {
                        caller.SendInfoMessage("[c/FFFF00:警告：DEBUG 模式会输出大量调试信息到控制台]");
                        TShock.Log.ConsoleInfo($"[AddFish] DEBUG 模式已开启 (操作者: {caller.Name})");
                    }
                    else
                    {
                        TShock.Log.ConsoleInfo($"[AddFish] DEBUG 模式已关闭 (操作者: {caller.Name})");
                    }
                    return true;
                default:
                    return false;
            }

        return false;
    }

    /// <summary>
    ///     导出统计信息命令。
    /// </summary>
    private static void ExportStatsCommand(TSPlayer caller)
    {
        try
        {
            var stats = FishDropRuleExporter.GetStatistics();
            
            caller.SendSuccessMessage("=== 钓鱼规则导出统计 ===");
            caller.SendInfoMessage($"总规则数: {stats.TotalRules}");
            caller.SendInfoMessage($"完全匹配: {stats.FullyMappedRules} ({GetPercentage(stats.FullyMappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"部分匹配: {stats.PartiallyMappedRules} ({GetPercentage(stats.PartiallyMappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"无法映射: {stats.UnmappedRules} ({GetPercentage(stats.UnmappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"使用 /adfa export 导出到文件");
        }
        catch (Exception ex)
        {
            caller.SendErrorMessage($"导出统计失败: {ex.Message}");
            TShock.Log.Error($"导出统计失败: {ex}");
        }
    }

    /// <summary>
    ///     导出规则到文件命令。
    /// </summary>
    private static void ExportToFileCommand(TSPlayer caller)
    {
        try
        {
            var exportPath = Path.Combine(Configuration.ConfigDirectory, "export.yml");
            FishDropRuleExporter.ExportToYamlFile(exportPath, skipPartiallyMapped: true);
            
            var stats = FishDropRuleExporter.GetStatistics();
            var exportedCount = FishDropRuleExporter.ExportSystemRules(skipPartiallyMapped: true).Count;
            
            caller.SendSuccessMessage($"已成功导出 {exportedCount} 条规则到文件:");
            caller.SendInfoMessage(exportPath);
            caller.SendInfoMessage($"总规则数: {stats.TotalRules}");
            caller.SendInfoMessage($"完全匹配: {stats.FullyMappedRules} ({GetPercentage(stats.FullyMappedRules, stats.TotalRules)}%)");
            caller.SendInfoMessage($"已导出: {exportedCount} 条完全匹配的规则");
        }
        catch (Exception ex)
        {
            caller.SendErrorMessage($"导出到文件失败: {ex.Message}");
            TShock.Log.Error($"导出到文件失败: {ex}");
        }
    }

    /// <summary>
    ///     计算百分比。
    /// </summary>
    private static double GetPercentage(int value, int total)
    {
        if (total == 0) return 0;
        return Math.Round(value * 100.0 / total, 2);
    }
}


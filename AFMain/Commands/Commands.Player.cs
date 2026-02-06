using System.Text;
using TShockAPI;

namespace AddFish.AFMain;

/// <summary>
///     玩家侧指令处理�?
/// </summary>
public partial class Commands
{
    private static void AppendPlayerHelp(TSPlayer player, StringBuilder helpMessage)
    {
        if (AddFish.Config.CustomFishRules != null && AddFish.Config.CustomFishRules.Count > 0)
            helpMessage.Append('\n').Append(Lang.T("addfish.help.player.rules"));
    }

    private static bool HandlePlayerCommand(CommandArgs args)
    {
        var player = args.Player;
        var sub = args.Parameters[0].ToLower();

        if (args.Parameters.Count == 1)
            switch (sub)
            {
                case "rules":
                    // 显示自定义钓鱼规�?
                    return HandleRulesCommand(args);
                default:
                    return false;
            }

        return false;
    }

    /// <summary>
    ///     处理查看自定义规则命令，支持分页�?
    /// </summary>
    private static bool HandleRulesCommand(CommandArgs args)
    {
        var customRules = AddFish.Config.CustomFishRules;
        
        if (customRules == null || customRules.Count == 0)
        {
            args.Player.SendInfoMessage(Lang.T("addfish.rules.noCustomRules"));
            return true;
        }

        // 解析页码
        int pageNumber = 1;
        if (args.Parameters.Count == 2)
        {
            if (!int.TryParse(args.Parameters[1], out pageNumber) || pageNumber < 1)
            {
                args.Player.SendErrorMessage(Lang.T("addfish.error.invalidPage"));
                return true;
            }
        }

        const int pageSize = 5; // 每页显示5条规�?
        int totalPages = (int)Math.Ceiling((double)customRules.Count / pageSize);
        
        if (pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = Math.Min(startIndex + pageSize, customRules.Count);

        // 显示标题
        args.Player.SendInfoMessage(Lang.T("addfish.rules.title", pageNumber, totalPages, customRules.Count));

        // 显示当前页的规则
        for (int i = startIndex; i < endIndex; i++)
        {
            var rule = customRules[i];
            var sb = new StringBuilder();

            // 规则编号
            sb.Append($"[c/F9D77E:#{i + 1}] ");

            // 物品列表
            if (rule.PossibleItems != null && rule.PossibleItems.Count > 0)
            {
                var itemNames = rule.PossibleItems.Select(id =>
                {
                    var item = TShock.Utils.GetItemById(id);
                    return item != null ? $"[c/92C5EC:{item.Name}]" : $"[c/FF6B6B:Unknown({id})]";
                });
                sb.Append(string.Join(", ", itemNames));
            }
            else
            {
                sb.Append("[c/FF6B6B:无物品]");
            }

            args.Player.SendInfoMessage(sb.ToString());

            // 概率信息
            var probability = (double)rule.ChanceNumerator / rule.ChanceDenominator * 100;
            sb.Clear();
            sb.Append($"  [c/B5E7A0:{Lang.T("addfish.rules.probability")}] ");
            sb.Append($"{rule.ChanceNumerator}/{rule.ChanceDenominator}");
            sb.Append($" ([c/FFD700:{probability:F2}%])");
            args.Player.SendInfoMessage(sb.ToString());

            // 稀有度
            if (!string.IsNullOrEmpty(rule.Rarity))
            {
                sb.Clear();
                sb.Append($"  [c/D4A5F9:{Lang.T("addfish.rules.rarity")}] ");
                
                if (Enums.FishRarityMapper.TryParse(rule.Rarity, out var rarityType))
                {
                    sb.Append(Enums.FishRarityMapper.GetLocalizedName(rarityType));
                }
                else
                {
                    sb.Append(rule.Rarity);
                }
                args.Player.SendInfoMessage(sb.ToString());
            }

            // 条件列表
            if (rule.Conditions != null && rule.Conditions.Count > 0)
            {
                sb.Clear();
                sb.Append($"  [c/A8E6CF:{Lang.T("addfish.rules.conditions")}] ");
                
                var conditionNames = new List<string>();
                foreach (var condStr in rule.Conditions)
                {
                    if (Enums.FishingConditionMapper.TryParse(condStr, out var condType))
                    {
                        conditionNames.Add(Enums.FishingConditionMapper.GetLocalizedName(condType));
                    }
                    else
                    {
                        conditionNames.Add(condStr);
                    }
                }
                sb.Append(string.Join(", ", conditionNames));
                args.Player.SendInfoMessage(sb.ToString());
            }

            // 空行分隔
            if (i < endIndex - 1)
            {
                args.Player.SendInfoMessage("");
            }
        }

        // 翻页提示
        if (totalPages > 1)
        {
            args.Player.SendInfoMessage(Lang.T("addfish.rules.pageHint", pageNumber, totalPages));
        }

        return true;
    }
}
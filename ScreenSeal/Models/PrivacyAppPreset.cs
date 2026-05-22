namespace ScreenSeal.Models;

public sealed class PrivacyAppPreset
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string[] ProcessNames { get; init; }
    public required string[] TitleKeywords { get; init; }
}

public static class PrivacyAppCatalog
{
    public static readonly IReadOnlyList<PrivacyAppPreset> All =
    [
        new()
        {
            Id = "wechat",
            DisplayName = "微信",
            ProcessNames = ["WeChat"],
            TitleKeywords = ["微信"]
        },
        new()
        {
            Id = "qq",
            DisplayName = "QQ",
            ProcessNames = ["QQ"],
            TitleKeywords = ["QQ"]
        },
        new()
        {
            Id = "tim",
            DisplayName = "TIM",
            ProcessNames = ["TIM"],
            TitleKeywords = ["TIM"]
        },
        new()
        {
            Id = "dingtalk",
            DisplayName = "钉钉",
            ProcessNames = ["DingTalk"],
            TitleKeywords = ["钉钉"]
        },
        new()
        {
            Id = "wxwork",
            DisplayName = "企业微信",
            ProcessNames = ["WXWork"],
            TitleKeywords = ["企业微信"]
        },
        new()
        {
            Id = "feishu",
            DisplayName = "飞书",
            ProcessNames = ["Feishu", "Lark"],
            TitleKeywords = ["飞书"]
        }
    ];

    public static PrivacyAppPreset? FindById(string id) =>
        All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static HashSet<string> GetEffectiveEnabledIds(AppConfig config)
    {
        if (config.EnabledAppIds == null)
            return All.Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return config.EnabledAppIds
            .Select(id => id.Trim())
            .Where(id => id.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}

using System.Globalization;
using System.Text;
using Terraria;
using TShockAPI;

namespace AddFish.Utils;

public class Tools
{
    /// <summary>
    ///     给玩家发送一个简单的渐变色消息。
    /// </summary>
    public static void SendGradientMessage(TSPlayer player, string text, string startHex = "F3A6FF",
        string endHex = "7CC7FF")
    {
        if (player == null || string.IsNullOrEmpty(text)) return;

        var (sr, sg, sb) = ParseHex(startHex);
        var (er, eg, eb) = ParseHex(endHex);

        var len = text.Length;
        var sbMsg = new StringBuilder(len * 12);
        for (var i = 0; i < len; i++)
        {
            var t = len <= 1 ? 0f : (float)i / (len - 1);
            var r = (int)MathF.Round(sr + (er - sr) * t);
            var g = (int)MathF.Round(sg + (eg - sg) * t);
            var b = (int)MathF.Round(sb + (eb - sb) * t);
            sbMsg.Append("[c/");
            sbMsg.Append(r.ToString("X2"));
            sbMsg.Append(g.ToString("X2"));
            sbMsg.Append(b.ToString("X2"));
            sbMsg.Append(":");
            sbMsg.Append(text[i]);
            sbMsg.Append(']');
        }

        player.SendInfoMessage(sbMsg.ToString());
    }

    private static (int r, int g, int b) ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return (255, 255, 255);
        hex = hex.TrimStart('#');
        if (hex.Length is not 6) return (255, 255, 255);
        var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return (r, g, b);
    }
}
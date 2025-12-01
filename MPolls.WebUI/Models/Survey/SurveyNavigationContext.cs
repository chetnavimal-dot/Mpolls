using System;
using System.Text;
using System.Text.Json;

namespace MPolls.WebUI.Models.Survey;

public sealed record SurveyNavigationContext(int CategoryId, string Ulid)
{
    public static string Encode(SurveyNavigationContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var json = JsonSerializer.Serialize(context);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public static bool TryDecode(string? encoded, out SurveyNavigationContext? context)
    {
        context = null;

        if (string.IsNullOrWhiteSpace(encoded))
        {
            return false;
        }

        try
        {
            var buffer = Convert.FromBase64String(encoded);
            var json = Encoding.UTF8.GetString(buffer);
            context = JsonSerializer.Deserialize<SurveyNavigationContext>(json);
        }
        catch
        {
            context = null;
            return false;
        }

        if (context is null || context.CategoryId <= 0 || string.IsNullOrWhiteSpace(context.Ulid))
        {
            context = null;
            return false;
        }

        return true;
    }
}

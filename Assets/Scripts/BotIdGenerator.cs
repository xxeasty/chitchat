using System;
using System.Text;
using System.Text.RegularExpressions;

public static class BotIdGenerator
{
    private static readonly System.Random rng = new System.Random();

    public static string GenerateBotId(string nickname, int randomLength = 6)
    {
        string baseName = Regex.Replace(nickname, @"[^a-zA-Z0-9]", "").ToLower();

        if (baseName.Length > 4)
            baseName = baseName.Substring(0, 4);
        if (string.IsNullOrEmpty(baseName))
            baseName = "bot";  // fallback

        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder randomPart = new StringBuilder();
        for (int i = 0; i < randomLength; i++)
        {
            randomPart.Append(chars[rng.Next(chars.Length)]);
        }

        return $"bot-{baseName}-{randomPart}";
    }
}
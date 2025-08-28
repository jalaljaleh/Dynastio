using Discord;

public static class DiscordButtonsExtensions
{
    public static ActionRowBuilder ToActionRowBuilder(this ButtonBuilder[] buttons)
    {
        var row = new ActionRowBuilder();
        foreach (var btn in buttons)
            row.WithButton(btn);
        return row;
    }

    // new params overload
    public static ActionRowBuilder ToActionRowBuilder( this ButtonBuilder first,  params ButtonBuilder[] rest)
    {
        var row = new ActionRowBuilder()
            .WithButton(first);

        foreach (var btn in rest)
            row.WithButton(btn);

        return row;
    }
}

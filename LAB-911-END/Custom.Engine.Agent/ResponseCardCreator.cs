using AdaptiveCards;
using Microsoft.Teams.AI.AI.Models;

namespace Custom.Engine.Agent;

internal static class ResponseCardCreator
{
    public static AdaptiveCard CreateResponseCard(ChatMessage response)
    {
        var citations = response.Context.Citations;
        var citationCards = new List<AdaptiveAction>();

        for (int i = 0; i < citations.Count; i++)
        {
            var citation = citations[i];
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
            {
                Body = [
                    new AdaptiveTextBlock
                    {
                        Text = citation.Title,
                        Weight = AdaptiveTextWeight.Bolder,
                        FontType = AdaptiveFontType.Default
                    },
                    new AdaptiveTextBlock
                    {
                        Text = citation.Content,
                        Wrap = true
                    }
                ]
            };

            citationCards.Add(new AdaptiveShowCardAction
            {
                Title = $"{i + 1}",
                Card = card
            });
        }

        var formattedText = FormatResponse(response.GetContent<string>());

        var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
        {
            Body = [
                new AdaptiveTextBlock
                {
                    Text = formattedText,
                    Wrap = true
                },
                new AdaptiveTextBlock
                {
                    Text = "Citations",
                    Weight = AdaptiveTextWeight.Bolder,
                    FontType = AdaptiveFontType.Default,
                    Wrap = true
                },
                new AdaptiveActionSet
                {
                    Actions = citationCards
                }
            ]
        };
        return adaptiveCard;
    }

    private static string FormatResponse(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\[doc(\d)+\]", "**[$1]** ");
    }
}

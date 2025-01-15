using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using Microsoft.Teams.AI.AI.Planners;
using Microsoft.Teams.AI.AI;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Custom.Engine.Agent;

internal class Actions
{
    [Action(AIConstants.SayCommandActionName, isDefault: false)]
    public static async Task<string> SayCommandAsync([ActionTurnContext] ITurnContext turnContext, [ActionParameters] PredictedSayCommand command, CancellationToken cancellationToken = default)
    {
        IMessageActivity activity;
        if (command?.Response?.Context?.Citations?.Count > 0)
        {
            AdaptiveCard card = ResponseCardCreator.CreateResponseCard(command.Response);
            Attachment attachment = new()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            activity = MessageFactory.Attachment(attachment);
        }
        else
        {
            activity = MessageFactory.Text(command.Response.GetContent<string>());
        }

        activity.Entities =
            [
                new Entity
                {
                    Type = "https://schema.org/Message",
                    Properties = new()
                    {
                        { "@type", "Message" },
                        { "@context", "https://schema.org" },
                        { "@id", string.Empty },
                        { "additionalType", JArray.FromObject(new string[] { "AIGeneratedContent" } ) },
                        { "usageInfo", JObject.FromObject(
                            new JObject(){
                                { "@type", "CreativeWork" },
                                { "name", "Confidential" },
                                { "description", "Sensitive information, do not share outside of your organization." },
                            })
                        }
                    }
                }
            ];

        activity.ChannelData = new
        {
            feedbackLoopEnabled = true
        };

        await turnContext.SendActivityAsync(activity, cancellationToken);

        return string.Empty;
    }

    [Action(AIConstants.FlaggedInputActionName)]
    public static async Task<string> OnFlaggedInput([ActionTurnContext] ITurnContext turnContext, [ActionParameters] Dictionary<string, object> entities)
    {
        string entitiesJsonString = System.Text.Json.JsonSerializer.Serialize(entities);
        await turnContext.SendActivityAsync($"I'm sorry your message was flagged: {entitiesJsonString}");
        return string.Empty;
    }

    [Action(AIConstants.FlaggedOutputActionName)]
    public static async Task<string> OnFlaggedOutput([ActionTurnContext] ITurnContext turnContext)
    {
        await turnContext.SendActivityAsync("I'm not allowed to talk about such things.");
        return string.Empty;
    }
}

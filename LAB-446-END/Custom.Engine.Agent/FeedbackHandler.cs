using Custom.Engine.Agent.Models;
using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.Application;
using Microsoft.Teams.AI.State;
using System.Text.Json;

namespace Custom.Engine.Agent;

internal class FeedbackHandler
{
    internal static async Task OnFeedback(ITurnContext turnContext, TurnState turnState, FeedbackLoopData feedbackLoopData, CancellationToken cancellationToken)
    {
        var reaction = feedbackLoopData.ActionValue.Reaction;
        var feedback = JsonSerializer.Deserialize<Feedback>(feedbackLoopData.ActionValue.Feedback).FeedbackText;

        await turnContext.SendActivityAsync($"Thank you for your feedback!", cancellationToken: cancellationToken);
        await turnContext.SendActivityAsync($"Reaction: {reaction}", cancellationToken: cancellationToken);
        await turnContext.SendActivityAsync($"Feedback: {feedback}", cancellationToken: cancellationToken);
    }
}
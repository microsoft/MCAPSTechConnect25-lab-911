using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.State;

namespace Custom.Engine.Agent;

internal class MessageHandlers
{
    internal static async Task NewChat(ITurnContext turnContext, TurnState turnState, CancellationToken cancellationToken)
    {
        turnState.DeleteConversationState();
        await turnContext.SendActivityAsync("Conversation history has been cleared and a new conversation has been started.", cancellationToken: cancellationToken);
    }
}

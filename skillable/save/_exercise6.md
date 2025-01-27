## Exercise 6: Feedback

Feedback is a crucial way to understand the quality of the responses that are produced by your agent once you put it in the hands of your end users. Using the Feedback Loop feature in Teams AI library, you can enable controls to collect postive and negative feedback from end users in the response.

Here, you'll create a feedback handler and register it with the application to capture user feedback.

### Step 1: Create Feedback model

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, create a new folder with the name **Models**.
1. In the **Models** folder, create a new file with the name **Feedback.cs** with the following contents:

    ```
    using System.Text.Json.Serialization;

    namespace Custom.Engine.Agent.Models;

    internal class Feedback
    {
        [JsonPropertyName("feedbackText")]
        public string FeedbackText { get; set; }
    }
    ```

### Step 2: Create Feedback handler

1. In the **Custom.Engine.Agent** project, create a new file with the name **FeedbackHandler.cs** with the following contents:

```
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
```
    
### Step 2: Enable Feedback Loop feature

Now, update the agent logic.

1. In the **Custom.Engine.Agent** project, open **Program.cs** file.
1. In the agent code, create a new **AIOptions** object after the **ActionPlanner** object.

    ```
    AIOptions<TurnState> options = new(planner)
    {
        EnableFeedbackLoop = true
    };
    ```

1. Update **Application** object, passing the new options object.

    ```
    Application<TurnState> app = new ApplicationBuilder<TurnState>()
        .WithAIOptions(options)
        .WithStorage(sp.GetService<IStorage>())
        .Build();
    ```

1. After the message handler, register the Feedback Loop handler with the application.

    ```
    app.OnFeedbackLoop(FeedbackHandler.OnFeedback);
    ```
    
1. Save your changes

Your agent code should look like the following:

```
builder.Services.AddTransient<IBot>(sp =>
{
    // Create loggers
    ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>();

    // Create Prompt Manager
    PromptManager prompts = new(new()
    {
        PromptFolder = "./Prompts"
    });

    // Create ActionPlanner
    ActionPlanner<TurnState> planner = new(
        options: new(
            model: sp.GetService<OpenAIModel>(),
            prompts: prompts,
            defaultPrompt: async (context, state, planner) =>
            {
                PromptTemplate template = prompts.GetPrompt("Chat");

                var dataSources = template.Configuration.Completion.AdditionalData["data_sources"];
                var dataSourcesString = JsonSerializer.Serialize(dataSources);

                var replacements = new Dictionary<string, string>
                {
                    { "$azure-search-key$", config.AZURE_SEARCH_KEY },
                    { "$azure-search-index-name$", config.AZURE_SEARCH_INDEX_NAME },
                    { "$azure-search-endpoint$", config.AZURE_SEARCH_ENDPOINT },
                };

                foreach (var replacement in replacements)
                {
                    dataSourcesString = dataSourcesString.Replace(replacement.Key, replacement.Value);
                }

                dataSources = JsonSerializer.Deserialize<JsonElement>(dataSourcesString);
                template.Configuration.Completion.AdditionalData["data_sources"] = dataSources;

                return await Task.FromResult(template);
            }
        )
        { LogRepairs = true },
        loggerFactory: loggerFactory
    );

    AIOptions<TurnState> options = new(planner)
    {
        EnableFeedbackLoop = true
    };

    Application<TurnState> app = new ApplicationBuilder<TurnState>()
        .WithAIOptions(options)
        .WithStorage(sp.GetService<IStorage>())
        .Build();

    app.OnMessage("/new", MessageHandlers.NewChat);

    app.OnFeedbackLoop(FeedbackHandler.OnFeedback);

    return app;
});
```

### Step 3: Run and debug

Now let's test the changes.

Continuing in Visual Studio:

1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar.

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++/new+++ and send the message to clear the conversation history and start a new chat.
1. In the message box, enter +++Can you suggest a candidate who is suitable for spanish speaking role that requires at least 2 years of .NET experience?+++ and send the message. Wait for the response.
1. In the repsonse, select either the thumbs up (👍) or thumbs down (👎) icon. A feedback dialog is displayed.
1. Enter a message into the message box and submit the feedback. Your reaction and feedback text is displayed in a response.
1. Close the browser to stop the debug session.
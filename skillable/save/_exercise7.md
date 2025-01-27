## Exercise 7: Customize agent response

You've seen so far that Teams AI library provides some user interface components automatically, such as the AI generated label and document citations when you integrated Azure OpenAI On Your Data. Suppose you want more granular control over how responses are represented, for example, you want to display additional controls. Teams AI library allows developers to override the **PredictedSAYCommand** action which is responsible for sending the repsonse from the language model to the Teams user interface.

Here, you'll render the language model response in an Adaptive Card. The Adaptive Card displays the languge model text response and includes controls to display additional citation information.

### Step 1: Create Adaptive Card creator class

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, create a file named **ResponseCardCreator.cs** with the following contents:

```
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
```

This class is responsible for creating an Adaptive Card that contains the response from the LLM and document citations.

### Step 2: Create Action handler

Next, create an action handler to override the **PredictedSAYCommand** action.

1. Create a file named **Actions.cs** with the following contents:

```
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
                        { "additionalType", JArray.FromObject(new string[] { "AIGeneratedContent" } ) }
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
}
```

The method is responsible for creating and sending a message activity. If the language model response includes citations, it creates an adaptive card and attaches it to the message. Otherwise, it sends a simple text message. 

An entity is defined in the activity which represents the AI generated label, and channelData is defined which enables the feedback controls. As we are overriding the default handler, we need to provide these in the activity otherwise they will not be displayed.

### Step 3: Register actions

Next, register the action in the agent code.

1. In the **Custom.Engine.Agent** project, open **Program.cs** file.
1. Register the **Actions** class with the application after the feeback loop handler.

    ```
    app.AI.ImportActions(new Actions());
    ```

1. Save your changes.

Your agent code should look like:

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

    app.AI.ImportActions(new Actions());

    return app;
});
```

### Step 4: Run and debug
    
Now let's test the change.

1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar. 

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++/new+++ and send the message to clear the conversation history and start a new chat.
1. In the message box, enter +++Can you suggest a candidate who is suitable for spanish speaking role that requires at least 2 years of .NET experience?+++ and send the message. Wait for the response.

Close the browser to stop the debug session.
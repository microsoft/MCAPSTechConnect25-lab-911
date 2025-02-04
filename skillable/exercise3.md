## Exercise 3: Message handlers

Suppose you want to run some logic when a message that contains a specific phrase or keyword is sent to the agent, a message handler allows you to do that.

Up to this point, every time you send and receive a message the contents of the messages are saved in the agent state. During development the agent state is stored in an emulated Azure Storage account hosted on your machine. You can inspect the agent state using Azure Storage Explorer.

> [!NOTE]
> Message handlers are processed before the ActionPlanner and so take priority for handling the response.
 
Here, you'll create a message handler that will clear the conversation history stored in the agent state when a message that contains **/new** is sent, and respond with a fixed message.

## Step 1: Create message handler

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, in the project root, create a file called **MessageHandlers.cs** with the following contents:

    ```
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
    ```

1. Save your changes.

## Step 2: Register message handler

1. Open **Program.cs**, find the agent code **builder.Services.AddTransient<IBot>(sp => ...)**, add the following code after the **app** declaration inside it:

    ```
    app.OnMessage("/new", MessageHandlers.NewChat);
    ```

1. Save your changes.

The agent code should look like:

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
                return await Task.FromResult(template);
            }
        )
        { LogRepairs = true },
        loggerFactory: loggerFactory
    );

    Application<TurnState> app = new ApplicationBuilder<TurnState>()
        .WithAIOptions(new(planner))
        .WithStorage(sp.GetService<IStorage>())
        .Build();

    app.OnMessage("/new", MessageHandlers.NewChat);

    return app;
});
```

## Step 3: Run and debug

Now let's test the change.

> [!TIP]
> Your debug session from the previous section should still be running, if not start a new debug session.

- In the message box, enter **/new** and send the message. Notice that the message in the response is not from the language model but from the message handler.

Close the browser to stop the debug session.

## Congratulations!


Great job! You have now completed the lab and you're ready to start building custom agents in pro code to tackle your business processes!

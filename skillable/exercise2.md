### Step 3: Run and debug

With everything in place, we are now ready to test our custom engine agent in Microsoft Teams for the first time.

First, we need to start a debug session to start our local web API that contains the agent logic.

Continue in Visual Studio:

1. To start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar. A browser window is launched and navigates to Microsoft Teams.
1. In the browser, sign in to Microsoft 365 using your Microsoft 365 account details.
    - **Username: +++@lab.CloudPortalCredential(User1).Username+++**
    - **Password: +++@lab.CloudPortalCredential(User1).Password+++**

> [!IMPORTANT]
> The first time you debug Teams the app install dialog will not appear, instead a Welcome to Teams dialog is shown instead. To install the app for the first time, you will need to steop and create a new debug session.

1. Close the browser to stop the debug session.
1. To start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar. A browser window is launched and navigates to Microsoft Teams.
1. Wait for Microsoft Teams to load and for the App install dialog to appear.

Previously, Teams Toolkit registered the app in the Teams Developer Portal. To use the app we need to install it for the current user. Teams Toolkit launches the browser using a special URL which enables developers to install the app before they test it.

> [!NOTE]
> If any changes are made to the app manifest file. Developers will need to run the Prepare Teams App dependencies process again and install the app for the changes to be reflected in Microsoft Teams.

Continuing in the web browser:

1. In the App install dialog, select **Add**.
1. In the App install confirmation dialog, select **Open**. The custom engine agent is displayed in Microsoft Teams.

Now let's test that everything is working as expected.

Continuing in the web browser:

1. Enter +++Hello, world!+++ in the message box and press <kbd>Enter</kbd> to send the message to the agent. A typing indicator appears whilst waiting for the agent to respond.
1. Notice the natural language response from the agent and a label **AI generated** is shown in the agent response.
1. Continue a conversation with the agent.
1. Go back to Visual Studio. Notice that in the Debug pane, Teams AI library is tracking the full conversation and displays appended conversation history in the output.
1. Close the browser to stop the debug session.

### Step 4: Examine agent configuration

The functionality of our agent is implemented using Teams AI library. Let's take a look at how our agent is configured.

In Visual Studio:

1. In the **Custom.Engine.Agent** project, open **Program.cs** file.
1. Examine the contents of the file.

The file sets up the web application and integrates it with Microsoft Bot Framework and services.

- **WebApplicationBuilder**: Initializes web application with controllers and HTTP client services.
- **Configuration**: Retrieve configuration options from the apps configration and sets up Bot Framework authentication.
- **Dependency injection**: Registers BotFrameworkAuthentication and TeamsAdapter services. Configures Azure Blob Storage for persisting agent state and sets up an Azure OpenAI model service.
- **Agent setup**: Registers the agent as a transient service. The agent logic is implemented using Teams AI library.

Let's take a look at the agent setup.

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

    return app;
});
```

The key elements of the agent setup are:

- **ILoggerFactory**: Used for logging messages to the output pane for debugging.
- **PromptManager**: Determines the location of prompt templates.
- **ActionPlanner**: Determines which model and prompt should be used when handling a user message. By default, the planner uses a prompt template named **Chat**.
- **ApplicationBuilder**: Creates an object which represents a Bot that can handle incoming activities.

The agent is added as a transient service which means that everytime a message is recieved from the Bot Framework, our agent code will be executed.
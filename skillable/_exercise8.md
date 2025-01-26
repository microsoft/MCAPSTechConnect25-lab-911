## Exercise 8: Sensitivity information

Not all company data should be shared outside your organsation, some data can be sensitive. As you noted in the previous section, you defined a label in the activity Entities collection which displayed the AI generated label in the response. 

Here, you'll update the entity properties to display a new label to inform users that the information provided may be sensitive and whether or not, and whether it can be shared outside of your organization.

### Step 1: Add sensitivity label

Continuing in Visual Studio: 

1. In the **Custom.Engine.Agent** project, open **Actions.cs** file.
1. Update the **Properties** collection of the activity Entity with a new property name **usageInfo**.

    ```
    { "usageInfo", JObject.FromObject(
        new JObject(){
            { "@type", "CreativeWork" },
            { "name", "Confidential" },
            { "description", "Sensitive information, do not share outside of your organization." },
        })
    }
    ```
1. Save your changes.

The Entities collection should look like:

```
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
```

### Step 2: Run and debug

Now let's test the change.

1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar. 

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++/new+++ and send the message to clear the conversation history and start a new chat.
1. In the message box, enter +++Can you suggest a candidate who is suitable for spanish speaking role that requires at least 2 years of .NET experience?+++ and send the message. Wait for the response.

Note that next to the AI Generated label is a new shield icon. Hover over the icon to view the sensitivity information that was provided in the entity properties.

Close your browser to stop the debug session.

## Exercise 9: Content Safety Moderation

Azure AI Content Safety is an AI service that detects harmful user-generated and AI-generated content in applications and services.

Here, you'll register the Azure Safety Content Moderator to moderate both inputs and output, and add actions to provide custom messages when the content safety measures are triggered.

### Step 1: Add flagged input and output action handlers

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, open **Actions.cs** file and add the following code to the Actions class.

    ```
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
    ```

1. Save your changes.

### Step 2: Configure Azure Content Safety environment variables

To save time we have already provisioned an Azure Content Safety resource in Azure for you to use in this lab. 

First, let's create some environment variables to store details that we will need to integrate with the service.

Continuing in Visual Studio:

1. In the **TeamApp** project, expand the **env** folder.
1. Open the **.env.local** file and add the following:
  
    ```
    AZURE_CONTENT_SAFETY_ENDPOINT=https://acs-ignite-2024-labs.cognitiveservices.azure.com/
    ```

1. Save your changes.

In a web browser:

1. In the address bar, type +++https://gist.github.com/garrytrinder/0da49ec4ba50b023e5b75a1c14fa1f22+++ and navigate to a GitHub gist containing environment variables.
1. Copy the value of the **SECRET_AZURE_CONTENT_SAFETY_KEY** variable to your clipboard.

Continuing in Visual Studio:

1. In the **TeamApp** project, expand the **env** folder.
1. Open the **.env.local.user** file.
1. Add the **SECRET_AZURE_CONTENT_SAFETY_KEY** variable, replacing [INSERT KEY] with the value stored in your clipboard.

    ```
    SECRET_AZURE_CONTENT_SAFETY_KEY=[INSERT KEY]
    ```

1. Save your changes.

Next, let's make sure that these value are written to the **appsettings.development.json** file so we can access them at runtime in our agent code.

1. In the **Custom.Engine.Agent** project, open **teamsapp.local.yml** file.
1. Add the following properties to the **file/createOrUpdateJsonFile** action:

    ```
    AZURE_CONTENT_SAFETY_KEY: ${{SECRET_AZURE_CONTENT_SAFETY_KEY}}
    AZURE_CONTENT_SAFETY_ENDPOINT: ${{AZURE_CONTENT_SAFETY_ENDPOINT}}
    ```

1. Save your changes.

The **file/createOrUpdateJsonFile** action should look like:

```yaml
- uses: file/createOrUpdateJsonFile
    with:
    target: ../Custom.Engine.Agent/appsettings.Development.json
    content:
        BOT_ID: ${{BOT_ID}}
        BOT_PASSWORD: ${{SECRET_BOT_PASSWORD}}
        AZURE_OPENAI_DEPLOYMENT_NAME: ${{AZURE_OPENAI_DEPLOYMENT_NAME}}
        AZURE_OPENAI_KEY: ${{SECRET_AZURE_OPENAI_API_KEY}}
        AZURE_OPENAI_ENDPOINT: ${{AZURE_OPENAI_ENDPOINT}}
        AZURE_STORAGE_CONNECTION_STRING: UseDevelopmentStorage=true
        AZURE_STORAGE_BLOB_CONTAINER_NAME: state
        AZURE_SEARCH_ENDPOINT: ${{AZURE_SEARCH_ENDPOINT}}
        AZURE_SEARCH_INDEX_NAME: ${{AZURE_SEARCH_INDEX_NAME}}
        AZURE_SEARCH_KEY: ${{SECRET_AZURE_SEARCH_KEY}}
        AZURE_CONTENT_SAFETY_KEY: ${{SECRET_AZURE_CONTENT_SAFETY_KEY}}
        AZURE_CONTENT_SAFETY_ENDPOINT: ${{AZURE_CONTENT_SAFETY_ENDPOINT}}
```

Now, extend the **ConfigOptions** model so we can easily access the new environment variable values in code.

1. Open **Config.cs**, update the **ConfigOptions** class with the following properties:

    ```
    public string AZURE_CONTENT_SAFETY_KEY { get; set; }
    public string AZURE_CONTENT_SAFETY_ENDPOINT { get; set; }
    ```

1. Save your changes.

The **ConfigOptions** class should look like:

```
public class ConfigOptions
{
    public string BOT_ID { get; set; }
    public string BOT_PASSWORD { get; set; }
    public string AZURE_OPENAI_KEY { get; set; }
    public string AZURE_OPENAI_ENDPOINT { get; set; }
    public string AZURE_OPENAI_DEPLOYMENT_NAME { get; set; }
    public string AZURE_STORAGE_CONNECTION_STRING { get; set; }
    public string AZURE_STORAGE_BLOB_CONTAINER_NAME { get; set; }
    public string AZURE_SEARCH_ENDPOINT { get; set; }                  
    public string AZURE_SEARCH_INDEX_NAME { get; set; }                
    public string AZURE_SEARCH_KEY { get; set; }
    public string AZURE_CONTENT_SAFETY_KEY { get; set; }
    public string AZURE_CONTENT_SAFETY_ENDPOINT { get; set; }
}
```

### Step 3: Register Azure Content Safety Moderator service

Now, register the Azure Content Safety moderator.

1. Open **Program.cs**.
1. Before the agent logic, register **AzureContentSafetyModerator** as a service.

    ```
    builder.Services.AddSingleton<IModerator<TurnState>>(sp =>
        new AzureContentSafetyModerator<TurnState>(new(
            config.AZURE_CONTENT_SAFETY_KEY,
            config.AZURE_CONTENT_SAFETY_ENDPOINT,
            ModerationType.Both
        ))
    );
    ```

1. In the bot logic, update the **AIOptions** object to register the safety moderator with the application.

    ```
    AIOptions<TurnState> options = new(planner)
    {
        EnableFeedbackLoop = true,
        Moderator = sp.GetService<IModerator<TurnState>>()
    };
    ```

1. Save your changes.

The code to register the content safety moderator and the agent logic in **Program.cs** should look like:

```
builder.Services.AddSingleton<IModerator<TurnState>>(sp =>
    new AzureContentSafetyModerator<TurnState>(new(
        config.AZURE_CONTENT_SAFETY_KEY,
        config.AZURE_CONTENT_SAFETY_ENDPOINT,
        ModerationType.Both
    ))
);

// Create the bot as transient. In this case the ASP Controller is expecting an IBot.
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
        EnableFeedbackLoop = true,
        Moderator = sp.GetService<IModerator<TurnState>>()
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

### Step 4: Test flagged input action

Now, let's test the change.

Continuing in Visual Studio:

1. Right-click **TeamsApp** project, expand the **Teams Toolkit** menu and select **Prepare Teams App Dependencies**.
1. Confirm the prompts and wait till the process completes.
1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar.

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++/new+++ and send the message to clear the conversation history and start a new chat.
1. In the message box, enter +++Physical punishment is a way to correct bad behavior and doesn’t cause harm to children.+++ and send the message. Wait for the response.

Notice that the agent response is from the flagged input action as the content of the message triggers the content safety policy. The response contains a payload that is sent from the Azure Content Safety service with details of why the message was flagged.

### CONGRATULATIONS! YOU HAVE COMPLETED LAB 911!
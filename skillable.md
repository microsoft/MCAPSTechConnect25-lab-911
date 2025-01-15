@lab.Title

Login to your VM with the following credentials...

**Username: ++@lab.VirtualMachine(Win11-Pro-Base-VM).Username++**

**Password: +++@lab.VirtualMachine(Win11-Pro-Base-VM).Password+++** 

Use this account to log into Microsoft 365:

**Username: +++@lab.CloudPortalCredential(User1).Username+++**

**Password: +++@lab.CloudPortalCredential(User1).Password+++**

<br>

---

# LAB 446 - Build Custom Engine Agents for Microsoft 365 Copilot

# Create a custom engine agent

Custom engine agents are chatbots for Microsoft Teams powered by generative AI, designed to provide sophisticated conversational experiences. Custom engine agents are built using the Teams AI library, which provides comprehensive AI functionalities, including managing prompts, actions, and model integration as well as extensive options for UI customization. This ensures that your chatbots leverage the full range of AI capabilities while delivering a seamless and engaging experience aligned with Microsoft platforms.

## What will we be doing?

Here, you create a custom engine agent that uses a language model hosted in Azure to answer questions using natural language:

- **Create**: Create a custom agent agent project and use Teams Toolkit in Visual Studio.
- **Provision**: Upload your custom engine agent to Microsoft Teams and validate the results.
- **Prompt template**: Determine the agent behaviour.
- **Suggested prompts**: Define prompts for starting new conversations.
- **Message handlers**: Run logic when recieving a specific keyword or phrase.
- **Chat with your data**: Integrate with Azure AI Search to implement Retrieval Augmentation Generation (RAG).
- **Feedback**: Collect feedback from end users.
- **Customize responses**: Customize the agent response.
- **Sensitive information**: Display sensitivity guidance to end users.
- **Content moderation**: Integrate Azure Content Safety service to detect harmful user-generated and AI-generated content.

## Exercise 1: Setup project in Visual Studio 2022

### Step 1: Open starter project

Start with opening the starter project in Visual Studio 2022.

1. Open **Visual Studio 2022**.
1. In the Visual Studio 2022 welcome dialog, select **Continue without code**.
1. Open the **File** menu, expand the **Open** menu and select **Project/solution...**.
1. In the **Open Project/Solution** file picker, on the left hand menu, select **This PC**.
1. Double click **Local Disk (C:)**.
1. Navigate to **C:\Users\LabUser\LAB-466-BEGIN** folder.
1. In the **LAB-466-BEGIN** folder, select **Custom.Engine.Agent.sln**, then select **Open**.

### Step 2: Examine the solution

The solution contains two projects:

- **Custom.Engine.Agent**: This is an ASP.NET Core Web API project which contains your agent code. The agent logic and generative AI capatbilies are implemented using Teams AI library. 
- **TeamsApp**: This is a Teams Toolkit project which contains the app package files, environment, workflow and infrastructure files. You will use this project to provision the required resources for your agent.

### Step 3: Create dev tunnel

Dev tunnels allow developers to securely share local web services across the internet. When users interact with the agent in Microsoft Teams, the Teams platform will send and recieve messages (called Activities) from your agent code via the Bot Framework. As the code is running on your local machine, the Dev Tunnel exposes the localhost domain which your web api runs on as a publicly accessible URL.

Continue in Visual Studio:

1. Open the **View** menu, expand **Other windows**, and select **Dev Tunnels**.
1. In the **Dev Tunnels** pane, select the **plus (+)** icon.
1. In the dialog window, create the tunnel using the following settings:
    1. **Account**: Expand the dropdown and select **Sign in**, then select **Work or school account**, then again and select **OK**. Use the Microsoft 365 account details to sign in. In the **Stay signed in to all your apps** dialog, select **No, sign in to this app only**.
        - **Username: +++@lab.CloudPortalCredential(User1).Username+++**
        - **Password: +++@lab.CloudPortalCredential(User1).Password+++**
    1. **Name**: +++custom-engine-agent+++
    1. **Tunnel type**: Temporary
    1. **Access**: Public
1. To create the tunnel, select **OK**.
1. In the confirmation prompt, select **OK**.
1. Close the Dev Tunnels window.

### Step 4: Configure Azure OpenAI key

To save time we have already provisioned a language model in Azure for you to use in this lab. Teams Toolkit uses environment (.env) files to store values centrally that can be used across your application.

In a web browser:

1. In the address bar, type +++https://gist.github.com/garrytrinder/0da49ec4ba50b023e5b75a1c14fa1f22+++ and navigate to a GitHub gist containing environment variables.
1. Copy the value of the **SECRET_AZURE_OPENAI_API_KEY** variable to your clipboard.

Continue in Visual Studio:

1. In the **TeamsApp** project, expand the **env** folder.
1. Rename **.env.local.user.sample** to **.env.local.user**.
1. Open **.env.local.user** file.

1. Update the contents of the file, replacing [INSERT KEY] with the value stored in your clipboard.

    ```
    SECRET_AZURE_OPENAI_API_KEY=[INSERT KEY]
    ```

1. Save the changes.

> [!NOTE]
> When Teams Toolkit uses an environment variable with that is prefixed with **SECRET**, it will ensure that the value does not appear in any logs. 

## Exercise 2: Provision resources

Teams Toolkit help developers automate tasks using workflow files. The workflow files are YML files which are stored in the root of the TeamsApp project. 

### Step 1: Review Teams Toolkit provisioning tasks

Continue in Visual Studio:

1. In **TeamsApp** project, open **teamsapp.local.yml**.
1. Examine the contents of the file.

The file contains a single stage called **Provision** which contains several tasks.

1. **teamsApp/create**: Registers an app in Teams Developer Portal and writes the app ID to **env\.env.local**.
1. **aadApp/create**: Registers an app in Microsoft Entra and writes several values to **env\.env.local**.
1. **aadApp/update**: Applies an app manifest to the Microsoft Entra app registration.
1. **arm/deploy**: Provisions the Azure Bot Service using Bicep. It writes several values back to **env\.env.local**.
1. **file/createOrUpdateJsonFile**: Updates **appsettings.development.json** file with environment variables which can be used by code at runtime.
1. **teamsApp/validateManifest**: Validates the app manifest file.
1. **teamsApp/zipAppPackage**: Creates the Teams app package.
1. **teamsApp/validateAppPackage**: Validates the app package.
1. **teamsApp/update**: Updates the app registration in the Teams Developer Portal.

### Step 2: Use Teams Toolkit to execute the tasks in the workflow file

1. Right-click **TeamsApp** project.
1. Expand the **Teams Toolkit** menu and select **Prepare Teams App Dependencies**.
1. In the **Microsoft 365 account** dialog, select the account you used to create the Dev Tunnel earlier and select **Continue**. This will start the Dev Tunnel and write the tunnel endpoint and domain to the **env\env.local** file.
1. In the **Provision** dialog, configure the resource group to be used to host the Azure Bot Service:
    1. **Subscription**: Expand the dropdown and select the subscription in the list
    1. **Resource group**: Select **New...**, enter +++rg-custom-engine-agent-local+++ the text field and then select **OK**.
    1. **Region**: Expand the dropdown and select **West US** in the list
    1. Select **Provision**
1. In the warning prompt, select **Provision**.
1. Wait for the process to complete, this can take a 1-3 minutes. Teams Toolkit will output its progress in the Output pane.
1. In the **Info** prompt, select **View provisioned resources** to open a browser.

Take a minute to examine the Azure Bot Service resource in the Azure Portal.

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

## Exercise 3: Prompt templates

Prompts play a crucial role in communicating and directing the behavior of language models.

Prompts are stored in the Prompts folder. A prompt is defined as a subfolder that contains two files:

 - **config.json**: The prompt configuration. This enables you to control parameters such as temperature, max tokens etc. that are passed to the language model.
 - **skprompt.txt**: The prompt text template. This text determines the behaviour of the agent.

Here, you'll update the default prompt to change the agents behaviour.

### Step 1: Update prompt template

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, expand the **Prompt** folder.
1. In the **Chat** folder, open the **skprompt.txt** file. 
1. Update the contents of the file:

    ```
    You are a career specialist named "Career Genie" that helps Human Resources team for writing job posts.
    You are friendly and professional.
    You always greet users with excitement and introduce yourself first.
    You like using emojis where appropriate.
    ```

1. Save changes.

### Step 2: Test the new prompt

Now let's test our change.

1. To start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar.

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++Hi+++ and send the message. Wait for the response. Notice the change in the response.
1. In the message box, enter +++Can you help me write a job post for a Senior Developer role?+++ and send the message. Wait for the response.

Continue the conversation by sending more messages.

- +++What would be the list of required skills for a Project Manager role?+++
- +++Can you share a job template?+++

Close the browser to stop the debug session.

## Exercise 3: Suggested prompts

Suggested prompts are shown in the user interface and a good way for users to discover how the agent can help them through examples.

Here, you'll define two suggested prompts.

### Step 1: Update app manifest

Continuing in Visual Studio:

1. In the **TeamsApp** project, expand the **appPackage** folder.
1. In the **appPackage** folder, open the **manifest.json** file.
1. In the **bots** array property, expand the first object with a **commandLists** array property.

    ```
    "commandLists": [
        {
            "scopes": [
                "personal"
            ],
            "commands": [
                {
                    "title": "Write a job post for <role>",
                    "description": "Generate a job posting for a specific role"
                },
                {
                    "title": "Skill required for <role>",
                    "description": "Identify skills required for a specific role"
                }
            ]
        }
    ]
    ```
1. Save your changes.

The **bots** array property should look like:

```
"bots": [
  {
    "botId": "${{BOT_ID}}",
    "scopes": [
      "personal"
    ],
    "supportsFiles": false,
    "isNotificationOnly": false,
    "commandLists": [
      {
        "scopes": [
          "personal"
        ],
        "commands": [
          {
            "title": "Write a job post for <role>",
            "description": "Generate a job posting for a specific role"
          },
          {
            "title": "Skill required for <role>",
            "description": "Identify skills required for a specific role"
          }
        ]
      }
    ]
  }
],
```

### Step 2: Test suggested prompts

As you've made a change to the app manifest file, we need to Run the Prepare Teams App Dependencies process to update the app registration in the Teams Developer Portal before starting a debug session to test it.

Continuing in Visual Studio:

1. Right-click **TeamsApp** project, expand the **Teams Toolkit** menu and select **Prepare Teams App Dependencies**.
1. Confirm the prompts and wait till the process completes.

Now let's test the change.

1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar.

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. Above the message box, select **View prompts** to open the prompt suggestions flyout.
1. In the **Prompts** dialog, select one of the prompts. The text is added into the message box.
1. In the message box, replace <b>&lt;role&gt;</b> with a job title, for example, +++Senior Software Engineer+++, and send the message.

The prompt suggestions can also be seen when the user opens the agent for the first time.

Continuing in the web browser:

1. In the Microsoft Teams side bar, go to **Chat**.
1. Find the chat with the name **Custom engine agent** in the list and select the **...** menu.
1. Select **Delete** and confirm the action.
1. In the Microsoft Teams side bar, select **...** to open the apps flyout.
1. Select **Custom engine agent** to start a new chat. The two suggested prompts are shown in the user interface.

Close the browser to stop the debug session.

## Exercise 4: Message handlers

Suppose you want to run some logic when a message that contains a specific phrase or keyword is sent to the agent, a message handler allows you to do that.

Up to this point, every time you send and recieve a message the contents of the messages are saved in the agent state. During development the agent state is stored in an emulated Azure Storage account hosted on your machine. You can inspect the agent state using Azure Storage Explorer.

> [!NOTE]
> Message handlers are processed before the ActionPlanner and so take priority for handling the response.
 
Here, you'll create a message handler that will clear the conversation history stored in the agent state when a message that contains **/new** is sent, and respond with a fixed message.

## Step 1: Create message handler

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, create a file called **MessageHandlers.cs** with the following contents:

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

1. Open **Program.cs**, in the agent code, add the following code after the **app** declaration :

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

## Exercise 5: Retrieval Augmentation Generation (RAG)

Retrieval Augmentation Generation (RAG) is a technique used to improve the accuracy and relevance of responses generated by language models. Suppose you have a collection of documents that you want the language model to reason over and use in it's responses. RAG enables you to provide extra knowledge and context beyond the data that the language model is trained on.

Azure OpenAI On Your Data enables you to run language models on your own enterprise data without needing to train or fine-tune models. You can specify sources to support the responses based on the latest information available in your designated data sources.

Here you'll implement RAG using Azure Open On Your Data to enable the language model to reason over resumes and provide candidate recommendations.

Like we did with the language model, we've already provisioned and configured the services in Azure for you to use.

We provisioned and configured the following resources:

- **Azure Storage Account**: Stores files uploaded through the Azure OpenAI On Your Data file upload feature.
- **Embeddings model**: Generates numerical representations (embeddings) of document contents for use with language models during the file upload process.
- **Azure AI Search**: Hosts the search index of our documents. Contains the document embeddings and additional metadata, such as file paths and timestamps.

### Step 1: Configure Azure AI Search environment variables

First, let's create some environment varibles to store details that we will need to integrate Azure AI Search.

Continuing in Visual Studio:

1. In the **TeamApp** project, expand the **env** folder.
1. Open the **.env.local** file and add the following environment variables:
  
    ```
    AZURE_SEARCH_ENDPOINT=https://aais-ignite-2024-labs.search.windows.net
    AZURE_SEARCH_INDEX_NAME=documents
    ```

1. Save your changes.

In a web browser:

1. In the address bar, type +++https://gist.github.com/garrytrinder/0da49ec4ba50b023e5b75a1c14fa1f22+++ and navigate to a GitHub gist containing environment variables.
1. Copy the value of the **SECRET_AZURE_SEARCH_KEY** variable to your clipboard.

Continue in Visual Studio:

1. In the **TeamApp** project, expand the **env** folder.
1. Open the **.env.local.user** file and add a new environment variable, replacing [INSERT KEY] with the value stored in your clipboard.

    ```
    SECRET_AZURE_SEARCH_KEY=[INSERT KEY]
    ```

1. Save your changes.

Next, let's make sure that these value are written to the **appsettings.development.json** file so we can access them at runtime in our agent code.

1. In the **Custom.Engine.Agent** project, open **teamsapp.local.yml** file.
1. Update the **file/createOrUpdateJsonFile** action with the new environment variables:

    ```
    AZURE_SEARCH_ENDPOINT: ${{AZURE_SEARCH_ENDPOINT}}
    AZURE_SEARCH_INDEX_NAME: ${{AZURE_SEARCH_INDEX_NAME}}
    AZURE_SEARCH_KEY: ${{SECRET_AZURE_SEARCH_KEY}}
    ```

> [!IMPORTANT]
> YAML is strict when it comes to indendation, make sure you indent the YAML after pasting by using tabs to remove the errors in the editor.

1. Save your changes.

The **file/createOrUpdateJsonFile** should look like:

```
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
```

Now, extend the model so we can easily access the new environment variable values in code.

1. Open **Config.cs**, update the **ConfigOptions** class, add the following properties:

    ```
    public string AZURE_SEARCH_ENDPOINT { get; set; }
    public string AZURE_SEARCH_INDEX_NAME { get; set; }
    public string AZURE_SEARCH_KEY { get; set; }
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
}
```

### Step 2: Integrate Azure AI Search in prompt template configration

Update the prompt template configuration file to integrate Azure OpenAI On Your Data data source.

Continuing in Visual Studio:

1. In the **Custom.Engine.Agent** project, expand the **Prompts** folder.
1. Expand the **Chat** folder and open the **config.json** file. Replace the file contents with the following code:

    ```
    {
      "schema": 1.1,
      "description": "Custom engine agent",
      "type": "completion",
      "completion": {
        "model": "gpt-4",
        "completion_type": "chat",
        "include_history": true,
        "include_input": true,
        "max_input_tokens": 100,
        "max_tokens": 1000,
        "temperature": 0.1,
        "top_p": 1,
        "presence_penalty": 0,
        "frequency_penalty": 0,
        "data_sources": [
          {
            "type": "azure_search",
            "parameters": {
              "endpoint": "$azure-search-endpoint$",
              "index_name": "$azure-search-index-name$",
              "authentication": {
                "type": "api_key",
                "key": "$azure-search-key$"
              }
            }
          }
        ]
      }
    }
    ```

1. Save your changes.

Let's examine some of the key changes made to the configuration. 

The properties **temperature** and **top_p** control the creativity, or randomness of language model outputs. To take a simple view, language models work by selecting the next probable word (token).

- **temperature**: Has been lowered to **0.1**. Instructs the model to be more deterministic, choosing only the most probable tokens in it's reasoning.
- **top_p**: Has been increased to **1**. Instructs the model to choose from the widest pool of words (tokens) possible in it's reasoning.

These changes optimise the language model for use in scenarios where precision and unambiguity is critical. You should always consider adjusting these parameters to be appropriate for your use case.

Azure OpenAI On Your Data integration is defined in the **data_sources** array. Here we define our configuration providing the minimum required information needed to use the retrieve documents from the Azure AI Search index in the model's reasoning process.

Notice that we use placeholders as values for some properties, for example **$azure-search-key$**, in the configuration file. These placeholders will be updated dynamically in code to ensure that we don't store senstitive information in plain text.

### Step 3: Replace prompt template configuration placeholders with values

Continuing in Visual Studio:

1. Open **Program.cs** file.
1. Replace the contents of the **defaultPrompt** function to dynamically replace the placeholders in the prompt template configuration with the values we stored in our environment variable files earlier.

    ```
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
    ```

1. Save your changes.

The **ActionPlanner** object should look like:

```
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
```

The **defaultPrompt** anonymous function provides a way to dyanmically alter the behaviour of our agent. This is where you can include logic to choose different prompt templates for different functions or behaviours that you want the agent to provide. Suppose you want to dynamically adjust the temperature or choose a different prompt template based in a specific input. Here is where you would add the logic to make those changes on the fly.

### Step 4: Update prompt template

Now, update the prompt text to reflect the change in agent behaviour.

1. In the **Custom.Engine.Agent** project, expand the **Prompts** folder, then expand the **chat** folder.
1. Open the **skprompt.txt** file, then replace the contents with the following:

    ```
    You are a career specialist named "Career Genie" that helps Human Resources team for finding the right candidate for the jobs. 
    You are friendly and professional.
    You always greet users with excitement and introduce yourself first.
    You like using emojis where appropriate.
    Always mention all citations in your content.
    ```

### Step 5: Run and debug

As we've made a change to the app manifest file, we need to Run the Prepare Teams App Dependencies process to update the app registration in the Teams Developer Portal before starting a debug session to test it.

Continuing in Visual Studio:

1. Right-click **TeamsApp** project, expand the **Teams Toolkit** menu and select **Prepare Teams App Dependencies**.
1. Confirm the prompts and wait till the process completes.

Now let's test the change.

1. Start a debug session, press <kbd>F5</kbd> on your keyboard, or select the **Start** button in the toolbar. 

Continuing in the web browser:

1. In the app dialog, select **Open** to open the agent in Microsoft Teams.
1. In the message box, enter +++Can you suggest a candidate who is suitable for spanish speaking role that requires at least 2 years of .NET experience?+++ and send the message. Wait for the response.

Note that the response contains a reference to a document. The document was used by the language model in it's reasoning when generating an answer and provided as part of the answer. Hover over the reference in the response to view more information about the document.

Try out some more prompts and review the outputs, for example:

- +++Who would be suitable for a position that requires 5+ python development experience?+++

Close the browser to stop the debug session.

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

### CONGRATULATIONS! YOU HAVE COMPLETED LAB 446!

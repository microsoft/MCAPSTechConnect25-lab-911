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
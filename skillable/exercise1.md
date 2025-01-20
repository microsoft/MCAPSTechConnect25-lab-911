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
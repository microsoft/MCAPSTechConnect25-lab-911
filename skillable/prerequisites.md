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
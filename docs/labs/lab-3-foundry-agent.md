# Lab 3: Run the Foundry agent

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 2, Expose the API as MCP](lab-2-expose-mcp.md) |
[Next: Lab 4, Add the Copilot Studio path](lab-4-copilot-studio.md)

## What you are building

In this lab, you run a .NET 10 hosted-agent application that connects a Foundry
model to the MCP tools exposed through APIM in
[Lab 2](lab-2-expose-mcp.md). The application is an ASP.NET Core process that
exposes a Responses protocol endpoint at
`http://localhost:8088/responses`.

The application does not host the language model and does not convert the REST
API into MCP. Foundry provides the model, and APIM already provides the MCP
server. The .NET application joins those components by:

- Receiving a user request through the Responses endpoint.
- Sending the conversation and agent instructions to the selected Foundry model.
- Advertising only the MCP tools allowed by the active environment profile.
- Applying different approval behavior to read and state-changing tools.
- Returning the model's final, tool-grounded answer to the caller.

The default profile uses the synthetic work-request tools:

| Tool | Purpose | Approval |
|---|---|---|
| `getAWorkRequest` | Read a synthetic work request | Automatic |
| `createAWorkRequest` | Create a synthetic work request | Required |
| `updateWorkRequestStatus` | Change synthetic work-request status | Required |

The approval boundary is configured in the agent, while APIM remains the
gateway for tool exposure, policies, and observability. Production systems
would also enforce authorization and business rules at the gateway and backend.

## How the request flows

```text
PowerShell or Agent Inspector
    -> local .NET Responses endpoint
    -> Foundry model deployment
    -> APIM-hosted MCP server
    -> synthetic REST operation and mock response
    -> Foundry model composes the final answer
    -> caller receives the Responses result
```

For the sample prompt, the model determines that it needs work-request data,
selects `getAWorkRequest`, calls it through the APIM MCP endpoint, and uses the
returned synthetic record to produce its recommendation. The model should not
invent the work-request details because the agent instructions require tool
data when the answer depends on it.

The source code is profile-driven. Environment variables select the MCP
endpoint, allowed tools, approval boundaries, agent name, and description.
This is why the same compiled application can use either the work-request
profile or the optional Petstore profile without a code change.

Running `dotnet run` creates a temporary local agent host. It does not create a
saved agent in the Foundry UI. Lab 3C optionally packages the same source and
creates an immutable Foundry agent version.

## Lab 3A: Configure and test the agent locally

1. Copy the environment template:

   ```powershell
   Copy-Item .\src\Workshop.Agent\.env.example .\src\Workshop.Agent\.env
   ```

2. Set:

   - `FOUNDRY_PROJECT_ENDPOINT`
   - `AZURE_AI_MODEL_DEPLOYMENT_NAME`
   - `MCP_SERVER_ENDPOINT`
   - `MCP_SERVER_NAME`
   - `MCP_AUTO_APPROVED_TOOLS`
   - `MCP_APPROVAL_REQUIRED_TOOLS`
   - `AGENT_NAME`
   - `AGENT_DESCRIPTION`
   - `AZURE_TOKEN_CREDENTIALS=AzureCliCredential`

   The tool lists are comma-separated APIM-generated MCP tool names. A tool
   must appear in only one list. The agent name and description identify the
   selected profile. The final setting makes local development use the identity
   from `az login` instead of probing the Azure managed identity endpoint. Keep
   this setting in the local `.env` file only. Do not add it to `azure.yaml`,
   because a deployed hosted agent should use its Azure identity.

3. Authenticate:

   ```powershell
   az login
   ```

4. Run the agent:

   ```powershell
   dotnet run --project .\src\Workshop.Agent\Workshop.Agent.csproj
   ```

5. Test the Responses endpoint with PowerShell or Agent Inspector.

   **Option A: PowerShell**

   ```powershell
   $body = @{
     input = "Review work request WR-1001 and recommend the next action."
     stream = $false
   } | ConvertTo-Json

   $response = Invoke-RestMethod `
     -Method Post `
     -Uri "http://localhost:8088/responses" `
     -ContentType "application/json" `
     -Body $body

   $response.output |
     Where-Object type -eq "message" |
     ForEach-Object { $_.content } |
     Where-Object type -eq "output_text" |
     Select-Object -ExpandProperty text
   ```

   `Invoke-RestMethod` converts the JSON response into nested PowerShell
   objects. The final pipeline extracts and prints the assistant's readable
   text instead of displaying `content=System.Object[]`.

   **Option B: Foundry Agent Inspector**

   Agent Inspector provides a visual view of the complete agent run, including
   the response, streaming events, MCP tool calls, timing, and run timeline.
   It connects to the agent process but does not start it.

   - Keep the terminal from step 4 running.
   - In Visual Studio Code, press `Ctrl+Shift+P`.
   - Run **Foundry Toolkit: Open Agent Inspector**.
   - Select the **Responses** protocol.
   - Connect to `http://localhost:8088`. If the connection form requires the
     full endpoint, use `http://localhost:8088/responses`.
   - Send this prompt:

     ```text
     Review work request WR-1001 and recommend the next action.
     ```

   If port 8088 is already in use, stop the local agent and start it on another
   port:

   ```powershell
   $env:PORT = "8090"
   dotnet run --project .\src\Workshop.Agent\Workshop.Agent.csproj
   ```

   Then connect Agent Inspector to `http://localhost:8090`.

If the response reports `ManagedIdentityCredential authentication failed` and
references `169.254.169.254`, confirm the local `.env` contains
`AZURE_TOKEN_CREDENTIALS=AzureCliCredential`, stop the running agent, and start
it again.

## Checkpoint

The agent should select `getAWorkRequest`, inspect the synthetic response, and
return a grounded recommendation. Read operations run without an approval
round-trip. Create and status-update operations still require approval.

## Lab 3B: Switch to the Petstore MCP profile (optional)

The agent code is configuration-driven and does not require a Petstore-specific
code change.

1. Stop the local agent.
2. Replace the local environment file:

   ```powershell
   Copy-Item `
     .\src\Workshop.Agent\.env.petstore.example `
     .\src\Workshop.Agent\.env `
     -Force
   ```

3. Set the Foundry project endpoint, model deployment, and Petstore MCP endpoint.
4. Start the agent again.
5. Repeat the Responses request with this input:

   ```text
   Find available pets. Summarize the count and show the ID, name, and status.
   ```

The Petstore template enables `findPetById` and `findsPetsByStatus` as
auto-approved read-only tools and leaves the approval-required list empty.
`MCP_SERVER_ENDPOINT` configures local execution.
`PETSTORE_MCP_SERVER_ENDPOINT` supplies the same endpoint to the optional
Petstore hosted-agent service.

## Lab 3C: Deploy the agent to Foundry Agent Service (optional)

The local `dotnet run` process is temporary and does not create an agent entry
in the Foundry UI. This extension performs a real deployment of the .NET
application as a hosted agent. It is optional in the workshop because it
requires additional time and the **Foundry Project Manager** role.

### What gets deployed

The deployment creates a named, versioned hosted agent inside the selected
Foundry project. Foundry Agent Service provides:

- A dedicated endpoint for the Responses protocol.
- A dedicated Microsoft Entra agent identity.
- Managed compute, scaling, session lifecycle, and observability.
- A per-session, VM-isolated sandbox that runs the agent application.
- An immutable agent version for each successful deployment.

The deployed process is not an Azure Container App that participants create or
manage. Foundry Agent Service owns the runtime infrastructure and starts the
agent sandbox when a session needs it. The hosted .NET application still calls
the Foundry model deployment and the APIM MCP endpoint at runtime.

```text
Client
    -> Foundry hosted-agent Responses endpoint
    -> managed .NET agent sandbox
    -> Foundry model deployment
    -> APIM-hosted MCP server
    -> synthetic REST operation
```

### Code deployment versus container deployment

This workshop uses **Code** with **Remote** package mode. Foundry Toolkit
packages the source as a ZIP, uploads it, restores the dependencies declared in
the `.csproj`, and prepares the managed runtime image. Participants do not need
a Dockerfile, local Docker installation, or customer-managed Azure Container
Registry for this path.

The Toolkit also supports **Container** deployment for applications that need a
custom image, operating-system packages, or custom Dockerfile behavior. That
path builds or references an image in Azure Container Registry. It is not used
in this workshop.

Choose either the Foundry Toolkit path or the Azure Developer CLI path. Both
create the same type of hosted-agent version, but they maintain separate local
deployment state. Do not assume that selecting a project in Foundry Toolkit
fully configured the active azd environment. Verify the azd values before using
the CLI path.

### Path A: Deploy with the Foundry Toolkit UI

This is the simplest participant path:

1. Stop the local agent.
2. In Visual Studio Code, open the Command Palette.
3. Run **Foundry Toolkit: Deploy Hosted Agent**.
4. Select the workshop Foundry project.
5. Choose **Code** as the deployment method and **Remote** as the package mode.
6. Confirm the `work-request-workshop-agent` name, .NET 10 runtime, model
   deployment, and environment values.
7. Select **Review + Deploy** and wait for the version to become active.
8. Open the Foundry project, select **Agents**, then select
   `work-request-workshop-agent` to view the saved version.

The repository's `azure.yaml` declares separate work-request and Petstore
hosted-agent services. Both use code deployment, so Docker and Azure Container
Registry are not required. Each later deployment with the same agent name
creates another immutable version.

### Path B: Deploy with Azure Developer CLI

Run every command from the repository root. The CLI path uses the selected azd
environment under `.azure`, not the project selection stored by Foundry
Toolkit. Values in `src/Workshop.Agent/.env` configure `dotnet run`; they do
not automatically populate the azd environment used for deployment.

#### 1. Sign in to Azure CLI and azd

```powershell
az login
azd auth login
azd auth login --check-status
```

Confirm that Azure CLI is using the intended subscription:

```powershell
az account show --query "{subscription:name, subscriptionId:id, tenantId:tenantId}" --output table
```

#### 2. Create or select the azd environment

List the environments already associated with the repository:

```powershell
azd env list
```

Select the workshop environment if it exists:

```powershell
azd env select foundry-apim-mcp-workshop
```

If no environment exists, create one:

```powershell
azd env new foundry-apim-mcp-workshop
```

#### 3. Bind azd to the existing Foundry project

Collect the project endpoint and existing model deployment name from the
target Foundry project. The endpoint looks like:

```text
https://<account>.services.ai.azure.com/api/projects/<project>
```

The `ai-project` service in `azure.yaml` uses this endpoint to connect to the
existing project. It does not create a second project.

Set the azd environment:

```powershell
$subscriptionId = az account show --query id --output tsv
$tenantId = az account show --query tenantId --output tsv
$projectEndpoint = "https://<account>.services.ai.azure.com/api/projects/<project>"

azd env set AZURE_SUBSCRIPTION_ID $subscriptionId
azd env set AZURE_TENANT_ID $tenantId
azd env set AZURE_AI_PROJECT_ENDPOINT $projectEndpoint
azd env set AZURE_AI_MODEL_DEPLOYMENT_NAME "gpt-4.1"
```

Foundry injects `FOUNDRY_PROJECT_ENDPOINT` into the hosted process. Do not add
that reserved variable to the service `env` map in `azure.yaml`. The local
`.env` file still supplies it when you run the application with `dotnet run`.

#### 4. Configure the MCP endpoint for the selected service

For the work-request agent:

```powershell
azd env set MCP_SERVER_ENDPOINT `
  "https://<apim-name>.azure-api.net/work-request-tools/mcp"
```

For the optional Petstore agent:

```powershell
azd env set PETSTORE_MCP_SERVER_ENDPOINT `
  "https://<apim-name>.azure-api.net/petstore-anon/mcp"
```

The repository declares two agent services. `azd ai agent doctor` validates
both and reports a missing Petstore variable when the optional value is not
configured. A missing Petstore value does not require deploying Petstore, but
the full-project doctor check remains incomplete until the value is set.

#### 5. Bind and verify the selected project

```powershell
azd env get-values
azd ai project show --output json
```

Confirm that the output identifies the intended project. Do not continue if it
points to another environment.

Preview the project binding:

```powershell
azd provision --preview --no-prompt
```

The preview should report `Using existing Foundry project; nothing to
provision`. Apply that binding to the selected azd environment:

```powershell
azd provision --no-prompt
```

This repository uses the `microsoft.foundry` provider and an existing project
endpoint. The command does not create a Foundry project, model deployment, API,
or APIM instance.

If both MCP endpoint variables are configured, run the complete local
configuration check:

```powershell
azd ai agent doctor --local-only
```

Package only the service you intend to deploy:

```powershell
azd package work-request-workshop-agent --no-prompt
```

#### 6. Deploy one named service

Do not run bare `azd deploy` in this repository. It attempts to deploy both
services and requires configuration for both profiles.

Deploy the work-request agent:

```powershell
azd deploy work-request-workshop-agent --no-prompt
azd ai agent show work-request-workshop-agent --output json
```

To deploy Petstore instead:

```powershell
azd package petstore-workshop-agent --no-prompt
azd deploy petstore-workshop-agent --no-prompt
azd ai agent show petstore-workshop-agent --output json
```

#### 7. Invoke the deployed agent

After the status becomes `active`, run one smoke test:

```powershell
azd ai agent invoke work-request-workshop-agent `
  "Review work request WR-1001 and recommend the next action." `
  --protocol responses
```

For Petstore:

```powershell
azd ai agent invoke petstore-workshop-agent `
  "Find available pets and summarize the results." `
  --protocol responses
```

#### Common CLI failures

| Error | Resolution |
|---|---|
| `AZURE_SUBSCRIPTION_ID is required` | Set `AZURE_SUBSCRIPTION_ID` from `az account show --query id --output tsv`. |
| `missing_project_endpoint` | Set `AZURE_AI_PROJECT_ENDPOINT`, then run `azd provision --no-prompt`. |
| Missing `infra\main.bicep` during `azd provision` | Confirm `azure.yaml` retains `infra.provider: microsoft.foundry`. |
| Missing `PETSTORE_MCP_SERVER_ENDPOINT` in `agent doctor` | The doctor command validates both services. Set the optional endpoint before running the full doctor check. |
| More than one agent service found | Pass `work-request-workshop-agent` or `petstore-workshop-agent` explicitly. |
| Authorization or role-assignment failure | Confirm the deploying identity has **Foundry Project Manager** on the target project. |
| `session_not_ready` after deployment | Wait 15 to 30 seconds and retry the invoke command. |

When using **Foundry Toolkit: Deploy Hosted Agent** for Petstore, copy
`.env.petstore.example` to `.env`, provide the real endpoint values, and deploy
a new agent named `petstore-workshop-agent`. The review page should show .NET
10, `dotnet Workshop.Agent.dll`, and 1 CPU / 2 GiB memory.

For platform details, see
[Hosted agents in Foundry Agent Service](https://learn.microsoft.com/azure/foundry/agents/concepts/hosted-agents)
and
[Deploy a hosted agent from source code](https://learn.microsoft.com/azure/foundry/agents/how-to/deploy-hosted-agent-code).

---

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 2, Expose the API as MCP](lab-2-expose-mcp.md) |
[Next: Lab 4, Add the Copilot Studio path](lab-4-copilot-studio.md)

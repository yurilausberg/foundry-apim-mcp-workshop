# Advanced azd Setup for Hosted Agents

Use this guide when preparing the Azure Developer CLI environment for Lab 3C
or troubleshooting a participant's CLI deployment. Participants using the
Foundry Toolkit UI do not need these steps.

The repository contains two hosted-agent services:

- `work-request-workshop-agent`
- `petstore-workshop-agent`

Run commands from the repository root. The selected azd environment is stored
under `.azure` and is separate from the `.env` file used by `dotnet run`.

## 1. Sign in

```powershell
az login
azd auth login
azd auth login --check-status
```

Confirm that Azure CLI is signed in:

```powershell
az account show --query state --output tsv
```

Do not paste account, tenant, subscription, resource, or user identifiers into
issues, screenshots, or shared transcripts.

## 2. Create or select the azd environment

List the environments associated with the repository:

```powershell
azd env list
```

Select an existing workshop environment:

```powershell
azd env select foundry-apim-mcp-workshop
```

If it does not exist, create it:

```powershell
azd env new foundry-apim-mcp-workshop
```

## 3. Connect to the existing Foundry project

Copy these values from the target Foundry project:

- Project endpoint.
- Full project ARM resource ID.
- Existing model deployment name.

The endpoint has this format:

```text
https://<account>.services.ai.azure.com/api/projects/<project>
```

The project ARM resource ID has this format:

```text
/subscriptions/<subscription-id>/resourceGroups/<resource-group>/providers/Microsoft.CognitiveServices/accounts/<account>/projects/<project>
```

Use **JSON View** in the Azure portal to copy the project resource ID, or query
it when you know the resource group, Foundry account name, and project name:

```powershell
$projectId = az cognitiveservices account project show `
  --subscription "<project-subscription-id>" `
  --resource-group "<resource-group>" `
  --name "<foundry-account-name>" `
  --project-name "<project-name>" `
  --query id `
  --output tsv
```

Set the required azd values:

```powershell
$projectEndpoint = "https://<account>.services.ai.azure.com/api/projects/<project>"
$projectId = "<full-project-arm-resource-id>"
$subscriptionId = ($projectId -split "/")[2]
$tenantId = az account show `
  --subscription $subscriptionId `
  --query tenantId `
  --output tsv
$projectLocation = az resource show `
  --ids $projectId `
  --query location `
  --output tsv

azd env set AZURE_SUBSCRIPTION_ID $subscriptionId
azd env set AZURE_TENANT_ID $tenantId
azd env set AZURE_LOCATION $projectLocation
azd env set AZURE_AI_PROJECT_ENDPOINT $projectEndpoint
azd env set AZURE_AI_PROJECT_ID $projectId
azd env set AZURE_AI_MODEL_DEPLOYMENT_NAME "gpt-4.1"
```

Use the subscription from the project ARM resource ID. It can differ from the
subscription currently selected by Azure CLI.

The `ai-project` service in `azure.yaml` connects to this existing project. It
does not create another project. Foundry injects `FOUNDRY_PROJECT_ENDPOINT`
into the hosted process, so do not add that reserved variable to the service
`env` map.

## 4. Configure the MCP profile

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

The full `azd ai agent doctor` check validates both services. It reports a
missing Petstore value until the optional Petstore endpoint is configured.
That warning does not prevent a named work-request deployment.

## 5. Bind and verify the project

```powershell
azd ai project show --output json
azd provision --preview --no-prompt
```

The project output can contain environment identifiers. Review it locally and
do not copy it into shared logs.

The preview should report:

```text
Using existing Foundry project; nothing to provision
```

Apply the binding:

```powershell
azd provision --no-prompt
```

This command does not create a Foundry project, model deployment, API, or APIM
instance. The repository uses the `microsoft.foundry` provider and an existing
project endpoint.

If both MCP profiles are configured, run the complete local check:

```powershell
azd ai agent doctor --local-only
```

Optionally package one service before the workshop:

```powershell
azd package work-request-workshop-agent --no-prompt
```

The project-level `.agentignore` keeps local `.env` files, build output, generated
checkpoints, symbols, and IDE state out of the deployment package. Review this
file before adding other local-only files under `src/Workshop.Agent`.

## 6. Deploy one named service

Do not run bare `azd deploy`. This repository declares two agent services, so a
bare command attempts to deploy both.

Work-request profile:

```powershell
azd deploy work-request-workshop-agent --no-prompt
azd ai agent show work-request-workshop-agent --output json
```

Petstore profile:

```powershell
azd deploy petstore-workshop-agent --no-prompt
azd ai agent show petstore-workshop-agent --output json
```

## 7. Invoke the deployed agent

Work-request profile:

```powershell
azd ai agent invoke work-request-workshop-agent `
  "Review work request WR-1001 and recommend the next action." `
  --protocol responses
```

Petstore profile:

```powershell
azd ai agent invoke petstore-workshop-agent `
  "Find available pets and summarize the results." `
  --protocol responses
```

## Troubleshooting

| Error | Resolution |
|---|---|
| `AZURE_SUBSCRIPTION_ID is required` | Set it from the subscription segment of the Foundry project ARM resource ID. |
| `Microsoft Foundry project ID is required` | Set the full project ARM resource ID as `AZURE_AI_PROJECT_ID`. The endpoint URL is not the project ID. |
| `AZURE_LOCATION is not set` | Query the project resource location and save it as `AZURE_LOCATION`. Code deployment must use the Foundry project region. |
| `missing_project_endpoint` | Set `AZURE_AI_PROJECT_ENDPOINT`, then run `azd provision --no-prompt`. |
| Missing `infra\main.bicep` during `azd provision` | Confirm `azure.yaml` retains `infra.provider: microsoft.foundry`. |
| Missing `PETSTORE_MCP_SERVER_ENDPOINT` in `agent doctor` | Configure the optional endpoint or deploy only the named work-request service. |
| More than one agent service found | Pass the intended service name explicitly. |
| Authorization or role-assignment failure | Confirm the deploying identity has **Foundry Project Manager** on the target project. |
| `session_not_ready` after deployment | Wait 15 to 30 seconds and retry the invoke command. |

[Back to Lab 3](labs/lab-3-foundry-agent.md)

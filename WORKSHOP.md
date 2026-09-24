# Workshop Guide

## Complete before the workshop

### Participant laptop

- Bring a laptop that can run Visual Studio Code, the .NET 10 SDK, and local
  command-line tools. Have local installation rights, or ask IT to install the
  required software before the session.
- Install the current stable Visual Studio Code release, Git, .NET 10 SDK,
  Node.js 22.19 or later, Azure CLI, and Azure Developer CLI.
- Use Windows PowerShell or install PowerShell 7 when running the workshop on
  macOS or Linux.
- Install the C# Dev Kit and Foundry Toolkit extensions.
- Confirm GitHub Copilot Chat is available in Visual Studio Code, **Agent**
  mode is enabled, and organizational policy permits workspace MCP servers.
- Install a current browser such as Microsoft Edge or Google Chrome.
- Clone or download this repository before the session and run:

  ```powershell
  .\scripts\verify-prereqs.ps1
  ```

### Identity and network access

- Use an Azure identity that can sign in to the workshop tenant and
  subscription. Complete multifactor authentication before the session.
- Confirm the laptop can reach the Azure portal, the Foundry project endpoint,
  and the APIM gateway. If the resources use private networking, connect to the
  required VPN and verify private DNS resolution.
- Allow outbound HTTPS on port 443 to `login.microsoftonline.com`,
  `portal.azure.com`, `management.azure.com`, `ai.azure.com`, the assigned
  `*.services.ai.azure.com` project endpoint, and the assigned
  `*.azure-api.net` APIM endpoint.
- Allow access to `github.com`, `api.githubcopilot.com`,
  `registry.npmjs.org`, `api.nuget.org`, and `marketplace.visualstudio.com`
  for repository access, Copilot Chat, packages, and extensions.
- Confirm the corporate proxy or firewall does not block or buffer Streamable
  HTTP responses from the APIM MCP endpoint.

### Shared Azure sandbox

- Prepare an APIM instance on a v2 SKU that supports MCP, such as Basic v2,
  Standard v2, or Premium v2. Grant participants permission to configure APIs,
  MCP servers, policies, and diagnostics.
- Prepare a Microsoft Foundry project and grant participants the **Foundry
  User** role. Grant **Foundry Project Manager** only when participants must
  deploy a hosted agent version or create or modify project connections.
- Deploy a base model such as `gpt-4.1` with approximately 250,000 TPM or more,
  and confirm sufficient subscription and regional quota for the expected
  concurrent group.
- Share the tenant, subscription, Foundry project endpoint, model deployment
  name, APIM service name, and resource group before the workshop.
- Use an isolated sandbox with synthetic data. Do not connect participant
  exercises to production systems or confidential data.

### Optional paths

- For the Copilot Studio lab, provide a licensed environment where participants
  can create or edit an agent and add MCP tools.
- For the public Petstore path, allow outbound HTTPS access to
  `petstore3.swagger.io` and treat the service as uncontrolled test data.

## Schedule

| Lab | Phase | Duration | Result |
|---|---|---:|---|
| Lab 0 | Orientation, scenario, and sandbox confirmation | 60 minutes | Shared architecture, success criteria, and verified prerequisites |
| Labs 1 and 2 | Import the API and expose it through APIM as MCP tools | 90 minutes | Tested REST API and working MCP endpoint |
| Lab 3 | Build and run the Foundry agent | 75 minutes | Code-first agent using the MCP tools, with optional hosted-version deployment |
| Lab 4 | Add the Copilot Studio path | 30 minutes | Low-code agent using the same tools |
| Lab 5 | Apply security, governance, and monitoring | 45 minutes | Governed sandbox design |
| Lab 6 | Demonstrate the complete pattern and define next steps | 45 minutes | End-to-end validation and action plan |

Breaks and lunch are outside the six hours of workshop content.

## Lab 0: Confirm the sandbox

1. Confirm the APIM instance uses a supported v2 SKU, such as Basic v2,
   Standard v2, or Premium v2.
2. Confirm the Foundry project has a base model deployment, such as
   `gpt-4.1`.
3. Confirm the model deployment has approximately 250,000 tokens per minute
   (TPM) or more allocated for the workshop. Verify that the subscription has
   sufficient quota for that model in the deployment region. This target
   reduces throttling when a larger participant group calls the model
   simultaneously. Adjust it for the expected group size and prompt volume.
4. Confirm Azure CLI authentication:

   ```powershell
   az account show --query "{subscription:name, tenant:tenantId}" --output table
   az account get-access-token --resource https://ai.azure.com --query expiresOn --output tsv
   ```

5. Confirm the participant has the Foundry User role on the project.
6. Confirm the lab will use synthetic data only.

## Lab 1: Import the synthetic API

1. Open the APIM instance in the Azure portal.
2. Select **APIs**, then **Add API**, then **OpenAPI**.
3. Import `openapi/work-request-api.yaml`.
4. Set the API URL suffix to `work-requests`.
5. Confirm these operation IDs were imported:

   - `getWorkRequest`
   - `createWorkRequest`
   - `updateWorkRequestStatus`

6. Open the API **Settings** tab, clear **Subscription required**, and save.
   This allows workshop clients to test without an APIM subscription key.
7. Switch to the **Design** tab and select **All operations**.
8. In **Inbound processing**, select the `</>` code editor.
9. Keep the existing `<base />` element and add the `mock-response` line
   directly after it:

   ```xml
   <inbound>
       <base />
       <mock-response status-code="200" content-type="application/json" />
   </inbound>
   ```

10. Select **Save**.
11. Confirm that **Mocking is enabled** appears for the API.
12. Test all three operations from the APIM test console.

Disabling **Subscription required** is for the isolated workshop sandbox only.
Production APIs should use an approved authentication and authorization model.

The API suffix supplies the `/work-requests` base path. The OpenAPI operation
paths are relative to that suffix, so the final PATCH URL is:

```text
https://<apim-name>.azure-api.net/work-requests/WR-1001/status
```

The OpenAPI request examples prepopulate `WR-1001` and
`status: ReadyForReview` in the APIM test console.

The response payloads are stored as examples in the OpenAPI document. Each
operation defines a `200 application/json` response. The API-level policy uses
APIM's built-in `mock-response` policy to select the matching example without a
backend service.

### Static mock limitations

The APIM `mock-response` policy demonstrates the API contract and MCP tool
shape. It is not a simulated data store:

- GET does not validate `requestId`. Any ID returns the same OpenAPI response
  example.
- POST does not create or persist a record.
- PATCH does not read the request body or update state. It returns the fixed
  response example.
- If APIM returns a schema-generated value such as the first `status` enum
  value (`Draft`), update or re-import the latest OpenAPI definition so APIM
  has the response examples.

Use a real sandbox backend or a conditional APIM policy if the workshop needs
record lookup, request validation, state changes, or negative-path behavior.

### Optional live backend: Swagger Petstore

For a workshop that needs real request processing without deploying a backend,
use the public Swagger Petstore API:

```text
https://petstore3.swagger.io/api/v3/openapi.json
```

1. Import the OpenAPI document from the URL instead of importing
   `openapi/work-request-api.yaml`.
2. Set an API URL suffix such as `petstore`.
3. Verify that the backend web service URL is:

   ```text
   https://petstore3.swagger.io/api/v3
   ```

4. Do not add the `mock-response` policy. Requests must reach the public
   backend.
5. Open the API **Settings** tab, clear **Subscription required**, and save.
   This allows workshop clients to test without an APIM subscription key.
6. For the MCP server, expose a small read-only tool set first, such as
   `findPetsByStatus` and `getPetById`.

Swagger Petstore is a third-party public demonstration service. It is not
operated, monitored, or covered by an availability commitment from the
workshop team. Its data, behavior, throttling, and availability can change
without notice. Do not send confidential data, credentials, personal data, or
production workloads to it.

### Checkpoint

For the static work-request path, the API must return synthetic JSON without
calling an organization backend. Participants should understand that the
responses are fixed contract examples.

For the optional Petstore path, APIM must successfully forward a read request
to the public backend. Participants should understand that the service is
external and uncontrolled.

## Lab 2: Expose the API as an MCP server

1. In APIM, select **APIs**, then **MCP Servers**.
2. Select **Create MCP server**.
3. Select **Expose an API as an MCP server**.
4. Choose the API imported in Lab 1.
5. Select the operations to expose as tools. For the static work-request path,
   select all three operations. For Petstore, start with read-only operations.
6. Enter a **Display name**, such as `Work Request Tools`.
7. Confirm the required **Name** field contains a URL-safe value, such as
   `work-request-tools`.
8. Create the MCP server.
9. In the **MCP Servers** list, find the new server. In the **Server URL**
   column, select the **Copy to clipboard** icon.
10. Save the copied URL for the client configuration. It should end in `/mcp`.

Do not add the governance policy yet. First validate the basic MCP connection.
The policy is added once in Lab 5.

### Checkpoint

The MCP client must discover the operations selected as tools.

### Test with MCP Inspector

1. From a terminal, start the Inspector:

   ```powershell
   npx @modelcontextprotocol/inspector
   ```

2. If `npx` asks to install the package, confirm the installation.
3. Open the Inspector URL printed in the terminal.
4. Select **Streamable HTTP** as the transport.
5. Paste the APIM MCP server URL into the server URL field and select
   **Connect**.
6. Open **Tools**, select **List Tools**, and confirm these generated MCP tool
   names are available:

   - `createAWorkRequest`
   - `getAWorkRequest`
   - `updateWorkRequestStatus`

   These MCP tool names differ from two of the OpenAPI operation IDs confirmed
   in Lab 1. Use the names returned by **List Tools** in client allowlists.

7. Select a read-only tool, enter its required arguments, and run it. For the
   synthetic API, invoke `getAWorkRequest` with `WR-1001`.

### Test in Visual Studio Code

1. Copy `.vscode/mcp.json.example` to `.vscode/mcp.json`.
2. Replace the placeholder URL with the APIM MCP server URL.
3. Save `.vscode/mcp.json`. VS Code discovers the workspace MCP configuration
   and starts the server when its tools are needed. Do not start it manually.
4. Open Copilot Chat with **Ctrl+Alt+I** and select **Agent** mode.
5. Select the **Configure Tools** icon near the chat input.
6. Search for `work-request`.
7. Find `work-request-workshop` and select **Refresh Tools**.
8. Expand the server and select the checkbox for each tool:

   - `createAWorkRequest`
   - `getAWorkRequest`
   - `updateWorkRequestStatus`

9. If VS Code asks whether you trust the server, review the URL and confirm
   only if it is the workshop APIM endpoint.
10. Ask:

   ```text
   Get work request WR-1001 and summarize its current status.
   ```

11. For the optional Petstore path, uncomment the `petstore-anon` entry in
    `.vscode/mcp.json`, replace its placeholder URL with the copied Petstore MCP
    server URL, and save the file.
12. Select **Configure Tools** again, search for `petstore`, find
    `petstore-anon`, and select **Refresh Tools**.
13. Expand `petstore-anon` and select the checkbox for both read-only tools:

    - `findPetById`: Returns a single pet by its numeric ID.
    - `findsPetsByStatus`: Finds pets by status. Provide multiple status values
      as a comma-separated string, such as `pending,available`.

14. Ask:

    ```text
    Find pets that are currently available.
    ```

### Troubleshooting

- If `work-request-workshop` is not listed, confirm the file is named
  `.vscode/mcp.json`, not `.vscode/mcp.json.example`, and that it contains
  valid JSON.
- If the editor continues to show **Starting**, open Chat in **Agent** mode and
  select **Configure Tools**. If the three work-request tools are listed, the
  server is connected and the displayed status is stale.
- If the tools are not listed, select **Cancel** beside the starting server,
  run **Developer: Reload Window** from the Command Palette, then use
  **MCP: List Servers** to disable and re-enable the server.
- If MCP Inspector connects to the same URL but VS Code still shows
  **Starting**, do not recreate the APIM MCP server. Treat the problem as a
  VS Code client-state issue and use Inspector as the lab validation fallback.
  This behavior is tracked in
  [microsoft/vscode#336805](https://github.com/microsoft/vscode/issues/336805).
- To inspect a connection failure, run **MCP: List Servers**, select
  `work-request-workshop`, and choose **Show Output**.
- A `401` normally means the client did not send the required header or token.
- If streaming fails, disable frontend response-body logging at the global APIM scope.
- Do not access `context.Response.Body` in an MCP server policy.
- Confirm the URL uses the generated MCP server endpoint, not the original REST API base URL.

## Lab 3: Run the Foundry agent

1. Copy the environment template:

   ```powershell
   Copy-Item .\src\Workshop.Agent\.env.example .\src\Workshop.Agent\.env
   ```

2. Set:

   - `FOUNDRY_PROJECT_ENDPOINT`
   - `AZURE_AI_MODEL_DEPLOYMENT_NAME`
   - `MCP_SERVER_ENDPOINT`
   - `AZURE_TOKEN_CREDENTIALS=AzureCliCredential`

   The final setting makes local development use the identity from `az login`
   instead of probing the Azure managed identity endpoint. Keep this setting in
   the local `.env` file only. Do not add it to `azure.yaml`, because a deployed
   hosted agent should use its Azure identity.

3. Authenticate:

   ```powershell
   az login
   ```

4. Run the agent:

   ```powershell
   dotnet run --project .\src\Workshop.Agent\Workshop.Agent.csproj
   ```

5. In another terminal, invoke the Responses endpoint:

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

If the response reports `ManagedIdentityCredential authentication failed` and
references `169.254.169.254`, confirm the local `.env` contains
`AZURE_TOKEN_CREDENTIALS=AzureCliCredential`, stop the running agent, and start
it again.

### Checkpoint

The agent should select `getAWorkRequest`, inspect the synthetic response, and
return a grounded recommendation. Read operations run without an approval
round-trip. Create and status-update operations still require approval.

### Optional extension: save a hosted agent version

The local `dotnet run` process is temporary and does not create an agent entry
in the Foundry UI. Participants need the **Foundry Project Manager** role to
deploy a hosted agent.

The simplest workshop path uses the Foundry Toolkit extension:

1. Stop the local agent.
2. In Visual Studio Code, open the Command Palette.
3. Run **Foundry Toolkit: Deploy Hosted Agent**.
4. Select the workshop Foundry project.
5. Choose **Code** as the deployment method.
6. Confirm the `work-request-workshop-agent` name, .NET 10 runtime, model
   deployment, and environment values.
7. Select **Review + Deploy** and wait for the version to become active.
8. Open the Foundry project, select **Agents**, then select
   `work-request-workshop-agent` to view the saved version.

The repository's `azure.yaml` already declares a Foundry hosted agent and uses
code deployment, so Docker and Azure Container Registry are not required.
Each later deployment with the same agent name creates another immutable
version.

The command-line equivalent is:

```powershell
azd deploy workshop-agent
azd ai agent show workshop-agent --output json
```

The facilitator must bind the repository's active `azd` environment to the
workshop Foundry project and set the model deployment and MCP server endpoint
before participants use the command-line path.

## Lab 4: Add the Copilot Studio path

1. Create or open a Copilot Studio agent in the workshop environment.
2. Add the APIM MCP server as a tool.
3. Configure the same sandbox authentication method used for the workshop.
4. Test the same prompt used in the Foundry lab.
5. Compare:

   - Setup effort.
   - Tool discovery.
   - Approval experience.
   - Environment and ALM controls.
   - Telemetry and troubleshooting.

## Lab 5: Secure and monitor

Start with the working sandbox. Add controls one at a time.

1. In APIM, select **APIs**, then **MCP Servers**, then select the workshop MCP
   server.
2. Open **Policies**, select the code editor, and apply
   `policies/mcp-governance.xml` once at the MCP server scope.
3. Select **Save**.
4. Enable Application Insights or Azure Monitor diagnostics.
5. Keep global frontend response payload logging at 0 bytes.
6. Note that `mcp-governance.xml` is the anonymous sandbox baseline. It adds
   correlation, rate limiting, and tracing, but it does not authenticate the
   caller.
7. Review inbound authentication options:

   - APIM subscription key for a bounded workshop.
   - Entra ID and OAuth for delegated user access.
   - Managed identity for service-to-service access where supported.

8. Decide which tools need explicit approval before execution.
9. Separate read operations from state-changing operations.

### Optional extension: delegated Entra OAuth

Use this extension only after the anonymous MCP endpoint works.

1. Follow [Optional Microsoft Entra delegated OAuth](docs/entra-delegated-oauth.md)
   to configure the protected API registration, public client registration,
   delegated scope, consent, and APIM named values.
2. Replace `policies/mcp-governance.xml` with
   `policies/mcp-governance-entra-delegated.xml`. Do not apply both policy
   files. The Entra policy includes the baseline governance controls.
3. Save the policy and confirm that an anonymous MCP Inspector connection now
   receives HTTP 401.
4. Acquire a delegated access token. The APIM developer portal token generator
   is acceptable for a one-time workshop test.
5. In MCP Inspector, add this custom HTTP header:

   ```text
   Authorization: Bearer <access-token>
   ```

6. Reconnect, list the tools, and invoke a read-only tool.

Authorization Code with PKCE is the recommended flow for an interactive public
client. Do not use a client secret for this client. APIM token validation does
not by itself enable automatic OAuth sign-in in MCP Inspector. Automatic sign-in
requires MCP protected-resource metadata, a `WWW-Authenticate` challenge, and a
compatible pre-registered client.

## Lab 6: Demonstrate the complete pattern

Use these prompts:

```text
Get work request WR-1001 and explain what is blocking approval.
```

```text
Create a draft work request for facility inspection services with a not-to-exceed amount of $25,000. Show the proposed tool arguments before execution.
```

```text
Update work request WR-1001 to ReadyForReview. Show the proposed tool arguments before execution.
```

For state-changing prompts, pause before execution and discuss approval,
identity, audit, and rollback expectations. The static APIM mock confirms the
tool invocation contract, but it does not persist the create request or apply
the status update.

## Closeout

Capture:

- Which production systems would replace the synthetic API.
- Which operations should be tools.
- Required user and workload identities.
- Approval boundaries for state-changing actions.
- Logging, retention, and support requirements.
- Pilot success criteria and named owners.

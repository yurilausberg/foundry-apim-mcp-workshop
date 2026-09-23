# Workshop Guide

## Schedule

| Lab | Phase | Duration | Result |
|---|---|---:|---|
| Lab 0 | Orientation, scenario, and sandbox confirmation | 60 minutes | Shared architecture, success criteria, and verified prerequisites |
| Labs 1 and 2 | Import the API and expose it through APIM as MCP tools | 90 minutes | Tested REST API and working MCP endpoint |
| Lab 3 | Build and run the Foundry agent | 75 minutes | Code-first agent using the MCP tools |
| Lab 4 | Add the Copilot Studio path | 30 minutes | Low-code agent using the same tools |
| Lab 5 | Apply security, governance, and monitoring | 45 minutes | Governed sandbox design |
| Lab 6 | Demonstrate the complete pattern and define next steps | 45 minutes | End-to-end validation and action plan |

Breaks and lunch are outside the six hours of workshop content.

## Lab 0: Confirm the sandbox

1. Confirm the APIM instance is in a supported tier.
2. Confirm the Foundry project and model deployment are ready.
3. Confirm Azure CLI authentication:

   ```powershell
   az account show --query "{subscription:name, tenant:tenantId}" --output table
   az account get-access-token --resource https://ai.azure.com --query expiresOn --output tsv
   ```

4. Confirm the participant has the Foundry User role on the project.
5. Confirm the lab will use synthetic data only.

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

### Test in Visual Studio Code

1. Copy `.vscode/mcp.json.example` to `.vscode/mcp.json`.
2. Replace the placeholder URL with the APIM MCP server URL.
3. Run **MCP: List Servers** and start the server.
4. In Copilot agent mode, enable the work-request tools.
5. Ask:

   ```text
   Get work request WR-1001 and summarize its current status.
   ```

   For the optional Petstore path, ask:

   ```text
   Find pets that are currently available.
   ```

### Troubleshooting

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

   Invoke-RestMethod `
     -Method Post `
     -Uri "http://localhost:8088/responses" `
     -ContentType "application/json" `
     -Body $body
   ```

### Checkpoint

The agent should select `getWorkRequest`, inspect the synthetic response, and
return a grounded recommendation.

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
6. Review inbound authentication options:

   - APIM subscription key for a bounded workshop.
   - Entra ID and OAuth for delegated user access.
   - Managed identity for service-to-service access where supported.

7. Decide which tools need explicit approval before execution.
8. Separate read operations from state-changing operations.

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

# Workshop Guide

## Schedule

| Phase | Duration | Result |
|---|---:|---|
| Orientation and scenario | 60 minutes | Shared architecture and success criteria |
| APIM REST-to-MCP lab | 90 minutes | Working MCP endpoint |
| Foundry agent lab | 75 minutes | Code-first agent using the MCP tools |
| Copilot Studio path | 30 minutes | Low-code agent using the same tools |
| Security and monitoring | 45 minutes | Governed sandbox design |
| Demo and next steps | 45 minutes | End-to-end validation and action plan |

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
8. In **Inbound processing**, select **+ Add policy**.
9. Select **Mock responses**.
10. Select **200 OK, application/json**, then select **Save**.
11. Confirm that **Mocking is enabled** appears for the API.
12. Test all three operations from the APIM test console.

As an alternative to the policy gallery, open the inbound policy code editor,
add the `<mock-response>` element shown in `policies/api-mock.xml`, and select
**Save**.

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

### Checkpoint

The API must return synthetic JSON without calling an organization backend.
Participants should understand that the responses are fixed contract examples.

## Lab 2: Expose the API as an MCP server

1. In APIM, select **APIs**, then **MCP Servers**.
2. Select **Create MCP server**.
3. Select **Expose an API as an MCP server**.
4. Choose the synthetic work-request API.
5. Expose the three operations as tools.
6. Record the generated server URL. It should end in `/mcp`.
7. Apply `policies/mcp-governance.xml` at the MCP server scope.

### Checkpoint

The MCP client must discover all three tools.

### Test in Visual Studio Code

1. Copy `.vscode/mcp.json.example` to `.vscode/mcp.json`.
2. Replace the placeholder URL with the APIM MCP server URL.
3. Run **MCP: List Servers** and start the server.
4. In Copilot agent mode, enable the work-request tools.
5. Ask:

   ```text
   Get work request WR-1001 and summarize its current status.
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

1. Add rate limiting and a correlation trace with `policies/mcp-governance.xml`.
2. Enable Application Insights or Azure Monitor diagnostics.
3. Keep global frontend response payload logging at 0 bytes.
4. Review inbound authentication options:

   - APIM subscription key for a bounded workshop.
   - Entra ID and OAuth for delegated user access.
   - Managed identity for service-to-service access where supported.

5. Decide which tools need explicit approval before execution.
6. Separate read operations from state-changing operations.

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

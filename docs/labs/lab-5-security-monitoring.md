# Lab 5: Secure and monitor

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 4, Add the Copilot Studio path](lab-4-copilot-studio.md) |
[Next: Lab 6, Demonstrate the complete pattern](lab-6-complete-pattern.md)

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
7. Review inbound access-control options:

   - APIM subscription key for bounded workshop access and usage tracking.
     A subscription key is not user authentication.
   - Entra ID and OAuth for delegated user access.
   - Managed identity for service-to-service access where supported.

8. Decide which tools need explicit approval before execution.
9. Separate read operations from state-changing operations.

## Verify correlation and diagnostics

1. Invoke a read-only tool through the MCP server.
2. In the APIM trace or connected monitoring destination, find the
   `correlation-id` trace metadata written by the governance policy.
3. Confirm that APIM preserves an incoming `x-correlation-id` header or creates
   a new GUID when the header is missing. The header is then forwarded to the
   downstream API and backend.

Open **Logs** from the Application Insights resource and run this query to join
MCP tool calls to the correlation trace written by the policy:

```kusto
let CorrelationTraces =
    traces
    | where timestamp > ago(2h)
    | extend CorrelationId = tostring(customDimensions["correlation-id"])
    | where isnotempty(CorrelationId)
    | project operation_Id, CorrelationId;
requests
| where timestamp > ago(2h)
| where url contains "/mcp"
| where tostring(customDimensions["api.type"]) startswith "mcp"
| where tostring(customDimensions["gen_ai.operation.name"]) == "tools/call"
| join kind=leftouter CorrelationTraces on operation_Id
| project
    timestamp,
    Tool = tostring(customDimensions["gen_ai.tool.name"]),
    CorrelationId,
    success,
    duration,
    operation_Id
| order by timestamp desc
```

When running the query directly from the Log Analytics workspace, use
`AppTraces`, `AppRequests`, `TimeGenerated`, `Properties`, `AppRoleName`,
`OperationId`, `Success`, and `DurationMs` instead of their Application
Insights resource-view equivalents above.

The `startswith "mcp"` filter accepts current APIM telemetry values such as
`mcp-backend` as well as the `Mcp` value shown in some documentation.
A populated `CorrelationId` confirms that the tool request and policy trace
share an operation ID. A blank value means the tool request was captured but no
matching correlation trace was found in the selected time window.

The workshop correlation begins at APIM unless the client or agent supplies the
header. Full end-to-end tracing across the client and agent requires additional
instrumentation, typically using W3C trace context. The correlation ID is
separate from the access token and its JWT claims.

## Production framing: Microsoft Agent 365

This lab establishes runtime controls in Entra ID, APIM, the agent, and the
monitoring stack. When it is enabled for the tenant, Microsoft Agent 365
complements those controls as the enterprise governance plane for agent
inventory, ownership, identity, lifecycle, security, and compliance.

Published Foundry agents appear in the Agent 365 registry automatically.
Activity ingestion and the broader security and compliance capabilities still
require Agent 365 licensing and administrator enablement. Hosted-agent telemetry
also requires the Agent 365 SDK and the required Microsoft Entra permissions.
See [Microsoft Agent 365 integration with Foundry](https://learn.microsoft.com/azure/foundry/agents/concepts/agent-365-integration).

## Optional extension: delegated Entra OAuth

Use this extension only after the anonymous MCP endpoint works.

1. Follow
   [Optional Microsoft Entra delegated OAuth](../entra-delegated-oauth.md)
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

The manual test and client-managed MCP OAuth both send the same delegated access
token as a bearer token. The manual path acquires the token elsewhere and adds
the header directly. A client-managed flow discovers the authorization server,
runs Authorization Code with PKCE, and manages the token lifecycle.

Authorization Code with PKCE is the recommended flow for an interactive public
client. Do not use a client secret for this client. APIM token validation does
not by itself enable automatic OAuth sign-in in MCP Inspector. Automatic
sign-in requires MCP protected-resource metadata, a `WWW-Authenticate`
challenge, and a compatible pre-registered client.

---

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 4, Add the Copilot Studio path](lab-4-copilot-studio.md) |
[Next: Lab 6, Demonstrate the complete pattern](lab-6-complete-pattern.md)

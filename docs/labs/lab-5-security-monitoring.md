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

Authorization Code with PKCE is the recommended flow for an interactive public
client. Do not use a client secret for this client. APIM token validation does
not by itself enable automatic OAuth sign-in in MCP Inspector. Automatic
sign-in requires MCP protected-resource metadata, a `WWW-Authenticate`
challenge, and a compatible pre-registered client.

---

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 4, Add the Copilot Studio path](lab-4-copilot-studio.md) |
[Next: Lab 6, Demonstrate the complete pattern](lab-6-complete-pattern.md)

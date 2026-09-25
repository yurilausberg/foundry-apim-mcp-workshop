# Facilitator Guide

## Before the session

1. Import the OpenAPI contract and apply the mock policies.
2. Create the APIM MCP server and record the endpoint.
3. Test tool discovery from Visual Studio Code.
4. Build the .NET project.
5. Run the agent and validate the three workshop prompts.
6. Prepare screenshots for each checkpoint in case portal access is slow.
   Crop or redact account names, user identities, tenant and subscription IDs,
   resource IDs, tokens, and private endpoints.

## Teaching pattern

- Explain the architecture before opening the portal.
- Use one read operation before any write operation.
- Keep the synthetic work-request scenario visible throughout the lab.
- Pause at each boundary: agent, MCP, APIM, REST API, and monitoring.
- Ask participants to name the production control that belongs at each boundary.

## Recovery paths

| Failure | Recovery |
|---|---|
| APIM import fails | Import the OpenAPI file from a local path and confirm YAML parsing |
| MCP tools are missing | Verify the selected operations and their unique `operationId` values |
| MCP streaming fails | Remove response-body policy access and set global frontend response logging to 0 bytes |
| Foundry authentication fails | Run `az login`, verify the tenant, and confirm Foundry User on the project |
| Local agent tries `169.254.169.254` | Set `AZURE_TOKEN_CREDENTIALS=AzureCliCredential` in the local `.env`, then restart the agent |
| Agent cannot call MCP | Validate the endpoint with Visual Studio Code first, then check the agent environment value |
| Entra-enabled MCP returns 401 | Check token expiry, `aud`, client application ID, and the delegated `scp` value |
| Inspector cannot start OAuth sign-in | Use a separately acquired bearer token unless OAuth discovery metadata and a compatible client registration are configured |
| Copilot Studio setup takes too long | Demonstrate the prepared agent and use the remaining time for governance comparison |

## Time protection

- Do not spend more than 15 minutes debugging one participant environment.
- Keep a prepared MCP endpoint available for the Foundry and Copilot Studio labs.
- Pre-deploy one hosted-agent version for demonstration. Treat participant
  deployment as optional when Foundry Project Manager access or build time is
  limited.
- Treat delegated OAuth as an extension after the anonymous endpoint works.
- Use `mcp-governance.xml` for recovery. Use
  `mcp-governance-entra-delegated.xml` only for the optional authenticated path.
- Acquire a fresh facilitator token shortly before the security demonstration.
  Never place it in the repository, workshop files, or screenshots.
- Preserve the final 20 minutes for owners, decisions, and next steps.

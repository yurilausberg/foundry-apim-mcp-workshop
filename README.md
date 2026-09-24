# Foundry, APIM, and MCP Workshop

A public workshop scaffold for turning an existing REST API into governed tools for AI agents.

The lab uses a synthetic work-request API, Azure API Management, Model Context Protocol, Microsoft Foundry, Microsoft Agent Framework, and an optional Copilot Studio path. It does not require organization data or access to organization systems.

## What participants build

```text
Synthetic OpenAPI contract
        |
        v
Azure API Management
  - REST API import
  - MCP server export
  - policies and monitoring
        |
        +----------------------+
        |                      |
        v                      v
Foundry code-first agent   Copilot Studio agent
```

## Repository contents

- `WORKSHOP.md`: Step-by-step workshop instructions.
- `openapi/work-request-api.yaml`: Synthetic API contract.
- `policies/`: APIM policy examples for mock responses, anonymous governance,
  and optional delegated Entra authentication.
- `src/Workshop.Agent/`: Current .NET hosted-agent starter.
- `src/Workshop.Agent/.env.example`: Work-request MCP agent configuration.
- `src/Workshop.Agent/.env.petstore.example`: Optional read-only Petstore MCP
  agent configuration.
- `docs/architecture.md`: Reference architecture and design notes.
- `docs/prerequisites.md`: Environment and access checklist.
- `docs/facilitator-guide.md`: Timing, checkpoints, and recovery paths.
- `docs/entra-delegated-oauth.md`: Optional delegated OAuth setup and MCP
  Inspector testing guidance.
- `scripts/verify-prereqs.ps1`: Local prerequisite checks.

## Quick start

1. Review [prerequisites](docs/prerequisites.md).
2. Run the local checks:

   ```powershell
   .\scripts\verify-prereqs.ps1
   ```

3. Follow [WORKSHOP.md](WORKSHOP.md).
4. Build the agent starter:

   ```powershell
   dotnet build .\src\Workshop.Agent\Workshop.Agent.csproj
   ```

## Current implementation choices

- .NET 10.
- Microsoft Agent Framework hosted-agent pattern.
- Foundry project endpoint authentication through `DefaultAzureCredential`.
- APIM Streamable HTTP MCP endpoint.
- Environment-configured MCP tool allowlists and approval boundaries.
- Separate hosted-agent profiles for the synthetic work-request and optional
  read-only Petstore MCP servers.
- Synthetic data and APIM mock policies for a safe sandbox.

## Source lineage

This workshop refreshes the earlier `agent-framework-mcp-demo` pattern. The classic persistent-agent code has been replaced with the current Foundry project endpoint and Agent Framework hosting model.

## References

- [Expose a REST API as an MCP server in Azure API Management](https://learn.microsoft.com/azure/api-management/export-rest-mcp-server)
- [MCP servers in Azure API Management](https://learn.microsoft.com/azure/api-management/mcp-server-overview)
- [Microsoft Foundry SDK overview](https://learn.microsoft.com/azure/foundry/how-to/develop/sdk-overview)
- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Foundry hosted-agent MCP sample](https://github.com/microsoft-foundry/foundry-samples/tree/main/samples/csharp/hosted-agents/agent-framework/mcp-tools)

## License

MIT

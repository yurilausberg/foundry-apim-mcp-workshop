# Prerequisites

## Local tools

- .NET 10 SDK or later.
- Node.js 22.19 or later for MCP Inspector.
- Azure CLI.
- Azure Developer CLI.
- Visual Studio Code.
- C# Dev Kit.
- Foundry Toolkit extension.
- GitHub Copilot access for the Visual Studio Code MCP test path.

## Azure access

- An API Management instance in Developer, Basic, Basic v2, Standard, Standard v2, Premium, or Premium v2.
- A Microsoft Foundry resource and project.
- A base model deployment, such as `gpt-4.1`, with approximately 250,000
  tokens per minute (TPM) or more allocated for concurrent workshop use.
- Sufficient model quota in the deployment region. Foundry model quota is
  shared at the subscription and region level.
- Foundry User on the project for development.
- Foundry Project Manager if project connections must be created or changed.
- Permission to configure APIs, MCP servers, policies, and diagnostics in APIM.

## Required values

| Value | Example |
|---|---|
| Foundry project endpoint | `https://resource.services.ai.azure.com/api/projects/project-name` |
| Model deployment name | `gpt-4.1` |
| APIM MCP endpoint | `https://instance.azure-api.net/work-requests-mcp/mcp` |

## Sandbox rules

- Use synthetic data only.
- Do not use customer credentials.
- Do not connect to production systems.
- Do not include secrets in `.env`, `mcp.json`, screenshots, or commits.
- Treat write operations as approval-required unless the facilitator explicitly enables them.

# Prerequisites

## Local tools

- .NET 10 SDK or later.
- Azure CLI.
- Azure Developer CLI.
- Visual Studio Code.
- C# Dev Kit.
- Foundry Toolkit extension.
- GitHub Copilot access for the Visual Studio Code MCP test path.

## Azure access

- An API Management instance in Developer, Basic, Basic v2, Standard, Standard v2, Premium, or Premium v2.
- A Microsoft Foundry resource and project.
- A deployed model.
- Foundry User on the project for development.
- Foundry Project Manager if project connections must be created or changed.
- Permission to configure APIs, MCP servers, policies, and diagnostics in APIM.

## Required values

| Value | Example |
|---|---|
| Foundry project endpoint | `https://resource.services.ai.azure.com/api/projects/project-name` |
| Model deployment name | `gpt-5.4-mini` |
| APIM MCP endpoint | `https://instance.azure-api.net/service-agreements-mcp/mcp` |

## Sandbox rules

- Use synthetic data only.
- Do not use customer credentials.
- Do not connect to production systems.
- Do not include secrets in `.env`, `mcp.json`, screenshots, or commits.
- Treat write operations as approval-required unless the facilitator explicitly enables them.

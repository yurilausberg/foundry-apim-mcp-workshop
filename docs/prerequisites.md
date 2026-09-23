# Prerequisites

## Local tools

- A participant laptop with installation rights, or all required tools
  preinstalled by IT.
- .NET 10 SDK or later.
- Node.js 22.19 or later for MCP Inspector.
- Git.
- Windows PowerShell, or PowerShell 7 on macOS and Linux.
- Azure CLI.
- Azure Developer CLI.
- Current stable Visual Studio Code.
- C# Dev Kit.
- Foundry Toolkit extension.
- GitHub Copilot access for the Visual Studio Code MCP test path.
- GitHub Copilot Chat **Agent** mode and workspace MCP servers enabled by
  organizational policy.
- A current browser such as Microsoft Edge or Google Chrome.

## Identity and network

- An Azure identity that can authenticate to the workshop tenant and
  subscription, including multifactor authentication.
- Network access to the Azure portal, Foundry project endpoint, and APIM
  gateway. Private resources require the appropriate VPN and private DNS.
- Outbound HTTPS on port 443 to `login.microsoftonline.com`,
  `portal.azure.com`, `management.azure.com`, `ai.azure.com`,
  `*.services.ai.azure.com`, `*.azure-api.net`, `github.com`,
  `api.githubcopilot.com`, `registry.npmjs.org`, `api.nuget.org`, and
  `marketplace.visualstudio.com`.
- A proxy or firewall configuration that permits Streamable HTTP responses
  from the APIM MCP endpoint.

## Azure access

- An API Management instance on a v2 SKU that supports MCP: Basic v2,
  Standard v2, or Premium v2.
- A Microsoft Foundry resource and project.
- A base model deployment, such as `gpt-4.1`, with approximately 250,000
  tokens per minute (TPM) or more allocated for concurrent workshop use.
- Sufficient model quota in the deployment region. Foundry model quota is
  shared at the subscription and region level.
- Foundry User on the project for development.
- Foundry Project Manager if project connections must be created or changed.
- Permission to configure APIs, MCP servers, policies, and diagnostics in APIM.

## Optional access

- A licensed Copilot Studio environment with permission to create or edit an
  agent and add MCP tools.
- Outbound HTTPS access to `petstore3.swagger.io` when using the optional
  public Petstore path.

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

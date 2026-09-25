# Workshop Guide

This page contains workshop preparation, schedule, and lab navigation. Each lab
has its own instructions and links to the previous and next lab.

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

- Prepare an APIM instance in a tier that supports MCP servers. Supported
  classic tiers include Developer, Basic, Standard, and Premium. Supported v2
  tiers include Basic v2, Standard v2, and Premium v2. Grant participants
  permission to configure APIs, MCP servers, policies, and diagnostics.
- Prepare a Microsoft Foundry project and grant participants the **Foundry
  User** role. Grant **Foundry Project Manager** only when participants must
  deploy a hosted agent version or create or modify project connections.
- Deploy a compatible model, such as `gpt-4.1` where available, and size its
  tokens-per-minute allocation for the expected participant count and prompt
  volume.
- Distribute the tenant, subscription, Foundry project endpoint, model
  deployment name, APIM service name, and resource group through an approved
  private channel. Do not place real environment values in this repository,
  slides, screenshots, or shared transcripts.
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
| [Lab 0](docs/labs/lab-0-confirm-sandbox.md) | Orientation, scenario, and sandbox confirmation | 60 minutes | Shared architecture, success criteria, and verified prerequisites |
| [Lab 1](docs/labs/lab-1-import-api.md) and [Lab 2](docs/labs/lab-2-expose-mcp.md) | Import the API and expose it through APIM as MCP tools | 90 minutes | Tested REST API and working MCP endpoint |
| [Lab 3](docs/labs/lab-3-foundry-agent.md) | Build and run the Foundry agent | 75 minutes | Code-first agent using MCP tools, an optional Petstore profile, and optional hosted deployment |
| [Lab 4](docs/labs/lab-4-copilot-studio.md) | Add the Copilot Studio path | 30 minutes | Low-code agent using the same tools |
| [Lab 5](docs/labs/lab-5-security-monitoring.md) | Apply security, governance, and monitoring | 45 minutes | Governed sandbox design |
| [Lab 6](docs/labs/lab-6-complete-pattern.md) | Demonstrate the complete pattern and define next steps | 45 minutes | End-to-end validation and action plan |

Breaks and lunch are outside the six hours of workshop content.

## Lab navigation

1. [Lab 0: Confirm the sandbox](docs/labs/lab-0-confirm-sandbox.md)
2. [Lab 1: Import the synthetic API](docs/labs/lab-1-import-api.md)
3. [Lab 2: Expose the API as an MCP server](docs/labs/lab-2-expose-mcp.md)
4. [Lab 3: Run the Foundry agent](docs/labs/lab-3-foundry-agent.md)
5. [Lab 4: Add the Copilot Studio path](docs/labs/lab-4-copilot-studio.md)
6. [Lab 5: Secure and monitor](docs/labs/lab-5-security-monitoring.md)
7. [Lab 6: Demonstrate the complete pattern](docs/labs/lab-6-complete-pattern.md)

## Closeout

Capture:

- Which production systems would replace the synthetic API.
- Which operations should be tools.
- Required user and workload identities.
- Approval boundaries for state-changing actions.
- Logging, retention, and support requirements.
- Pilot success criteria and accountable owner roles.

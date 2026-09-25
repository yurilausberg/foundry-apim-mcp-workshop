# Security

## Reporting

Do not open a public issue for a suspected vulnerability. Use the repository's
private vulnerability reporting flow on the **Security** tab when it is
available. Include reproduction details and affected files, but do not include
live credentials, tokens, personal data, or organization identifiers.

## Workshop data

This repository uses synthetic data only. Do not add customer data, credentials, tenant identifiers, subscription identifiers, or internal endpoints.

## Secrets

- Keep `.env` and `.vscode/mcp.json` local.
- Use Microsoft Entra ID or managed identity for production designs.
- Treat subscription keys as secrets and usage-control identifiers, not as user
  authentication.
- Do not use client secrets in public or interactive clients.
- Rotate any credential that appears in terminal output, screenshots, commits, or shared documents.

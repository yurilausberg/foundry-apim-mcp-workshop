# Security

## Reporting

Do not open a public issue for a suspected vulnerability. Contact the repository owner privately with reproduction details and affected files.

## Workshop data

This repository uses synthetic data only. Do not add customer data, credentials, tenant identifiers, subscription identifiers, or internal endpoints.

## Secrets

- Keep `.env` and `.vscode/mcp.json` local.
- Use Microsoft Entra ID or managed identity for production designs.
- Treat subscription keys and client secrets as temporary workshop credentials.
- Rotate any credential that appears in terminal output, screenshots, commits, or shared documents.

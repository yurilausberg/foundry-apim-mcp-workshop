# Lab 0: Confirm the sandbox

[Workshop overview](../../WORKSHOP.md) |
[Next: Lab 1, Import the synthetic API](lab-1-import-api.md)

1. Confirm the APIM instance uses a tier that supports MCP servers. Supported
   classic tiers include Developer, Basic, Standard, and Premium. Supported v2
   tiers include Basic v2, Standard v2, and Premium v2.
2. Confirm the Foundry project has a base model deployment, such as
   `gpt-4.1`.
3. Confirm the model deployment has enough tokens-per-minute capacity for the
   expected group size and prompt volume. Verify that the subscription has
   sufficient quota for that model in the deployment region.
4. Confirm the Foundry project is connected to a workspace-based Application
   Insights resource.
5. Confirm APIM diagnostics send gateway telemetry to Application Insights or
   Log Analytics.
6. Confirm the participant can open **Logs** for the Foundry Application
   Insights resource and query its linked Log Analytics workspace. The
   facilitator can use prepared smoke traffic if participant traffic does not
   exist yet.
7. Confirm Azure CLI authentication:

   ```powershell
   az account show --query state --output tsv
   az account get-access-token --resource https://ai.azure.com --query expiresOn --output tsv
   ```

   Do not paste account, tenant, or subscription details into shared logs or
   screenshots.

8. Confirm the participant has the Foundry User role on the project.
9. Confirm the lab will use synthetic data only.

---

[Workshop overview](../../WORKSHOP.md) |
[Next: Lab 1, Import the synthetic API](lab-1-import-api.md)

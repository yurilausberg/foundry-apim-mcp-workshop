# Lab 0: Confirm the sandbox

[Workshop overview](../../WORKSHOP.md) |
[Next: Lab 1, Import the synthetic API](lab-1-import-api.md)

1. Confirm the APIM instance uses a supported v2 SKU, such as Basic v2,
   Standard v2, or Premium v2.
2. Confirm the Foundry project has a base model deployment, such as
   `gpt-4.1`.
3. Confirm the model deployment has approximately 250,000 tokens per minute
   (TPM) or more allocated for the workshop. Verify that the subscription has
   sufficient quota for that model in the deployment region. This target
   reduces throttling when a larger participant group calls the model
   simultaneously. Adjust it for the expected group size and prompt volume.
4. Confirm Azure CLI authentication:

   ```powershell
   az account show --query "{subscription:name, tenant:tenantId}" --output table
   az account get-access-token --resource https://ai.azure.com --query expiresOn --output tsv
   ```

5. Confirm the participant has the Foundry User role on the project.
6. Confirm the lab will use synthetic data only.

---

[Workshop overview](../../WORKSHOP.md) |
[Next: Lab 1, Import the synthetic API](lab-1-import-api.md)

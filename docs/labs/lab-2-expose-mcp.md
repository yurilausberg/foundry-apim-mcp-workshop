# Lab 2: Expose the API as an MCP server

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 1, Import the synthetic API](lab-1-import-api.md) |
[Next: Lab 3, Run the Foundry agent](lab-3-foundry-agent.md)

1. In APIM, select **APIs**, then **MCP Servers**.
2. Select **Create MCP server**.
3. Select **Expose an API as an MCP server**.
4. Choose the API imported in [Lab 1](lab-1-import-api.md).
5. Select the operations to expose as tools. For the static work-request path,
   select all three operations. For Petstore, start with read-only operations.
6. Enter a **Display name**, such as `Work Request Tools`.
7. Confirm the required **Name** field contains a URL-safe value, such as
   `work-request-tools`.
8. Create the MCP server.
9. In the **MCP Servers** list, find the new server. In the **Server URL**
   column, select the **Copy to clipboard** icon.
10. Save the copied URL for the client configuration. It should end in `/mcp`.

Do not add the governance policy yet. First validate the basic MCP connection.
The policy is added once in
[Lab 5](lab-5-security-monitoring.md).

## Checkpoint

The MCP client must discover the operations selected as tools.

## Test with MCP Inspector

1. From a terminal, start the Inspector:

   ```powershell
   npx @modelcontextprotocol/inspector
   ```

2. If `npx` asks to install the package, confirm the installation.
3. Open the Inspector URL printed in the terminal.
4. Select **Streamable HTTP** as the transport.
5. Paste the APIM MCP server URL into the server URL field and select
   **Connect**.
6. Open **Tools**, select **List Tools**, and confirm these generated MCP tool
   names are available:

   - `createAWorkRequest`
   - `getAWorkRequest`
   - `updateWorkRequestStatus`

   These MCP tool names differ from two of the OpenAPI operation IDs confirmed
   in [Lab 1](lab-1-import-api.md). Use the names returned by **List Tools** in
   client allowlists.

7. Select a read-only tool, enter its required arguments, and run it. For the
   synthetic API, invoke `getAWorkRequest` with `WR-1001`.

## Test in Visual Studio Code

1. Copy `.vscode/mcp.json.example` to `.vscode/mcp.json`.
2. Replace the placeholder URL with the APIM MCP server URL.
3. Save `.vscode/mcp.json`. VS Code discovers the workspace MCP configuration
   and starts the server when its tools are needed. Do not start it manually.
4. Open Copilot Chat with **Ctrl+Alt+I** and select **Agent** mode.
5. Select the **Configure Tools** icon near the chat input.
6. Search for `work-request`.
7. Find `work-request-workshop` and select **Refresh Tools**.
8. Expand the server and select the checkbox for each tool:

   - `createAWorkRequest`
   - `getAWorkRequest`
   - `updateWorkRequestStatus`

9. If VS Code asks whether you trust the server, review the URL and confirm
   only if it is the workshop APIM endpoint.
10. Ask:

   ```text
   Get work request WR-1001 and summarize its current status.
   ```

11. For the optional Petstore path, uncomment the `petstore-anon` entry in
    `.vscode/mcp.json`, replace its placeholder URL with the copied Petstore MCP
    server URL, and save the file.
12. Select **Configure Tools** again, search for `petstore`, find
    `petstore-anon`, and select **Refresh Tools**.
13. Expand `petstore-anon` and select the checkbox for both read-only tools:

    - `findPetById`: Returns a single pet by its numeric ID.
    - `findsPetsByStatus`: Finds pets by status. Provide multiple status values
      as a comma-separated string, such as `pending,available`.

14. Ask:

    ```text
    Find pets that are currently available.
    ```

## Troubleshooting

- If `work-request-workshop` is not listed, confirm the file is named
  `.vscode/mcp.json`, not `.vscode/mcp.json.example`, and that it contains
  valid JSON.
- If the editor continues to show **Starting**, open Chat in **Agent** mode and
  select **Configure Tools**. If the three work-request tools are listed, the
  server is connected and the displayed status is stale.
- If the tools are not listed, select **Cancel** beside the starting server,
  run **Developer: Reload Window** from the Command Palette, then use
  **MCP: List Servers** to disable and re-enable the server.
- If MCP Inspector connects to the same URL but VS Code still shows
  **Starting**, do not recreate the APIM MCP server. Treat the problem as a
  VS Code client-state issue and use Inspector as the lab validation fallback.
  This behavior is tracked in
  [microsoft/vscode#336805](https://github.com/microsoft/vscode/issues/336805).
- To inspect a connection failure, run **MCP: List Servers**, select
  `work-request-workshop`, and choose **Show Output**.
- A `401` normally means the client did not send the required header or token.
- If streaming fails, disable frontend response-body logging at the global APIM
  scope.
- Do not access `context.Response.Body` in an MCP server policy.
- Confirm the URL uses the generated MCP server endpoint, not the original REST
  API base URL.

---

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 1, Import the synthetic API](lab-1-import-api.md) |
[Next: Lab 3, Run the Foundry agent](lab-3-foundry-agent.md)

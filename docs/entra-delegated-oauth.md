# Optional Microsoft Entra delegated OAuth

The baseline `policies/mcp-governance.xml` policy is intentionally anonymous.
It adds correlation, rate limiting, and tracing, but it does not validate an
access token.

Use `policies/mcp-governance-entra-delegated.xml` when the workshop reaches the
optional security extension. This policy replaces the baseline policy and
requires a delegated Microsoft Entra access token.

## What the policy does

The optional policy validates:

- The Microsoft Entra tenant that issued the token.
- The client application that requested the token.
- The token audience for the protected MCP API.
- A delegated scope in the `scp` claim.

The policy only validates tokens. It does not perform user sign-in, issue
tokens, or publish the OAuth discovery metadata used by MCP clients.

A bearer token and OAuth are not separate authentication modes. OAuth is the
flow that obtains the access token. Bearer describes how the client presents
that token to the MCP resource. The manual workshop test and client-managed MCP
OAuth both send the same `Authorization` header, and APIM validates the same
issuer, audience, client, and delegated-scope claims.

## App registration outline

Use two app registrations:

1. **MCP API registration**
   - Configure the registration as the protected web API.
   - Use version 2 access tokens.
   - Expose a delegated scope such as `Mcp.Access`.
2. **Interactive client registration**
   - Configure the redirect URI used by the interactive client.
   - Grant delegated permission to the MCP API scope.
   - Grant user or admin consent as required by the tenant.
   - Use Authorization Code with PKCE.
   - Do not create or embed a client secret for a public interactive client.

The value validated as the audience must exactly match the `aud` claim in the
issued access token. Depending on the app registration, this is commonly the
API application ID URI or the API application client ID.

## Create the APIM named values

Create these named values in the APIM instance:

| Named value | Value |
|---|---|
| `entra-tenant-id` | Microsoft Entra tenant ID |
| `entra-client-application-id` | Application client ID of the interactive OAuth client |
| `entra-mcp-audience` | Exact audience expected in the access token |
| `entra-mcp-scope` | Scope value expected in the `scp` claim, such as `Mcp.Access` |

These values are identifiers, not client secrets. The optional policy does not
need a client secret.

## Apply the optional policy

1. In APIM, select **APIs**, then **MCP Servers**, then select the workshop MCP
   server.
2. Open **Policies** and select the code editor.
3. Replace the baseline policy with
   `policies/mcp-governance-entra-delegated.xml`.
4. Select **Save**.
5. Connect without a token and confirm APIM returns HTTP 401.

Do not combine the baseline and Entra policy files. The Entra policy already
contains the same correlation, rate limiting, and tracing controls.

## Test with a manually supplied token

For a one-time workshop test, generating a delegated token through the APIM
developer portal is acceptable.

1. Acquire an access token for the MCP API delegated scope.
2. Start MCP Inspector:

   ```powershell
   npx @modelcontextprotocol/inspector
   ```

3. Select **Streamable HTTP** and enter the APIM MCP server URL.
4. Add a custom HTTP header:

   ```text
   Authorization: Bearer <access-token>
   ```

5. Connect, list the tools, and invoke a read-only tool.

Access tokens expire. Do not save a token in this repository, a shared workshop
file, screenshots, or shell history.

## Move to client-managed OAuth for repeated or production use

For repeated facilitator testing, use an MSAL-based public client that performs
interactive Authorization Code with PKCE and caches tokens for the signed-in
user. The resulting access token is still presented to APIM as a bearer token.
This removes the dependency on the developer portal, but MCP Inspector still
needs manual token injection unless the MCP server publishes OAuth discovery
metadata that Inspector can use.

For a client-native sign-in experience, add:

- OAuth protected resource metadata for the MCP resource.
- A 401 `WWW-Authenticate` challenge that points to that metadata.
- Authorization server metadata and a client registration compatible with the
  MCP client.

Microsoft Entra does not provide OAuth Dynamic Client Registration. Plan for a
pre-registered client or another supported client identification mechanism.
This is a separate extension from APIM access-token validation.

## Troubleshooting

| Result | Check |
|---|---|
| HTTP 401 before sign-in | Expected when the Authorization header is absent |
| HTTP 401 with a token | Check token expiry, tenant, `aud`, client application ID, and `scp` |
| Audience validation fails | Set `entra-mcp-audience` to the exact `aud` claim |
| Scope validation fails | Set `entra-mcp-scope` to the short scope value in `scp`, such as `Mcp.Access` |
| Anonymous clients stop working | Restore `policies/mcp-governance.xml` or configure the client to send a token |

## References

- [Validate Microsoft Entra tokens in API Management](https://learn.microsoft.com/azure/api-management/validate-azure-ad-token-policy)
- [Microsoft identity platform authorization code flow](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-auth-code-flow)
- [MCP authorization specification](https://modelcontextprotocol.io/specification/latest/basic/authorization)

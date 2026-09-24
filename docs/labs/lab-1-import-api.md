# Lab 1: Import the synthetic API

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 0, Confirm the sandbox](lab-0-confirm-sandbox.md) |
[Next: Lab 2, Expose the API as MCP](lab-2-expose-mcp.md)

1. Open the APIM instance in the Azure portal.
2. Select **APIs**, then **Add API**, then **OpenAPI**.
3. Import `openapi/work-request-api.yaml`.
4. Set the API URL suffix to `work-requests`.
5. Confirm these operation IDs were imported:

   - `getWorkRequest`
   - `createWorkRequest`
   - `updateWorkRequestStatus`

6. Open the API **Settings** tab, clear **Subscription required**, and save.
   This allows workshop clients to test without an APIM subscription key.
7. Switch to the **Design** tab and select **All operations**.
8. In **Inbound processing**, select the `</>` code editor.
9. Keep the existing `<base />` element and add the `mock-response` line
   directly after it:

   ```xml
   <inbound>
       <base />
       <mock-response status-code="200" content-type="application/json" />
   </inbound>
   ```

10. Select **Save**.
11. Confirm that **Mocking is enabled** appears for the API.
12. Test all three operations from the APIM test console.

Disabling **Subscription required** is for the isolated workshop sandbox only.
Production APIs should use an approved authentication and authorization model.

The API suffix supplies the `/work-requests` base path. The OpenAPI operation
paths are relative to that suffix, so the final PATCH URL is:

```text
https://<apim-name>.azure-api.net/work-requests/WR-1001/status
```

The OpenAPI request examples prepopulate `WR-1001` and
`status: ReadyForReview` in the APIM test console.

The response payloads are stored as examples in the OpenAPI document. Each
operation defines a `200 application/json` response. The API-level policy uses
APIM's built-in `mock-response` policy to select the matching example without a
backend service.

## Static mock limitations

The APIM `mock-response` policy demonstrates the API contract and MCP tool
shape. It is not a simulated data store:

- GET does not validate `requestId`. Any ID returns the same OpenAPI response
  example.
- POST does not create or persist a record.
- PATCH does not read the request body or update state. It returns the fixed
  response example.
- If APIM returns a schema-generated value such as the first `status` enum
  value (`Draft`), update or re-import the latest OpenAPI definition so APIM
  has the response examples.

Use a real sandbox backend or a conditional APIM policy if the workshop needs
record lookup, request validation, state changes, or negative-path behavior.

## Optional live backend: Swagger Petstore

For a workshop that needs real request processing without deploying a backend,
use the public Swagger Petstore API:

```text
https://petstore3.swagger.io/api/v3/openapi.json
```

1. Import the OpenAPI document from the URL instead of importing
   `openapi/work-request-api.yaml`.
2. Set an API URL suffix such as `petstore`.
3. Verify that the backend web service URL is:

   ```text
   https://petstore3.swagger.io/api/v3
   ```

4. Do not add the `mock-response` policy. Requests must reach the public
   backend.
5. Open the API **Settings** tab, clear **Subscription required**, and save.
   This allows workshop clients to test without an APIM subscription key.
6. For the MCP server, expose a small read-only tool set first, such as
   `findPetsByStatus` and `getPetById`.

Swagger Petstore is a third-party public demonstration service. It is not
operated, monitored, or covered by an availability commitment from the
workshop team. Its data, behavior, throttling, and availability can change
without notice. Do not send confidential data, credentials, personal data, or
production workloads to it.

## Checkpoint

For the static work-request path, the API must return synthetic JSON without
calling an organization backend. Participants should understand that the
responses are fixed contract examples.

For the optional Petstore path, APIM must successfully forward a read request
to the public backend. Participants should understand that the service is
external and uncontrolled.

---

[Workshop overview](../../WORKSHOP.md) |
[Previous: Lab 0, Confirm the sandbox](lab-0-confirm-sandbox.md) |
[Next: Lab 2, Expose the API as MCP](lab-2-expose-mcp.md)

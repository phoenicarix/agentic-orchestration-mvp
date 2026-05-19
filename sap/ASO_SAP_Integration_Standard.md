# ASO SAP Integration Standard

SAP endpoint inventory, APIM/Function wrapper rules, response contract, idempotency, security, and telemetry.

> GitHub path: `sap/ASO_SAP_Integration_Standard.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## SAP integration standard

SAP S/4HANA is the financial truth, but ASO never connects directly to SAP from AI agents, Power Automate, or Dataverse. All SAP access passes through Azure API Management and Azure Functions.

> **Note:** No autonomous ERP posting by any AI agent. Power Automate may call SAP write endpoints only after approval, idempotency, telemetry, and testing are proven.

### SAP preparation

- Create or confirm read-only communication user.
- Create or confirm write-capable communication user for approved commercial submissions only.
- Create communication system for Azure integration.
- Create communication arrangements for required APIs.
- Record arrangement ID, base URL, auth model, service path, owner, and environment.

### Endpoint inventory

| Capability | Endpoint | Method | Used by | Rule |
| --- | --- | --- | --- | --- |
| Business partner search | /sap/business-partner/search | GET | Foundry / lead agents | Read-only through APIM -> Function -> SAP. |
| Customer profile | /sap/customer-profile | GET | Foundry / opportunity agents | Normalize response before use. |
| Product affinity | /sap/products/by-customer | GET | Foundry | Used for recommendations. |
| Sales orders | /sap/sales-orders/by-customer | GET | Foundry | Used for account context. |
| Account commercial profile | /sap/account-commercial-profile | GET | Foundry | Read-only commercial context. |
| Prepare order | /sap/commercial/prepare-order | POST | Power Automate after approval path design | Preparation only; validate payload and idempotency. |
| Submit order | /sap/commercial/submit-order | POST | Power Automate only after approval | Write operation; idempotency required; no blind retry. |

### Wrapper endpoint response contract

```text
{
  "message_id": "uuid",
  "correlation_id": "uuid",
  "status": "success|failed|partial_success",
  "data": {},
  "error": {
    "code": "",
    "message": "",
    "retryable": true,
    "origin": "sap-wrapper"
  },
  "trace_id": "string"
}
```

### Idempotency rules

- Every write-path request must include an idempotency key.
- The idempotency key must be stored with the Pending Commercial Action.
- Do not blindly retry create/submit operations without idempotency verification.
- Commercial submit requires human approval before APIM call.
- SAP document references must be written back to Dataverse after successful response.

### Security and observability

- Function App uses managed identity where possible.
- Secrets live in Key Vault, not Power Automate or GitHub.
- APIM validates JWT and passes correlation headers.
- Application Insights logs request counts, dependency duration, timeout rate, and failures.
- Failed submissions preserve payload, error message, trace ID, and create Integration Operator task.

# ASO Azure, APIM, Functions, and Telemetry Standard

Azure resource naming, APIM policies, Function App foundation, Key Vault, telemetry, and HTTP Entra connection timing.

> GitHub path: `azure/ASO_Azure_APIM_Functions_Foundation_Standard.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## Azure, APIM, Functions, Key Vault, and telemetry foundation

This file defines the Azure resource foundation and APIM/Function boundary used for SAP integration and future orchestration support. It intentionally avoids fake endpoint values. Authenticated connection references are created only when real endpoints and Entra audience values exist.

### Azure naming pattern

| Resource | Pattern | Example | Notes |
| --- | --- | --- | --- |
| Resource group | rg-<workload>-<env>-<region>-<###> | rg-aso-dev-weu-001 | Container for ASO Azure assets. |
| Function App | func-<workload>-<purpose>-<env>-<region>-<###> | func-aso-sap-dev-weu-001 | Hosts SAP wrapper endpoints. |
| API Management | apim-<workload>-<env>-<region>-<###> | apim-aso-dev-weu-001 | API gateway boundary for SAP wrapper. |
| Key Vault | kv-<workload>-<env>-<region>-<###> | kv-aso-dev-weu-001 | Store secrets; never commit secret values. |
| Application Insights | appi-<workload>-<env>-<region>-<###> | appi-aso-dev-weu-001 | Telemetry and diagnostics. |
| Storage account | lowercase alphanumeric only | stasoapidevweu001 | Function/runtime storage if required. |

### Foundation resources

- Create resource group only when the Azure subscription, tenant, and naming are approved.
- Create Application Insights early if Foundry/Functions telemetry will be linked.
- Create Key Vault before secrets are needed; never store secrets in GitHub or environment variables.
- Create Function App and APIM during the SAP/APIM phase when API contracts are defined.
- Create HTTP with Microsoft Entra ID connection references only when real base URLs and Entra resource URIs exist.

### HTTP with Microsoft Entra ID connection guidance

| Connection reference | Create now? | Why |
| --- | --- | --- |
| ASO Foundry HTTP Entra Connection Reference | No until real endpoint/audience exists | The Base Resource URL must be the real orchestrator API endpoint; the Resource URI must be the real Entra App ID URI. |
| ASO APIM HTTP Entra Connection Reference | No until APIM/SAP wrapper exists | The Base Resource URL must be the real APIM URL; the Resource URI must be the protected API audience. |

### APIM policy requirements

- Validate JWT on inbound requests.
- Require x-correlation-id and x-message-id.
- Apply rate limits and backend routing.
- Preserve trace IDs outbound.
- Retry only safe read operations.
- Do not retry non-idempotent write operations unless an idempotency key exists.

### Telemetry requirements

| Metric / signal | Why |
| --- | --- |
| Function request count | Baseline usage and capacity. |
| SAP dependency duration | Detect downstream slowness. |
| SAP timeout rate | Identify retry/stability issues. |
| APIM failures | Detect auth, policy, or backend failures. |
| Foundry orchestration latency | Evaluate AI/orchestration responsiveness. |
| Invalid schema output rate | Catch schema contract failures. |
| Customer Insights journey activation failures | Detect communication-plane start issues. |

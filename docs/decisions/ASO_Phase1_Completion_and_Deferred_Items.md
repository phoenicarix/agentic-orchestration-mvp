# ASO Phase 1 Completion and Deferred Items

Public-safe record of what Phase 1 completed, what was deferred, and which architecture boundaries remain protected.

> GitHub path: `docs/decisions/ASO_Phase1_Completion_and_Deferred_Items.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## Phase 1 completion and deferred-items register

This document records the completed Phase 1 foundation and the items intentionally deferred because they either do not exist in the trial or require real endpoints, app registrations, or later-phase architecture.

### Phase 1 completed foundation

| Area | Result |
| --- | --- |
| Main build environment | Phoenicarix-Sales |
| Customer Insights trial dependency | Phoenicarix-CI |
| Default environment | Not used for ASO build |
| Publisher | Agentic Sales Orchestrator; prefix aso |
| Solutions | ASO.Core, ASO.Automation, ASO.Operations |
| Environment variables | 16 ASO variables in ASO.Automation with exact schema contract |
| Connection references created | Dataverse, Teams Alerts, Outlook Internal Notification |
| Boundary preserved | No HubSpot, no direct SAP, no Outlook/Power Automate lifecycle sends, no ERP submit active |

### Deferred items

| Item | Status | Reason | When to create |
| --- | --- | --- | --- |
| SIT/UAT/Pre-Prod/Prod | Not applicable for trial MVP | MVP is public demo/trial and not production-intended. | Only if the project becomes a production programme. |
| ASO Approvals Connection Reference | Deferred | Approvals connector unavailable in current trial picker. | When connector appears or approval flow is built. |
| ASO Foundry HTTP Entra Connection Reference | Deferred | No real Foundry orchestrator endpoint or Entra App ID URI yet. | Foundry/API phase. |
| ASO APIM HTTP Entra Connection Reference | Deferred | No real APIM/SAP wrapper endpoint or Entra App ID URI yet. | SAP/APIM phase. |
| Customer Insights journeys | Not started | Separate Customer Insights trial dependency; later communication plane phase. | Phase 4. |
| SAP wrapper/APIM | Not started | SAP must only be accessed through APIM + Functions. | Phase 5. |
| Power Automate orchestration flows | Not started | Need schema, choices, and contracts first. | Phase 8. |
| HubSpot | Not started | HubSpot must be implemented last. | Phase 11. |

### Immediate remediation rule

If the environment variables were created with incorrect schema names and no flows or integrations depend on them yet, recreate them now. After dependencies exist, schema change becomes controlled change management.

| Incorrect / old variable | Required schema name | Reason |
| --- | --- | --- |
| aso_APIMBaseURL | aso_ApimBaseUrl | Acronym casing standard. |
| aso_CustomerInsightsComplianceProfile | aso_ComplianceProfileName | Must match Phase 1 contract. |
| aso_FeatureFlagERPSubmit | aso_FeatureFlagErpSubmit | Acronym casing standard. |
| aso_FeatureFlagSAPReads | aso_FeatureFlagSapReads | Acronym casing standard. |
| aso_FoundryAPIAudience | aso_FoundryApiAudience | Acronym casing standard. |
| aso_FoundryOrchestratorURL | aso_FoundryOrchestratorUrl | URL becomes Url in env variable schema. |
| aso_SAPWrapperVersion | aso_SapWrapperVersion | SAP becomes Sap in env variable schema. |

### Approval gates

| Gate | Approver | Approval question |
| --- | --- | --- |
| Naming gate | Solution Architect + CRM Architect | Do component names and schema names follow the standard exactly? |
| Schema gate | CRM Architect + Security Architect | Are data types, security, keys, and duplicate detection approved? |
| Communication gate | Marketing Ops + Compliance | Are consent, compliance profile, preference center, and journey controls valid? |
| SAP gate | Integration Architect + Security Architect | Is all SAP access through APIM/Functions with idempotency and telemetry? |
| AI gate | AI Architect + Solution Architect | Do agents follow responsibility boundaries and evaluation standards? |
| Release gate | Platform Owner + Stream Leads | Are tests, runbooks, monitoring, rollback, and approvals complete? |

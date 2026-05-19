# ASO Operations, Testing, ALM, and Release Gates

Observability, dashboards, alerts, E2E tests, ALM, customer-ready checklist, and approval gates.

> GitHub path: `docs/architecture/ASO_Operations_Testing_ALM_and_Release_Gates.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## Operations, testing, ALM, and release gates

This document centralizes the operational standards that are distributed throughout the end-to-end guide: observability, tests, ALM, release evidence, runbooks, and approval gates.

### End-to-end testing before HubSpot

| Test | Expected result | Pass criteria |
| --- | --- | --- |
| New lead, no SAP match | Lead simulated in Dataverse; Foundry qualifies; SAP search no match; Dataverse updated; Lead Nurture starts if eligible. | No duplicate lead; Agent Run Log exists; AI fields populated; Communication State correct; Journey ledger row exists. |
| Existing SAP customer lead | Lead matches SAP; Foundry calls SAP wrapper via APIM; existing customer review flagged if needed; seller task created. | No direct ERP write; SAP context normalized; review flag set if ambiguous. |
| Opportunity risk and deal guidance | Opportunity agent generates risk/recommendation; Foundry validates; Power Automate updates fields and creates task. | Seller sees guidance; no unauthorized communication or ERP action. |
| Commercial approval and SAP submit | Pending action created; approval required; APIM called after approval; SAP reference returned; Dataverse updated. | Approval captured; idempotency key present; SAP reference persisted; no blind retry. |
| Sales agent and Foundry disagree | Sales Qualification Agent says strong fit but SAP/Foundry says ambiguous; Foundry requires review. | No automatic AE routing; review task created; disagreement logged. |

### Operations dashboards

- Runs by status.
- Failed runs by component.
- Average confidence by agent type.
- Leads requiring human review.
- Opportunities high risk.
- Journey failures.
- SAP submit failures.
- Pending commercial approvals.

### Alerts

- SAP wrapper 5xx spike.
- APIM unauthorized calls.
- Foundry failure rate above threshold.
- Invalid JSON/schema failures.
- Flow retry storm.
- Customer Insights send volume drop.
- Journey start failures.
- Unsubscribe/bounce anomaly.
- Pending commercial actions stuck beyond SLA.

### ALM and release standard

| Stage | Standard |
| --- | --- |
| Build | Build unmanaged only in development/trial build environment, inside approved ASO solutions. |
| Export | Export manual backups after major milestones in trial; use pipelines for enterprise target. |
| Promotion | Enterprise target uses managed solutions from DEV through SIT/UAT/PREPROD/PROD. |
| Versioning | Trial build starts at 0.1.0.0; release candidates use 1.0.0-rc where applicable; production uses semantic versioning. |
| Source control | Use Git integration, SolutionPackager, Power Platform CLI, or pipelines where available in enterprise delivery. |
| Public GitHub | Publish only sanitized architecture, standards, sample schemas, mock data, and public-safe screenshots. |

### Customer-ready implementation checklist

| Checkpoint | Required evidence |
| --- | --- |
| Architecture boundaries approved | HubSpot ingress only, Dataverse operational truth, Customer Insights only sends, Foundry orchestrates, Power Automate executes, SAP via APIM/Functions. |
| Naming standard approved | Schema names, variables, choices, flows, views, Azure resources, journeys, agents, and APIs use the standard. |
| Phase 1 complete | Publisher, solutions, environment variables, connection references, control notes, and trial constraints validated. |
| Dataverse schema complete | Global choices, Lead/Opportunity/Account/Contact extensions, custom log/ledger tables, keys, duplicate detection. |
| Sales workspace complete | Forms, tabs, sections, views, roles, field-level security and seller experience validated. |
| Customer Insights plane ready | Compliance profile, domain, triggers, segments, emails, journeys, consent writeback and ledger configured. |
| SAP boundary ready | Azure resources, APIM, Functions, Key Vault, SAP wrapper, endpoints, idempotency, telemetry and API tests complete. |
| Sales AI agents ready | Sales Qualification and Opportunity/Deal Close agents configured in governed mode and tested. |
| Foundry ready | Parent orchestrator, child agents, canonical schemas, tool controls, safety policy, evaluation datasets validated. |
| Power Automate ready | Solution-aware flows, env variables, connection references, scopes, retries, logs, approvals tested. |
| Operations ready | Dashboards, alerts, SLAs, runbooks, support roles, replay process, incident response ready. |
| E2E tests passed without HubSpot | Lead, SAP context, opportunity risk, approval/SAP submit, disagreement safety tests passed. |
| HubSpot last | Ingress mapping tested only after core ASO path is stable. |

### Approval gates

| Gate | Approver | Approval question |
| --- | --- | --- |
| Naming gate | Solution Architect + CRM Architect | Do component names and schema names follow the standard exactly? |
| Schema gate | CRM Architect + Security Architect | Are data types, security, keys, and duplicate detection approved? |
| Communication gate | Marketing Ops + Compliance | Are consent, compliance profile, preference center, and journey controls valid? |
| SAP gate | Integration Architect + Security Architect | Is all SAP access through APIM/Functions with idempotency and telemetry? |
| AI gate | AI Architect + Solution Architect | Do agents follow responsibility boundaries and evaluation standards? |
| Release gate | Platform Owner + Stream Leads | Are tests, runbooks, monitoring, rollback, and approvals complete? |

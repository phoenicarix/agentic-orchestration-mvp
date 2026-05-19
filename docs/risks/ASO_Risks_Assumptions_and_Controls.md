# ASO Risks, Assumptions, and Controls

Risk register, mitigation model, customer-ready implementation checklist, governance cadence, and operating controls.

> GitHub path: `docs/risks/ASO_Risks_Assumptions_and_Controls.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## Risks, assumptions, and control model

This document gives the project team a public-safe risk register and mitigation model. It converts the architecture boundaries into controls that can be reviewed during customer conversations and implementation checkpoints.

### Risk register

| Risk | Impact | Mitigation | Control evidence |
| --- | --- | --- | --- |
| Schema names created incorrectly | Flows, integrations, reports, tests, and documentation may bind to wrong names. | Naming gate before creation; delete/recreate before dependencies exist. | Schema contract table and review sign-off. |
| Default environment used for ASO | ALM confusion, unmanaged components, poor supportability. | Use only Phoenicarix-Sales for build; default environment is not used. | Solution screenshots and repo decision log. |
| Customer emails sent via Outlook/Power Automate | Compliance and consent breach. | CIJ is the only outbound lifecycle communication layer. | Journey catalogue and flow design review. |
| Direct SAP access | Security, audit, and idempotency risk. | SAP only through APIM + Azure Functions. | APIM policy and wrapper test evidence. |
| HubSpot implemented too early | Breaks delivery order and data control. | HubSpot last; Dataverse truth after ingress. | Architecture checkpoint before HubSpot phase. |
| Personal connections treated as production-ready | Operational ownership and access risk. | Use connection references and service ownership model. | Connection inventory and deployment review. |
| No correlation IDs | Difficult support and incident triage. | Mandatory message_id, correlation_id, trace_id. | Run log validation and monitoring alerts. |
| Sales AI treated as final authority | Unreviewed recommendations may drive risky actions. | Sales agents are advisory; Foundry validates; humans approve high-impact actions. | Safety policy tests and review tasks. |

### Control principles

- Every high-impact commercial action must pass through seller or manager approval.
- Every external call must carry message_id, correlation_id, and trace_id where supported.
- Every customer lifecycle communication must execute through Customer Insights - Journeys.
- Every SAP read/write path must be mediated through APIM and Azure Functions.
- Every schema name is reviewed before creation and frozen after dependencies exist.
- Every flow must be solution-aware, use connection references, use environment variables, and write diagnostic logs.

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

### Operating review cadence

| Cadence | Attendees | Purpose | Outputs |
| --- | --- | --- | --- |
| Weekly operations review | Ops Lead, Power Platform Lead, CRM Architect, AI Architect | Review failures, risks, blockers, and runbook gaps. | Action log, blocker status, incident follow-ups. |
| Monthly architecture review | Solution Architect, CRM, AI, Integration, Security, Marketing Ops | Approve design changes, standards, and exceptions. | Decision log, updated architecture notes. |
| Quarterly strategy review | Executive sponsor, platform owner, stream leads | Assess roadmap, value, KPIs, and scaling decisions. | Roadmap changes, funding decisions, governance maturity plan. |

# ASO HubSpot Ingress-Last Standard

HubSpot ingress-only boundary, field mappings, prerequisites, restrictions, and acceptance criteria.

> GitHub path: `hubspot/ASO_HubSpot_Ingress_Last_Standard.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## HubSpot ingress-last standard

HubSpot is upstream source-only for lead/contact ingress. It must not own opportunity progression, lifecycle communications, SAP actions, AI orchestration, or seller truth after ingestion.

> **Note:** HubSpot is implemented last. The controlled Dynamics/Dataverse, Customer Insights, SAP/APIM, Sales AI, Foundry, and Power Automate path must be proven before upstream ingress is enabled.

### HubSpot rules

| Rule | Standard |
| --- | --- |
| Sync direction | HubSpot -> Dynamics Lead for first release. |
| Required mappings | HubSpot contact ID -> aso_hubspotcontactid; source -> aso_hubspotsource; email -> emailaddress1; phone -> telephone1; company -> companyname; title -> jobtitle. |
| Restrictions | No opportunity writeback unless explicitly approved; no lifecycle customer communications after Dynamics ingress; no overwrite of seller-owned fields; no duplicate creation on retry. |
| Validation | Test one lead, confirm duplicate detection, confirm Power Automate lead flow runs, confirm HubSpot does not own opportunity progression. |

### HubSpot delivery prerequisites

- Dataverse schema and duplicate detection are complete.
- Customer Insights communication plane is configured and consent-aware.
- SAP/APIM read and write boundaries are tested.
- Sales AI and Foundry orchestration are governed and validated.
- Power Automate Lead flow is stable.
- E2E tests pass without HubSpot.

### Ingress acceptance criteria

| Acceptance point | Required evidence |
| --- | --- |
| No duplicate lead | HubSpot contact ID alternate key or governed duplicate detection works. |
| Dataverse ownership | Lead owner, status, source, and ASO fields are correctly populated after ingress. |
| No downstream ownership | HubSpot does not write opportunity progression or journey ownership. |
| No lifecycle sends | HubSpot is not used for lifecycle customer communication after Dynamics ingress. |
| Operational trace | Lead flow records message_id/correlation_id and Agent Run Log where applicable. |

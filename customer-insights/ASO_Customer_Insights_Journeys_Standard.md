# ASO Customer Insights - Journeys Standard

Compliance profile, triggers, segments, email assets, journeys, consent governance, and writeback rules.

> GitHub path: `customer-insights/ASO_Customer_Insights_Journeys_Standard.md`

> Public safety: do not publish tenant IDs, credentials, secrets, real customer data, private endpoints, or connection strings.

## Customer Insights - Journeys communication plane

Customer Insights - Journeys is the only outbound customer lifecycle communication execution layer. This file defines compliance, triggers, segments, emails, journeys, writeback, and data boundaries.

> **Note:** No customer lifecycle emails may be sent through Outlook, Power Automate, Foundry, Sales AI agents, or HubSpot. Customer Insights - Journeys owns outbound lifecycle communication.

### Communication assets

| Asset | Naming / standard | Required rule |
| --- | --- | --- |
| Compliance profile | Global Commercial or region-specific equivalent | Use approved preference center, purpose Sales Lifecycle, topics Lead, Qualified Lead, Opportunity, Quote Proposal, Order, Onboarding, Retention, Expansion. |
| Triggers | ASO <Lifecycle Action> Requested | Attributes: recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName. |
| Segments | <Entity/Audience> - <Lifecycle> - <Readiness> | Use normalized Dataverse fields: lifecycle stage, communication state, participation status, email consent status. |
| Emails | EML - <Lifecycle> - <Purpose> | Authenticated domain, compliance profile, unsubscribe/preference link, approved Dataverse fields only. |
| Journeys | JRNY - ASO - <Lifecycle> - v<major> | Publish only after test send and compliance validation. |
| Writeback | Normalize into Dataverse fields and Journey Participation Ledger | Record starts, completions, exits, failures, and meaningful interactions. |

### Recommended lifecycle triggers

| Trigger | Attributes |
| --- | --- |
| ASO Lead Nurture Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |
| ASO Qualified Lead Follow Up Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |
| ASO Opportunity Progression Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |
| ASO Quote Proposal Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |
| ASO Order Confirmation Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |
| ASO Onboarding Requested | recordType, recordId, correlationId, lifecycleStage, journeyKey, complianceProfileName |

### Recommended segments

| Segment | Audience | Core conditions |
| --- | --- | --- |
| Lead - Nurture Ready | Lead | Lifecycle Communication Stage = Lead; Communication State = Eligible; Journey Participation Status is not Active; Email Consent Status = OptedIn. |
| Qualified Lead - Follow Up Ready | Lead or Contact | Lifecycle Communication Stage = QualifiedLead; Communication State = Eligible; Email Consent Status = OptedIn; Journey Participation Status is not Active. |
| Opportunity - Proposal Ready | Contact with related opportunity | Related opportunity stage = Opportunity or QuoteProposal; Communication State = Eligible; Email Consent Status = OptedIn. |
| Order - Onboarding Ready | Contact | Lifecycle Communication Stage = Order or Onboarding; Communication State = Eligible; Email Consent Status = OptedIn. |
| Account - Retention Ready | Account or Contact | Retention readiness and consent conditions per operating model. |
| Account - Expansion Ready | Account or Contact | Expansion readiness and consent conditions per operating model. |

### Email asset rules

- Every email must use an authenticated sending domain.
- Every email must use the approved compliance profile.
- Every email must include an unsubscribe or preference center link.
- Personalization must use approved Dataverse fields only.
- SAP-derived facts may appear only after normalization into Dataverse fields.
- AI draft text may be used only after approval or governed template review.

### Journey catalogue

| Journey | Name | Entry | Critical rule |
| --- | --- | --- | --- |
| Lead Nurture | JRNY - ASO - Lead Nurture - v1 | Lead - Nurture Ready or ASO Lead Nurture Requested | Publish only after test send and compliance validation. |
| Qualified Lead Follow-Up | JRNY - ASO - Qualified Lead Follow Up - v1 | Trigger or segment | On engagement, raise task or writeback action. |
| Opportunity Progression | JRNY - ASO - Opportunity Progression - v1 | ASO Opportunity Progression Requested | Write participation and interaction signals back to Dataverse. |
| Quote Proposal | JRNY - ASO - Quote Proposal - v1 | ASO Quote Proposal Requested | Trigger follow-up task if no engagement after defined period. |
| Order Confirmation | JRNY - ASO - Order Confirmation - v1 | After SAP order reference is persisted in Dataverse | Never send before SAP response is validated. |
| Onboarding | JRNY - ASO - Onboarding - v1 | Order/Onboarding readiness | Use validated order and onboarding state only. |

### Writeback model

Power Automate must maintain normalized Dataverse fields: Communication State, Journey Participation Status, Last Journey ID/Name, Last Interaction Type, Last Interaction On, Consent Status, and Communication Hold Reason. Create a Journey Participation Ledger row for each journey start, completion, exit, failure, and meaningful interaction.

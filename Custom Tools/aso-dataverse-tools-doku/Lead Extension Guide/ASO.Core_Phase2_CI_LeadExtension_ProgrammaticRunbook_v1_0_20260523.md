# Agentic Sales Orchestrator
## Programmatic Lead Table Extension Runbook

**Version:** v1.0 - 2026-05-23  
**Environment:** Phoenicarix-CI (`https://phoenicarix-ci.crm4.dynamics.com`)  
**Solution:** ASO.Core (`ASOCore`)  
**Target table:** Lead (`lead`)  
**Audience:** Product Owner, Customer IT, Dynamics 365/Power Platform developers, operations/support.
## 1. Executive summary

We extended the existing Microsoft Dataverse **Lead** table programmatically instead of creating each custom column manually in the Power Apps maker UI. The target was the Agentic Sales Orchestrator Phase 2 data model in **ASO.Core**. The script connected to the **Phoenicarix-CI** Dataverse environment, checked whether each `aso_` Lead column already existed, created missing columns through Dataverse metadata requests, associated the created columns with the `ASO.Core` solution, and published the Lead table customizations.

In simple terms: instead of clicking **New column** more than thirty times, we gave Dataverse a controlled, repeatable instruction list.
## 2. Why we used a script instead of Copilot or manual UI

| Option | What happened / reason | Decision |
|---|---|---|
| Copilot in Create new tables workspace | It could not reliably modify the existing standard Lead table and returned errors. | Not used for implementation. |
| Manual UI column creation | Safe, but very slow and error-prone for 30+ fields. | Avoided where possible. |
| Programmatic Dataverse metadata creation | Fast, repeatable, validates existing columns, keeps schema names consistent, and can target a solution. | Chosen for Lead extension. |
## 3. Explanation for different audiences

| Audience | Explanation |
|---|---|
| Grandma | We added many new labeled drawers to the Lead record. Instead of adding each drawer by hand, we used a small program to add them correctly. |
| Product Owner | We created the data foundation needed for AI qualification, Foundry orchestration, SAP context, Customer Insights governance, and sales-agent output tracking on Leads. |
| Developer | We used a .NET 8 console application with `Microsoft.PowerPlatform.Dataverse.Client` and Dataverse metadata requests, especially `CreateAttributeRequest`, to create Lead attributes in `ASO.Core`. |
| Admin / Operations | The script is repeatable and skips existing fields, so it can be safely rerun after partial execution, then validated in `ASO.Core -> Tables -> Lead -> Columns`. |
## 4. Scope and non-scope

### In scope

- Connect to Phoenicarix-CI.
- Target solution `ASO.Core`.
- Target table `lead`.
- Create missing `aso_` Lead fields from the Phase 2 Lead extension.
- Reuse existing global choices where defined.
- Create local choices where the values belong only to the Lead column.
- Publish Lead table metadata after creation.

### Not in scope

- No HubSpot integration.
- No SAP direct connection.
- No Customer Insights journey activation.
- No outbound customer email sending.
- No Foundry orchestration call.
- No form layout changes yet.
- No production deployment.
## 5. Prerequisites

| Prerequisite | Status / target |
|---|---|
| Environment | Phoenicarix-CI |
| Dataverse URL | `https://phoenicarix-ci.crm4.dynamics.com` |
| Solution | ASO.Core |
| Solution unique name | `ASOCore` |
| Publisher prefix | `aso` |
| Target table | `lead` |
| Global choices | Created before running the Lead extension script |
| Local machine | macOS with .NET 8 installed |
| Authentication | Interactive OAuth login with the Power Apps user |
## 6. Global choices used by the Lead extension

These global choices must exist before the script runs, because several Lead columns reference them. Dataverse may display names in lowercase internally. The script uses the lowercase logical names.

| Global choice logical name used by script | Used by Lead column | Values / purpose |
|---|---|---|
| `aso_aistatus` | AI Agent Status | NotStarted, Running, Completed, Escalated, Failed, PartiallyCompleted, Blocked |
| `aso_communicationstate` | Communication State | NotEligible, Eligible, Queued, InJourney, WaitingInteraction, Completed, Suppressed, Failed, Blocked |
| `aso_lifecyclecommunicationstage` | Lifecycle Communication Stage | Lead, QualifiedLead, Opportunity, QuoteProposal, Order, Onboarding, Retention, Expansion |
| `aso_journeyparticipationstatus` | Journey Participation Status | NotStarted, Active, Paused, Completed, Exited, Error |
| `aso_emailconsentstatus` | Email Consent Status | OptedIn, OptedOut, Pending, Unknown, Blocked |

## 7. Lead fields created programmatically

| Display name | Schema/logical name | Dataverse type | Script metadata class | Notes |
|---|---|---|---|---|
| HubSpot Contact ID | `aso_hubspotcontactid` | Text | `StringAttributeMetadata` | External ingress key; HubSpot not configured yet. |
| HubSpot Source | `aso_hubspotsource` | Text | `StringAttributeMetadata` | Ingress source reference; kept as text for MVP flexibility. |
| SAP Business Partner ID | `aso_sapbusinesspartnerid` | Text | `StringAttributeMetadata` | SAP reference only. |
| SAP Customer ID | `aso_sapcustomerid` | Text | `StringAttributeMetadata` | SAP customer reference only. |
| AI Fit Level | `aso_aifitlevel` | Choice | `Local PicklistAttributeMetadata` | Local values: Strong, Moderate, Weak. |
| AI Lead Score | `aso_aileadscore` | Whole number | `IntegerAttributeMetadata` | Range 0-100. |
| AI Qualification Rationale | `aso_aiqualificationrationale` | Multiline text | `MemoAttributeMetadata` | Seller-facing AI rationale. |
| AI Outreach Draft | `aso_aioutreachdraft` | Multiline text | `MemoAttributeMetadata` | Draft only; not direct-send. |
| AI Routing Decision | `aso_airoutingdecision` | Choice | `Local PicklistAttributeMetadata` | Local values: Nurture, SDR, AE, Reject, NeedsReview, ExistingAccountReview. |
| AI Confidence | `aso_aiconfidence` | Decimal | `DecimalAttributeMetadata` | Range 0.00-1.00; precision 2. |
| AI Last Run On | `aso_ailastrunon` | Date and time | `DateTimeAttributeMetadata` | User-local timestamp. |
| AI Agent Status | `aso_aiagentstatus` | Choice | `Global PicklistAttributeMetadata` | Uses existing global choice aso_aistatus. |
| AI SAP Context Summary | `aso_aisapcontextsummary` | Multiline text | `MemoAttributeMetadata` | SAP-derived summary after governed integration. |
| AI SAP Match Flag | `aso_aisapmatchflag` | Yes/No | `BooleanAttributeMetadata` | Default No. |
| AI Correlation ID | `aso_aicorrelationid` | Text | `StringAttributeMetadata` | Last orchestration/integration correlation id. |
| Sales Qualification Agent Status | `aso_salesqualificationagentstatus` | Choice | `Local PicklistAttributeMetadata` | Local values: NotStarted, Running, Completed, Failed, NeedsReview. |
| Sales Qualification Agent Score | `aso_salesqualificationagentscore` | Whole number | `IntegerAttributeMetadata` | Range 0-100. |
| Sales Qualification Agent Rationale | `aso_salesqualificationagentrationale` | Multiline text | `MemoAttributeMetadata` | Sales Qualification Agent output. |
| Sales Qualification Agent Last Run On | `aso_salesqualificationagentlastrunon` | Date/time | `DateTimeAttributeMetadata` | User-local timestamp. |
| Foundry Final Qualification Decision | `aso_foundryfinalqualificationdecision` | Text | `StringAttributeMetadata` | MVP text field because taxonomy may evolve. |
| Foundry Review Required | `aso_foundryreviewrequired` | Yes/No | `BooleanAttributeMetadata` | Human review gate; default No. |
| Communication State | `aso_communicationstate` | Choice | `Global PicklistAttributeMetadata` | Uses existing global choice aso_communicationstate. |
| Lifecycle Communication Stage | `aso_lifecyclecommunicationstage` | Choice | `Global PicklistAttributeMetadata` | Uses existing global choice aso_lifecyclecommunicationstage. |
| Journey Participation Status | `aso_journeyparticipationstatus` | Choice | `Global PicklistAttributeMetadata` | Uses existing global choice aso_journeyparticipationstatus. |
| Customer Insights Journey ID | `aso_customerinsightsjourneyid` | Text | `StringAttributeMetadata` | Latest journey reference. |
| Customer Insights Journey Name | `aso_customerinsightsjourneyname` | Text | `StringAttributeMetadata` | Latest journey name. |
| Customer Insights Last Entry On | `aso_customerinsightslastentryon` | Date/time | `DateTimeAttributeMetadata` | Last journey entry timestamp. |
| Customer Insights Last Interaction On | `aso_customerinsightslastinteractionon` | Date/time | `DateTimeAttributeMetadata` | Last Customer Insights interaction timestamp. |
| Customer Insights Last Interaction Type | `aso_customerinsightslastinteractiontype` | Choice | `Local PicklistAttributeMetadata` | Local values: EmailSent, Open, Click, FormSubmit, Reply, Unsubscribe, Bounce, CustomAction. |
| Email Consent Status | `aso_emailconsentstatus` | Choice | `Global PicklistAttributeMetadata` | Uses existing global choice aso_emailconsentstatus. |
| Compliance Profile Name | `aso_complianceprofilename` | Text | `StringAttributeMetadata` | Customer Insights compliance profile name. |
| Communication Hold Reason | `aso_communicationholdreason` | Multiline text | `MemoAttributeMetadata` | Suppression / hold reason. |

## 8. What the script does step by step

1. Defines the target Dataverse environment URL.
2. Defines the target unmanaged solution unique name: `ASOCore`.
3. Defines the target table logical name: `lead`.
4. Creates an interactive OAuth connection using `ServiceClient`.
5. Retrieves existing Lead attributes with `RetrieveEntityRequest`.
6. Builds a list of desired Lead fields as metadata objects.
7. Loops over the list.
8. Skips a field if its logical name already exists.
9. Creates missing fields with `CreateAttributeRequest`.
10. Passes `SolutionUniqueName = ASOCore` so created fields are associated with ASO.Core.
11. Logs each result to the terminal.
12. Publishes Lead table customizations with `PublishXmlRequest`.
## 9. Local choices vs global choices

| Choice type | Meaning | Example in this run | Why it matters |
|---|---|---|---|
| Global choice | A reusable option set created as its own solution component. Many columns can reference it. | `aso_communicationstate` | Keeps common states consistent across Lead, Opportunity, Account, and Contact. |
| Local choice | An option set stored directly on one column. | `aso_airoutingdecision` | Useful when the values are specific to one field and not yet worth standardizing globally. |

The script uses global choices only where the Phase 2 model says “Use aso_...”. It uses local choices for field-specific values such as AI Routing Decision and Customer Insights Last Interaction Type.
## 10. Terminal commands used

```bash
# 1. Create and enter the local project folder
mkdir -p ~/aso-dataverse-tools/lead-extension
cd ~/aso-dataverse-tools/lead-extension

# 2. Create a .NET 8 console project
dotnet new console --framework net8.0

# 3. Add the Dataverse client SDK package
dotnet add package Microsoft.PowerPlatform.Dataverse.Client

# 4. Open/edit Program.cs
nano Program.cs

# 5. Run the metadata creation script
dotnet run
```
## 11. Validation checklist

| Check | How to validate | Expected result |
|---|---|---|
| Script finished | Terminal output | `Done. Validate in ASO.Core -> Tables -> Lead -> Columns.` |
| Fields created | Power Apps -> Phoenicarix-CI -> Solutions -> ASO.Core -> Tables -> Lead -> Columns | New `aso_` fields are visible. |
| No default solution pollution | Open ASO.Core and confirm components are inside the solution | Fields are associated with ASO.Core. |
| Naming consistency | Search for `aso_` in Lead columns | Logical names use `aso_` prefix. |
| Choice reuse | Open AI Agent Status, Communication State, Lifecycle Stage, Journey Participation Status, Email Consent Status | They reference existing global choices. |
| Local choice values | Open AI Fit Level, AI Routing Decision, Sales Qualification Agent Status, Customer Insights Last Interaction Type | Values match the agreed Phase 2 model. |
| Publishing | Refresh maker portal after script | Columns are visible and usable. |
## 12. Backup naming after successful Lead extension

Use the agreed backup naming standard.

```text
ASO.Core_Phase2_CI_LeadExtension_unmanaged_v1_0_20260523.zip
ASO.Core_Phase2_CI_LeadExtension_managed_v1_0_20260523.zip
```

The unmanaged backup is useful for development recovery. The managed backup is useful to test import behavior and simulate downstream managed deployment.
## 13. Troubleshooting

| Symptom | Likely cause | Action |
|---|---|---|
| ASO.Core not found | Connected to wrong environment or wrong solution unique name | Run `pac auth list`, select Phoenicarix-CI, run `pac solution list | grep -i aso`. |
| Global choice error | Choice logical name differs from script | Check ASO.Core -> Choices and adjust `GlobalChoice(..., globalChoiceName, ...)`. |
| Some fields already exist | Script was rerun or partially completed earlier | This is expected; the script skips existing fields. |
| Authentication window appears | Interactive OAuth login is required | Log in with the same account used in Power Apps. |
| Field appears in environment but not solution | Solution association failed or wrong solution name | Add existing column to ASO.Core manually, then correct `SolutionUniqueName`. |
| Display labels are English in German UI | Script uses language code 1033 | Add localized labels later if required. |
## 14. Governance and architecture guardrails

- Dataverse remains the operational sales truth.
- Customer Insights - Journeys remains the only lifecycle communication execution layer.
- The Lead extension prepares fields only; it does not start customer communication.
- SAP fields are references and context fields only; they are not an SAP integration.
- HubSpot fields are ingress references only; HubSpot is not configured in this step.
- AI draft fields are draft-only and must not be sent directly.
- High-impact commercial automation remains out of scope until later phases and approvals exist.
## 15. Handover notes for the customer IT team

1. Keep the script in source control, not only on one laptop.
2. Record the run date, environment URL, solution unique name, and script version in the implementation workbook.
3. Export ASO.Core after validation.
4. Do not edit the generated schema names manually after creation.
5. Add the fields to forms only after the schema is validated.
6. Repeat the same pattern for Opportunity, Account, and Contact after Lead is confirmed.
## Appendix A - Full script snapshot

```csharp
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";
const string SolutionUniqueName = "ASOCore";
const string EntityLogicalName = "lead";
const int LanguageCode = 1033;

var connectionString =
    $@"AuthType=OAuth;
       Url={DataverseUrl};
       LoginPrompt=Auto;
       ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;
       RedirectUri=http://localhost";

using var service = new ServiceClient(connectionString);

if (!service.IsReady)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Connection failed:");
    Console.WriteLine(service.LastError);
    Console.ResetColor();
    return;
}

Console.WriteLine($"Connected to: {DataverseUrl}");
Console.WriteLine($"Target solution: {SolutionUniqueName}");
Console.WriteLine($"Target table: {EntityLogicalName}");
Console.WriteLine();

var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);

var fields = new List<AttributeMetadata>
{
    Text("aso_hubspotcontactid", "HubSpot Contact ID", 100, "External ingress key; do not configure HubSpot yet."),
    Text("aso_hubspotsource", "HubSpot Source", 100, "Ingress source reference; keep governed."),
    Text("aso_sapbusinesspartnerid", "SAP Business Partner ID", 100, "SAP reference only."),
    Text("aso_sapcustomerid", "SAP Customer ID", 100, "SAP reference only."),
    LocalChoice("aso_aifitlevel", "AI Fit Level", new[] { "Strong", "Moderate", "Weak" }, "AI fit level."),
    WholeNumber("aso_aileadscore", "AI Lead Score", 0, 100, "AI lead score 0-100."),
    Memo("aso_aiqualificationrationale", "AI Qualification Rationale", "Seller-facing rationale."),
    Memo("aso_aioutreachdraft", "AI Outreach Draft", "Draft only; not direct-send."),
    LocalChoice("aso_airoutingdecision", "AI Routing Decision", new[] { "Nurture", "SDR", "AE", "Reject", "NeedsReview", "ExistingAccountReview" }, "AI routing decision."),
    DecimalNumber("aso_aiconfidence", "AI Confidence", 0m, 1m, 2, "AI confidence 0.00-1.00."),
    DateTimeField("aso_ailastrunon", "AI Last Run On", "Last AI run timestamp."),
    GlobalChoice("aso_aiagentstatus", "AI Agent Status", "aso_aistatus", "Uses global AI status choice."),
    Memo("aso_aisapcontextsummary", "AI SAP Context Summary", "SAP-derived summary after governed integration."),
    YesNo("aso_aisapmatchflag", "AI SAP Match Flag", "Likely SAP match."),
    Text("aso_aicorrelationid", "AI Correlation ID", 100, "Last run correlation."),
    LocalChoice("aso_salesqualificationagentstatus", "Sales Qualification Agent Status", new[] { "NotStarted", "Running", "Completed", "Failed", "NeedsReview" }, "Sales qualification agent status."),
    WholeNumber("aso_salesqualificationagentscore", "Sales Qualification Agent Score", 0, 100, "Sales agent score if available."),
    Memo("aso_salesqualificationagentrationale", "Sales Qualification Agent Rationale", "Output from Sales Qualification Agent."),
    DateTimeField("aso_salesqualificationagentlastrunon", "Sales Qualification Agent Last Run On", "Timestamp."),
    Text("aso_foundryfinalqualificationdecision", "Foundry Final Qualification Decision", 100, "Post-validation decision."),
    YesNo("aso_foundryreviewrequired", "Foundry Review Required", "Human review gate."),
    GlobalChoice("aso_communicationstate", "Communication State", "aso_communicationstate", "Uses global Communication State choice."),
    GlobalChoice("aso_lifecyclecommunicationstage", "Lifecycle Communication Stage", "aso_lifecyclecommunicationstage", "Uses global Lifecycle Communication Stage choice."),
    GlobalChoice("aso_journeyparticipationstatus", "Journey Participation Status", "aso_journeyparticipationstatus", "Uses global Journey Participation Status choice."),
    Text("aso_customerinsightsjourneyid", "Customer Insights Journey ID", 100, "Latest journey reference."),
    Text("aso_customerinsightsjourneyname", "Customer Insights Journey Name", 200, "Latest journey name."),
    DateTimeField("aso_customerinsightslastentryon", "Customer Insights Last Entry On", "Last entry."),
    DateTimeField("aso_customerinsightslastinteractionon", "Customer Insights Last Interaction On", "Last interaction."),
    LocalChoice("aso_customerinsightslastinteractiontype", "Customer Insights Last Interaction Type", new[] { "EmailSent", "Open", "Click", "FormSubmit", "Reply", "Unsubscribe", "Bounce", "CustomAction" }, "Customer Insights last interaction type."),
    GlobalChoice("aso_emailconsentstatus", "Email Consent Status", "aso_emailconsentstatus", "Uses global Email Consent Status choice."),
    Text("aso_complianceprofilename", "Compliance Profile Name", 200, "Profile used."),
    Memo("aso_communicationholdreason", "Communication Hold Reason", "Suppression / hold reason.")
};

foreach (var field in fields)
{
    var logicalName = field.SchemaName!.ToLowerInvariant();

    if (existingAttributes.Contains(logicalName))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"SKIP existing: {logicalName}");
        Console.ResetColor();
        continue;
    }

    try
    {
        Console.WriteLine($"Creating: {logicalName} ...");
        service.Execute(new CreateAttributeRequest
        {
            EntityName = EntityLogicalName,
            Attribute = field,
            SolutionUniqueName = SolutionUniqueName
        });
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"CREATED: {logicalName}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"FAILED: {logicalName}");
        Console.WriteLine(ex.Message);
        Console.ResetColor();
    }
}

Console.WriteLine();
Console.WriteLine("Publishing Lead table customizations...");
service.Execute(new PublishXmlRequest
{
    ParameterXml = "<importexportxml><entities><entity>lead</entity></entities></importexportxml>"
});
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done. Validate in ASO.Core -> Tables -> Lead -> Columns.");
Console.ResetColor();

static HashSet<string> GetExistingAttributeLogicalNames(ServiceClient service, string entityLogicalName)
{
    var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
    {
        LogicalName = entityLogicalName,
        EntityFilters = EntityFilters.Attributes,
        RetrieveAsIfPublished = true
    });
    return response.EntityMetadata.Attributes
        .Where(a => !string.IsNullOrWhiteSpace(a.LogicalName))
        .Select(a => a.LogicalName!)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}

static Label Label(string value) => new(value, LanguageCode);
static AttributeRequiredLevelManagedProperty Optional() => new(AttributeRequiredLevel.None);

static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    MaxLength = maxLength
};

static MemoAttributeMetadata Memo(string schemaName, string displayName, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    MaxLength = 4000
};

static IntegerAttributeMetadata WholeNumber(string schemaName, string displayName, int min, int max, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    MinValue = min,
    MaxValue = max,
    Format = IntegerFormat.None
};

static DecimalAttributeMetadata DecimalNumber(string schemaName, string displayName, decimal min, decimal max, int precision, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    MinValue = min,
    MaxValue = max,
    Precision = precision
};

static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    Format = DateTimeFormat.DateAndTime,
    DateTimeBehavior = DateTimeBehavior.UserLocal
};

static BooleanAttributeMetadata YesNo(string schemaName, string displayName, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    DefaultValue = false,
    OptionSet = new BooleanOptionSetMetadata(
        new OptionMetadata(Label("Yes"), 1),
        new OptionMetadata(Label("No"), 0))
};

static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)
{
    var optionSet = new OptionSetMetadata { IsGlobal = false, OptionSetType = OptionSetType.Picklist };
    foreach (var value in values)
    {
        optionSet.Options.Add(new OptionMetadata(Label(value), null));
    }
    return new PicklistAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        OptionSet = optionSet
    };
}

static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description) => new()
{
    SchemaName = schemaName,
    DisplayName = Label(displayName),
    Description = Label(description),
    RequiredLevel = Optional(),
    OptionSet = new OptionSetMetadata { IsGlobal = true, Name = globalChoiceName, OptionSetType = OptionSetType.Picklist }
};

```

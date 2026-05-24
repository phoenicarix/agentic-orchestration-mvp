using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";
const string SolutionUniqueName = "ASOCore";
const string EntityLogicalName = "opportunity";
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
    Text("aso_sapcustomerid", "SAP Customer ID", 100, "SAP reference only."),
    Text("aso_saporderreference", "SAP Order Reference", 100, "Persist only after approved SAP process."),

    LocalChoice("aso_aiopportunitytier", "AI Opportunity Tier", new[] { "A", "B", "C" }, "AI opportunity tier."),
    Memo("aso_aisummary", "AI Summary", "AI account/opportunity summary."),
    Memo("aso_airisks", "AI Risks", "Identified risks."),
    Memo("aso_aistakeholdergaps", "AI Stakeholder Gaps", "Stakeholder coverage gaps."),
    Memo("aso_ainextactions", "AI Next Actions", "Recommended next actions."),
    LocalChoice("aso_airisklevel", "AI Risk Level", new[] { "Low", "Medium", "High" }, "AI risk level."),

    Memo("aso_aifollowupmessage", "AI Follow-up Message Draft", "Draft only; no direct customer send."),
    DecimalNumber("aso_aiconfidence", "AI Confidence", 0m, 1m, 2, "AI confidence 0.00-1.00."),
    DateTimeField("aso_ailastrunon", "AI Last Run On", "Last run timestamp."),
    GlobalChoice("aso_aiagentstatus", "AI Agent Status", "aso_aistatus", "Uses global AI status choice."),

    Memo("aso_aisapcommercialsummary", "AI SAP Commercial Summary", "Normalized SAP summary after governed reads."),
    Memo("aso_aisapproductsummary", "AI SAP Product Summary", "Normalized SAP product context."),
    Text("aso_aicorrelationid", "AI Correlation ID", 100, "Last run correlation."),

    GlobalChoice("aso_salesopportunityagentstatus", "Sales Opportunity Agent Status", "aso_aistatus", "Status of seller-facing opportunity agent output."),
    Text("aso_salesopportunityagentimportance", "Sales Opportunity Agent Importance", 100, "Native output varies by tenant."),
    Text("aso_salesopportunityagentrisk", "Sales Opportunity Agent Risk", 100, "Native output varies by tenant."),
    Memo("aso_salesopportunityagentrecommendation", "Sales Opportunity Agent Recommendation", "Seller-facing recommendation."),
    Memo("aso_salesdealcloseagentguidance", "Sales Deal Close Agent Guidance", "Close-readiness guidance."),
    DateTimeField("aso_salesdealcloseagentlastrunon", "Sales Deal Close Agent Last Run On", "Timestamp."),

    Memo("aso_foundryfinalopportunitydecision", "Foundry Final Opportunity Decision", "Final governed orchestration decision."),
    YesNo("aso_foundryreviewrequired", "Foundry Review Required", "Human review gate."),

    GlobalChoice("aso_communicationstate", "Communication State", "aso_communicationstate", "Uses global Communication State choice."),
    GlobalChoice("aso_lifecyclecommunicationstage", "Lifecycle Communication Stage", "aso_lifecyclecommunicationstage", "Uses global Lifecycle Communication Stage choice."),
    GlobalChoice("aso_journeyparticipationstatus", "Journey Participation Status", "aso_journeyparticipationstatus", "Uses global Journey Participation Status choice."),

    Text("aso_customerinsightsjourneyid", "Customer Insights Journey ID", 100, "Latest journey reference."),
    Text("aso_customerinsightsjourneyname", "Customer Insights Journey Name", 200, "Latest journey name."),
    DateTimeField("aso_customerinsightslastentryon", "Customer Insights Last Entry On", "Last entry."),
    DateTimeField("aso_customerinsightslastinteractionon", "Customer Insights Last Interaction On", "Last interaction."),
    LocalChoice("aso_customerinsightslastinteractiontype", "Customer Insights Last Interaction Type",
        new[] { "EmailSent", "Open", "Click", "FormSubmit", "Reply", "Unsubscribe", "Bounce", "CustomAction" },
        "Customer Insights last interaction type."),

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

        var request = new CreateAttributeRequest
        {
            EntityName = EntityLogicalName,
            Attribute = field,
            SolutionUniqueName = SolutionUniqueName
        };

        service.Execute(request);

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
Console.WriteLine("Publishing Opportunity table customizations...");

service.Execute(new PublishXmlRequest
{
    ParameterXml = "<importexportxml><entities><entity>opportunity</entity></entities></importexportxml>"
});

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done. Validate in ASO.Core → Tables → Opportunity → Columns.");
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

static AttributeRequiredLevelManagedProperty Optional()
{
    return new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
}

static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description)
{
    return new StringAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        MaxLength = maxLength
    };
}

static MemoAttributeMetadata Memo(string schemaName, string displayName, string description)
{
    return new MemoAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        MaxLength = 4000
    };
}

static DecimalAttributeMetadata DecimalNumber(string schemaName, string displayName, decimal min, decimal max, int precision, string description)
{
    return new DecimalAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        MinValue = min,
        MaxValue = max,
        Precision = precision
    };
}

static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description)
{
    return new DateTimeAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        Format = DateTimeFormat.DateAndTime,
        DateTimeBehavior = DateTimeBehavior.UserLocal
    };
}

static BooleanAttributeMetadata YesNo(string schemaName, string displayName, string description)
{
    return new BooleanAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        DefaultValue = false,
        OptionSet = new BooleanOptionSetMetadata(
            new OptionMetadata(Label("Yes"), 1),
            new OptionMetadata(Label("No"), 0)
        )
    };
}

static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)
{
    var optionSet = new OptionSetMetadata
    {
        IsGlobal = false,
        OptionSetType = OptionSetType.Picklist
    };

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

static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description)
{
    return new PicklistAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        OptionSet = new OptionSetMetadata
        {
            IsGlobal = true,
            Name = globalChoiceName,
            OptionSetType = OptionSetType.Picklist
        }
    };
}
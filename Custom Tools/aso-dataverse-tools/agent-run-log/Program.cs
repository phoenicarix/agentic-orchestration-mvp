using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";
const string SolutionUniqueName = "ASOCore";
const string EntityLogicalName = "aso_agentrunlog";
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

if (!EntityExists(service, EntityLogicalName))
{
    Console.WriteLine("Creating Agent Run Log table...");

    var createEntityRequest = new CreateEntityRequest
    {
        Entity = new EntityMetadata
        {
            SchemaName = "aso_agentrunlog",
            DisplayName = Label("Agent Run Log"),
            DisplayCollectionName = Label("Agent Run Logs"),
            Description = Label("Audit, traceability, replay review, and AI/orchestration troubleshooting table for Agentic Sales Orchestrator."),
            OwnershipType = OwnershipTypes.OrganizationOwned
        },
        PrimaryAttribute = new StringAttributeMetadata
        {
            SchemaName = "aso_name",
            DisplayName = Label("Name"),
            Description = Label("Primary name for the Agent Run Log record."),
            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            MaxLength = 200
        },
        SolutionUniqueName = SolutionUniqueName
    };

    service.Execute(createEntityRequest);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("CREATED TABLE: aso_agentrunlog");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("SKIP existing table: aso_agentrunlog");
    Console.ResetColor();
}

var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);

var fields = new List<AttributeMetadata>
{
    GlobalChoice("aso_agenttype", "Agent Type", "aso_agenttype", "Agent or component type that produced this run log."),
    Text("aso_recordtype", "Record Type", 100, "Logical or business type of the related Dataverse record."),
    Text("aso_recordid", "Record ID", 100, "Identifier of the related Dataverse record."),
    Text("aso_messageid", "Message ID", 100, "Message or event identifier for traceability."),
    Text("aso_correlationid", "Correlation ID", 100, "Correlation ID shared across Power Automate, Foundry, Dataverse, and integration components."),

    Memo("aso_inputpayload", "Input Payload", "Input payload used for the agent/orchestration run."),
    Memo("aso_outputpayload", "Output Payload", "Output payload returned by the agent/orchestration run."),

    DecimalNumber("aso_confidence", "Confidence", 0m, 1m, 2, "Confidence score between 0.00 and 1.00."),
    GlobalChoice("aso_status", "Status", "aso_aistatus", "Processing status for the agent/orchestration run."),
    Memo("aso_errormessage", "Error Message", "Error details if the agent/orchestration run failed."),

    DateTimeField("aso_startedon", "Started On", "Timestamp when the run started."),
    DateTimeField("aso_finishedon", "Finished On", "Timestamp when the run finished."),

    Text("aso_traceid", "Trace ID", 100, "Trace ID for observability and diagnostics."),
    Memo("aso_sourcesused", "Sources Used", "Sources, systems, or context used by the agent/orchestration run."),

    YesNo("aso_sapcalled", "SAP Called", "Indicates whether the run called SAP through the governed integration path."),
    WholeNumber("aso_retrycount", "Retry Count", 0, 100, "Number of retry attempts."),

    Text("aso_foundryrunid", "Foundry Run ID", 100, "Microsoft Foundry run identifier."),
    Text("aso_salesagentrunid", "Sales Agent Run ID", 100, "Sales Agent run identifier."),
    Text("aso_customerinsightsjourneyid", "Customer Insights Journey ID", 100, "Related Customer Insights journey identifier, if applicable.")
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
Console.WriteLine("Publishing Agent Run Log table customizations...");

service.Execute(new PublishXmlRequest
{
    ParameterXml = "<importexportxml><entities><entity>aso_agentrunlog</entity></entities></importexportxml>"
});

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done. Validate in ASO.Core → Tables → Agent Run Log → Columns.");
Console.ResetColor();

static bool EntityExists(ServiceClient service, string entityLogicalName)
{
    try
    {
        service.Execute(new RetrieveEntityRequest
        {
            LogicalName = entityLogicalName,
            EntityFilters = EntityFilters.Entity,
            RetrieveAsIfPublished = true
        });

        return true;
    }
    catch
    {
        return false;
    }
}

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

static IntegerAttributeMetadata WholeNumber(string schemaName, string displayName, int min, int max, string description)
{
    return new IntegerAttributeMetadata
    {
        SchemaName = schemaName,
        DisplayName = Label(displayName),
        Description = Label(description),
        RequiredLevel = Optional(),
        MinValue = min,
        MaxValue = max,
        Format = IntegerFormat.None
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
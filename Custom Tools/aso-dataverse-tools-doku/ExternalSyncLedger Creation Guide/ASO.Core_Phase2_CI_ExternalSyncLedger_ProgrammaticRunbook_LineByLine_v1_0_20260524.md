# Agentic Sales Orchestrator - External Sync Ledger Programmatic Extension Runbook

Customer-ready enterprise implementation guide with true line-by-line C# script explanation

| Attribute | Value |
|---|---|

| Target environment | Phoenicarix-CI - https://phoenicarix-ci.crm4.dynamics.com |
| Solution | ASO.Core - unique name ASOCore |
| Target table | External Sync Ledger - logical name aso_externalsyncledger |
| Implementation method | .NET 8 console application using Microsoft.PowerPlatform.Dataverse.Client |
| Purpose | Create the External Sync Ledger custom table and metadata columns programmatically |
| Version | v1.0 - 2026-05-24 |
| Audience | Customer IT team, product owners, junior developers, senior technical consultants, non-technical stakeholders |

## Executive summary

We created a new Dataverse custom table named External Sync Ledger inside the ASO.Core solution in the Phoenicarix-CI environment. The table is used to track external IDs, source systems, Dataverse row mapping, last processed hashes, synchronization timestamps, statuses, and correlation IDs. In plain language, it is the ASO project's integration notebook: it remembers how external records such as HubSpot, SAP, Foundry, Power Automate, or Customer Insights items map to Dataverse records and when they were last processed.


## Business objective

The business objective is to make future integrations traceable, repeatable, and supportable. When an external event arrives from HubSpot, SAP, Customer Insights, Foundry, or Power Automate, the system needs a governed place to store which external record corresponds to which Dataverse row. This avoids duplicate processing, improves troubleshooting, and enables controlled synchronization without relying on memory, spreadsheets, or hidden flow state.


## Technical objective

The technical objective was to create the custom table aso_externalsyncledger and its required columns programmatically through the Dataverse SDK. The script checks whether the table exists, creates it if missing, retrieves existing fields, creates missing fields, places everything in ASO.Core using the solution unique name ASOCore, and publishes the table metadata.


## Scope

In scope: create the External Sync Ledger custom table, create the columns defined by the script, use ASO.Core as the solution context, reuse the existing global choice aso_aistatus for Status, create Source System as a local choice, publish the table metadata, and validate the table in Power Apps. Out of scope: no Power Automate flow, no SAP connector, no HubSpot connector, no Customer Insights journey activation, no data migration, no form design, no security role hardening, no production deployment.


## Environment and solution context

The work was executed in Phoenicarix-CI at https://phoenicarix-ci.crm4.dynamics.com. The target solution was ASO.Core with unique name ASOCore. The table logical name is aso_externalsyncledger. The publisher prefix is aso. All custom metadata should be kept in ASO.Core so that it can be exported, backed up, reviewed, and promoted later through ALM.


## Why programmatic creation

The table and fields could be created manually in Power Apps, but a script is faster, repeatable, auditable, and less error-prone. The script also supports safe reruns: if the table or a column already exists, the script skips it rather than trying to recreate it. This matters because metadata creation often happens in several steps during an MVP.


## Prerequisites

The maker/admin must have access to Phoenicarix-CI and the ASO.Core solution. The required global choice aso_aistatus must already exist. .NET 8 and the Microsoft.PowerPlatform.Dataverse.Client package must be available locally. The user running the script needs privileges to create custom tables, create columns, customize the system, publish customizations, and add components to the unmanaged solution. Backups of ASO.Core should be exported before and after schema changes.


## Backups and recovery

Before creating new metadata, export ASO.Core as unmanaged and managed if required by the project practice. After successful External Sync Ledger creation, export the solution again. The user reported backing up the solution after this step. Recommended names are ASO.Core_Phase2_CI_ExternalSyncLedgerCreated_unmanaged_v1_0_20260524.zip and ASO.Core_Phase2_CI_ExternalSyncLedgerCreated_managed_v1_0_20260524.zip. Recovery in a trial/MVP usually means importing the previous unmanaged backup into a clean environment or manually removing erroneous components if dependencies allow it. Deleting Dataverse columns in a live environment must be handled carefully because data and dependencies may exist.


## Architecture/context

External Sync Ledger is an operational traceability table. It is not the business source of truth. Dynamics 365/Dataverse remains the operational ASO data platform, Customer Insights remains the customer lifecycle communication layer, SAP remains external ERP context through governed integration, and Foundry/Power Automate orchestration can later use this table to avoid duplicate processing and to trace events across components.


## Step-by-step implementation summary

1. Created the local project folder external-sync-ledger. 2. Created a .NET 8 console project. 3. Added Microsoft.PowerPlatform.Dataverse.Client. 4. Replaced Program.cs with the External Sync Ledger script. 5. Ran dotnet run. 6. The script connected to Phoenicarix-CI. 7. It created aso_externalsyncledger if missing. 8. It created the defined fields. 9. It published the table. 10. The table and columns were validated in Power Apps under ASO.Core > Tables > External Sync Ledger > Columns.


## Field inventory

| Display name | Schema/logical name | Type | Size / values | Business and technical purpose |
|---|---|---|---|---|
| Name | aso_name | Primary Name / Text | 200 | Primary record name required by Dataverse for the custom table. |
| Source System | aso_sourcesystem | Local choice | HubSpot, SAP, PowerAutomate, Foundry, CustomerInsights | Identifies the external or orchestration system that owns or produced the external identifier. |
| External Entity | aso_externalentity | Text | 100 | Stores the external object type, such as contact, lead, business partner, or journey. |
| External ID | aso_externalid | Text | 200 | Stores the ID from the external source system for matching and idempotency. |
| Dataverse Entity | aso_dataverseentity | Text | 100 | Stores the target Dataverse table or logical business entity name. |
| Dataverse ID | aso_dataverseid | Text | 100 | Stores the Dataverse row identifier linked to the external record. |
| Last Hash | aso_lasthash | Text | 200 | Stores the last processed hash for change detection and duplicate prevention. |
| Last Processed On | aso_lastprocessedon | Date and time | User local | Records the latest successful or attempted processing timestamp. |
| Status | aso_status | Global choice | aso_aistatus | Stores processing state using the existing ASO AI Status global choice for MVP consistency. |
| Correlation ID | aso_correlationid | Text | 100 | Connects this ledger entry to traces across Power Automate, Dataverse, Foundry, integrations, and monitoring. |

## Validation checklist

| Check | Expected result |
|---|---|
| Script completed | Terminal shows Done. Validate in ASO.Core -> Tables -> External Sync Ledger -> Columns. |
| Table exists | Power Apps shows External Sync Ledger under ASO.Core -> Tables. |
| Columns exist | Search columns for aso_ and confirm Source System, External Entity, External ID, Dataverse Entity, Dataverse ID, Last Hash, Last Processed On, Status, Correlation ID. |
| Solution association | Columns and table appear inside ASO.Core, not only in the default solution. |
| Status choice | Status uses the global ASO AI Status choice aso_aistatus. |
| Source System choice | Source System contains HubSpot, SAP, PowerAutomate, Foundry, CustomerInsights. |
| Backup | Managed/unmanaged ASO.Core backup exported after success. |

## Troubleshooting guide

| Symptom | Likely cause | Action |
|---|---|---|
| Connection failed | Wrong URL, expired login, no permission | Authenticate again and confirm Phoenicarix-CI URL. |
| ASOCore not found | Wrong solution unique name or wrong environment | Run pac solution list \| grep -i aso and confirm ASOCore. |
| Global choice error | aso_aistatus not found or differently named | Open ASO.Core -> Choices and confirm the logical name. |
| Table already exists | Script rerun after successful/partial execution | Expected; the script skips existing table. |
| Column already exists | Script rerun or previous partial success | Expected; the script skips existing columns. |
| Table exists but not in solution | Wrong or missing SolutionUniqueName | Add existing table/columns to ASO.Core and correct script. |
| Build error / no Main method | Program.cs not fully replaced or saved | Replace Program.cs with the full script and save before running dotnet run. |

## Audience-specific handover notes

**For a non-technical stakeholder:** We created a controlled register that remembers links between external systems and Dynamics/Dataverse records. This helps avoid duplicates and makes support easier.

**For the Product Owner:** This table supports integration traceability, idempotency, synchronization governance, and future reporting. It creates no live integration by itself.

**For developers and technical consultants:** The implementation uses Dataverse metadata SDK requests, not data inserts. It creates EntityMetadata and AttributeMetadata, references the ASOCore solution unique name, reuses aso_aistatus as a global choice, creates a local Source System choice, and publishes only aso_externalsyncledger.


# Part 2 - True line-by-line C# script explanation

The following table explains every line of the exact script used for the External Sync Ledger implementation. Blank lines and braces are included because they affect readability or code structure.

| Line / Code | Explanation in simple language | Technical explanation | Why it matters | Common mistake / warning |
|---|---|---|---|---|
| 1: `using Microsoft.Crm.Sdk.Messages;` | Imports Dataverse message classes. | Provides classes such as PublishXmlRequest that are used to publish metadata customizations. | Without the import, the script could not reference these SDK messages by short name. | Missing import causes compiler errors unless fully qualified class names are used. |
| 2: `using Microsoft.PowerPlatform.Dataverse.Client;` | Imports the Dataverse client package. | Provides ServiceClient, the main object used to authenticate and execute Dataverse SDK requests. | This is the connection gateway to the Phoenicarix-CI Dataverse environment. | Missing package/import prevents authentication and SDK calls. |
| 3: `using Microsoft.Xrm.Sdk;` | Imports core Dataverse SDK types. | Provides SDK types such as Label and Entity-related base classes. | Metadata labels and request execution depend on these types. | Missing import can cause unresolved type errors. |
| 4: `using Microsoft.Xrm.Sdk.Messages;` | Imports Dataverse SDK request/response messages. | Provides classes such as CreateEntityRequest, CreateAttributeRequest, RetrieveEntityRequest, and RetrieveEntityResponse. | These messages are the mechanism used to read and create metadata. | Missing import causes build errors for request classes. |
| 5: `using Microsoft.Xrm.Sdk.Metadata;` | Imports Dataverse metadata model classes. | Provides EntityMetadata, StringAttributeMetadata, PicklistAttributeMetadata, OptionSetMetadata, and other metadata definitions. | Column and table creation are expressed through metadata objects. | Missing import prevents declaring the table and column metadata. |
| 6: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 7: `const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";` | Defines the target Dataverse environment URL. | All connection and metadata requests are directed to https://phoenicarix-ci.crm4.dynamics.com. | This prevents accidentally creating schema in the wrong environment. | Wrong URL creates or changes metadata in the wrong Dataverse environment. |
| 8: `const string SolutionUniqueName = "ASOCore";` | Defines the target solution unique name. | CreateEntityRequest and CreateAttributeRequest use ASOCore to place components in the ASO.Core solution layer. | Solution-aware creation is essential for ALM, export, backup, and handover. | Using a display name instead of unique name, or a wrong name, may leave components outside the expected solution. |
| 9: `const string EntityLogicalName = "aso_externalsyncledger";` | Defines the target custom table logical name. | aso_externalsyncledger is the Dataverse logical name used by SDK requests. | This ensures all columns are created on the External Sync Ledger table. | Wrong logical name can create fields on the wrong table or cause retrieve failures. |
| 10: `const int LanguageCode = 1033;` | Defines the label language code. | 1033 is English. Dataverse uses this code for display names and descriptions created by Label(). | Consistent labels make the schema understandable to IT and business users. | Wrong language code can create labels under an unexpected locale. |
| 11: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 12: `var connectionString =` | Starts building the Dataverse connection string. | The following lines define authentication mode, environment URL, login behavior, client ID, and redirect URI. | A correct connection string is required before any metadata operation can run. | Incomplete connection string causes ServiceClient connection failure. |
| 13: `    $@"AuthType=OAuth;` | Sets OAuth as the authentication type. | OAuth is the modern authentication protocol used for interactive Dataverse sign-in. | It avoids hardcoded passwords and supports Microsoft identity sign-in. | Wrong auth type prevents the local script from logging in. |
| 14: `       Url={DataverseUrl};` | Injects the configured environment URL into the connection string. | String interpolation replaces {DataverseUrl} with the Phoenicarix-CI URL at runtime. | The target environment is defined once and reused safely. | Hardcoding inconsistent URLs can lead to accidental environment drift. |
| 15: `       LoginPrompt=Auto;` | Allows interactive login when needed. | ServiceClient can open a browser/login prompt if no valid token exists. | This is practical for running the script from a Mac during MVP setup. | If disabled and no token exists, authentication may fail silently or require extra setup. |
| 16: `       ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;` | Provides the client application ID for interactive Dataverse tooling. | This public client ID is commonly used for local Dataverse tooling scenarios. | It enables the OAuth flow without creating a separate app registration for this MVP. | An invalid client ID blocks authentication. |
| 17: `       RedirectUri=http://localhost";` | Sets the local redirect URI for OAuth sign-in. | After browser sign-in, authentication returns to localhost so the local process can receive the token. | This completes the interactive login flow on the developer machine. | If the redirect URI is invalid for the client, sign-in fails. |
| 18: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 19: `using var service = new ServiceClient(connectionString);` | Creates the Dataverse service client. | ServiceClient uses the connection string to authenticate and becomes the object used for all SDK calls. | Every retrieve, create, and publish operation runs through this client. | If ServiceClient cannot initialize, no metadata operation should continue. |
| 20: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 21: `if (!service.IsReady)` | Checks whether the connection is ready. | IsReady reports whether authentication and environment connection succeeded. | It prevents schema changes from running against an invalid or unauthenticated connection. | Skipping this check can lead to unclear failures later in the script. |
| 22: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 23: `    Console.ForegroundColor = ConsoleColor.Red;` | Changes terminal output color to red. | Red is used for connection and creation failures. | It makes errors visible to the operator during execution. | If omitted, errors still print but are harder to spot in long output. |
| 24: `    Console.WriteLine("Connection failed:");` | Prints a connection failure heading. | This runs only if ServiceClient is not ready. | It tells the operator to fix login, URL, or permissions before continuing. | Without it, connection issues would be less clear. |
| 25: `    Console.WriteLine(service.LastError);` | Prints the detailed connection error. | ServiceClient.LastError often contains the specific authentication or environment issue. | It enables fast troubleshooting. | Without it, support teams must guess the failure cause. |
| 26: `    Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 27: `    return;` | Stops the script early. | In the connection-failure block, return prevents the rest of the script from running. | No metadata changes should be attempted without a valid connection. | Removing it can cause confusing follow-up errors. |
| 28: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 29: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 30: `Console.WriteLine($"Connected to: {DataverseUrl}");` | Prints the connected Dataverse URL. | The operator sees the exact environment before metadata changes happen. | This is a safety check against running in the wrong environment. | If ignored, schema may be changed in the wrong tenant/environment. |
| 31: `Console.WriteLine($"Target solution: {SolutionUniqueName}");` | Prints the target solution unique name. | It confirms ASOCore is the target solution layer. | Operators can compare this with pac solution list or Power Apps solution overview. | Wrong solution name means components may not export with ASO.Core. |
| 32: `Console.WriteLine($"Target table: {EntityLogicalName}");` | Prints the target table logical name. | The script displays aso_externalsyncledger so the operator knows which table is being worked on. | This prevents confusion with Lead, Opportunity, Account, or Contact scripts. | Running the wrong script on the wrong table changes the wrong schema. |
| 33: `Console.WriteLine();` | Prints a blank line in the console. | This separates output sections for readability. | Readable logs are easier to review and screenshot for handover. | No functional risk if removed. |
| 34: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 35: `if (!EntityExists(service, EntityLogicalName))` | Checks whether the custom table already exists. | The helper method tries to retrieve the table metadata; the condition is true only if it is missing. | This makes the script safe to rerun after partial execution. | Without it, a rerun could fail on duplicate table creation. |
| 36: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 37: `    Console.WriteLine("Creating External Sync Ledger table...");` | Prints that the table creation will start. | The operator sees that the script is creating the custom table, not only columns. | This makes the execution trace clear. | If omitted, table creation is less visible in logs. |
| 38: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 39: `    var createEntityRequest = new CreateEntityRequest` | Creates the table creation request object. | CreateEntityRequest is the Dataverse SDK message used to create a new custom table/entity. | This is the main request that defines the External Sync Ledger table. | If built incorrectly, the table may not be created or may be created with wrong metadata. |
| 40: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 41: `        Entity = new EntityMetadata` | Starts the table metadata definition. | EntityMetadata describes the custom table's schema name, labels, description, and ownership type. | Dataverse needs entity metadata before it can create the table. | Missing entity metadata causes request failure. |
| 42: `        {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 43: `            SchemaName = "aso_externalsyncledger",` | Sets the custom table schema name. | The schema/logical name uses the ASO publisher prefix and becomes the technical identifier for the External Sync Ledger table. | All later column requests target this table name. | A typo creates the wrong table name or makes subsequent column creation fail. |
| 44: `            DisplayName = Label("External Sync Ledger"),` | Sets the table display name. | Label() creates a localized display label for makers and users. | This is what the customer IT team sees in Power Apps. | Wrong label causes confusion but not usually runtime failure. |
| 45: `            DisplayCollectionName = Label("External Sync Ledgers"),` | Sets the plural display name. | Dataverse uses the collection label in navigation, lists, and maker UI. | A clear plural name improves usability. | Wrong plural name is cosmetic but confusing. |
| 46: `            Description = Label("Tracks external IDs, hashes, processed times, and status across ingress and integration systems."),` | Adds the table description. | The description explains that the table stores external IDs, hashes, processed timestamps, and status information. | Good descriptions improve maintainability and governance. | Missing descriptions make the schema harder to understand later. |
| 47: `            OwnershipType = OwnershipTypes.OrganizationOwned` | Sets table ownership to organization-owned. | Organization-owned means rows are not owned by individual users; access is governed at table/security role level. | This fits a technical ledger used by integration and orchestration logic. | Wrong ownership type can create unnecessary owner/assignment complexity. |
| 48: `        },` | Closes a nested initializer item. | This ends an inner object initializer inside another request definition. | It separates table metadata from primary attribute metadata in the request. | Wrong punctuation can break object initialization. |
| 49: `        PrimaryAttribute = new StringAttributeMetadata` | Starts the primary name column definition. | Every custom table needs a primary name attribute; this one is a text field. | Dataverse uses the primary name in lookups and views. | Omitting it prevents custom table creation. |
| 50: `        {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 51: `            SchemaName = "aso_name",` | Sets the primary name column schema name. | aso_name becomes the technical name of the primary name column. | It provides a readable identity for each ledger row. | Wrong primary schema name may confuse downstream documentation and views. |
| 52: `            DisplayName = Label("Name"),` | Sets the primary column display name. | Users see this label in views and forms. | A simple Name field is a standard Dataverse pattern. | Misleading labels reduce usability. |
| 53: `            Description = Label("Primary name for the External Sync Ledger record."),` | Adds the primary column description. | It clarifies why the Name column exists. | Descriptions help IT and makers understand metadata. | No functional risk if omitted, but handover quality decreases. |
| 54: `            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),` | Marks the primary name as required. | Dataverse requires a primary name value for custom table records. | This keeps ledger records identifiable. | Making it optional can create poor-quality blank-name records. |
| 55: `            MaxLength = 200` | Sets text length to 200 characters. | This constrains the primary name or text field length where used. | It avoids unlimited values while allowing meaningful names/IDs. | Too short can truncate meaningful values; too long can reduce data discipline. |
| 56: `        },` | Closes a nested initializer item. | This ends an inner object initializer inside another request definition. | It separates table metadata from primary attribute metadata in the request. | Wrong punctuation can break object initialization. |
| 57: `        SolutionUniqueName = SolutionUniqueName` | Associates the created component with ASO.Core. | The SDK request uses the value ASOCore to add the table/column to the correct solution. | This is critical for export, backup, ALM, and customer handover. | Wrong or missing solution association can leave components outside the solution package. |
| 58: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 59: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 60: `    service.Execute(createEntityRequest);` | Sends the table creation request to Dataverse. | This is the line that actually creates the External Sync Ledger table if it does not already exist. | Everything before this line only prepares the request. | If this fails, no table exists for later column creation. |
| 61: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 62: `    Console.ForegroundColor = ConsoleColor.Green;` | Changes terminal output color to green. | Green is used for successful creation or completion messages. | It gives the operator immediate confirmation of successful operations. | If omitted, success messages still print but are less obvious. |
| 63: `    Console.WriteLine("CREATED TABLE: aso_externalsyncledger");` | Prints the table creation success message. | The terminal shows that aso_externalsyncledger was created. | This gives the operator a clear checkpoint. | If missing, validation still possible but console evidence is weaker. |
| 64: `    Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 65: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 66: `else` | Starts the alternative path when the table already exists. | If EntityExists returns true, the script skips table creation and continues with columns. | This supports safe reruns. | Without else logic, reruns may fail on duplicate table creation. |
| 67: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 68: `    Console.ForegroundColor = ConsoleColor.Yellow;` | Changes terminal output color to yellow. | Yellow is used for safe skip messages when table/columns already exist. | It distinguishes idempotent skips from errors. | If omitted, rerun behavior may be harder to read. |
| 69: `    Console.WriteLine("SKIP existing table: aso_externalsyncledger");` | Prints that the table already exists. | The script is intentionally idempotent and does not recreate existing schema. | This is normal during reruns or after partial success. | Do not treat yellow skip as an error. |
| 70: `    Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 71: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 72: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 73: `var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);` | Retrieves existing column logical names. | The helper method returns all attributes already present on the External Sync Ledger table. | This allows the script to skip columns that already exist. | Without this check, reruns would fail with duplicate column errors. |
| 74: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 75: `var fields = new List<AttributeMetadata>` | Starts the planned column list. | Each item in the list is an AttributeMetadata object describing a column to create. | The list is the declarative schema inventory for the table. | If a required field is missing from the list, the table will not match the implementation guide. |
| 76: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 77: `    LocalChoice("aso_sourcesystem", "Source System",` | Creates the Source System choice column. | This uses a local Dataverse choice because the source-system list is specific to the synchronization ledger and includes HubSpot, SAP, Power Automate, Foundry, and Customer Insights. | It tells IT which external system produced or owns an external identifier. | If values are misspelled or changed later without migration planning, reports and flows can filter incorrectly. |
| 78: `        new[] { "HubSpot", "SAP", "PowerAutomate", "Foundry", "CustomerInsights" },` | Lists the Source System option values. | The array becomes the choice options for aso_sourcesystem. | These values identify which external system a ledger row belongs to. | Changing labels later can affect reports, filters, and integrations. |
| 79: `        "External or integration system that produced or owns the synchronized identifier."),` | Provides the Source System column description. | This completes the multi-line LocalChoice call for aso_sourcesystem. | The description tells makers why this column exists. | If incomplete, the code will not compile because the function call remains unfinished. |
| 80: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 81: `    Text("aso_externalentity", "External Entity", 100, "External entity or object name, for example contact, lead, business partner, or journey."),` | Creates a text column for the external object/entity name. | StringAttributeMetadata creates a single-line text field with a maximum length of 100. | It separates an external system ID from the external object type, for example HubSpot contact vs SAP customer. | If omitted, the same external ID could be ambiguous across systems or object types. |
| 82: `    Text("aso_externalid", "External ID", 200, "External system identifier."),` | Creates a text column for the external system record ID. | The 200-character length allows longer external identifiers while keeping the field queryable. | This is central to matching Dataverse rows to source-system records. | Too small a length could truncate IDs; wrong schema name breaks integration mapping. |
| 83: `    Text("aso_dataverseentity", "Dataverse Entity", 100, "Dataverse table/entity logical or business name."),` | Creates a text column for the Dataverse table/entity name. | The script stores this as text rather than a polymorphic lookup because this MVP ledger tracks multiple tables generically. | It helps identify whether the ledger entry maps to Lead, Contact, Account, Opportunity, or another row type. | If treated as free text without governance, inconsistent names can reduce reporting quality. |
| 84: `    Text("aso_dataverseid", "Dataverse ID", 100, "Dataverse row identifier."),` | Creates a text column for the Dataverse row ID. | This stores the GUID/string identifier of the matched Dataverse record. | It links external identifiers back to the internal business record. | If the value is not kept accurate, writeback/replay logic may update the wrong row. |
| 85: `    Text("aso_lasthash", "Last Hash", 200, "Last processed hash used for idempotency and change detection."),` | Creates a text column for the last processed hash. | The hash can represent the last processed payload or selected source fields. | It enables idempotency: if the hash did not change, the integration can skip unnecessary processing. | Wrong hash logic can cause duplicates or missed updates. |
| 86: `    DateTimeField("aso_lastprocessedon", "Last Processed On", "Timestamp of the latest successful or attempted processing."),` | Creates a date/time column for processing timestamp. | DateTimeAttributeMetadata with UserLocal stores time in Dataverse while displaying in the user context. | It supports operations monitoring and tells when a sync attempt last happened. | Wrong DateTimeBehavior can confuse users across time zones. |
| 87: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 88: `    GlobalChoice("aso_status", "Status", "aso_aistatus", "Processing status. Uses existing ASO AI Status global choice for MVP consistency."),` | Creates a Status choice column using an existing global choice. | The field references the global option set named aso_aistatus. | It gives the ledger a consistent processing status vocabulary for MVP. | If the global choice name is wrong, this field creation fails. |
| 89: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 90: `    Text("aso_correlationid", "Correlation ID", 100, "Correlation ID shared across integration, automation, and orchestration components.")` | Creates a text column for the correlation ID. | A short text field stores trace identifiers from flows, integrations, and orchestration runs. | It lets IT trace one business event across different logs and components. | If omitted, troubleshooting cross-system failures becomes much harder. |
| 91: `};` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 92: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 93: `foreach (var field in fields)` | Starts a loop over all planned fields. | The loop processes each AttributeMetadata item one by one. | This avoids repeated manual code for every column. | If the loop is missing, no planned columns are created. |
| 94: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 95: `    var logicalName = field.SchemaName!.ToLowerInvariant();` | Normalizes the field schema name to lowercase. | Dataverse logical names are lowercase, so this helps compare planned fields with existing ones reliably. | This supports safe duplicate detection. | Without normalization, comparison could miss existing fields in some cases. |
| 96: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 97: `    if (existingAttributes.Contains(logicalName))` | Checks whether the current column already exists. | The existing logical-name set is used to avoid duplicate creation attempts. | This makes the script rerunnable. | Without it, partial reruns would fail on columns created earlier. |
| 98: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 99: `        Console.ForegroundColor = ConsoleColor.Yellow;` | Changes terminal output color to yellow. | Yellow is used for safe skip messages when table/columns already exist. | It distinguishes idempotent skips from errors. | If omitted, rerun behavior may be harder to read. |
| 100: `        Console.WriteLine($"SKIP existing: {logicalName}");` | Prints a skipped-field message. | This tells the operator which column already existed and was not recreated. | It confirms idempotent behavior. | Do not confuse skip with failure. |
| 101: `        Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 102: `        continue;` | Moves to the next field in the loop. | After a field is detected as existing, the script skips creation logic for that field. | This prevents duplicate errors. | Removing it would still try to create an existing column. |
| 103: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 104: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 105: `    try` | Starts protected execution for column creation. | If an error happens while creating one field, catch handles it and prints the specific failure. | This improves troubleshooting. | Without try/catch, the script could crash with less context. |
| 106: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 107: `        Console.WriteLine($"Creating: {logicalName} ...");` | Prints the field about to be created. | The operator sees progress field by field. | Useful for validation and screenshots. | No functional risk if removed, but less traceability. |
| 108: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 109: `        var request = new CreateAttributeRequest` | Creates the column creation request. | CreateAttributeRequest is the SDK message for adding one column/attribute to a table. | This is the core Dataverse metadata operation for fields. | Wrong request type or missing properties prevents field creation. |
| 110: `        {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 111: `            EntityName = EntityLogicalName,` | Sets the target table for the column. | The value resolves to aso_externalsyncledger. | It ensures every field is created on External Sync Ledger. | Wrong EntityName creates fields on the wrong table or fails. |
| 112: `            Attribute = field,` | Attaches the current field metadata to the request. | The current AttributeMetadata object contains schema name, display label, description, type, and settings. | Dataverse needs this metadata to create the exact field. | If missing, the request does not know what column to create. |
| 113: `            SolutionUniqueName = SolutionUniqueName` | Associates the created component with ASO.Core. | The SDK request uses the value ASOCore to add the table/column to the correct solution. | This is critical for export, backup, ALM, and customer handover. | Wrong or missing solution association can leave components outside the solution package. |
| 114: `        };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 115: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 116: `        service.Execute(request);` | Executes the create-column request. | This sends the CreateAttributeRequest to Dataverse. | This is the line that actually creates a missing column. | Failures here usually indicate invalid metadata, missing choices, or permissions issues. |
| 117: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 118: `        Console.ForegroundColor = ConsoleColor.Green;` | Changes terminal output color to green. | Green is used for successful creation or completion messages. | It gives the operator immediate confirmation of successful operations. | If omitted, success messages still print but are less obvious. |
| 119: `        Console.WriteLine($"CREATED: {logicalName}");` | Prints a successful field creation message. | The green terminal line confirms which field was created. | This gives a clear audit trail of the run. | If removed, validation relies only on Power Apps UI. |
| 120: `        Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 121: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 122: `    catch (Exception ex)` | Starts the error handling block. | Any exception from field creation is captured in ex. | This allows the script to print the error and keep the context visible. | Without catch, the script stops abruptly on the first error. |
| 123: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 124: `        Console.ForegroundColor = ConsoleColor.Red;` | Changes terminal output color to red. | Red is used for connection and creation failures. | It makes errors visible to the operator during execution. | If omitted, errors still print but are harder to spot in long output. |
| 125: `        Console.WriteLine($"FAILED: {logicalName}");` | Prints the failed field name. | The operator sees exactly which column caused a problem. | Essential for troubleshooting partial schema creation. | Without it, error diagnosis is slower. |
| 126: `        Console.WriteLine(ex.Message);` | Prints the exception message. | Dataverse or the SDK returns useful details such as missing option set, invalid schema name, or permission problem. | It enables targeted fixes. | Without it, failure reason is hidden. |
| 127: `        Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 128: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 129: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 130: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 131: `Console.WriteLine();` | Prints a blank line in the console. | This separates output sections for readability. | Readable logs are easier to review and screenshot for handover. | No functional risk if removed. |
| 132: `Console.WriteLine("Publishing External Sync Ledger table customizations...");` | Prints that publishing is starting. | The operator sees the transition from creation to publishing. | Publishing makes metadata available in the maker UI and runtime. | If not published, created fields may not appear immediately. |
| 133: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 134: `service.Execute(new PublishXmlRequest` | Starts the publish request. | PublishXmlRequest publishes selected metadata customizations. | Targeted publish is more controlled than publishing everything. | If omitted, users may not see changes until a later publish. |
| 135: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 136: `    ParameterXml = "<importexportxml><entities><entity>aso_externalsyncledger</entity></entities></importexportxml>"` | Specifies the table to publish. | The XML tells Dataverse to publish the aso_externalsyncledger entity customizations only. | This limits the publish scope to the table touched by the script. | Wrong XML can publish nothing or target the wrong entity. |
| 137: `});` | C# statement or structural line used by the script. | This line participates in setting up metadata, control flow, or SDK execution for Dataverse. | It contributes to creating or validating the External Sync Ledger schema. | Changing it without understanding the surrounding block can break compilation or metadata behavior. |
| 138: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 139: `Console.ForegroundColor = ConsoleColor.Green;` | Changes terminal output color to green. | Green is used for successful creation or completion messages. | It gives the operator immediate confirmation of successful operations. | If omitted, success messages still print but are less obvious. |
| 140: `Console.WriteLine("Done. Validate in ASO.Core → Tables → External Sync Ledger → Columns.");` | Prints the final success/validation instruction. | It tells the operator to validate in ASO.Core > Tables > External Sync Ledger > Columns. | This reinforces that the script is not complete until UI validation is done. | Do not skip validation after seeing this message. |
| 141: `Console.ResetColor();` | Resets terminal color to normal. | After red, green, or yellow output, ResetColor prevents later text from staying colored. | This keeps the console readable. | If missing, all later output may appear in the previous color. |
| 142: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 143: `static bool EntityExists(ServiceClient service, string entityLogicalName)` | Defines the helper method that checks table existence. | It receives ServiceClient and table logical name, then tries to retrieve entity metadata. | This supports safe reruns and avoids duplicate table creation. | A faulty helper can hide real retrieve errors. |
| 144: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 145: `    try` | Starts protected execution for column creation. | If an error happens while creating one field, catch handles it and prints the specific failure. | This improves troubleshooting. | Without try/catch, the script could crash with less context. |
| 146: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 147: `        service.Execute(new RetrieveEntityRequest` | Executes a retrieve-entity metadata request. | The request asks Dataverse for table metadata. | This is used to check table existence or retrieve attributes. | Wrong logical name causes retrieve failure. |
| 148: `        {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 149: `            LogicalName = entityLogicalName,` | Sets which table metadata to retrieve. | The helper method receives the table logical name as a parameter. | It makes the helper reusable. | Wrong value means checking or retrieving the wrong table. |
| 150: `            EntityFilters = EntityFilters.Entity,` | Requests only table-level metadata. | EntityFilters.Entity is enough to check whether the table exists. | It is lighter than retrieving all attributes. | Using the wrong filter may return more data than needed. |
| 151: `            RetrieveAsIfPublished = true` | Retrieves metadata as if it is published. | This helps include current customization state while inspecting metadata. | It improves reliability during maker/development sessions. | Wrong setting can make recently changed metadata harder to detect. |
| 152: `        });` | C# statement or structural line used by the script. | This line participates in setting up metadata, control flow, or SDK execution for Dataverse. | It contributes to creating or validating the External Sync Ledger schema. | Changing it without understanding the surrounding block can break compilation or metadata behavior. |
| 153: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 154: `        return true;` | Returns true when the table exists. | If retrieving entity metadata succeeds, the helper concludes the table exists. | The main script then skips table creation. | If the retrieve succeeds for the wrong table, the script may skip creation incorrectly. |
| 155: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 156: `    catch` | Starts the catch block for a failed table retrieve. | In this helper, failure means the table likely does not exist. | The script uses this to decide to create the table. | This may also catch permission errors, so run with proper admin permissions. |
| 157: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 158: `        return false;` | Returns false when table retrieve failed. | The main script interprets this as table missing. | This triggers table creation. | If the failure was due to permissions rather than missing table, creation may also fail. |
| 159: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 160: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 161: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 162: `static HashSet<string> GetExistingAttributeLogicalNames(ServiceClient service, string entityLogicalName)` | Defines a helper to collect existing column logical names. | It returns a case-insensitive set of attribute logical names for the target table. | This supports idempotent reruns. | Incorrect implementation can create duplicate-attempt failures. |
| 163: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 164: `    var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest` | Executes a retrieve request and casts the response. | The SDK returns a generic response, which is cast to RetrieveEntityResponse to access EntityMetadata. | This exposes the table's attribute list. | Wrong cast or request type causes runtime errors. |
| 165: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 166: `        LogicalName = entityLogicalName,` | Sets which table metadata to retrieve. | The helper method receives the table logical name as a parameter. | It makes the helper reusable. | Wrong value means checking or retrieving the wrong table. |
| 167: `        EntityFilters = EntityFilters.Attributes,` | Requests attribute/column metadata. | This is needed to inspect existing columns on the table. | It powers the duplicate-check logic. | Using only Entity filter here would not return attributes. |
| 168: `        RetrieveAsIfPublished = true` | Retrieves metadata as if it is published. | This helps include current customization state while inspecting metadata. | It improves reliability during maker/development sessions. | Wrong setting can make recently changed metadata harder to detect. |
| 169: `    });` | C# statement or structural line used by the script. | This line participates in setting up metadata, control flow, or SDK execution for Dataverse. | It contributes to creating or validating the External Sync Ledger schema. | Changing it without understanding the surrounding block can break compilation or metadata behavior. |
| 170: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 171: `    return response.EntityMetadata.Attributes` | Starts returning the table's attribute logical names. | LINQ is used to filter and project metadata into a HashSet. | A set enables fast lookup during field creation. | If attributes are null or not retrieved, this fails. |
| 172: `        .Where(a => !string.IsNullOrWhiteSpace(a.LogicalName))` | Filters out attributes without logical names. | The predicate keeps only valid metadata entries. | It avoids null/blank values in the lookup set. | Skipping this can cause null-related warnings or errors. |
| 173: `        .Select(a => a.LogicalName!)` | Selects only the logical name of each attribute. | The script does not need full metadata for duplicate checking. | This keeps the lookup lightweight. | Selecting the wrong property breaks duplicate detection. |
| 174: `        .ToHashSet(StringComparer.OrdinalIgnoreCase);` | Converts logical names into a case-insensitive set. | StringComparer.OrdinalIgnoreCase makes lookups robust to case differences. | This improves idempotency. | A case-sensitive set can miss equivalent names. |
| 175: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 176: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 177: `static Label Label(string value) => new(value, LanguageCode);` | Defines a short helper for Dataverse labels. | It creates a Label object using the global LanguageCode value. | This avoids repeating new Label(value, 1033) throughout the script. | Wrong language code or helper breaks labels/descriptions. |
| 178: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 179: `static AttributeRequiredLevelManagedProperty Optional()` | Defines a helper for optional columns. | It returns AttributeRequiredLevel.None wrapped in a managed property object. | Most ledger fields are optional because flows/integrations will populate them when available. | Making fields required too early can block record creation. |
| 180: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 181: `    return new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);` | Returns the optional requirement level. | Dataverse stores required level as a managed property. | Optional fields are safe for MVP where not every system populates every value immediately. | Using required incorrectly can break integration inserts. |
| 182: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 183: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 184: `static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description)` | Defines a helper for single-line text columns. | It receives schema name, display name, max length, and description, then returns StringAttributeMetadata. | This avoids repeated boilerplate for text fields. | Wrong helper settings affect every text column created through it. |
| 185: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 186: `    return new StringAttributeMetadata` | Creates a text-column metadata object. | StringAttributeMetadata tells Dataverse to create a single line of text field. | Used for external IDs, entity names, hashes, and correlation IDs. | Wrong metadata class creates the wrong field type. |
| 187: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 188: `        SchemaName = schemaName,` | Applies the schema name passed to the helper. | The schema name becomes the Dataverse logical identifier after creation. | Correct schema names are required for flows, scripts, and ALM consistency. | Typos are expensive to fix after creation. |
| 189: `        DisplayName = Label(displayName),` | Applies the human-readable display label. | The helper converts the display string into a Dataverse Label object. | Users and makers see this label in Power Apps. | Unclear display names reduce usability. |
| 190: `        Description = Label(description),` | Applies the field description. | Descriptions become metadata documentation visible to makers/admins. | This supports future maintainability. | Missing descriptions make the data model harder to interpret. |
| 191: `        RequiredLevel = Optional(),` | Marks the field optional. | The helper returns AttributeRequiredLevel.None for non-primary fields. | Integration-led fields should not block manual or automated row creation in MVP. | Making fields required can break sync processes. |
| 192: `        MaxLength = maxLength` | Applies the requested maximum text length. | The helper parameter controls field length per column. | This balances flexibility with governance. | Too short truncates values; too long can reduce discipline. |
| 193: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 194: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 195: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 196: `static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description)` | Defines a helper for date/time columns. | It builds DateTimeAttributeMetadata using schema name, label, and description. | Used for Last Processed On. | Wrong DateTime helper can produce incorrect behavior for timestamps. |
| 197: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 198: `    return new DateTimeAttributeMetadata` | Creates date/time metadata. | DateTimeAttributeMetadata tells Dataverse to create a date/time column. | Timestamps are essential for sync monitoring. | Wrong metadata class would not support time values. |
| 199: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 200: `        SchemaName = schemaName,` | Applies the schema name passed to the helper. | The schema name becomes the Dataverse logical identifier after creation. | Correct schema names are required for flows, scripts, and ALM consistency. | Typos are expensive to fix after creation. |
| 201: `        DisplayName = Label(displayName),` | Applies the human-readable display label. | The helper converts the display string into a Dataverse Label object. | Users and makers see this label in Power Apps. | Unclear display names reduce usability. |
| 202: `        Description = Label(description),` | Applies the field description. | Descriptions become metadata documentation visible to makers/admins. | This supports future maintainability. | Missing descriptions make the data model harder to interpret. |
| 203: `        RequiredLevel = Optional(),` | Marks the field optional. | The helper returns AttributeRequiredLevel.None for non-primary fields. | Integration-led fields should not block manual or automated row creation in MVP. | Making fields required can break sync processes. |
| 204: `        Format = DateTimeFormat.DateAndTime,` | Sets the field to store both date and time. | DateAndTime records a full timestamp, not only a date. | Sync ledger needs precise processing timestamps. | Date-only format would lose useful diagnostic precision. |
| 205: `        DateTimeBehavior = DateTimeBehavior.UserLocal` | Sets user-local display behavior. | Dataverse stores time consistently and displays it according to user locale/time zone behavior. | This is acceptable for MVP operator-facing timestamps. | For backend-only audit logs, production may review whether UTC-only behavior is preferred. |
| 206: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 207: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 208: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 209: `static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)` | Defines a helper for local choice columns. | The helper creates a picklist whose options belong only to this column. | Used for Source System because the values are ledger-specific. | Using local choice where global consistency is required can create duplicate taxonomies. |
| 210: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 211: `    var optionSet = new OptionSetMetadata` | Creates the option set metadata object. | This object stores choice options and whether the option set is local or global. | Choice metadata is required for picklist fields. | Incorrect IsGlobal setting changes the choice behavior. |
| 212: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 213: `        IsGlobal = false,` | Marks the option set as local. | Local option values are stored on this specific field rather than reused globally. | This is appropriate for Source System in this MVP ledger. | If it should be shared across tables later, migration/refactoring is needed. |
| 214: `        OptionSetType = OptionSetType.Picklist` | Sets the option set type to picklist. | Picklist creates a standard single-select choice column. | This matches the Source System and Status selection pattern. | Wrong option set type can create incorrect choice behavior. |
| 215: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 216: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 217: `    foreach (var value in values)` | Loops through each local choice value. | The helper receives an array of labels and adds each one as an OptionMetadata entry. | This avoids writing repetitive option creation code. | If the values array is wrong, choice values are wrong. |
| 218: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 219: `        optionSet.Options.Add(new OptionMetadata(Label(value), null));` | Adds one choice option to the local option set. | Each label becomes a selectable Dataverse choice value; null allows Dataverse to assign numeric option values. | Auto-assigned values are fine for MVP as long as labels are governed. | Hardcoding values can cause conflicts if unmanaged layering is not planned. |
| 220: `    }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 221: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 222: `    return new PicklistAttributeMetadata` | Creates picklist/choice column metadata. | PicklistAttributeMetadata is the SDK class for Dataverse choice fields. | Used for both local and global choices. | Wrong metadata class prevents choice behavior. |
| 223: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 224: `        SchemaName = schemaName,` | Applies the schema name passed to the helper. | The schema name becomes the Dataverse logical identifier after creation. | Correct schema names are required for flows, scripts, and ALM consistency. | Typos are expensive to fix after creation. |
| 225: `        DisplayName = Label(displayName),` | Applies the human-readable display label. | The helper converts the display string into a Dataverse Label object. | Users and makers see this label in Power Apps. | Unclear display names reduce usability. |
| 226: `        Description = Label(description),` | Applies the field description. | Descriptions become metadata documentation visible to makers/admins. | This supports future maintainability. | Missing descriptions make the data model harder to interpret. |
| 227: `        RequiredLevel = Optional(),` | Marks the field optional. | The helper returns AttributeRequiredLevel.None for non-primary fields. | Integration-led fields should not block manual or automated row creation in MVP. | Making fields required can break sync processes. |
| 228: `        OptionSet = optionSet` | Attaches the local option set to the choice field. | This connects the generated Source System options to the field metadata. | Without it, the choice field has no options. | Missing option set causes invalid field definition. |
| 229: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 230: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 231: `(blank)` | Blank spacing line. | No runtime behavior. It only separates sections to make the script readable. | Readable scripts are easier to review and safer to operate. | No technical risk; deleting it only reduces readability. |
| 232: `static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description)` | Defines a helper for global choice columns. | The helper creates a picklist field that references an existing global option set by name. | Used for Status via aso_aistatus. | If the global choice does not exist or has a different logical name, creation fails. |
| 233: `{` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 234: `    return new PicklistAttributeMetadata` | Creates picklist/choice column metadata. | PicklistAttributeMetadata is the SDK class for Dataverse choice fields. | Used for both local and global choices. | Wrong metadata class prevents choice behavior. |
| 235: `    {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 236: `        SchemaName = schemaName,` | Applies the schema name passed to the helper. | The schema name becomes the Dataverse logical identifier after creation. | Correct schema names are required for flows, scripts, and ALM consistency. | Typos are expensive to fix after creation. |
| 237: `        DisplayName = Label(displayName),` | Applies the human-readable display label. | The helper converts the display string into a Dataverse Label object. | Users and makers see this label in Power Apps. | Unclear display names reduce usability. |
| 238: `        Description = Label(description),` | Applies the field description. | Descriptions become metadata documentation visible to makers/admins. | This supports future maintainability. | Missing descriptions make the data model harder to interpret. |
| 239: `        RequiredLevel = Optional(),` | Marks the field optional. | The helper returns AttributeRequiredLevel.None for non-primary fields. | Integration-led fields should not block manual or automated row creation in MVP. | Making fields required can break sync processes. |
| 240: `        OptionSet = new OptionSetMetadata` | C# statement or structural line used by the script. | This line participates in setting up metadata, control flow, or SDK execution for Dataverse. | It contributes to creating or validating the External Sync Ledger schema. | Changing it without understanding the surrounding block can break compilation or metadata behavior. |
| 241: `        {` | Opens a code block. | C# uses braces to group statements under an if, else, try, catch, loop, method, or initializer. | Correct structure ensures the right statements execute together. | Missing or misplaced braces cause compile errors or wrong control flow. |
| 242: `            IsGlobal = true,` | Marks the option set reference as global. | This tells Dataverse the field should use an existing shared option set. | Shared choices keep statuses consistent across ASO tables. | Setting it false would create a local choice instead and break consistency. |
| 243: `            Name = globalChoiceName,` | Sets the name of the existing global choice to reuse. | For Status, this resolves to aso_aistatus. | The script references the global choice created earlier in Phase 2. | Wrong name causes a global choice lookup error. |
| 244: `            OptionSetType = OptionSetType.Picklist` | Sets the option set type to picklist. | Picklist creates a standard single-select choice column. | This matches the Source System and Status selection pattern. | Wrong option set type can create incorrect choice behavior. |
| 245: `        }` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |
| 246: `    };` | Closes an initializer and ends the statement. | This finishes a C# object or collection initializer such as a request or field list. | It tells the compiler the metadata object/list definition is complete. | Missing semicolon or brace causes compile errors. |
| 247: `}` | Closes a code block. | This ends the current C# scope opened by a matching brace. | It keeps conditions, loops, methods, and object initializers structurally valid. | Missing closing braces cause build errors. |

## Appendix A - Full script

```csharp
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";
const string SolutionUniqueName = "ASOCore";
const string EntityLogicalName = "aso_externalsyncledger";
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
    Console.WriteLine("Creating External Sync Ledger table...");

    var createEntityRequest = new CreateEntityRequest
    {
        Entity = new EntityMetadata
        {
            SchemaName = "aso_externalsyncledger",
            DisplayName = Label("External Sync Ledger"),
            DisplayCollectionName = Label("External Sync Ledgers"),
            Description = Label("Tracks external IDs, hashes, processed times, and status across ingress and integration systems."),
            OwnershipType = OwnershipTypes.OrganizationOwned
        },
        PrimaryAttribute = new StringAttributeMetadata
        {
            SchemaName = "aso_name",
            DisplayName = Label("Name"),
            Description = Label("Primary name for the External Sync Ledger record."),
            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            MaxLength = 200
        },
        SolutionUniqueName = SolutionUniqueName
    };

    service.Execute(createEntityRequest);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("CREATED TABLE: aso_externalsyncledger");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("SKIP existing table: aso_externalsyncledger");
    Console.ResetColor();
}

var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);

var fields = new List<AttributeMetadata>
{
    LocalChoice("aso_sourcesystem", "Source System",
        new[] { "HubSpot", "SAP", "PowerAutomate", "Foundry", "CustomerInsights" },
        "External or integration system that produced or owns the synchronized identifier."),

    Text("aso_externalentity", "External Entity", 100, "External entity or object name, for example contact, lead, business partner, or journey."),
    Text("aso_externalid", "External ID", 200, "External system identifier."),
    Text("aso_dataverseentity", "Dataverse Entity", 100, "Dataverse table/entity logical or business name."),
    Text("aso_dataverseid", "Dataverse ID", 100, "Dataverse row identifier."),
    Text("aso_lasthash", "Last Hash", 200, "Last processed hash used for idempotency and change detection."),
    DateTimeField("aso_lastprocessedon", "Last Processed On", "Timestamp of the latest successful or attempted processing."),

    GlobalChoice("aso_status", "Status", "aso_aistatus", "Processing status. Uses existing ASO AI Status global choice for MVP consistency."),

    Text("aso_correlationid", "Correlation ID", 100, "Correlation ID shared across integration, automation, and orchestration components.")
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
Console.WriteLine("Publishing External Sync Ledger table customizations...");

service.Execute(new PublishXmlRequest
{
    ParameterXml = "<importexportxml><entities><entity>aso_externalsyncledger</entity></entities></importexportxml>"
});

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Done. Validate in ASO.Core → Tables → External Sync Ledger → Columns.");
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
```

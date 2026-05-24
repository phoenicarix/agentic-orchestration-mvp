# ASO.Core Phase 2 CI - Lead Extension C# Script Explained
**Document type:** Enterprise technical explanation and code walkthrough  **Project:** Agentic Sales Orchestrator  **Environment:** Phoenicarix-CI  **Target solution:** ASO.Core  **Target table:** Existing Microsoft Dataverse Lead table (`lead`)  **Version:** v1.0  **Date:** 2026-05-23  
---
## 1. Executive summary
This document explains the C# / .NET script used to programmatically extend the existing Dynamics 365 / Dataverse Lead table for the Agentic Sales Orchestrator MVP. The script creates ASO-specific Lead columns in bulk, associates them with the `ASO.Core` solution, skips columns that already exist, publishes the Lead table customizations, and writes clear terminal output for validation.
The business reason is simple: manually creating dozens of columns in the Power Apps UI is slow and error-prone. A script is faster, repeatable, auditable, and easier for customer IT teams to review.
## 2. Explanation for three audiences
### For a non-technical stakeholder
Think of the Lead table as a customer intake form. We added new empty boxes to that form, such as AI score, SAP customer reference, communication status, and Customer Insights journey information. The script created those boxes automatically instead of forcing a consultant to click through them one by one.
### For a product owner
The script establishes the Lead data contract needed by later phases. Later Power Automate flows, Foundry orchestration, Sales Qualification Agent output, SAP context, and Customer Insights writeback can all read from or write to known, governed fields. This reduces ambiguity in backlog items and acceptance criteria.
### For a developer or platform admin
The script uses the Microsoft Dataverse .NET SDK. It creates `AttributeMetadata` objects, checks existing attributes using `RetrieveEntityRequest`, creates missing columns using `CreateAttributeRequest`, attaches new attributes to the `ASO.Core` solution through `SolutionUniqueName`, and publishes only the Lead entity using `PublishXmlRequest`.
## 3. What the script creates
| # | Display name | Schema name | Type | Purpose |
|---:|---|---|---|---|
| 1 | HubSpot Contact ID | `aso_hubspotcontactid` | Text | External HubSpot/ingress identifier. |
| 2 | HubSpot Source | `aso_hubspotsource` | Text | Source label for the inbound lead channel. |
| 3 | SAP Business Partner ID | `aso_sapbusinesspartnerid` | Text | Reference to SAP Business Partner. |
| 4 | SAP Customer ID | `aso_sapcustomerid` | Text | Reference to SAP Customer. |
| 5 | AI Fit Level | `aso_aifitlevel` | Local choice | Strong, Moderate, Weak. |
| 6 | AI Lead Score | `aso_aileadscore` | Whole number | Score constrained to 0-100. |
| 7 | AI Qualification Rationale | `aso_aiqualificationrationale` | Multiline text | Seller-facing explanation from AI. |
| 8 | AI Outreach Draft | `aso_aioutreachdraft` | Multiline text | Draft message only; not direct-send. |
| 9 | AI Routing Decision | `aso_airoutingdecision` | Local choice | Nurture, SDR, AE, Reject, NeedsReview, ExistingAccountReview. |
| 10 | AI Confidence | `aso_aiconfidence` | Decimal | 0.00-1.00 confidence with precision 2. |
| 11 | AI Last Run On | `aso_ailastrunon` | Date and time | Last AI orchestration timestamp. |
| 12 | AI Agent Status | `aso_aiagentstatus` | Global choice | Uses aso_aistatus / aso_AIStatus. |
| 13 | AI SAP Context Summary | `aso_aisapcontextsummary` | Multiline text | SAP-derived AI context summary. |
| 14 | AI SAP Match Flag | `aso_aisapmatchflag` | Yes/No | Likely SAP match, default No. |
| 15 | AI Correlation ID | `aso_aicorrelationid` | Text | Correlation identifier for traceability. |
| 16 | Sales Qualification Agent Status | `aso_salesqualificationagentstatus` | Local choice | NotStarted, Running, Completed, Failed, NeedsReview. |
| 17 | Sales Qualification Agent Score | `aso_salesqualificationagentscore` | Whole number | Score constrained to 0-100. |
| 18 | Sales Qualification Agent Rationale | `aso_salesqualificationagentrationale` | Multiline text | Output rationale from Sales Qualification Agent. |
| 19 | Sales Qualification Agent Last Run On | `aso_salesqualificationagentlastrunon` | Date and time | Sales agent last run timestamp. |
| 20 | Foundry Final Qualification Decision | `aso_foundryfinalqualificationdecision` | Text | Final post-validation decision. |
| 21 | Foundry Review Required | `aso_foundryreviewrequired` | Yes/No | Human review gate, default No. |
| 22 | Communication State | `aso_communicationstate` | Global choice | Uses aso_communicationstate. |
| 23 | Lifecycle Communication Stage | `aso_lifecyclecommunicationstage` | Global choice | Uses aso_lifecyclecommunicationstage. |
| 24 | Journey Participation Status | `aso_journeyparticipationstatus` | Global choice | Uses aso_journeyparticipationstatus. |
| 25 | Customer Insights Journey ID | `aso_customerinsightsjourneyid` | Text | Latest journey reference. |
| 26 | Customer Insights Journey Name | `aso_customerinsightsjourneyname` | Text | Latest journey name. |
| 27 | Customer Insights Last Entry On | `aso_customerinsightslastentryon` | Date and time | Last journey entry timestamp. |
| 28 | Customer Insights Last Interaction On | `aso_customerinsightslastinteractionon` | Date and time | Last interaction timestamp. |
| 29 | Customer Insights Last Interaction Type | `aso_customerinsightslastinteractiontype` | Local choice | EmailSent, Open, Click, FormSubmit, Reply, Unsubscribe, Bounce, CustomAction. |
| 30 | Email Consent Status | `aso_emailconsentstatus` | Global choice | Uses aso_emailconsentstatus. |
| 31 | Compliance Profile Name | `aso_complianceprofilename` | Text | Customer Insights compliance profile name. |
| 32 | Communication Hold Reason | `aso_communicationholdreason` | Multiline text | Suppression or hold reason. |

## 4. Preconditions
- .NET SDK is installed on the Mac.
- Power Platform CLI authentication has been completed against `https://phoenicarix-ci.crm4.dynamics.com`.
- `ASO.Core` exists in Phoenicarix-CI.
- The Step 2 global choices already exist in `ASO.Core`: `aso_aistatus`, `aso_communicationstate`, `aso_lifecyclecommunicationstage`, `aso_journeyparticipationstatus`, and `aso_emailconsentstatus`.
- A backup of `ASO.Core` was exported before running the script.
## 5. Commands used on Mac
```bash
mkdir -p ~/aso-dataverse-tools/lead-extension
cd ~/aso-dataverse-tools/lead-extension
dotnet new console --framework net8.0
dotnet add package Microsoft.PowerPlatform.Dataverse.Client
nano Program.cs
dotnet run
```
## 6. Full script with line numbers
```csharp
001: using Microsoft.Crm.Sdk.Messages;
002: using Microsoft.PowerPlatform.Dataverse.Client;
003: using Microsoft.Xrm.Sdk;
004: using Microsoft.Xrm.Sdk.Messages;
005: using Microsoft.Xrm.Sdk.Metadata;
006: 
007: const string DataverseUrl = "https://phoenicarix-ci.crm4.dynamics.com";
008: const string SolutionUniqueName = "ASOCore";
009: const string EntityLogicalName = "lead";
010: const int LanguageCode = 1033;
011: 
012: var connectionString =
013:     $@"AuthType=OAuth;
014:        Url={DataverseUrl};
015:        LoginPrompt=Auto;
016:        ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;
017:        RedirectUri=http://localhost";
018: 
019: using var service = new ServiceClient(connectionString);
020: 
021: if (!service.IsReady)
022: {
023:     Console.ForegroundColor = ConsoleColor.Red;
024:     Console.WriteLine("Connection failed:");
025:     Console.WriteLine(service.LastError);
026:     Console.ResetColor();
027:     return;
028: }
029: 
030: Console.WriteLine($"Connected to: {DataverseUrl}");
031: Console.WriteLine($"Target solution: {SolutionUniqueName}");
032: Console.WriteLine($"Target table: {EntityLogicalName}");
033: Console.WriteLine();
034: 
035: var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);
036: 
037: var fields = new List<AttributeMetadata>
038: {
039:     Text("aso_hubspotcontactid", "HubSpot Contact ID", 100, "External ingress key; do not configure HubSpot yet."),
040:     Text("aso_hubspotsource", "HubSpot Source", 100, "Ingress source reference; keep governed."),
041:     Text("aso_sapbusinesspartnerid", "SAP Business Partner ID", 100, "SAP reference only."),
042:     Text("aso_sapcustomerid", "SAP Customer ID", 100, "SAP reference only."),
043: 
044:     LocalChoice("aso_aifitlevel", "AI Fit Level", new[] { "Strong", "Moderate", "Weak" }, "AI fit level."),
045:     WholeNumber("aso_aileadscore", "AI Lead Score", 0, 100, "AI lead score 0-100."),
046: 
047:     Memo("aso_aiqualificationrationale", "AI Qualification Rationale", "Seller-facing rationale."),
048:     Memo("aso_aioutreachdraft", "AI Outreach Draft", "Draft only; not direct-send."),
049: 
050:     LocalChoice("aso_airoutingdecision", "AI Routing Decision",
051:         new[] { "Nurture", "SDR", "AE", "Reject", "NeedsReview", "ExistingAccountReview" },
052:         "AI routing decision."),
053: 
054:     DecimalNumber("aso_aiconfidence", "AI Confidence", 0m, 1m, 2, "AI confidence 0.00-1.00."),
055:     DateTimeField("aso_ailastrunon", "AI Last Run On", "Last AI run timestamp."),
056: 
057:     GlobalChoice("aso_aiagentstatus", "AI Agent Status", "aso_aistatus", "Uses global AI status choice."),
058: 
059:     Memo("aso_aisapcontextsummary", "AI SAP Context Summary", "SAP-derived summary after governed integration."),
060:     YesNo("aso_aisapmatchflag", "AI SAP Match Flag", "Likely SAP match."),
061:     Text("aso_aicorrelationid", "AI Correlation ID", 100, "Last run correlation."),
062: 
063:     LocalChoice("aso_salesqualificationagentstatus", "Sales Qualification Agent Status",
064:         new[] { "NotStarted", "Running", "Completed", "Failed", "NeedsReview" },
065:         "Sales qualification agent status."),
066: 
067:     WholeNumber("aso_salesqualificationagentscore", "Sales Qualification Agent Score", 0, 100, "Sales agent score if available."),
068:     Memo("aso_salesqualificationagentrationale", "Sales Qualification Agent Rationale", "Output from Sales Qualification Agent."),
069:     DateTimeField("aso_salesqualificationagentlastrunon", "Sales Qualification Agent Last Run On", "Timestamp."),
070: 
071:     Text("aso_foundryfinalqualificationdecision", "Foundry Final Qualification Decision", 100, "Post-validation decision."),
072:     YesNo("aso_foundryreviewrequired", "Foundry Review Required", "Human review gate."),
073: 
074:     GlobalChoice("aso_communicationstate", "Communication State", "aso_communicationstate", "Uses global Communication State choice."),
075:     GlobalChoice("aso_lifecyclecommunicationstage", "Lifecycle Communication Stage", "aso_lifecyclecommunicationstage", "Uses global Lifecycle Communication Stage choice."),
076:     GlobalChoice("aso_journeyparticipationstatus", "Journey Participation Status", "aso_journeyparticipationstatus", "Uses global Journey Participation Status choice."),
077: 
078:     Text("aso_customerinsightsjourneyid", "Customer Insights Journey ID", 100, "Latest journey reference."),
079:     Text("aso_customerinsightsjourneyname", "Customer Insights Journey Name", 200, "Latest journey name."),
080:     DateTimeField("aso_customerinsightslastentryon", "Customer Insights Last Entry On", "Last entry."),
081:     DateTimeField("aso_customerinsightslastinteractionon", "Customer Insights Last Interaction On", "Last interaction."),
082: 
083:     LocalChoice("aso_customerinsightslastinteractiontype", "Customer Insights Last Interaction Type",
084:         new[] { "EmailSent", "Open", "Click", "FormSubmit", "Reply", "Unsubscribe", "Bounce", "CustomAction" },
085:         "Customer Insights last interaction type."),
086: 
087:     GlobalChoice("aso_emailconsentstatus", "Email Consent Status", "aso_emailconsentstatus", "Uses global Email Consent Status choice."),
088: 
089:     Text("aso_complianceprofilename", "Compliance Profile Name", 200, "Profile used."),
090:     Memo("aso_communicationholdreason", "Communication Hold Reason", "Suppression / hold reason.")
091: };
092: 
093: foreach (var field in fields)
094: {
095:     var logicalName = field.SchemaName!.ToLowerInvariant();
096: 
097:     if (existingAttributes.Contains(logicalName))
098:     {
099:         Console.ForegroundColor = ConsoleColor.Yellow;
100:         Console.WriteLine($"SKIP existing: {logicalName}");
101:         Console.ResetColor();
102:         continue;
103:     }
104: 
105:     try
106:     {
107:         Console.WriteLine($"Creating: {logicalName} ...");
108: 
109:         var request = new CreateAttributeRequest
110:         {
111:             EntityName = EntityLogicalName,
112:             Attribute = field,
113:             SolutionUniqueName = SolutionUniqueName
114:         };
115: 
116:         service.Execute(request);
117: 
118:         Console.ForegroundColor = ConsoleColor.Green;
119:         Console.WriteLine($"CREATED: {logicalName}");
120:         Console.ResetColor();
121:     }
122:     catch (Exception ex)
123:     {
124:         Console.ForegroundColor = ConsoleColor.Red;
125:         Console.WriteLine($"FAILED: {logicalName}");
126:         Console.WriteLine(ex.Message);
127:         Console.ResetColor();
128:     }
129: }
130: 
131: Console.WriteLine();
132: Console.WriteLine("Publishing Lead table customizations...");
133: 
134: service.Execute(new PublishXmlRequest
135: {
136:     ParameterXml = "<importexportxml><entities><entity>lead</entity></entities></importexportxml>"
137: });
138: 
139: Console.ForegroundColor = ConsoleColor.Green;
140: Console.WriteLine("Done. Validate in ASO.Core → Tables → Lead → Columns.");
141: Console.ResetColor();
142: 
143: static HashSet<string> GetExistingAttributeLogicalNames(ServiceClient service, string entityLogicalName)
144: {
145:     var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
146:     {
147:         LogicalName = entityLogicalName,
148:         EntityFilters = EntityFilters.Attributes,
149:         RetrieveAsIfPublished = true
150:     });
151: 
152:     return response.EntityMetadata.Attributes
153:         .Where(a => !string.IsNullOrWhiteSpace(a.LogicalName))
154:         .Select(a => a.LogicalName!)
155:         .ToHashSet(StringComparer.OrdinalIgnoreCase);
156: }
157: 
158: static Label Label(string value) => new(value, LanguageCode);
159: 
160: static AttributeRequiredLevelManagedProperty Optional()
161: {
162:     return new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
163: }
164: 
165: static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description)
166: {
167:     return new StringAttributeMetadata
168:     {
169:         SchemaName = schemaName,
170:         DisplayName = Label(displayName),
171:         Description = Label(description),
172:         RequiredLevel = Optional(),
173:         MaxLength = maxLength
174:     };
175: }
176: 
177: static MemoAttributeMetadata Memo(string schemaName, string displayName, string description)
178: {
179:     return new MemoAttributeMetadata
180:     {
181:         SchemaName = schemaName,
182:         DisplayName = Label(displayName),
183:         Description = Label(description),
184:         RequiredLevel = Optional(),
185:         MaxLength = 4000
186:     };
187: }
188: 
189: static IntegerAttributeMetadata WholeNumber(string schemaName, string displayName, int min, int max, string description)
190: {
191:     return new IntegerAttributeMetadata
192:     {
193:         SchemaName = schemaName,
194:         DisplayName = Label(displayName),
195:         Description = Label(description),
196:         RequiredLevel = Optional(),
197:         MinValue = min,
198:         MaxValue = max,
199:         Format = IntegerFormat.None
200:     };
201: }
202: 
203: static DecimalAttributeMetadata DecimalNumber(string schemaName, string displayName, decimal min, decimal max, int precision, string description)
204: {
205:     return new DecimalAttributeMetadata
206:     {
207:         SchemaName = schemaName,
208:         DisplayName = Label(displayName),
209:         Description = Label(description),
210:         RequiredLevel = Optional(),
211:         MinValue = min,
212:         MaxValue = max,
213:         Precision = precision
214:     };
215: }
216: 
217: static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description)
218: {
219:     return new DateTimeAttributeMetadata
220:     {
221:         SchemaName = schemaName,
222:         DisplayName = Label(displayName),
223:         Description = Label(description),
224:         RequiredLevel = Optional(),
225:         Format = DateTimeFormat.DateAndTime,
226:         DateTimeBehavior = DateTimeBehavior.UserLocal
227:     };
228: }
229: 
230: static BooleanAttributeMetadata YesNo(string schemaName, string displayName, string description)
231: {
232:     return new BooleanAttributeMetadata
233:     {
234:         SchemaName = schemaName,
235:         DisplayName = Label(displayName),
236:         Description = Label(description),
237:         RequiredLevel = Optional(),
238:         DefaultValue = false,
239:         OptionSet = new BooleanOptionSetMetadata(
240:             new OptionMetadata(Label("Yes"), 1),
241:             new OptionMetadata(Label("No"), 0)
242:         )
243:     };
244: }
245: 
246: static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)
247: {
248:     var optionSet = new OptionSetMetadata
249:     {
250:         IsGlobal = false,
251:         OptionSetType = OptionSetType.Picklist
252:     };
253: 
254:     foreach (var value in values)
255:     {
256:         optionSet.Options.Add(new OptionMetadata(Label(value), null));
257:     }
258: 
259:     return new PicklistAttributeMetadata
260:     {
261:         SchemaName = schemaName,
262:         DisplayName = Label(displayName),
263:         Description = Label(description),
264:         RequiredLevel = Optional(),
265:         OptionSet = optionSet
266:     };
267: }
268: 
269: static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description)
270: {
271:     return new PicklistAttributeMetadata
272:     {
273:         SchemaName = schemaName,
274:         DisplayName = Label(displayName),
275:         Description = Label(description),
276:         RequiredLevel = Optional(),
277:         OptionSet = new OptionSetMetadata
278:         {
279:             IsGlobal = true,
280:             Name = globalChoiceName,
281:             OptionSetType = OptionSetType.Picklist
282:         }
283:     };
284: }
```
## 7. Line-by-line explanation
Blank lines are included in the line numbering. In the table below, blank lines are described as readability spacing.
| Line | Code | Explanation |
|---:|---|---|
| 1 | `using Microsoft.Crm.Sdk.Messages;` | Imports the Microsoft.Crm.Sdk.Messages namespace so the script can use SDK classes without writing their full names every time. |
| 2 | `using Microsoft.PowerPlatform.Dataverse.Client;` | Imports the Microsoft.PowerPlatform.Dataverse.Client namespace so the script can use SDK classes without writing their full names every time. |
| 3 | `using Microsoft.Xrm.Sdk;` | Imports the Microsoft.Xrm.Sdk namespace so the script can use SDK classes without writing their full names every time. |
| 4 | `using Microsoft.Xrm.Sdk.Messages;` | Imports the Microsoft.Xrm.Sdk.Messages namespace so the script can use SDK classes without writing their full names every time. |
| 5 | `using Microsoft.Xrm.Sdk.Metadata;` | Imports the Microsoft.Xrm.Sdk.Metadata namespace so the script can use SDK classes without writing their full names every time. |
| 6 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 7 | `const string DataverseUrl = &quot;https://phoenicarix-ci.crm4.dynamics.com&quot;;` | Defines the Dataverse environment URL. This points the script to Phoenicarix-CI, the current ASO trial build environment. |
| 8 | `const string SolutionUniqueName = &quot;ASOCore&quot;;` | Defines the technical solution unique name. The SDK uses this to add created columns to ASO.Core instead of leaving them only in the default solution layer. |
| 9 | `const string EntityLogicalName = &quot;lead&quot;;` | Defines the logical name of the target Dataverse table. 'lead' is the standard Dynamics 365 Lead table. |
| 10 | `const int LanguageCode = 1033;` | Defines the language code for labels. 1033 is English (United States), used for display names and descriptions. |
| 11 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 12 | `var connectionString =` | Starts building the connection string used by the Dataverse client to authenticate and connect. |
| 13 | `    $@&quot;AuthType=OAuth;` | Specifies OAuth authentication. This is the modern interactive login pattern for Dataverse tools. |
| 14 | `       Url={DataverseUrl};` | Injects the configured Dataverse URL into the connection string. |
| 15 | `       LoginPrompt=Auto;` | Allows the client to open an interactive login prompt only when needed. |
| 16 | `       ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;` | Uses Microsoft's public Dataverse tooling client ID, a common client ID for interactive Dataverse tooling scenarios. |
| 17 | `       RedirectUri=http://localhost&quot;;` | Defines the local redirect URL used during interactive OAuth login. |
| 18 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 19 | `using var service = new ServiceClient(connectionString);` | Imports the var service = new ServiceClient(connectionString) namespace so the script can use SDK classes without writing their full names every time. |
| 20 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 21 | `if (!service.IsReady)` | Checks whether the Dataverse connection was established successfully before attempting metadata changes. |
| 22 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 23 | `    Console.ForegroundColor = ConsoleColor.Red;` | Changes console output to red to make failures visible in the terminal. |
| 24 | `    Console.WriteLine(&quot;Connection failed:&quot;);` | Prints a clear connection failure heading if authentication or environment access fails. |
| 25 | `    Console.WriteLine(service.LastError);` | Prints the detailed ServiceClient error message to support troubleshooting. |
| 26 | `    Console.ResetColor();` | Resets the terminal color so later output is not accidentally colored. |
| 27 | `    return;` | Stops the script immediately. This prevents metadata changes from running without a valid connection. |
| 28 | `}` | Closes the current code block. |
| 29 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 30 | `Console.WriteLine($&quot;Connected to: {DataverseUrl}&quot;);` | Prints the environment URL so the operator can confirm the script is targeting the intended Dataverse environment. |
| 31 | `Console.WriteLine($&quot;Target solution: {SolutionUniqueName}&quot;);` | Prints the solution unique name so the operator can confirm the target solution layer. |
| 32 | `Console.WriteLine($&quot;Target table: {EntityLogicalName}&quot;);` | Prints the target table logical name, which should be 'lead'. |
| 33 | `Console.WriteLine();` | Writes a blank line to make terminal output easier to read. |
| 34 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 35 | `var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);` | Retrieves all existing Lead column logical names. This enables safe re-runs by skipping columns that already exist. |
| 36 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 37 | `var fields = new List&lt;AttributeMetadata&gt;` | Creates an in-memory list of Dataverse column definitions. Nothing is created yet; the list is only a blueprint. |
| 38 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 39 | `    Text(&quot;aso_hubspotcontactid&quot;, &quot;HubSpot Contact ID&quot;, 100, &quot;External ingress key; do not configure HubSpot yet.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 40 | `    Text(&quot;aso_hubspotsource&quot;, &quot;HubSpot Source&quot;, 100, &quot;Ingress source reference; keep governed.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 41 | `    Text(&quot;aso_sapbusinesspartnerid&quot;, &quot;SAP Business Partner ID&quot;, 100, &quot;SAP reference only.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 42 | `    Text(&quot;aso_sapcustomerid&quot;, &quot;SAP Customer ID&quot;, 100, &quot;SAP reference only.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 43 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 44 | `    LocalChoice(&quot;aso_aifitlevel&quot;, &quot;AI Fit Level&quot;, new[] { &quot;Strong&quot;, &quot;Moderate&quot;, &quot;Weak&quot; }, &quot;AI fit level.&quot;),` | Adds a local choice column definition. The options belong only to this specific Lead column. |
| 45 | `    WholeNumber(&quot;aso_aileadscore&quot;, &quot;AI Lead Score&quot;, 0, 100, &quot;AI lead score 0-100.&quot;),` | Adds a whole-number column definition. The script uses min and max values to keep scores within the intended range. |
| 46 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 47 | `    Memo(&quot;aso_aiqualificationrationale&quot;, &quot;AI Qualification Rationale&quot;, &quot;Seller-facing rationale.&quot;),` | Adds a multiline text column definition. Memo fields store longer rationale, summaries, drafts, and hold reasons. |
| 48 | `    Memo(&quot;aso_aioutreachdraft&quot;, &quot;AI Outreach Draft&quot;, &quot;Draft only; not direct-send.&quot;),` | Adds a multiline text column definition. Memo fields store longer rationale, summaries, drafts, and hold reasons. |
| 49 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 50 | `    LocalChoice(&quot;aso_airoutingdecision&quot;, &quot;AI Routing Decision&quot;,` | Adds a local choice column definition. The options belong only to this specific Lead column. |
| 51 | `        new[] { &quot;Nurture&quot;, &quot;SDR&quot;, &quot;AE&quot;, &quot;Reject&quot;, &quot;NeedsReview&quot;, &quot;ExistingAccountReview&quot; },` | Provides the list of option labels for the local choice column being defined above. |
| 52 | `        &quot;AI routing decision.&quot;),` | Provides the final description argument for the multi-line field definition above. |
| 53 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 54 | `    DecimalNumber(&quot;aso_aiconfidence&quot;, &quot;AI Confidence&quot;, 0m, 1m, 2, &quot;AI confidence 0.00-1.00.&quot;),` | Adds a decimal-number column definition. This is used for confidence values between 0.00 and 1.00. |
| 55 | `    DateTimeField(&quot;aso_ailastrunon&quot;, &quot;AI Last Run On&quot;, &quot;Last AI run timestamp.&quot;),` | Adds a Date and Time column definition. The helper later configures DateTimeBehavior.UserLocal. |
| 56 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 57 | `    GlobalChoice(&quot;aso_aiagentstatus&quot;, &quot;AI Agent Status&quot;, &quot;aso_aistatus&quot;, &quot;Uses global AI status choice.&quot;),` | Adds a choice column definition that reuses an existing global choice from ASO.Core. |
| 58 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 59 | `    Memo(&quot;aso_aisapcontextsummary&quot;, &quot;AI SAP Context Summary&quot;, &quot;SAP-derived summary after governed integration.&quot;),` | Adds a multiline text column definition. Memo fields store longer rationale, summaries, drafts, and hold reasons. |
| 60 | `    YesNo(&quot;aso_aisapmatchflag&quot;, &quot;AI SAP Match Flag&quot;, &quot;Likely SAP match.&quot;),` | Adds a Boolean Yes/No column definition with a default value of No/false. |
| 61 | `    Text(&quot;aso_aicorrelationid&quot;, &quot;AI Correlation ID&quot;, 100, &quot;Last run correlation.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 62 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 63 | `    LocalChoice(&quot;aso_salesqualificationagentstatus&quot;, &quot;Sales Qualification Agent Status&quot;,` | Adds a local choice column definition. The options belong only to this specific Lead column. |
| 64 | `        new[] { &quot;NotStarted&quot;, &quot;Running&quot;, &quot;Completed&quot;, &quot;Failed&quot;, &quot;NeedsReview&quot; },` | Provides the list of option labels for the local choice column being defined above. |
| 65 | `        &quot;Sales qualification agent status.&quot;),` | Provides the final description argument for the multi-line field definition above. |
| 66 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 67 | `    WholeNumber(&quot;aso_salesqualificationagentscore&quot;, &quot;Sales Qualification Agent Score&quot;, 0, 100, &quot;Sales agent score if available.&quot;),` | Adds a whole-number column definition. The script uses min and max values to keep scores within the intended range. |
| 68 | `    Memo(&quot;aso_salesqualificationagentrationale&quot;, &quot;Sales Qualification Agent Rationale&quot;, &quot;Output from Sales Qualification Agent.&quot;),` | Adds a multiline text column definition. Memo fields store longer rationale, summaries, drafts, and hold reasons. |
| 69 | `    DateTimeField(&quot;aso_salesqualificationagentlastrunon&quot;, &quot;Sales Qualification Agent Last Run On&quot;, &quot;Timestamp.&quot;),` | Adds a Date and Time column definition. The helper later configures DateTimeBehavior.UserLocal. |
| 70 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 71 | `    Text(&quot;aso_foundryfinalqualificationdecision&quot;, &quot;Foundry Final Qualification Decision&quot;, 100, &quot;Post-validation decision.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 72 | `    YesNo(&quot;aso_foundryreviewrequired&quot;, &quot;Foundry Review Required&quot;, &quot;Human review gate.&quot;),` | Adds a Boolean Yes/No column definition with a default value of No/false. |
| 73 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 74 | `    GlobalChoice(&quot;aso_communicationstate&quot;, &quot;Communication State&quot;, &quot;aso_communicationstate&quot;, &quot;Uses global Communication State choice.&quot;),` | Adds a choice column definition that reuses an existing global choice from ASO.Core. |
| 75 | `    GlobalChoice(&quot;aso_lifecyclecommunicationstage&quot;, &quot;Lifecycle Communication Stage&quot;, &quot;aso_lifecyclecommunicationstage&quot;, &quot;Uses global Lifecycle Communication Stage choice.&quot;),` | Adds a choice column definition that reuses an existing global choice from ASO.Core. |
| 76 | `    GlobalChoice(&quot;aso_journeyparticipationstatus&quot;, &quot;Journey Participation Status&quot;, &quot;aso_journeyparticipationstatus&quot;, &quot;Uses global Journey Participation Status choice.&quot;),` | Adds a choice column definition that reuses an existing global choice from ASO.Core. |
| 77 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 78 | `    Text(&quot;aso_customerinsightsjourneyid&quot;, &quot;Customer Insights Journey ID&quot;, 100, &quot;Latest journey reference.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 79 | `    Text(&quot;aso_customerinsightsjourneyname&quot;, &quot;Customer Insights Journey Name&quot;, 200, &quot;Latest journey name.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 80 | `    DateTimeField(&quot;aso_customerinsightslastentryon&quot;, &quot;Customer Insights Last Entry On&quot;, &quot;Last entry.&quot;),` | Adds a Date and Time column definition. The helper later configures DateTimeBehavior.UserLocal. |
| 81 | `    DateTimeField(&quot;aso_customerinsightslastinteractionon&quot;, &quot;Customer Insights Last Interaction On&quot;, &quot;Last interaction.&quot;),` | Adds a Date and Time column definition. The helper later configures DateTimeBehavior.UserLocal. |
| 82 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 83 | `    LocalChoice(&quot;aso_customerinsightslastinteractiontype&quot;, &quot;Customer Insights Last Interaction Type&quot;,` | Adds a local choice column definition. The options belong only to this specific Lead column. |
| 84 | `        new[] { &quot;EmailSent&quot;, &quot;Open&quot;, &quot;Click&quot;, &quot;FormSubmit&quot;, &quot;Reply&quot;, &quot;Unsubscribe&quot;, &quot;Bounce&quot;, &quot;CustomAction&quot; },` | Provides the list of option labels for the local choice column being defined above. |
| 85 | `        &quot;Customer Insights last interaction type.&quot;),` | Provides the final description argument for the multi-line field definition above. |
| 86 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 87 | `    GlobalChoice(&quot;aso_emailconsentstatus&quot;, &quot;Email Consent Status&quot;, &quot;aso_emailconsentstatus&quot;, &quot;Uses global Email Consent Status choice.&quot;),` | Adds a choice column definition that reuses an existing global choice from ASO.Core. |
| 88 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 89 | `    Text(&quot;aso_complianceprofilename&quot;, &quot;Compliance Profile Name&quot;, 200, &quot;Profile used.&quot;),` | Adds a single-line text column definition to the blueprint. Text columns are used for IDs, names, and short references. |
| 90 | `    Memo(&quot;aso_communicationholdreason&quot;, &quot;Communication Hold Reason&quot;, &quot;Suppression / hold reason.&quot;)` | Adds a multiline text column definition. Memo fields store longer rationale, summaries, drafts, and hold reasons. |
| 91 | `};` | Closes an object or collection initializer and ends the statement. |
| 92 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 93 | `foreach (var field in fields)` | Starts a loop over every field blueprint so each column can be created consistently. |
| 94 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 95 | `    var logicalName = field.SchemaName!.ToLowerInvariant();` | Reads the schema name from the field definition and normalizes it to lowercase for reliable comparison with existing attributes. |
| 96 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 97 | `    if (existingAttributes.Contains(logicalName))` | Checks whether this column already exists. This makes the script idempotent enough for safe retry after partial success. |
| 98 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 99 | `        Console.ForegroundColor = ConsoleColor.Yellow;` | Changes console output to yellow to indicate a non-fatal skip because a field already exists. |
| 100 | `        Console.WriteLine($&quot;SKIP existing: {logicalName}&quot;);` | Prints that an existing column was found and will not be recreated. |
| 101 | `        Console.ResetColor();` | Resets the terminal color so later output is not accidentally colored. |
| 102 | `        continue;` | Moves to the next field without running the creation logic for the current one. |
| 103 | `    }` | Closes the current code block. |
| 104 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 105 | `    try` | Starts protected execution for one field creation attempt. If this field fails, the script can catch the error and continue. |
| 106 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 107 | `        Console.WriteLine($&quot;Creating: {logicalName} ...&quot;);` | Prints which column is about to be created. This gives live progress feedback in the terminal. |
| 108 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 109 | `        var request = new CreateAttributeRequest` | Builds the Dataverse SDK request that creates one column on the target table. |
| 110 | `        {` | Opens a code block. The statements until the matching closing brace belong together. |
| 111 | `            EntityName = EntityLogicalName,` | Tells the create request which table receives the column. In this script, it resolves to the Lead table. |
| 112 | `            Attribute = field,` | Attaches the current column metadata blueprint to the create request. |
| 113 | `            SolutionUniqueName = SolutionUniqueName` | Associates the created column with ASO.Core so the component appears in the intended unmanaged solution. |
| 114 | `        };` | Closes an object or collection initializer and ends the statement. |
| 115 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 116 | `        service.Execute(request);` | Sends the CreateAttributeRequest to Dataverse. This is the exact moment the column is created. |
| 117 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 118 | `        Console.ForegroundColor = ConsoleColor.Green;` | Changes console output to green to indicate successful creation or completion. |
| 119 | `        Console.WriteLine($&quot;CREATED: {logicalName}&quot;);` | Prints successful creation for the current logical name. |
| 120 | `        Console.ResetColor();` | Resets the terminal color so later output is not accidentally colored. |
| 121 | `    }` | Closes the current code block. |
| 122 | `    catch (Exception ex)` | Catches any error for the current field so a single failure does not hide the exact problem. |
| 123 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 124 | `        Console.ForegroundColor = ConsoleColor.Red;` | Changes console output to red to make failures visible in the terminal. |
| 125 | `        Console.WriteLine($&quot;FAILED: {logicalName}&quot;);` | Prints which column failed so the operator can troubleshoot the specific field. |
| 126 | `        Console.WriteLine(ex.Message);` | Prints the Dataverse or SDK error message returned for the failed field. |
| 127 | `        Console.ResetColor();` | Resets the terminal color so later output is not accidentally colored. |
| 128 | `    }` | Closes the current code block. |
| 129 | `}` | Closes the current code block. |
| 130 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 131 | `Console.WriteLine();` | Writes a blank line to make terminal output easier to read. |
| 132 | `Console.WriteLine(&quot;Publishing Lead table customizations...&quot;);` | Prints that the creation loop is finished and the script is now publishing Lead metadata. |
| 133 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 134 | `service.Execute(new PublishXmlRequest` | Sends a publish request so the Lead metadata changes become available in the maker portal and apps. |
| 135 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 136 | `    ParameterXml = &quot;&lt;importexportxml&gt;&lt;entities&gt;&lt;entity&gt;lead&lt;/entity&gt;&lt;/entities&gt;&lt;/importexportxml&gt;&quot;` | Specifies that only the Lead table/entity should be published, avoiding a broad environment-wide publish. |
| 137 | `});` | Part of the surrounding C# statement or object initializer. It contributes to building or executing the Dataverse metadata operation. |
| 138 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 139 | `Console.ForegroundColor = ConsoleColor.Green;` | Changes console output to green to indicate successful creation or completion. |
| 140 | `Console.WriteLine(&quot;Done. Validate in ASO.Core → Tables → Lead → Columns.&quot;);` | Prints the final success instruction for manual validation in Power Apps. |
| 141 | `Console.ResetColor();` | Resets the terminal color so later output is not accidentally colored. |
| 142 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 143 | `static HashSet&lt;string&gt; GetExistingAttributeLogicalNames(ServiceClient service, string entityLogicalName)` | Declares a helper function that returns existing column logical names as a set for fast lookup. |
| 144 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 145 | `    var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest` | Calls Dataverse metadata retrieval and casts the response so the script can inspect the Lead table attributes. |
| 146 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 147 | `        LogicalName = entityLogicalName,` | Passes the target entity logical name into the metadata retrieval request. |
| 148 | `        EntityFilters = EntityFilters.Attributes,` | Requests only attribute/column metadata, which is faster and enough for this duplicate check. |
| 149 | `        RetrieveAsIfPublished = true` | Includes unpublished metadata changes in the result, useful during iterative schema work. |
| 150 | `    });` | Part of the surrounding C# statement or object initializer. It contributes to building or executing the Dataverse metadata operation. |
| 151 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 152 | `    return response.EntityMetadata.Attributes` | Starts building the returned set from the attributes collection on the table metadata. |
| 153 | `        .Where(a =&gt; !string.IsNullOrWhiteSpace(a.LogicalName))` | Filters out metadata rows without a logical name to avoid null or empty values. |
| 154 | `        .Select(a =&gt; a.LogicalName!)` | Projects each attribute metadata object into only its logical name string. |
| 155 | `        .ToHashSet(StringComparer.OrdinalIgnoreCase);` | Converts the logical names into a case-insensitive HashSet, making existence checks fast and reliable. |
| 156 | `}` | Closes the current code block. |
| 157 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 158 | `static Label Label(string value) =&gt; new(value, LanguageCode);` | Declares a helper that wraps plain text into a Dataverse Label with the configured language code. |
| 159 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 160 | `static AttributeRequiredLevelManagedProperty Optional()` | Declares a helper that marks custom columns as optional rather than required. |
| 161 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 162 | `    return new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);` | Creates the optional required-level metadata object used by all column helpers. |
| 163 | `}` | Closes the current code block. |
| 164 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 165 | `static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description)` | Declares the helper that builds single-line text column metadata. |
| 166 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 167 | `    return new StringAttributeMetadata` | Creates the Dataverse metadata object for a single-line text column. |
| 168 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 169 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 170 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 171 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 172 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 173 | `        MaxLength = maxLength` | Sets the maximum text length for the column. |
| 174 | `    };` | Closes an object or collection initializer and ends the statement. |
| 175 | `}` | Closes the current code block. |
| 176 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 177 | `static MemoAttributeMetadata Memo(string schemaName, string displayName, string description)` | Declares the helper that builds multiline text column metadata. |
| 178 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 179 | `    return new MemoAttributeMetadata` | Creates the Dataverse metadata object for a multiline text column. |
| 180 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 181 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 182 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 183 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 184 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 185 | `        MaxLength = 4000` | Sets the maximum text length for the column. |
| 186 | `    };` | Closes an object or collection initializer and ends the statement. |
| 187 | `}` | Closes the current code block. |
| 188 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 189 | `static IntegerAttributeMetadata WholeNumber(string schemaName, string displayName, int min, int max, string description)` | Declares the helper that builds whole-number column metadata. |
| 190 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 191 | `    return new IntegerAttributeMetadata` | Creates the Dataverse metadata object for a whole-number column. |
| 192 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 193 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 194 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 195 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 196 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 197 | `        MinValue = min,` | Sets the minimum allowed numeric value. |
| 198 | `        MaxValue = max,` | Sets the maximum allowed numeric value. |
| 199 | `        Format = IntegerFormat.None` | Uses a plain whole-number format rather than duration, timezone, or language-specific integer formats. |
| 200 | `    };` | Closes an object or collection initializer and ends the statement. |
| 201 | `}` | Closes the current code block. |
| 202 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 203 | `static DecimalAttributeMetadata DecimalNumber(string schemaName, string displayName, decimal min, decimal max, int precision, string description)` | Declares the helper that builds decimal-number column metadata. |
| 204 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 205 | `    return new DecimalAttributeMetadata` | Creates the Dataverse metadata object for a decimal-number column. |
| 206 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 207 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 208 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 209 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 210 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 211 | `        MinValue = min,` | Sets the minimum allowed numeric value. |
| 212 | `        MaxValue = max,` | Sets the maximum allowed numeric value. |
| 213 | `        Precision = precision` | Sets the number of decimal places allowed for the decimal column. |
| 214 | `    };` | Closes an object or collection initializer and ends the statement. |
| 215 | `}` | Closes the current code block. |
| 216 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 217 | `static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description)` | Declares the helper that builds date/time column metadata. |
| 218 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 219 | `    return new DateTimeAttributeMetadata` | Creates the Dataverse metadata object for a date/time column. |
| 220 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 221 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 222 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 223 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 224 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 225 | `        Format = DateTimeFormat.DateAndTime,` | Configures the date/time column to store both date and time, not date-only. |
| 226 | `        DateTimeBehavior = DateTimeBehavior.UserLocal` | Stores the value as user-local date/time behavior, matching the MVP tenant time-policy guidance. |
| 227 | `    };` | Closes an object or collection initializer and ends the statement. |
| 228 | `}` | Closes the current code block. |
| 229 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 230 | `static BooleanAttributeMetadata YesNo(string schemaName, string displayName, string description)` | Declares the helper that builds Yes/No column metadata. |
| 231 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 232 | `    return new BooleanAttributeMetadata` | Creates the Dataverse metadata object for a Boolean Yes/No column. |
| 233 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 234 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 235 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 236 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 237 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 238 | `        DefaultValue = false,` | Sets Boolean fields to No by default, which is safer for flags such as review required or SAP match. |
| 239 | `        OptionSet = new BooleanOptionSetMetadata(` | Defines the two labels and values behind a Dataverse Yes/No column. |
| 240 | `            new OptionMetadata(Label(&quot;Yes&quot;), 1),` | Defines the Yes option with numeric value 1. |
| 241 | `            new OptionMetadata(Label(&quot;No&quot;), 0)` | Defines the No option with numeric value 0. |
| 242 | `        )` | Part of the surrounding C# statement or object initializer. It contributes to building or executing the Dataverse metadata operation. |
| 243 | `    };` | Closes an object or collection initializer and ends the statement. |
| 244 | `}` | Closes the current code block. |
| 245 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 246 | `static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)` | Declares the helper that builds a local Dataverse choice column. |
| 247 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 248 | `    var optionSet = new OptionSetMetadata` | Creates an option set metadata object that will hold the local choice options. |
| 249 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 250 | `        IsGlobal = false,` | Marks the option set as local to this column, not reusable as a global choice. |
| 251 | `        OptionSetType = OptionSetType.Picklist` | Specifies that the choice is a normal single-select picklist. |
| 252 | `    };` | Closes an object or collection initializer and ends the statement. |
| 253 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 254 | `    foreach (var value in values)` | Loops over the provided option labels for the local choice helper. |
| 255 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 256 | `        optionSet.Options.Add(new OptionMetadata(Label(value), null));` | Adds one option to the local choice. The null value lets Dataverse assign the numeric option value automatically. |
| 257 | `    }` | Closes the current code block. |
| 258 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 259 | `    return new PicklistAttributeMetadata` | Creates the Dataverse metadata object for a single-select choice column. |
| 260 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 261 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 262 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 263 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 264 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 265 | `        OptionSet = optionSet` | Attaches the local option set built above to the picklist column metadata. |
| 266 | `    };` | Closes an object or collection initializer and ends the statement. |
| 267 | `}` | Closes the current code block. |
| 268 | `(blank)` | Blank spacing line. It improves readability only; it does not change behavior. |
| 269 | `static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description)` | Declares the helper that builds a choice column bound to an existing global choice. |
| 270 | `{` | Opens a code block. The statements until the matching closing brace belong together. |
| 271 | `    return new PicklistAttributeMetadata` | Creates the Dataverse metadata object for a single-select choice column. |
| 272 | `    {` | Opens a code block. The statements until the matching closing brace belong together. |
| 273 | `        SchemaName = schemaName,` | Sets the column schema/logical name passed into the helper. This must use the aso_ prefix. |
| 274 | `        DisplayName = Label(displayName),` | Sets the user-facing column name by wrapping the display text into a Dataverse label. |
| 275 | `        Description = Label(description),` | Sets the maker-facing column description by wrapping the text into a Dataverse label. |
| 276 | `        RequiredLevel = Optional(),` | Marks the field as optional. This avoids breaking existing Lead creation and import processes. |
| 277 | `        OptionSet = new OptionSetMetadata` | Part of the surrounding C# statement or object initializer. It contributes to building or executing the Dataverse metadata operation. |
| 278 | `        {` | Opens a code block. The statements until the matching closing brace belong together. |
| 279 | `            IsGlobal = true,` | Marks the option set reference as global, meaning Dataverse should reuse an existing global choice. |
| 280 | `            Name = globalChoiceName,` | Specifies the global choice logical name that this column should reuse. |
| 281 | `            OptionSetType = OptionSetType.Picklist` | Specifies that the choice is a normal single-select picklist. |
| 282 | `        }` | Closes the current code block. |
| 283 | `    };` | Closes an object or collection initializer and ends the statement. |
| 284 | `}` | Closes the current code block. |

## 8. How the script works end to end
1. It imports the Dataverse SDK namespaces.
2. It sets the environment, solution, table, and label language.
3. It opens an authenticated `ServiceClient` connection to Dataverse.
4. It checks whether the connection is usable.
5. It reads the existing Lead column logical names.
6. It builds an in-memory list of desired ASO Lead columns.
7. It loops through each desired column.
8. It skips any column that already exists.
9. It creates every missing column using `CreateAttributeRequest`.
10. It associates each created column with `ASO.Core` through `SolutionUniqueName`.
11. It publishes the Lead table customizations.
12. It instructs the operator to validate the result in Power Apps.
## 9. Why the script uses helper functions
The script contains helpers such as `Text`, `Memo`, `WholeNumber`, `DecimalNumber`, `DateTimeField`, `YesNo`, `LocalChoice`, and `GlobalChoice`. These helpers prevent copy/paste mistakes. Instead of manually writing full metadata objects for every field, the field list stays compact and readable. This also makes future scripts for Opportunity, Account, and Contact easier to create from the same pattern.
## 10. Global choices versus local choices in this script
A global choice is reused across several tables or columns. In the script, `AI Agent Status`, `Communication State`, `Lifecycle Communication Stage`, `Journey Participation Status`, and `Email Consent Status` reuse global choices that were created in ASO.Core first.
A local choice belongs only to one column. In the script, `AI Fit Level`, `AI Routing Decision`, `Sales Qualification Agent Status`, and `Customer Insights Last Interaction Type` are local choices because their values are specific to the Lead table extension.
## 11. Safety and idempotency
The script is not a full deployment framework, but it is safe enough for controlled MVP schema work because it checks whether each field already exists before creating it. If the script is run a second time, existing fields are skipped rather than recreated. This is important when a long metadata script partially succeeds and then fails on one field.
The script also uses `try/catch` per field. This means a single field error is printed clearly and does not hide which field caused the issue.
## 12. Validation checklist
After the script finishes, validate:
- Power Apps environment is `Phoenicarix-CI`.
- Solution is `ASO.Core`.
- Table is `Lead`.
- Search for `aso_` under Lead columns.
- Confirm all expected Lead fields exist.
- Confirm global-choice fields use the intended global choices.
- Publish all customizations if the script did not publish successfully.
- Export managed and unmanaged backups.
## 13. Recommended backup names
```text
ASO.Core_Phase2_CI_LeadExtension_unmanaged_v1_0_20260523.zip
ASO.Core_Phase2_CI_LeadExtension_managed_v1_0_20260523.zip
```
## 14. Troubleshooting
| Symptom | Likely cause | Fix |
|---|---|---|
| Connection failed | Wrong environment URL or login issue | Confirm URL and authenticate again. |
| Global choice field fails | The global choice logical name is different | Open ASO.Core > Choices and confirm the internal name. |
| Column already exists | Script was already run or field was manually created | This is normally safe; the script skips it. |
| Field created but not visible in solution | Solution association issue | Add existing column to ASO.Core manually or verify SolutionUniqueName. |
| Compiler error | Package missing or code pasted incorrectly | Run `dotnet restore`, inspect Program.cs, and compare braces. |
| Login popup does not open | OAuth prompt/session issue | Close browser auth windows and rerun `dotnet run`. |

## 15. Developer notes
- `CreateAttributeRequest` is metadata operation, not data operation. It changes table schema, not Lead records.
- `SolutionUniqueName` is critical for ALM discipline. Without it, the field may still exist but not be obviously part of ASO.Core.
- `RetrieveAsIfPublished = true` helps during iterative development because it reads metadata that may not yet be fully published.
- Local choice option numeric values are assigned by Dataverse because the script passes `null` as the option value. This avoids manual option-number collisions.
- Date/time fields use `UserLocal`, which is appropriate for user-facing timestamps in this MVP.
## 16. What not to do
- Do not run this script against the wrong environment.
- Do not change `EntityLogicalName` unless you are intentionally creating a separate script for another table.
- Do not run it before global choices are created.
- Do not use the default solution for governed ASO schema work.
- Do not treat this as a production deployment pipeline. It is an MVP metadata automation script.
## 17. Next steps
After the Lead table extension is validated and backed up, repeat the same controlled pattern for Opportunity, Account, and Contact. Each table should get its own script or a carefully parameterized script with explicit table-specific field definitions.

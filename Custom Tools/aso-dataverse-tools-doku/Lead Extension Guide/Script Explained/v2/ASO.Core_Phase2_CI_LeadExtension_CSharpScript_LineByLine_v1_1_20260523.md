# ASO.Core Phase 2 CI - Lead Extension C# Script: True Line-by-Line Explanation
**Document type:** customer-ready developer handover and code walkthrough  
**Environment:** Phoenicarix-CI  
**Solution:** ASO.Core  
**Target table:** Lead (`lead`)  
**Script file:** `Program.cs`  
**Version:** v1.1 corrected line-by-line edition  
**Date:** 2026-05-23  

> Correction note: this edition explicitly explains every numbered line of the C# script. Blank lines and braces are included because they are part of the script structure.

---

## 1. What this script does

This C# script connects to the Phoenicarix-CI Dataverse environment, targets the existing Microsoft Dynamics 365 Lead table, checks which ASO columns already exist, creates the missing Lead extension columns, associates each created column with the ASO.Core solution, and publishes the Lead metadata.

## 2. Field inventory created by the script

| # | Display name | Schema name | Type | Purpose |
|---:|---|---|---|---|
| 1 | HubSpot Contact ID | `aso_hubspotcontactid` | Text | External ingress key; does not configure HubSpot yet. |
| 2 | HubSpot Source | `aso_hubspotsource` | Text | Inbound source reference. |
| 3 | SAP Business Partner ID | `aso_sapbusinesspartnerid` | Text | SAP Business Partner reference only. |
| 4 | SAP Customer ID | `aso_sapcustomerid` | Text | SAP Customer reference only. |
| 5 | AI Fit Level | `aso_aifitlevel` | Local choice | Strong, Moderate, Weak. |
| 6 | AI Lead Score | `aso_aileadscore` | Whole number | 0-100 AI score. |
| 7 | AI Qualification Rationale | `aso_aiqualificationrationale` | Multiline text | Seller-facing AI rationale. |
| 8 | AI Outreach Draft | `aso_aioutreachdraft` | Multiline text | Draft only, not a send mechanism. |
| 9 | AI Routing Decision | `aso_airoutingdecision` | Local choice | Nurture, SDR, AE, Reject, NeedsReview, ExistingAccountReview. |
| 10 | AI Confidence | `aso_aiconfidence` | Decimal | 0.00-1.00 confidence score. |
| 11 | AI Last Run On | `aso_ailastrunon` | Date/time | Last AI run timestamp. |
| 12 | AI Agent Status | `aso_aiagentstatus` | Global choice | Uses aso_aistatus global choice. |
| 13 | AI SAP Context Summary | `aso_aisapcontextsummary` | Multiline text | SAP-derived summary. |
| 14 | AI SAP Match Flag | `aso_aisapmatchflag` | Yes/No | Likely SAP match flag, default No. |
| 15 | AI Correlation ID | `aso_aicorrelationid` | Text | Latest orchestration/integration correlation ID. |
| 16 | Sales Qualification Agent Status | `aso_salesqualificationagentstatus` | Local choice | NotStarted, Running, Completed, Failed, NeedsReview. |
| 17 | Sales Qualification Agent Score | `aso_salesqualificationagentscore` | Whole number | 0-100 Sales Qualification Agent score. |
| 18 | Sales Qualification Agent Rationale | `aso_salesqualificationagentrationale` | Multiline text | Sales Qualification Agent rationale. |
| 19 | Sales Qualification Agent Last Run On | `aso_salesqualificationagentlastrunon` | Date/time | Sales Qualification Agent run timestamp. |
| 20 | Foundry Final Qualification Decision | `aso_foundryfinalqualificationdecision` | Text | Foundry final decision as flexible text for MVP. |
| 21 | Foundry Review Required | `aso_foundryreviewrequired` | Yes/No | Human review gate, default No. |
| 22 | Communication State | `aso_communicationstate` | Global choice | Uses aso_communicationstate global choice. |
| 23 | Lifecycle Communication Stage | `aso_lifecyclecommunicationstage` | Global choice | Uses aso_lifecyclecommunicationstage global choice. |
| 24 | Journey Participation Status | `aso_journeyparticipationstatus` | Global choice | Uses aso_journeyparticipationstatus global choice. |
| 25 | Customer Insights Journey ID | `aso_customerinsightsjourneyid` | Text | Latest CI journey ID. |
| 26 | Customer Insights Journey Name | `aso_customerinsightsjourneyname` | Text | Latest CI journey name. |
| 27 | Customer Insights Last Entry On | `aso_customerinsightslastentryon` | Date/time | Last journey entry timestamp. |
| 28 | Customer Insights Last Interaction On | `aso_customerinsightslastinteractionon` | Date/time | Last CI interaction timestamp. |
| 29 | Customer Insights Last Interaction Type | `aso_customerinsightslastinteractiontype` | Local choice | EmailSent, Open, Click, FormSubmit, Reply, Unsubscribe, Bounce, CustomAction. |
| 30 | Email Consent Status | `aso_emailconsentstatus` | Global choice | Uses aso_emailconsentstatus global choice. |
| 31 | Compliance Profile Name | `aso_complianceprofilename` | Text | Compliance profile used. |
| 32 | Communication Hold Reason | `aso_communicationholdreason` | Multiline text | Suppression / hold reason. |

## 3. Full script with line numbers

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

## 4. Every line explained

| Line | Code | What the line does | Why / how it works |
|---:|---|---|---|
| 1 | `using Microsoft.Crm.Sdk.Messages;` | Imports the Dataverse SDK message classes from Microsoft.Crm.Sdk.Messages. | Needed for PublishXmlRequest, which publishes the Lead metadata after the columns are created. |
| 2 | `using Microsoft.PowerPlatform.Dataverse.Client;` | Imports the Dataverse ServiceClient class. | This client handles authentication, connection management, and execution of Dataverse requests from the console app. |
| 3 | `using Microsoft.Xrm.Sdk;` | Imports core Dataverse SDK types. | Provides base interfaces and common SDK types used by metadata request and response objects. |
| 4 | `using Microsoft.Xrm.Sdk.Messages;` | Imports Dataverse SDK request and response message types. | Needed for CreateAttributeRequest, RetrieveEntityRequest, and RetrieveEntityResponse. |
| 5 | `using Microsoft.Xrm.Sdk.Metadata;` | Imports Dataverse metadata classes. | Needed for AttributeMetadata, StringAttributeMetadata, PicklistAttributeMetadata, OptionSetMetadata, and related schema objects. |
| 6 | `(blank)` | Blank separator between imports and configuration. | Improves readability; it has no runtime effect. |
| 7 | `const string DataverseUrl = &quot;https://phoenicarix-ci.crm4.dynamics.com&quot;;` | Defines the target Dataverse environment URL as Phoenicarix-CI. | This is the environment where the script connects and creates the Lead columns. |
| 8 | `const string SolutionUniqueName = &quot;ASOCore&quot;;` | Defines the unmanaged solution unique name as ASOCore. | Dataverse uses this value to associate newly created columns with ASO.Core for ALM discipline. |
| 9 | `const string EntityLogicalName = &quot;lead&quot;;` | Defines the target table logical name as lead. | This points all metadata operations to the standard Dynamics 365 Lead table, not a new custom table. |
| 10 | `const int LanguageCode = 1033;` | Defines the language code 1033. | 1033 represents English (United States) and is used for display labels and descriptions. |
| 11 | `(blank)` | Blank separator before connection-string construction. | Keeps configuration constants visually separate from connection setup. |
| 12 | `var connectionString =` | Starts the connectionString variable assignment. | The following multi-line string contains the OAuth settings used by ServiceClient. |
| 13 | `    $@&quot;AuthType=OAuth;` | Starts an interpolated verbatim OAuth connection string and sets AuthType=OAuth. | OAuth enables interactive Microsoft identity authentication against Dataverse. |
| 14 | `       Url={DataverseUrl};` | Adds the Dataverse URL to the connection string. | The {DataverseUrl} placeholder is replaced with https://phoenicarix-ci.crm4.dynamics.com. |
| 15 | `       LoginPrompt=Auto;` | Sets LoginPrompt=Auto. | The client only prompts for login if no valid token/session is available. |
| 16 | `       ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;` | Sets the public Microsoft tooling client ID. | This client ID is commonly used by Microsoft Dataverse tooling for interactive OAuth scenarios. |
| 17 | `       RedirectUri=http://localhost&quot;;` | Sets RedirectUri=http://localhost and closes the connection string. | The local redirect URI is used by the interactive OAuth login flow. |
| 18 | `(blank)` | Blank separator before creating the Dataverse client. | Improves readability; it has no runtime effect. |
| 19 | `using var service = new ServiceClient(connectionString);` | Creates the Dataverse ServiceClient using the connection string. | using var ensures the connection object is automatically cleaned up when the script exits. |
| 20 | `(blank)` | Blank separator before connection validation. | Improves readability; it has no runtime effect. |
| 21 | `if (!service.IsReady)` | Checks whether the ServiceClient connection is not ready. | Prevents the script from trying to create schema if authentication or connectivity failed. |
| 22 | `{` | Opens the failure-handling block for an unusable connection. | The statements inside run only when service.IsReady is false. |
| 23 | `    Console.ForegroundColor = ConsoleColor.Red;` | Sets terminal text to red. | Makes connection failure output visually obvious. |
| 24 | `    Console.WriteLine(&quot;Connection failed:&quot;);` | Writes the heading Connection failed. | Gives the operator a clear message that the script stopped before metadata changes. |
| 25 | `    Console.WriteLine(service.LastError);` | Writes the detailed ServiceClient error. | Shows the actual authentication, permission, or connection problem returned by the client. |
| 26 | `    Console.ResetColor();` | Resets terminal text color. | Prevents subsequent terminal output from staying red. |
| 27 | `    return;` | Stops the program immediately. | No metadata request is sent if the connection is invalid. |
| 28 | `}` | Closes the failure-handling block. | The script continues only if the connection was ready. |
| 29 | `(blank)` | Blank separator before successful connection output. | Improves readability; it has no runtime effect. |
| 30 | `Console.WriteLine($&quot;Connected to: {DataverseUrl}&quot;);` | Prints the connected Dataverse URL. | Lets the operator verify the script is targeting Phoenicarix-CI, not another environment. |
| 31 | `Console.WriteLine($&quot;Target solution: {SolutionUniqueName}&quot;);` | Prints the target solution unique name. | Lets the operator verify that created columns are intended for ASO.Core. |
| 32 | `Console.WriteLine($&quot;Target table: {EntityLogicalName}&quot;);` | Prints the target table logical name. | Confirms that the table is lead. |
| 33 | `Console.WriteLine();` | Prints a blank line. | Separates the connection banner from the creation log. |
| 34 | `(blank)` | Blank separator before reading current metadata. | Improves readability; it has no runtime effect. |
| 35 | `var existingAttributes = GetExistingAttributeLogicalNames(service, EntityLogicalName);` | Reads existing Lead column logical names into existingAttributes. | This lookup allows the script to skip fields that are already present and makes reruns safer. |
| 36 | `(blank)` | Blank separator before the field blueprint list. | Improves readability; it has no runtime effect. |
| 37 | `var fields = new List&lt;AttributeMetadata&gt;` | Starts a list of AttributeMetadata objects named fields. | This list is the full in-memory blueprint of ASO Lead columns to create. |
| 38 | `{` | Opens the fields collection initializer. | Every field definition until the matching closing brace is added to the list. |
| 39 | `    Text(&quot;aso_hubspotcontactid&quot;, &quot;HubSpot Contact ID&quot;, 100, &quot;External ingress key; do not configure HubSpot yet.&quot;),` | Adds the HubSpot Contact ID text-column definition with schema name aso_hubspotcontactid. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 40 | `    Text(&quot;aso_hubspotsource&quot;, &quot;HubSpot Source&quot;, 100, &quot;Ingress source reference; keep governed.&quot;),` | Adds the HubSpot Source text-column definition with schema name aso_hubspotsource. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 41 | `    Text(&quot;aso_sapbusinesspartnerid&quot;, &quot;SAP Business Partner ID&quot;, 100, &quot;SAP reference only.&quot;),` | Adds the SAP Business Partner ID text-column definition with schema name aso_sapbusinesspartnerid. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 42 | `    Text(&quot;aso_sapcustomerid&quot;, &quot;SAP Customer ID&quot;, 100, &quot;SAP reference only.&quot;),` | Adds the SAP Customer ID text-column definition with schema name aso_sapcustomerid. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 43 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 44 | `    LocalChoice(&quot;aso_aifitlevel&quot;, &quot;AI Fit Level&quot;, new[] { &quot;Strong&quot;, &quot;Moderate&quot;, &quot;Weak&quot; }, &quot;AI fit level.&quot;),` | Starts or defines the local-choice field AI Fit Level with schema name aso_aifitlevel. | LocalChoice creates a choice whose options belong only to this Lead column, not to a shared global choice. Options in this call: Strong, Moderate, Weak. |
| 45 | `    WholeNumber(&quot;aso_aileadscore&quot;, &quot;AI Lead Score&quot;, 0, 100, &quot;AI lead score 0-100.&quot;),` | Adds the AI Lead Score whole-number definition with schema name aso_aileadscore. | The WholeNumber helper enforces numeric bounds 0-100, keeping scoring fields inside the intended range. |
| 46 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 47 | `    Memo(&quot;aso_aiqualificationrationale&quot;, &quot;AI Qualification Rationale&quot;, &quot;Seller-facing rationale.&quot;),` | Adds the AI Qualification Rationale multiline-text definition with schema name aso_aiqualificationrationale. | The Memo helper will create MemoAttributeMetadata with MaxLength 4000, suitable for rationale, drafts, summaries, or hold reasons. |
| 48 | `    Memo(&quot;aso_aioutreachdraft&quot;, &quot;AI Outreach Draft&quot;, &quot;Draft only; not direct-send.&quot;),` | Adds the AI Outreach Draft multiline-text definition with schema name aso_aioutreachdraft. | The Memo helper will create MemoAttributeMetadata with MaxLength 4000, suitable for rationale, drafts, summaries, or hold reasons. |
| 49 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 50 | `    LocalChoice(&quot;aso_airoutingdecision&quot;, &quot;AI Routing Decision&quot;,` | Starts the AI Routing Decision local-choice field definition. | The schema name is aso_airoutingdecision and the display name is AI Routing Decision; the option labels are supplied on the next line. |
| 51 | `        new[] { &quot;Nurture&quot;, &quot;SDR&quot;, &quot;AE&quot;, &quot;Reject&quot;, &quot;NeedsReview&quot;, &quot;ExistingAccountReview&quot; },` | Provides the option labels for the local choice field started immediately above. | Dataverse will create one local choice option for each label: Nurture, SDR, AE, Reject, NeedsReview, ExistingAccountReview. |
| 52 | `        &quot;AI routing decision.&quot;),` | Provides the description argument and closes the multi-line local-choice definition. | This description appears in maker metadata: AI routing decision. |
| 53 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 54 | `    DecimalNumber(&quot;aso_aiconfidence&quot;, &quot;AI Confidence&quot;, 0m, 1m, 2, &quot;AI confidence 0.00-1.00.&quot;),` | Adds the AI Confidence decimal definition with schema name aso_aiconfidence. | The DecimalNumber helper sets MinValue 0, MaxValue 1, and Precision 2 for confidence values such as 0.75 or 0.90. |
| 55 | `    DateTimeField(&quot;aso_ailastrunon&quot;, &quot;AI Last Run On&quot;, &quot;Last AI run timestamp.&quot;),` | Adds the AI Last Run On date/time definition with schema name aso_ailastrunon. | The DateTimeField helper configures a DateAndTime field with UserLocal behavior for user-facing timestamps. |
| 56 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 57 | `    GlobalChoice(&quot;aso_aiagentstatus&quot;, &quot;AI Agent Status&quot;, &quot;aso_aistatus&quot;, &quot;Uses global AI status choice.&quot;),` | Adds the AI Agent Status choice definition with schema name aso_aiagentstatus. | GlobalChoice binds this column to the existing global choice aso_aistatus; the script does not recreate the global choice values. |
| 58 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 59 | `    Memo(&quot;aso_aisapcontextsummary&quot;, &quot;AI SAP Context Summary&quot;, &quot;SAP-derived summary after governed integration.&quot;),` | Adds the AI SAP Context Summary multiline-text definition with schema name aso_aisapcontextsummary. | The Memo helper will create MemoAttributeMetadata with MaxLength 4000, suitable for rationale, drafts, summaries, or hold reasons. |
| 60 | `    YesNo(&quot;aso_aisapmatchflag&quot;, &quot;AI SAP Match Flag&quot;, &quot;Likely SAP match.&quot;),` | Adds the AI SAP Match Flag Yes/No definition with schema name aso_aisapmatchflag. | The YesNo helper creates BooleanAttributeMetadata with default false/No and Yes=1, No=0 options. |
| 61 | `    Text(&quot;aso_aicorrelationid&quot;, &quot;AI Correlation ID&quot;, 100, &quot;Last run correlation.&quot;),` | Adds the AI Correlation ID text-column definition with schema name aso_aicorrelationid. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 62 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 63 | `    LocalChoice(&quot;aso_salesqualificationagentstatus&quot;, &quot;Sales Qualification Agent Status&quot;,` | Starts the Sales Qualification Agent Status local-choice field definition. | The schema name is aso_salesqualificationagentstatus and the display name is Sales Qualification Agent Status; the option labels are supplied on the next line. |
| 64 | `        new[] { &quot;NotStarted&quot;, &quot;Running&quot;, &quot;Completed&quot;, &quot;Failed&quot;, &quot;NeedsReview&quot; },` | Provides the option labels for Sales Qualification Agent Status. | Dataverse will create local options: NotStarted, Running, Completed, Failed, and NeedsReview. |
| 65 | `        &quot;Sales qualification agent status.&quot;),` | Provides the description and closes Sales Qualification Agent Status. | This completes the LocalChoice call with the maker-facing description Sales qualification agent status. |
| 66 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 67 | `    WholeNumber(&quot;aso_salesqualificationagentscore&quot;, &quot;Sales Qualification Agent Score&quot;, 0, 100, &quot;Sales agent score if available.&quot;),` | Adds the Sales Qualification Agent Score whole-number definition with schema name aso_salesqualificationagentscore. | The WholeNumber helper enforces numeric bounds 0-100, keeping scoring fields inside the intended range. |
| 68 | `    Memo(&quot;aso_salesqualificationagentrationale&quot;, &quot;Sales Qualification Agent Rationale&quot;, &quot;Output from Sales Qualification Agent.&quot;),` | Adds the Sales Qualification Agent Rationale multiline-text definition with schema name aso_salesqualificationagentrationale. | The Memo helper will create MemoAttributeMetadata with MaxLength 4000, suitable for rationale, drafts, summaries, or hold reasons. |
| 69 | `    DateTimeField(&quot;aso_salesqualificationagentlastrunon&quot;, &quot;Sales Qualification Agent Last Run On&quot;, &quot;Timestamp.&quot;),` | Adds the Sales Qualification Agent Last Run On date/time definition with schema name aso_salesqualificationagentlastrunon. | The DateTimeField helper configures a DateAndTime field with UserLocal behavior for user-facing timestamps. |
| 70 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 71 | `    Text(&quot;aso_foundryfinalqualificationdecision&quot;, &quot;Foundry Final Qualification Decision&quot;, 100, &quot;Post-validation decision.&quot;),` | Adds the Foundry Final Qualification Decision text-column definition with schema name aso_foundryfinalqualificationdecision. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 72 | `    YesNo(&quot;aso_foundryreviewrequired&quot;, &quot;Foundry Review Required&quot;, &quot;Human review gate.&quot;),` | Adds the Foundry Review Required Yes/No definition with schema name aso_foundryreviewrequired. | The YesNo helper creates BooleanAttributeMetadata with default false/No and Yes=1, No=0 options. |
| 73 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 74 | `    GlobalChoice(&quot;aso_communicationstate&quot;, &quot;Communication State&quot;, &quot;aso_communicationstate&quot;, &quot;Uses global Communication State choice.&quot;),` | Adds the Communication State choice definition with schema name aso_communicationstate. | GlobalChoice binds this column to the existing global choice aso_communicationstate; the script does not recreate the global choice values. |
| 75 | `    GlobalChoice(&quot;aso_lifecyclecommunicationstage&quot;, &quot;Lifecycle Communication Stage&quot;, &quot;aso_lifecyclecommunicationstage&quot;, &quot;Uses global Lifecycle Communication Stage choice.&quot;),` | Adds the Lifecycle Communication Stage choice definition with schema name aso_lifecyclecommunicationstage. | GlobalChoice binds this column to the existing global choice aso_lifecyclecommunicationstage; the script does not recreate the global choice values. |
| 76 | `    GlobalChoice(&quot;aso_journeyparticipationstatus&quot;, &quot;Journey Participation Status&quot;, &quot;aso_journeyparticipationstatus&quot;, &quot;Uses global Journey Participation Status choice.&quot;),` | Adds the Journey Participation Status choice definition with schema name aso_journeyparticipationstatus. | GlobalChoice binds this column to the existing global choice aso_journeyparticipationstatus; the script does not recreate the global choice values. |
| 77 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 78 | `    Text(&quot;aso_customerinsightsjourneyid&quot;, &quot;Customer Insights Journey ID&quot;, 100, &quot;Latest journey reference.&quot;),` | Adds the Customer Insights Journey ID text-column definition with schema name aso_customerinsightsjourneyid. | The Text helper will turn this into StringAttributeMetadata with max length 100; it will later be created on Lead and added to ASO.Core. |
| 79 | `    Text(&quot;aso_customerinsightsjourneyname&quot;, &quot;Customer Insights Journey Name&quot;, 200, &quot;Latest journey name.&quot;),` | Adds the Customer Insights Journey Name text-column definition with schema name aso_customerinsightsjourneyname. | The Text helper will turn this into StringAttributeMetadata with max length 200; it will later be created on Lead and added to ASO.Core. |
| 80 | `    DateTimeField(&quot;aso_customerinsightslastentryon&quot;, &quot;Customer Insights Last Entry On&quot;, &quot;Last entry.&quot;),` | Adds the Customer Insights Last Entry On date/time definition with schema name aso_customerinsightslastentryon. | The DateTimeField helper configures a DateAndTime field with UserLocal behavior for user-facing timestamps. |
| 81 | `    DateTimeField(&quot;aso_customerinsightslastinteractionon&quot;, &quot;Customer Insights Last Interaction On&quot;, &quot;Last interaction.&quot;),` | Adds the Customer Insights Last Interaction On date/time definition with schema name aso_customerinsightslastinteractionon. | The DateTimeField helper configures a DateAndTime field with UserLocal behavior for user-facing timestamps. |
| 82 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 83 | `    LocalChoice(&quot;aso_customerinsightslastinteractiontype&quot;, &quot;Customer Insights Last Interaction Type&quot;,` | Starts the Customer Insights Last Interaction Type local-choice field definition. | The schema name is aso_customerinsightslastinteractiontype and the option labels are supplied on the next line. |
| 84 | `        new[] { &quot;EmailSent&quot;, &quot;Open&quot;, &quot;Click&quot;, &quot;FormSubmit&quot;, &quot;Reply&quot;, &quot;Unsubscribe&quot;, &quot;Bounce&quot;, &quot;CustomAction&quot; },` | Provides the option labels for Customer Insights Last Interaction Type. | Dataverse will create local options: EmailSent, Open, Click, FormSubmit, Reply, Unsubscribe, Bounce, and CustomAction. |
| 85 | `        &quot;Customer Insights last interaction type.&quot;),` | Provides the description and closes Customer Insights Last Interaction Type. | This completes the LocalChoice call with the maker-facing description Customer Insights last interaction type. |
| 86 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 87 | `    GlobalChoice(&quot;aso_emailconsentstatus&quot;, &quot;Email Consent Status&quot;, &quot;aso_emailconsentstatus&quot;, &quot;Uses global Email Consent Status choice.&quot;),` | Adds the Email Consent Status choice definition with schema name aso_emailconsentstatus. | GlobalChoice binds this column to the existing global choice aso_emailconsentstatus; the script does not recreate the global choice values. |
| 88 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 89 | `    Text(&quot;aso_complianceprofilename&quot;, &quot;Compliance Profile Name&quot;, 200, &quot;Profile used.&quot;),` | Adds the Compliance Profile Name text-column definition with schema name aso_complianceprofilename. | The Text helper will turn this into StringAttributeMetadata with max length 200; it will later be created on Lead and added to ASO.Core. |
| 90 | `    Memo(&quot;aso_communicationholdreason&quot;, &quot;Communication Hold Reason&quot;, &quot;Suppression / hold reason.&quot;)` | Adds the Communication Hold Reason multiline-text definition with schema name aso_communicationholdreason. | The Memo helper will create MemoAttributeMetadata with MaxLength 4000, suitable for rationale, drafts, summaries, or hold reasons. |
| 91 | `};` | Closes the current object or collection initializer in Lead field blueprint list. | The semicolon ends the C# statement. |
| 92 | `(blank)` | Closes the fields collection initializer and ends the fields assignment. | At this point the script has a complete list of desired Lead column definitions, but none are created yet. |
| 93 | `foreach (var field in fields)` | Blank separator before the creation loop. | Improves readability; it has no runtime effect. |
| 94 | `{` | Starts a loop over every field definition in fields. | Each iteration attempts to create one Lead column if it does not already exist. |
| 95 | `    var logicalName = field.SchemaName!.ToLowerInvariant();` | Opens the loop body. | The following statements run once for each field definition. |
| 96 | `(blank)` | Gets the field schema name and converts it to lowercase. | Dataverse logical names are typically lowercase, so this normalizes comparison against existing attributes. |
| 97 | `    if (existingAttributes.Contains(logicalName))` | Blank separator before the skip check. | Improves readability; it has no runtime effect. |
| 98 | `    {` | Checks whether the current logical name already exists on Lead. | This avoids duplicate-column errors and supports safe reruns after partial execution. |
| 99 | `        Console.ForegroundColor = ConsoleColor.Yellow;` | Opens the existing-field skip block. | The following statements run only if the field already exists. |
| 100 | `        Console.WriteLine($&quot;SKIP existing: {logicalName}&quot;);` | Sets terminal text to yellow. | Yellow indicates a warning/non-error condition: the field was skipped because it already exists. |
| 101 | `        Console.ResetColor();` | Prints SKIP existing with the field logical name. | Gives a clear audit trail for fields not recreated. |
| 102 | `        continue;` | Resets terminal color. | Returns terminal output to the default color. |
| 103 | `    }` | Skips to the next field in the loop. | No create request is sent for this already-existing field. |
| 104 | `(blank)` | Closes the existing-field skip block. | The script continues to the create path only for missing fields. |
| 105 | `    try` | Blank separator before protected create logic. | Improves readability; it has no runtime effect. |
| 106 | `    {` | Starts a try block for creating the current field. | If Dataverse rejects one field, the script can catch and report the specific error. |
| 107 | `        Console.WriteLine($&quot;Creating: {logicalName} ...&quot;);` | Opens the try block. | The request construction and execution below are protected by the catch block. |
| 108 | `(blank)` | Prints Creating with the current logical name. | Shows live progress before the metadata request is sent. |
| 109 | `        var request = new CreateAttributeRequest` | Blank separator before creating the SDK request object. | Improves readability; it has no runtime effect. |
| 110 | `        {` | Creates a new CreateAttributeRequest object. | This SDK request represents one Dataverse column-creation operation. |
| 111 | `            EntityName = EntityLogicalName,` | Opens the CreateAttributeRequest initializer. | The next properties configure which table, attribute, and solution are used. |
| 112 | `            Attribute = field,` | Sets EntityName to EntityLogicalName. | This tells Dataverse to create the field on the Lead table. |
| 113 | `            SolutionUniqueName = SolutionUniqueName` | Sets Attribute to the current field metadata object. | This passes the field blueprint being processed in the loop. |
| 114 | `        };` | Sets SolutionUniqueName to ASOCore. | This associates the new column with the ASO.Core solution during creation. |
| 115 | `(blank)` | Closes the CreateAttributeRequest initializer. | The request object is now ready to send. |
| 116 | `        service.Execute(request);` | Blank separator before executing the create request. | Improves readability; it has no runtime effect. |
| 117 | `(blank)` | Executes the CreateAttributeRequest against Dataverse. | This is the exact line that creates the missing column in Dataverse metadata. |
| 118 | `        Console.ForegroundColor = ConsoleColor.Green;` | Blank separator before success output. | Improves readability; it has no runtime effect. |
| 119 | `        Console.WriteLine($&quot;CREATED: {logicalName}&quot;);` | Sets terminal text to green. | Green marks a successful column creation. |
| 120 | `        Console.ResetColor();` | Prints CREATED with the current logical name. | Provides evidence in the terminal that the column was created. |
| 121 | `    }` | Resets terminal color. | Returns terminal output to default after the success message. |
| 122 | `    catch (Exception ex)` | Closes the try block. | The create attempt for this field is complete if no exception occurred. |
| 123 | `    {` | Starts the catch block for any exception during this field creation. | This catches SDK, Dataverse, permission, duplicate, or metadata validation errors for the current field. |
| 124 | `        Console.ForegroundColor = ConsoleColor.Red;` | Opens the catch block. | The statements below report the failure cleanly. |
| 125 | `        Console.WriteLine($&quot;FAILED: {logicalName}&quot;);` | Sets terminal text to red. | Red marks an error for the current field. |
| 126 | `        Console.WriteLine(ex.Message);` | Prints FAILED with the field logical name. | Identifies exactly which field failed. |
| 127 | `        Console.ResetColor();` | Prints the exception message. | Shows the detailed Dataverse/SDK reason for the failure. |
| 128 | `    }` | Resets terminal color. | Returns terminal output to default after the failure message. |
| 129 | `}` | Closes the catch block. | After logging the failure, the loop continues to the next field. |
| 130 | `(blank)` | Closes the foreach loop body. | All create/skip handling for the current field is complete. |
| 131 | `Console.WriteLine();` | Blank separator before publish step. | Improves readability; it has no runtime effect. |
| 132 | `Console.WriteLine(&quot;Publishing Lead table customizations...&quot;);` | Prints a blank line. | Separates the field creation log from the publish log. |
| 133 | `(blank)` | Prints that Lead customizations are being published. | Signals that schema creation is complete and metadata publishing is beginning. |
| 134 | `service.Execute(new PublishXmlRequest` | Blank separator before the publish request. | Improves readability; it has no runtime effect. |
| 135 | `{` | Executes a PublishXmlRequest. | Publishes Dataverse metadata changes so the Lead columns become visible and usable. |
| 136 | `    ParameterXml = &quot;&lt;importexportxml&gt;&lt;entities&gt;&lt;entity&gt;lead&lt;/entity&gt;&lt;/entities&gt;&lt;/importexportxml&gt;&quot;` | Opens the PublishXmlRequest initializer. | The next line limits what gets published. |
| 137 | `});` | Sets ParameterXml to publish only the Lead entity. | This avoids publishing the entire environment and targets just the table changed by the script. |
| 138 | `(blank)` | Closes the PublishXmlRequest initializer and Execute call. | The publish request is sent to Dataverse. |
| 139 | `Console.ForegroundColor = ConsoleColor.Green;` | Blank separator before final success output. | Improves readability; it has no runtime effect. |
| 140 | `Console.WriteLine(&quot;Done. Validate in ASO.Core → Tables → Lead → Columns.&quot;);` | Sets terminal text to green. | Green marks the final completion message. |
| 141 | `Console.ResetColor();` | Prints the final validation instruction. | Tells the operator exactly where to validate the result in Power Apps. |
| 142 | `(blank)` | Resets terminal color. | Returns terminal output to default. |
| 143 | `static HashSet&lt;string&gt; GetExistingAttributeLogicalNames(ServiceClient service, string entityLogicalName)` | Blank separator before helper functions. | Separates the main script flow from reusable helper methods. |
| 144 | `{` | Opens a C# block for publish and final message. | All following indented statements are part of that block until the matching closing brace. |
| 145 | `    var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest` | Executes a RetrieveEntityRequest and casts the result to RetrieveEntityResponse. | This retrieves Lead table metadata so the script can inspect existing attributes. |
| 146 | `    {` | Opens a C# block for publish and final message. | All following indented statements are part of that block until the matching closing brace. |
| 147 | `        LogicalName = entityLogicalName,` | Sets the table logical name for metadata retrieval. | The caller passes lead, so Dataverse returns metadata for the Lead table. |
| 148 | `        EntityFilters = EntityFilters.Attributes,` | Requests only attribute/column metadata. | This is faster and more focused than retrieving all table metadata. |
| 149 | `        RetrieveAsIfPublished = true` | Includes unpublished metadata changes in the retrieval result. | Useful when running and validating schema scripts during active customization work. |
| 150 | `    });` | Declares a helper function returning a HashSet of existing attribute logical names. | A HashSet gives fast case-insensitive lookups when deciding whether to skip a field. |
| 151 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 152 | `    return response.EntityMetadata.Attributes` | Starts returning data from the retrieved table attributes collection. | The following chained LINQ calls transform attribute objects into a set of logical names. |
| 153 | `        .Where(a =&gt; !string.IsNullOrWhiteSpace(a.LogicalName))` | Filters out attributes that have no logical name. | Prevents null or empty names from entering the duplicate-check set. |
| 154 | `        .Select(a =&gt; a.LogicalName!)` | Selects only the attribute logical name from each metadata object. | The skip check only needs names, not the full metadata object. |
| 155 | `        .ToHashSet(StringComparer.OrdinalIgnoreCase);` | Converts the list of logical names to a case-insensitive HashSet. | HashSet lookup is fast and StringComparer.OrdinalIgnoreCase avoids casing issues. |
| 156 | `}` | Closes the current C# block in ExistingAttributes. | Control returns to the outer scope after this line. |
| 157 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 158 | `static Label Label(string value) =&gt; new(value, LanguageCode);` | Declares a compact helper method named Label. | It wraps text into a Dataverse Label with the configured language code. |
| 159 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 160 | `static AttributeRequiredLevelManagedProperty Optional()` | Declares a helper method named Optional. | All generated columns call this to avoid making new fields mandatory on existing Lead records. |
| 161 | `{` | Opens a C# block for ExistingAttributes. | All following indented statements are part of that block until the matching closing brace. |
| 162 | `    return new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);` | Creates a required-level metadata object with AttributeRequiredLevel.None. | This marks the generated field as optional in Dataverse metadata. |
| 163 | `}` | Closes the current C# block in ExistingAttributes. | Control returns to the outer scope after this line. |
| 164 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 165 | `static StringAttributeMetadata Text(string schemaName, string displayName, int maxLength, string description)` | Declares the Text helper. | This helper creates single-line text column metadata from schema name, display name, max length, and description. |
| 166 | `{` | Opens a C# block for the previous statement. | All following indented statements are part of that block until the matching closing brace. |
| 167 | `    return new StringAttributeMetadata` | Declares a Label helper. | It converts a plain string into a Dataverse Label using the configured language code. |
| 168 | `    {` | Opens a C# block for label and optional required-level helpers. | All following indented statements are part of that block until the matching closing brace. |
| 169 | `        SchemaName = schemaName,` | Declares an Optional helper. | It centralizes the rule that all generated columns are optional for the MVP. |
| 170 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 171 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 172 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 173 | `        MaxLength = maxLength` | Sets the maximum length passed into the Text helper. | This controls how many characters the single-line text field can store. |
| 174 | `    };` | Declares the Text helper function. | It returns metadata for a single-line Dataverse text column. |
| 175 | `}` | Closes the current C# block in Text. | Control returns to the outer scope after this line. |
| 176 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 177 | `static MemoAttributeMetadata Memo(string schemaName, string displayName, string description)` | Declares the Memo helper. | This helper creates multiline text column metadata for longer content fields. |
| 178 | `{` | Opens a C# block for Text. | All following indented statements are part of that block until the matching closing brace. |
| 179 | `    return new MemoAttributeMetadata` | Creates and returns a MemoAttributeMetadata object. | Dataverse uses this object to create a multiline text column. |
| 180 | `    {` | Opens a C# block for Text. | All following indented statements are part of that block until the matching closing brace. |
| 181 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 182 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 183 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 184 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 185 | `        MaxLength = 4000` | Sets multiline text maximum length to 4000 characters. | This gives enough room for rationales and summaries while keeping the MVP field size controlled. |
| 186 | `    };` | Closes the current object or collection initializer in the script. | The semicolon ends the C# statement. |
| 187 | `}` | Declares the Memo helper function. | It returns metadata for a multiline Dataverse text column. |
| 188 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 189 | `static IntegerAttributeMetadata WholeNumber(string schemaName, string displayName, int min, int max, string description)` | Declares the WholeNumber helper. | This helper creates integer column metadata with explicit minimum and maximum values. |
| 190 | `{` | Opens a C# block for Memo. | All following indented statements are part of that block until the matching closing brace. |
| 191 | `    return new IntegerAttributeMetadata` | Creates and returns an IntegerAttributeMetadata object. | Dataverse uses this object to create a whole-number column. |
| 192 | `    {` | Opens a C# block for Memo. | All following indented statements are part of that block until the matching closing brace. |
| 193 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 194 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 195 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 196 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 197 | `        MinValue = min,` | Sets the minimum numeric value from the helper parameter. | For score/confidence fields this enforces the lower boundary defined in the field list. |
| 198 | `        MaxValue = max,` | Sets the maximum numeric value from the helper parameter. | For score/confidence fields this enforces the upper boundary defined in the field list. |
| 199 | `        Format = IntegerFormat.None` | Declares the WholeNumber helper function. | It returns metadata for an integer column with minimum and maximum values. |
| 200 | `    };` | Closes the current object or collection initializer in WholeNumber. | The semicolon ends the C# statement. |
| 201 | `}` | Closes the current C# block in WholeNumber. | Control returns to the outer scope after this line. |
| 202 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 203 | `static DecimalAttributeMetadata DecimalNumber(string schemaName, string displayName, decimal min, decimal max, int precision, string description)` | Declares the DecimalNumber helper. | This helper creates decimal column metadata with bounds and precision. |
| 204 | `{` | Opens a C# block for WholeNumber. | All following indented statements are part of that block until the matching closing brace. |
| 205 | `    return new DecimalAttributeMetadata` | Creates and returns a DecimalAttributeMetadata object. | Dataverse uses this object to create a decimal column. |
| 206 | `    {` | Opens a C# block for WholeNumber. | All following indented statements are part of that block until the matching closing brace. |
| 207 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 208 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 209 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 210 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 211 | `        MinValue = min,` | Sets the minimum numeric value from the helper parameter. | For score/confidence fields this enforces the lower boundary defined in the field list. |
| 212 | `        MaxValue = max,` | Sets the maximum numeric value from the helper parameter. | For score/confidence fields this enforces the upper boundary defined in the field list. |
| 213 | `        Precision = precision` | Sets the allowed number of decimal places. | For AI Confidence, the script passes precision 2 so values can be stored like 0.85. |
| 214 | `    };` | Declares the DecimalNumber helper function. | It returns metadata for a decimal column with min, max, and precision. |
| 215 | `}` | Closes the current C# block in DecimalNumber. | Control returns to the outer scope after this line. |
| 216 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 217 | `static DateTimeAttributeMetadata DateTimeField(string schemaName, string displayName, string description)` | Declares the DateTimeField helper. | This helper creates Date and Time column metadata with a consistent behavior. |
| 218 | `{` | Opens a C# block for DecimalNumber. | All following indented statements are part of that block until the matching closing brace. |
| 219 | `    return new DateTimeAttributeMetadata` | Creates and returns a DateTimeAttributeMetadata object. | Dataverse uses this object to create a Date and Time column. |
| 220 | `    {` | Opens a C# block for DecimalNumber. | All following indented statements are part of that block until the matching closing brace. |
| 221 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 222 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 223 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 224 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 225 | `        Format = DateTimeFormat.DateAndTime,` | Sets the date/time field to store both date and time. | The field captures exact run or interaction timestamps, not just dates. |
| 226 | `        DateTimeBehavior = DateTimeBehavior.UserLocal` | Sets date/time behavior to UserLocal. | Users see timestamps adjusted according to their Dataverse/user timezone behavior. |
| 227 | `    };` | Closes the current object or collection initializer in DecimalNumber. | The semicolon ends the C# statement. |
| 228 | `}` | Closes the current C# block in the script. | Control returns to the outer scope after this line. |
| 229 | `(blank)` | Declares the DateTimeField helper function. | It returns metadata for a Date and Time column. |
| 230 | `static BooleanAttributeMetadata YesNo(string schemaName, string displayName, string description)` | Declares the YesNo helper. | This helper creates Boolean/Yes-No metadata with a default value and labels. |
| 231 | `{` | Opens a C# block for DateTimeField. | All following indented statements are part of that block until the matching closing brace. |
| 232 | `    return new BooleanAttributeMetadata` | Creates and returns a BooleanAttributeMetadata object. | Dataverse uses this object to create a Yes/No column. |
| 233 | `    {` | Opens a C# block for DateTimeField. | All following indented statements are part of that block until the matching closing brace. |
| 234 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 235 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 236 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 237 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 238 | `        DefaultValue = false,` | Sets the default Boolean value to false. | New Yes/No fields default to No, which is safer for review-required and SAP-match flags. |
| 239 | `        OptionSet = new BooleanOptionSetMetadata(` | Starts defining the Yes/No option set metadata. | The next two lines define the labels and numeric values for Yes and No. |
| 240 | `            new OptionMetadata(Label(&quot;Yes&quot;), 1),` | Defines the Yes option for a Boolean column. | Dataverse stores Yes as numeric value 1 behind the label. |
| 241 | `            new OptionMetadata(Label(&quot;No&quot;), 0)` | Defines the No option for a Boolean column. | Dataverse stores No as numeric value 0 behind the label. |
| 242 | `        )` | Closes the BooleanOptionSetMetadata constructor argument list. | This finishes the Yes/No option set after defining Yes and No options. |
| 243 | `    };` | Declares the YesNo helper function. | It returns metadata for a Dataverse Boolean/Yes-No column. |
| 244 | `}` | Closes the current C# block in YesNo. | Control returns to the outer scope after this line. |
| 245 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 246 | `static PicklistAttributeMetadata LocalChoice(string schemaName, string displayName, string[] values, string description)` | Declares the LocalChoice helper. | This helper creates a local Dataverse choice column from a list of labels. |
| 247 | `{` | Opens a C# block for YesNo. | All following indented statements are part of that block until the matching closing brace. |
| 248 | `    var optionSet = new OptionSetMetadata` | Creates an OptionSetMetadata object for a local choice. | This object will hold all option labels for one local picklist column. |
| 249 | `    {` | Opens a C# block for YesNo. | All following indented statements are part of that block until the matching closing brace. |
| 250 | `        IsGlobal = false,` | Marks this option set as local. | The choice values are stored on the column itself and are not reused globally. |
| 251 | `        OptionSetType = OptionSetType.Picklist` | Sets the option set type to Picklist. | This creates a normal single-select choice field. |
| 252 | `    };` | Closes the current object or collection initializer in YesNo. | The semicolon ends the C# statement. |
| 253 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 254 | `    foreach (var value in values)` | Loops through the option labels supplied to LocalChoice. | Each label becomes one choice option in the local option set. |
| 255 | `    {` | Opens a C# block for YesNo. | All following indented statements are part of that block until the matching closing brace. |
| 256 | `        optionSet.Options.Add(new OptionMetadata(Label(value), null));` | Adds one local choice option to the option set. | The null numeric value lets Dataverse generate the numeric option value automatically. |
| 257 | `    }` | Closes the current C# block in YesNo. | Control returns to the outer scope after this line. |
| 258 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 259 | `    return new PicklistAttributeMetadata` | Creates and returns a PicklistAttributeMetadata object. | Dataverse uses this object to create a single-select choice column. |
| 260 | `    {` | Declares the LocalChoice helper function. | It returns metadata for a choice column with options stored locally on that column. |
| 261 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 262 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 263 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 264 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 265 | `        OptionSet = optionSet` | Attaches the local option set to the picklist column metadata. | Without this line, the choice column would not receive the options built above. |
| 266 | `    };` | Closes the current object or collection initializer in LocalChoice. | The semicolon ends the C# statement. |
| 267 | `}` | Closes the current C# block in LocalChoice. | Control returns to the outer scope after this line. |
| 268 | `(blank)` | Blank spacing line. | It improves readability and separates logical parts of the script; it has no runtime effect. |
| 269 | `static PicklistAttributeMetadata GlobalChoice(string schemaName, string displayName, string globalChoiceName, string description)` | Declares the GlobalChoice helper. | This helper creates a choice column that references an existing global choice by name. |
| 270 | `{` | Opens a C# block for LocalChoice. | All following indented statements are part of that block until the matching closing brace. |
| 271 | `    return new PicklistAttributeMetadata` | Creates and returns a PicklistAttributeMetadata object. | Dataverse uses this object to create a single-select choice column. |
| 272 | `    {` | Opens a C# block for LocalChoice. | All following indented statements are part of that block until the matching closing brace. |
| 273 | `        SchemaName = schemaName,` | Sets the Dataverse schema name on the metadata object. | The helper receives values such as aso_hubspotcontactid and writes them into the column definition. |
| 274 | `        DisplayName = Label(displayName),` | Sets the user-facing display name on the metadata object. | The Label helper applies the configured language code so the maker UI shows the correct name. |
| 275 | `        Description = Label(description),` | Sets the field description on the metadata object. | Descriptions help makers and admins understand why the column exists. |
| 276 | `        RequiredLevel = Optional(),` | Marks the field as optional. | This prevents the new field from becoming mandatory on Lead records and breaking existing processes. |
| 277 | `        OptionSet = new OptionSetMetadata` | Creates an option set reference for a global choice column. | The nested properties identify the global choice to reuse. |
| 278 | `        {` | Opens a C# block for LocalChoice. | All following indented statements are part of that block until the matching closing brace. |
| 279 | `            IsGlobal = true,` | Marks this option set reference as global. | Dataverse should bind the column to an existing global choice instead of creating local options. |
| 280 | `            Name = globalChoiceName,` | Sets the existing global choice logical name to bind. | For example, this points the field to aso_communicationstate or aso_emailconsentstatus. |
| 281 | `            OptionSetType = OptionSetType.Picklist` | Sets the option set type to Picklist. | This creates a normal single-select choice field. |
| 282 | `        }` | Closes the current C# block in the script. | Control returns to the outer scope after this line. |
| 283 | `    };` | Declares the GlobalChoice helper function. | It returns metadata for a choice column bound to an existing global choice. |
| 284 | `}` | Closes the current C# block in GlobalChoice. | Control returns to the outer scope after this line. |

## 5. Reading the script as an execution flow

1. Lines 1-5 import the required Microsoft Dataverse SDK namespaces.
2. Lines 7-10 define the target environment, solution, table, and label language.
3. Lines 12-19 build the connection string and create the ServiceClient.
4. Lines 21-28 stop execution if the connection is not ready.
5. Lines 30-35 print runtime context and read current Lead attributes.
6. Lines 37-92 define the desired ASO Lead column blueprint.
7. Lines 94-130 loop through the blueprint, skip existing fields, and create missing fields.
8. Lines 132-142 publish Lead customizations and print the final validation instruction.
9. Lines 150-284 define reusable helper functions that generate Dataverse metadata objects.

## 6. Validation after running

- Go to Power Apps -> Phoenicarix-CI -> Solutions -> ASO.Core -> Tables -> Lead -> Columns.
- Search for `aso_`.
- Confirm the fields from the inventory are present.
- Confirm choice fields have the intended local/global choice behavior.
- Export backups using the agreed naming pattern.

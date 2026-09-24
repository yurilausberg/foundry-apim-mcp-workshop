#pragma warning disable MEAI001

using Azure.AI.AgentServer.Core;
using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Extensions.AI;

Env.NoClobber().TraversePath().Load();

var projectEndpoint = new Uri(
    Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException(
        "FOUNDRY_PROJECT_ENDPOINT environment variable is not set."));

var deploymentName =
    Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME")
    ?? "gpt-4.1";

var mcpServerEndpoint =
    Environment.GetEnvironmentVariable("MCP_SERVER_ENDPOINT")
    ?? throw new InvalidOperationException(
        "MCP_SERVER_ENDPOINT environment variable is not set.");

var mcpServerName =
    Environment.GetEnvironmentVariable("MCP_SERVER_NAME")
    ?? "workshop_tools";

var autoApprovedTools = ParseToolNames("MCP_AUTO_APPROVED_TOOLS");
var approvalRequiredTools = ParseToolNames("MCP_APPROVAL_REQUIRED_TOOLS");

if (autoApprovedTools.Length == 0 && approvalRequiredTools.Length == 0)
{
    throw new InvalidOperationException(
        "Set MCP_AUTO_APPROVED_TOOLS, MCP_APPROVAL_REQUIRED_TOOLS, or both.");
}

var duplicateTool = autoApprovedTools
    .Intersect(approvalRequiredTools, StringComparer.Ordinal)
    .FirstOrDefault();

if (duplicateTool is not null)
{
    throw new InvalidOperationException(
        $"MCP tool '{duplicateTool}' cannot be both auto-approved and approval-required.");
}

List<AITool> mcpTools = [];

if (autoApprovedTools.Length > 0)
{
    mcpTools.Add(new HostedMcpServerTool(
        serverName: $"{mcpServerName}_auto",
        serverAddress: mcpServerEndpoint)
    {
        AllowedTools = autoApprovedTools,
        ApprovalMode = HostedMcpServerToolApprovalMode.NeverRequire
    });
}

if (approvalRequiredTools.Length > 0)
{
    mcpTools.Add(new HostedMcpServerTool(
        serverName: $"{mcpServerName}_approval",
        serverAddress: mcpServerEndpoint)
    {
        AllowedTools = approvalRequiredTools,
        ApprovalMode = HostedMcpServerToolApprovalMode.AlwaysRequire
    });
}

var agentName =
    Environment.GetEnvironmentVariable("AGENT_NAME")
    ?? "mcp-workshop-agent";

var agentDescription =
    Environment.GetEnvironmentVariable("AGENT_DESCRIPTION")
    ?? "Workshop agent using tools exposed through APIM and MCP";

Azure.Core.TokenCredential credential =
    string.Equals(
        Environment.GetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS"),
        nameof(AzureCliCredential),
        StringComparison.OrdinalIgnoreCase)
        ? new AzureCliCredential()
        : new DefaultAzureCredential();

AIAgent agent = new AIProjectClient(
        projectEndpoint,
        credential)
    .AsAIAgent(
        model: deploymentName,
        instructions: """
            You help workshop participants use tools exposed through MCP.
            Use the available tools whenever the answer depends on tool data.
            Treat all tool data as synthetic or public demonstration data unless
            the user explicitly provides another approved context.
            Never claim the data is from a production or customer system.
            Explain which tool results support your answer.
            Do not execute approval-required actions until the user confirms.
            """,
        name: agentName,
        description: agentDescription,
        tools: mcpTools);

var builder = AgentHost.CreateBuilder(args);
builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol(
    "responses",
    endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();

static string[] ParseToolNames(string variableName)
{
    var value = Environment.GetEnvironmentVariable(variableName);

    return string.IsNullOrWhiteSpace(value)
        ? []
        : value.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}

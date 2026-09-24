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
    ?? "work_request_tools";

var readMcpTool = new HostedMcpServerTool(
    serverName: $"{mcpServerName}_read",
    serverAddress: mcpServerEndpoint)
{
    AllowedTools =
    [
        "getAWorkRequest"
    ],
    ApprovalMode = HostedMcpServerToolApprovalMode.NeverRequire
};

var writeMcpTool = new HostedMcpServerTool(
    serverName: $"{mcpServerName}_write",
    serverAddress: mcpServerEndpoint)
{
    AllowedTools =
    [
        "createAWorkRequest",
        "updateWorkRequestStatus"
    ],
    ApprovalMode = HostedMcpServerToolApprovalMode.AlwaysRequire
};

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
            You help workshop participants evaluate synthetic work requests.
            Use the available MCP tools for work-request data.
            Never claim the data is from a production or customer system.
            Explain the evidence used for every recommendation.
            Treat create and status-update operations as state-changing actions.
            """,
        name: "work-request-workshop-agent",
        description: "Workshop agent using work-request tools exposed through APIM and MCP",
        tools: [readMcpTool, writeMcpTool]);

var builder = AgentHost.CreateBuilder(args);
builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol(
    "responses",
    endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();

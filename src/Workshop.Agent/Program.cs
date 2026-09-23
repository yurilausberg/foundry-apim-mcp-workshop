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
    ?? "gpt-5.4-mini";

var mcpServerEndpoint =
    Environment.GetEnvironmentVariable("MCP_SERVER_ENDPOINT")
    ?? throw new InvalidOperationException(
        "MCP_SERVER_ENDPOINT environment variable is not set.");

var mcpServerName =
    Environment.GetEnvironmentVariable("MCP_SERVER_NAME")
    ?? "service_agreement_tools";

var mcpTool = new HostedMcpServerTool(
    serverName: mcpServerName,
    serverAddress: mcpServerEndpoint)
{
    AllowedTools =
    [
        "getServiceAgreement",
        "createServiceAgreement",
        "updateServiceAgreementStatus"
    ],
    ApprovalMode = HostedMcpServerToolApprovalMode.AlwaysRequire
};

AIAgent agent = new AIProjectClient(
        projectEndpoint,
        new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: """
            You help workshop participants evaluate synthetic service agreement requests.
            Use the available MCP tools for agreement data.
            Never claim the data is from a production or customer system.
            Explain the evidence used for every recommendation.
            Treat create and status-update operations as state-changing actions.
            """,
        name: "service-agreement-workshop-agent",
        description: "Workshop agent using service agreement tools exposed through APIM and MCP",
        tools: [mcpTool]);

var builder = AgentHost.CreateBuilder(args);
builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol(
    "responses",
    endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();

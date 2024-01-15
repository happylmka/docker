using cz.dvojak.k8s.EdgeOperator.Configuration;
using KubeOps.Operator;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEdgeOperatorProject();
var app = builder.Build();
app.ConfigureEdgeOperatorApp();
app.PreRunLogging();
await app.RunOperatorAsync(args);
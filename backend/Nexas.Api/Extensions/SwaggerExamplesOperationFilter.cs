using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nexas.Api.Extensions;

public class SwaggerExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDesc = context.ApiDescription;
        var path = apiDesc.RelativePath ?? string.Empty;
        var method = apiDesc.HttpMethod?.ToUpperInvariant() ?? string.Empty;

        // Purchases POST (already handled)
        if (method == "POST" && path.Contains("purchases", StringComparison.OrdinalIgnoreCase))
        {
            ApplyPurchasesExamples(operation);
            return;
        }

        // Courses
        if (path.StartsWith("courses", StringComparison.OrdinalIgnoreCase))
        {
            if (method == "GET" && !path.Contains("{id}"))
            {
                // GET /courses - list
                if (operation.Responses != null && operation.Responses.TryGetValue("200", out var res200))
                {
                    if (res200.Content != null && res200.Content.TryGetValue("application/json", out var resContent))
                    {
                        var courseObj = new OpenApiObject
                        {
                            ["id"] = new OpenApiInteger(1),
                            ["name"] = new OpenApiString("ASP.NET Core 8 - Clean Architecture"),
                            ["description"] = new OpenApiString("Curso introdutório"),
                            ["priceSingle"] = new OpenApiDouble(299.9)
                        };

                        var arr = new OpenApiArray();
                        arr.Add(courseObj);
                        resContent.Example = arr;
                    }
                }
            }

            if (method == "GET" && path.Contains("{id}"))
            {
                // GET /courses/{id}
                if (operation.Responses != null && operation.Responses.TryGetValue("200", out var res200))
                {
                    if (res200.Content != null && res200.Content.TryGetValue("application/json", out var resContent))
                    {
                        var courseObj = new OpenApiObject
                        {
                            ["id"] = new OpenApiInteger(1),
                            ["name"] = new OpenApiString("ASP.NET Core 8 - Clean Architecture"),
                            ["description"] = new OpenApiString("Descrição detalhada do curso"),
                            ["modules"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(10),
                                    ["name"] = new OpenApiString("Módulo 1: Fundamentos"),
                                    ["lessons"] = new OpenApiArray
                                    {
                                        new OpenApiObject
                                        {
                                            ["id"] = new OpenApiInteger(100),
                                            ["name"] = new OpenApiString("Introdução")
                                        }
                                    }
                                }
                            }
                        };

                        resContent.Example = courseObj;
                    }
                }
            }

            if (method == "POST")
            {
                // POST /courses - request example
                if (operation.RequestBody?.Content != null && operation.RequestBody.Content.TryGetValue("application/json", out var reqContent))
                {
                    var reqExample = new OpenApiObject
                    {
                        ["name"] = new OpenApiString("C# Advanced"),
                        ["description"] = new OpenApiString("Masterclass de C# avançado"),
                        ["priceSingle"] = new OpenApiDouble(199.9)
                    };

                    reqContent.Example = reqExample;
                }

                // POST response 201
                if (operation.Responses != null && operation.Responses.TryGetValue("201", out var res201))
                {
                    if (res201.Content != null && res201.Content.TryGetValue("application/json", out var resContent))
                    {
                        var resExample = new OpenApiObject
                        {
                            ["message"] = new OpenApiString("Curso criado com sucesso!"),
                            ["id"] = new OpenApiInteger(42)
                        };

                        resContent.Example = resExample;
                    }
                }
            }

            return;
        }

        // Me (GET /me)
        if (method == "GET" && path.StartsWith("me", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.Responses != null && operation.Responses.TryGetValue("200", out var res200))
            {
                if (res200.Content != null && res200.Content.TryGetValue("application/json", out var resContent))
                {
                    var obj = new OpenApiObject
                    {
                        ["user_id"] = new OpenApiInteger(1),
                        ["external_id"] = new OpenApiString("user_123_from_agivys"),
                        ["email"] = new OpenApiString("teacher@nexas.com"),
                        ["created_at"] = new OpenApiString("2024-05-08T10:30:00Z")
                    };

                    resContent.Example = obj;
                }
            }

            return;
        }

        // Subscriptions POST
        if (method == "POST" && path.Contains("subscriptions", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.RequestBody?.Content != null && operation.RequestBody.Content.TryGetValue("application/json", out var reqContent))
            {
                var reqExample = new OpenApiObject
                {
                    ["planName"] = new OpenApiString("Premium"),
                    ["value"] = new OpenApiDouble(49.9)
                };

                reqContent.Example = reqExample;
            }

            if (operation.Responses != null && operation.Responses.TryGetValue("201", out var res201))
            {
                if (res201.Content != null && res201.Content.TryGetValue("application/json", out var resContent))
                {
                    var resExample = new OpenApiObject
                    {
                        ["subscriptionId"] = new OpenApiInteger(7)
                    };

                    resContent.Example = resExample;
                }
            }

            return;
        }
    }

    private void ApplyPurchasesExamples(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content != null && operation.RequestBody.Content.TryGetValue("application/json", out var reqContent))
        {
            var reqExample = new OpenApiObject
            {
                ["courseId"] = new OpenApiInteger(1),
                ["amount"] = new OpenApiDouble(197.9),
                ["paymentMethod"] = new OpenApiString("CREDIT_CARD"),
                ["card"] = new OpenApiObject
                {
                    ["holderName"] = new OpenApiString("JOEDER BLANCA TESTE"),
                    ["number"] = new OpenApiString("4444444444444444"),
                    ["expiryMonth"] = new OpenApiString("12"),
                    ["expiryYear"] = new OpenApiString("2030"),
                    ["ccv"] = new OpenApiString("123"),
                    ["holderCpfCnpj"] = new OpenApiString("39395533870")
                }
            };

            reqContent.Example = reqExample;
        }

        if (operation.Responses != null && operation.Responses.TryGetValue("201", out var res201))
        {
            if (res201.Content != null && res201.Content.TryGetValue("application/json", out var resContent))
            {
                var resExample = new OpenApiObject
                {
                    ["purchaseId"] = new OpenApiInteger(3),
                    ["status"] = new OpenApiString("CONFIRMED"),
                    ["pixQrCode"] = new OpenApiString(string.Empty),
                    ["pixCopyPaste"] = new OpenApiString(string.Empty),
                    ["asaasPaymentId"] = new OpenApiString("pay_yiygnzqwo7syi2o3")
                };

                resContent.Example = resExample;
            }
        }
    }
}

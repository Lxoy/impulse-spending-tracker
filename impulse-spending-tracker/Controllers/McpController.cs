using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using impulse_spending_tracker.Models;
using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly UserProfileRepository _userProfiles;
        private readonly MerchantRepository _merchants;
        private readonly BudgetPlanRepository _budgetPlans;
        private readonly PurchaseRepository _purchases;
        private readonly SpendingSessionRepository _sessions;
        private readonly TriggerTypeRepository _triggerTypes;
        private readonly WishlistItemRepository _wishlistItems;

        public McpController(
            UserProfileRepository userProfiles,
            MerchantRepository merchants,
            BudgetPlanRepository budgetPlans,
            PurchaseRepository purchases,
            SpendingSessionRepository sessions,
            TriggerTypeRepository triggerTypes,
            WishlistItemRepository wishlistItems)
        {
            _userProfiles = userProfiles;
            _merchants = merchants;
            _budgetPlans = budgetPlans;
            _purchases = purchases;
            _sessions = sessions;
            _triggerTypes = triggerTypes;
            _wishlistItems = wishlistItems;
        }

        [HttpPost]
        public IActionResult Handle([FromBody] McpRequest request)
        {
            try
            {
                return request.Method switch
                {
                    "tools/list" => Ok(McpResult(request.Id, new { tools = GetVisibleTools() })),
                    "tools/call" => Ok(McpResult(request.Id, CallTool(request.Params))),
                    _ => BadRequest(McpError(request.Id, -32601, "Unknown MCP method."))
                };
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(McpError(request.Id, -32602, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(McpError(request.Id, -32004, ex.Message));
            }
        }

        private IEnumerable<McpTool> GetVisibleTools()
        {
            return Tools.Where(CanUseTool);
        }

        private object CallTool(JsonElement? parameters)
        {
            if (!parameters.HasValue)
            {
                throw new ArgumentException("Missing tool call parameters.");
            }

            var name = GetString(parameters.Value, "name", required: true);
            var tool = Tools.SingleOrDefault(item => item.Name == name);
            if (tool is null)
            {
                throw new ArgumentException($"Unknown MCP tool '{name}'.");
            }

            if (!CanUseTool(tool))
            {
                throw new UnauthorizedAccessException($"Current user cannot use MCP tool '{name}'.");
            }

            var arguments = parameters.Value.TryGetProperty("arguments", out var args) ? args : default;

            return name switch
            {
                "entities.list" => ListEntities(arguments),
                "userProfiles.calculateRiskScore" => new { riskScore = _userProfiles.CalculateRiskScore(GetInt(arguments, "userProfileId", required: true)) },
                "merchants.create" => CreateMerchant(arguments),
                "merchants.update" => UpdateMerchant(arguments),
                "merchants.delete" => DeleteMerchant(arguments),
                "triggerTypes.create" => CreateTriggerType(arguments),
                "triggerTypes.update" => UpdateTriggerType(arguments),
                "triggerTypes.delete" => DeleteTriggerType(arguments),
                _ => throw new ArgumentException($"MCP tool '{name}' has no executor.")
            };
        }

        private object ListEntities(JsonElement arguments)
        {
            var entity = GetString(arguments, "entity", required: true);
            var query = GetString(arguments, "query", required: false);

            return entity switch
            {
                "userProfiles" => _userProfiles.GetAll()
                    .Where(item => Matches(query, item.FirstName, item.LastName, item.Email))
                    .Select(item => new { item.Id, item.FirstName, item.LastName, item.Email, item.MonthlyNetIncome, item.RiskToleranceScore }),
                "merchants" => _merchants.GetAll()
                    .Where(item => Matches(query, item.Name, item.Category, item.CountryCode))
                    .Select(item => new { item.Id, item.Name, item.Category, item.CountryCode, item.IsOnlineOnly, item.AverageDeliveryDays }),
                "budgetPlans" => _budgetPlans.GetAll()
                    .Where(item => Matches(query, item.Name, item.UserProfile?.FirstName, item.UserProfile?.LastName, item.UserProfile?.Email))
                    .Select(item => new { item.Id, item.UserProfileId, item.Name, item.ValidFrom, item.ValidTo, item.MonthlyLimit, item.ImpulseCapPercentage, item.IsActive }),
                "purchases" => _purchases.GetAll()
                    .Where(item => Matches(query, item.Title, item.Currency, item.MoodBeforePurchase, item.Notes, item.Merchant?.Name, item.UserProfile?.Email))
                    .Select(item => new { item.Id, item.UserProfileId, item.MerchantId, item.Title, item.Amount, item.Currency, item.PurchasedAt, item.NeedLevel, item.TriggerType }),
                "spendingSessions" => _sessions.GetAll()
                    .Where(item => Matches(query, item.Platform, item.Channel, item.UserProfile?.Email))
                    .Select(item => new { item.Id, item.UserProfileId, item.StartedAt, item.EndedAt, item.Platform, item.Channel, item.SessionBudget, item.SpentAmount, item.CheckoutCompleted }),
                "triggerTypes" => _triggerTypes.GetAll()
                    .Where(item => Matches(query, item.Name, item.Description))
                    .Select(item => new { item.Id, item.Name, item.ColorHex, item.Description }),
                "wishlistItems" => _wishlistItems.GetAll()
                    .Where(item => Matches(query, item.Name, item.Reason, item.LinkUrl, item.UserProfile?.Email))
                    .Select(item => new { item.Id, item.UserProfileId, item.Name, item.DesiredPrice, item.CurrentPrice, item.Priority, item.IsPurchased, item.LinkUrl }),
                _ => throw new ArgumentException($"Unknown entity '{entity}'.")
            };
        }

        private object CreateMerchant(JsonElement arguments)
        {
            var merchant = ReadMerchant(arguments);
            Validate(merchant);
            _merchants.Create(merchant);
            return new { merchant.Id };
        }

        private object UpdateMerchant(JsonElement arguments)
        {
            var id = GetInt(arguments, "id", required: true);
            if (_merchants.GetById(id) is null)
            {
                throw new InvalidOperationException($"Merchant {id} was not found.");
            }

            var merchant = ReadMerchant(arguments);
            merchant.Id = id;
            Validate(merchant);
            _merchants.Update(merchant);
            return new { merchant.Id };
        }

        private object DeleteMerchant(JsonElement arguments)
        {
            var id = GetInt(arguments, "id", required: true);
            var merchant = _merchants.GetById(id) ?? throw new InvalidOperationException($"Merchant {id} was not found.");
            _merchants.Delete(merchant);
            return new { deleted = true, id };
        }

        private object CreateTriggerType(JsonElement arguments)
        {
            var triggerType = ReadTriggerType(arguments);
            Validate(triggerType);
            _triggerTypes.Create(triggerType);
            return new { triggerType.Id };
        }

        private object UpdateTriggerType(JsonElement arguments)
        {
            var id = GetInt(arguments, "id", required: true);
            if (_triggerTypes.GetById(id) is null)
            {
                throw new InvalidOperationException($"Trigger type {id} was not found.");
            }

            var triggerType = ReadTriggerType(arguments);
            triggerType.Id = id;
            Validate(triggerType);
            _triggerTypes.Update(triggerType);
            return new { triggerType.Id };
        }

        private object DeleteTriggerType(JsonElement arguments)
        {
            var id = GetInt(arguments, "id", required: true);
            var triggerType = _triggerTypes.GetById(id) ?? throw new InvalidOperationException($"Trigger type {id} was not found.");
            _triggerTypes.Delete(triggerType);
            return new { deleted = true, id };
        }

        private Merchant ReadMerchant(JsonElement arguments)
        {
            return new Merchant
            {
                Name = GetRequiredString(arguments, "name"),
                Category = GetRequiredString(arguments, "category"),
                CountryCode = GetRequiredString(arguments, "countryCode"),
                IsOnlineOnly = GetBool(arguments, "isOnlineOnly"),
                AverageDeliveryDays = GetNullableInt(arguments, "averageDeliveryDays")
            };
        }

        private TriggerType ReadTriggerType(JsonElement arguments)
        {
            return new TriggerType
            {
                Name = GetRequiredString(arguments, "name"),
                ColorHex = GetRequiredString(arguments, "colorHex"),
                Description = GetString(arguments, "description", required: false) ?? string.Empty
            };
        }

        private bool CanUseTool(McpTool tool)
        {
            if (tool.AllowedRoles.Length == 0)
            {
                return true;
            }

            return tool.AllowedRoles.Any(User.IsInRole);
        }

        private static bool Matches(string? query, params string?[] values)
        {
            return string.IsNullOrWhiteSpace(query)
                   || values.Any(value => value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true);
        }

        private static void Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
            {
                return;
            }

            throw new ArgumentException(string.Join(" ", results.Select(item => item.ErrorMessage)));
        }

        private static string? GetString(JsonElement element, string name, bool required)
        {
            if (element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null)
            {
                return value.GetString();
            }

            if (required)
            {
                throw new ArgumentException($"Missing required argument '{name}'.");
            }

            return null;
        }

        private static string GetRequiredString(JsonElement element, string name)
        {
            return GetString(element, name, required: true) ?? string.Empty;
        }

        private static int GetInt(JsonElement element, string name, bool required)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.TryGetInt32(out var result))
            {
                return result;
            }

            if (required)
            {
                throw new ArgumentException($"Missing required integer argument '{name}'.");
            }

            return 0;
        }

        private static int? GetNullableInt(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                   && element.TryGetProperty(name, out var value)
                   && value.ValueKind != JsonValueKind.Null
                   && value.TryGetInt32(out var result)
                ? result
                : null;
        }

        private static bool GetBool(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                   && element.TryGetProperty(name, out var value)
                   && value.ValueKind == JsonValueKind.True;
        }

        private static object McpResult(object? id, object result)
        {
            return new { jsonrpc = "2.0", id, result };
        }

        private static object McpError(object? id, int code, string message)
        {
            return new { jsonrpc = "2.0", id, error = new { code, message } };
        }

        private static object ObjectSchema(params (string Name, string Type, bool Required)[] properties)
        {
            return new
            {
                type = "object",
                properties = properties.ToDictionary(
                    item => item.Name,
                    item => (object)new { type = item.Type }),
                required = properties.Where(item => item.Required).Select(item => item.Name).ToArray()
            };
        }

        private static readonly McpTool[] Tools =
        {
            new("entities.list", "List or search existing app entities.", ObjectSchema(("entity", "string", true), ("query", "string", false))),
            new("userProfiles.calculateRiskScore", "Calculate the existing behavioral risk score for a user profile.", ObjectSchema(("userProfileId", "integer", true))),
            new("merchants.create", "Create a merchant.", ObjectSchema(("name", "string", true), ("category", "string", true), ("countryCode", "string", true), ("isOnlineOnly", "boolean", false), ("averageDeliveryDays", "integer", false)), "Admin", "Manager"),
            new("merchants.update", "Update a merchant.", ObjectSchema(("id", "integer", true), ("name", "string", true), ("category", "string", true), ("countryCode", "string", true), ("isOnlineOnly", "boolean", false), ("averageDeliveryDays", "integer", false)), "Admin", "Manager"),
            new("merchants.delete", "Delete a merchant.", ObjectSchema(("id", "integer", true)), "Admin"),
            new("triggerTypes.create", "Create a trigger type.", ObjectSchema(("name", "string", true), ("colorHex", "string", true), ("description", "string", false)), "Admin"),
            new("triggerTypes.update", "Update a trigger type.", ObjectSchema(("id", "integer", true), ("name", "string", true), ("colorHex", "string", true), ("description", "string", false)), "Admin"),
            new("triggerTypes.delete", "Delete a trigger type.", ObjectSchema(("id", "integer", true)), "Admin")
        };

        public sealed record McpRequest(string Method, JsonElement? Params, object? Id = null);

        public sealed record McpTool(string Name, string Description, object InputSchema, params string[] AllowedRoles)
        {
            public string[] AllowedRoles { get; init; } = AllowedRoles;
        }
    }
}

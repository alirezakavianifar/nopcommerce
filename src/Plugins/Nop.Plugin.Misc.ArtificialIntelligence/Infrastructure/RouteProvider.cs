using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Admin configuration routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.Admin.Configure",
            pattern: "Admin/ArtificialIntelligence/Configure",
            defaults: new { controller = "AiAdmin", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.Admin.DuplicateQueue",
            pattern: "Admin/AiDuplicateProduct/List",
            defaults: new { controller = "AiAdmin", action = "DuplicateQueueList", area = "Admin" });

        // Storefront AI routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.VisualSearch",
            pattern: "AiSearch/VisualSearch",
            defaults: new { controller = "AiStorefront", action = "VisualSearch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.VoiceSearch",
            pattern: "AiSearch/VoiceSearch",
            defaults: new { controller = "AiStorefront", action = "VoiceSearch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.TextSearch",
            pattern: "AiSearch/TextSearch",
            defaults: new { controller = "AiStorefront", action = "TextSearch" });

        // REST API routes for mobile, warehouse, courier external apps
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.Api.VisualSearch",
            pattern: "api/ai/visual-search",
            defaults: new { controller = "AiStorefront", action = "VisualSearch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.Api.VoiceSearch",
            pattern: "api/ai/voice-search",
            defaults: new { controller = "AiStorefront", action = "VoiceSearch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.Api.TextSearch",
            pattern: "api/ai/text-search",
            defaults: new { controller = "AiStorefront", action = "TextSearch" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.ChatbotSendMessage",
            pattern: "AiChat/SendMessage",
            defaults: new { controller = "AiStorefront", action = "ChatbotSendMessage" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.ArtificialIntelligence.ChatbotHandoff",
            pattern: "AiChat/Handoff",
            defaults: new { controller = "AiStorefront", action = "ChatbotHandoff" });
    }

    public int Priority => 0;
}

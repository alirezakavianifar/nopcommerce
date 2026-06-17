using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.ConditionalMethods.Models;

/// <summary>
/// View model for the multi-city warehouse warning widget
/// </summary>
public record MultiCityWarningModel : BaseNopModel
{
    /// <summary>Whether products in the current cart span warehouses from different cities</summary>
    public bool HasMultipleCities { get; set; }

    /// <summary>The distinct origin cities detected in the current cart</summary>
    public IList<string> Cities { get; set; } = new List<string>();
}

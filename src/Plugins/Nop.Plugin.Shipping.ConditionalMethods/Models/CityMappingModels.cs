using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.ConditionalMethods.Models;

/// <summary>
/// Search model for city mapping grid
/// </summary>
public record CityMappingSearchModel : BaseSearchModel
{
    public int ShippingTypeId { get; set; }
    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();

    public string CityName { get; set; }
}

/// <summary>
/// List model for city mapping grid
/// </summary>
public record CityMappingListModel : BasePagedListModel<CityMappingModel>
{
}

/// <summary>
/// Row model for city mapping grid
/// </summary>
public record CityMappingModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ShippingType")]
    public int ShippingTypeId { get; set; }

    public string ShippingTypeName { get; set; }

    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.CityName")]
    public string CityName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.StateProvinceId")]
    public int StateProvinceId { get; set; }

    public string StateProvinceName { get; set; }

    public IList<SelectListItem> AvailableStateProvinces { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.IsActive")]
    public bool IsActive { get; set; }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.ConditionalMethods.Models;

/// <summary>
/// Search model for warehouse mapping grid
/// </summary>
public record WarehouseMappingSearchModel : BaseSearchModel
{
    public int ShippingTypeId { get; set; }
    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();
}

/// <summary>
/// List model for warehouse mapping grid
/// </summary>
public record WarehouseMappingListModel : BasePagedListModel<WarehouseMappingModel>
{
}

/// <summary>
/// Row model for warehouse mapping grid
/// </summary>
public record WarehouseMappingModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ShippingType")]
    public int ShippingTypeId { get; set; }

    public string ShippingTypeName { get; set; }

    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.WarehouseId")]
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; }

    public IList<SelectListItem> AvailableWarehouses { get; set; } = new List<SelectListItem>();
}

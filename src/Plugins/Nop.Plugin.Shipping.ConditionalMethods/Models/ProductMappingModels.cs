using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.ConditionalMethods.Models;

/// <summary>
/// Search model for product mapping grid
/// </summary>
public record ProductMappingSearchModel : BaseSearchModel
{
    public int ShippingTypeId { get; set; }
    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();
}

/// <summary>
/// List model for product mapping grid
/// </summary>
public record ProductMappingListModel : BasePagedListModel<ProductMappingModel>
{
}

/// <summary>
/// Row model for product mapping grid
/// </summary>
public record ProductMappingModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ShippingType")]
    public int ShippingTypeId { get; set; }

    public string ShippingTypeName { get; set; }

    public IList<SelectListItem> AvailableShippingTypes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ProductId")]
    public int ProductId { get; set; }

    public string ProductName { get; set; }
}

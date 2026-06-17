using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;
using Nop.Plugin.Shipping.ConditionalMethods.Models;
using Nop.Plugin.Shipping.ConditionalMethods.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Security;

namespace Nop.Plugin.Shipping.ConditionalMethods.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class ConditionalShippingAdminController : BasePluginController
{
    #region Fields

    private readonly IConditionalShippingService _conditionalShippingService;
    private readonly ISettingService _settingService;
    private readonly ConditionalShippingSettings _settings;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    #endregion

    #region Ctor

    public ConditionalShippingAdminController(
        IConditionalShippingService conditionalShippingService,
        ISettingService settingService,
        ConditionalShippingSettings settings,
        IStateProvinceService stateProvinceService,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _conditionalShippingService = conditionalShippingService;
        _settingService = settingService;
        _settings = settings;
        _stateProvinceService = stateProvinceService;
        _productService = productService;
        _warehouseService = warehouseService;
    }

    #endregion

    #region Helpers

    private static IList<SelectListItem> BuildShippingTypeList(int? selectedId = null)
    {
        return Enum.GetValues<ConditionalShippingType>()
            .Select(t => new SelectListItem
            {
                Text = t.ToString(),
                Value = ((int)t).ToString(),
                Selected = selectedId.HasValue && selectedId.Value == (int)t
            })
            .ToList();
    }

    private async Task<IList<SelectListItem>> BuildStateProvinceListAsync(int? selectedId = null)
    {
        var provinces = await _stateProvinceService.GetStateProvincesAsync(showHidden: true);
        var list = provinces.Select(p => new SelectListItem
        {
            Text = p.Name,
            Value = p.Id.ToString(),
            Selected = selectedId.HasValue && selectedId.Value == p.Id
        }).ToList();

        list.Insert(0, new SelectListItem { Text = "-- All --", Value = "0", Selected = !selectedId.HasValue || selectedId.Value == 0 });
        return list;
    }

    private async Task<IList<SelectListItem>> BuildWarehouseListAsync(int? selectedId = null)
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync();
        return warehouses.Select(w => new SelectListItem
        {
            Text = w.Name,
            Value = w.Id.ToString(),
            Selected = selectedId.HasValue && selectedId.Value == w.Id
        }).ToList();
    }

    #endregion

    #region Configure

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult Configure()
    {
        var model = new ConfigurationModel
        {
            ExpressEnabled = _settings.ExpressEnabled,
            ExpressPercentageIncrease = _settings.ExpressPercentageIncrease,
            ExpressFixedAddition = _settings.ExpressFixedAddition,
            ExpressMinAddition = _settings.ExpressMinAddition,
            ExpressMaxAddition = _settings.ExpressMaxAddition,
            ExpressCourierApiKey = _settings.ExpressCourierApiKey,
            ExpressCourierApiEndpoint = _settings.ExpressCourierApiEndpoint,
            ExpressPostalBaseRate = _settings.ExpressPostalBaseRate,

            TransportationEnabled = _settings.TransportationEnabled,
            TransportationPercentageDecrease = _settings.TransportationPercentageDecrease,
            TransportationFixedDeduction = _settings.TransportationFixedDeduction,
            TransportationMinDeduction = _settings.TransportationMinDeduction,
            TransportationMaxDeduction = _settings.TransportationMaxDeduction,
            TransportationBaseRate = _settings.TransportationBaseRate,

            FreightEnabled = _settings.FreightEnabled,
            FreightCostModeId = _settings.FreightCostModeId,
            FreightFixedRate = _settings.FreightFixedRate,
            FreightCostPerKg = _settings.FreightCostPerKg,
            FreightMinCostPerKg = _settings.FreightMinCostPerKg,
            FreightMaxCostPerKg = _settings.FreightMaxCostPerKg,
            FreightCostPerKm = _settings.FreightCostPerKm,
            FreightMinCostPerKm = _settings.FreightMinCostPerKm,
            FreightMaxCostPerKm = _settings.FreightMaxCostPerKm,
            FreightApiKey = _settings.FreightApiKey,
            FreightApiEndpoint = _settings.FreightApiEndpoint,

            CourierEnabled = _settings.CourierEnabled,
            CourierApiKey = _settings.CourierApiKey,
            CourierApiEndpoint = _settings.CourierApiEndpoint,

            AvailableFreightCostModes = Enum.GetValues<FreightCostMode>()
                .Select(m => new SelectListItem
                {
                    Text = m.ToString(),
                    Value = ((int)m).ToString(),
                    Selected = _settings.FreightCostModeId == (int)m
                }).ToList()
        };

        return View("~/Plugins/Shipping.ConditionalMethods/Views/Admin/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> SaveConfigure(ConfigurationModel model)
    {
        _settings.ExpressEnabled = model.ExpressEnabled;
        _settings.ExpressPercentageIncrease = model.ExpressPercentageIncrease;
        _settings.ExpressFixedAddition = model.ExpressFixedAddition;
        _settings.ExpressMinAddition = model.ExpressMinAddition;
        _settings.ExpressMaxAddition = model.ExpressMaxAddition;
        _settings.ExpressCourierApiKey = model.ExpressCourierApiKey;
        _settings.ExpressCourierApiEndpoint = model.ExpressCourierApiEndpoint;
        _settings.ExpressPostalBaseRate = model.ExpressPostalBaseRate;

        _settings.TransportationEnabled = model.TransportationEnabled;
        _settings.TransportationPercentageDecrease = model.TransportationPercentageDecrease;
        _settings.TransportationFixedDeduction = model.TransportationFixedDeduction;
        _settings.TransportationMinDeduction = model.TransportationMinDeduction;
        _settings.TransportationMaxDeduction = model.TransportationMaxDeduction;
        _settings.TransportationBaseRate = model.TransportationBaseRate;

        _settings.FreightEnabled = model.FreightEnabled;
        _settings.FreightCostModeId = model.FreightCostModeId;
        _settings.FreightFixedRate = model.FreightFixedRate;
        _settings.FreightCostPerKg = model.FreightCostPerKg;
        _settings.FreightMinCostPerKg = model.FreightMinCostPerKg;
        _settings.FreightMaxCostPerKg = model.FreightMaxCostPerKg;
        _settings.FreightCostPerKm = model.FreightCostPerKm;
        _settings.FreightMinCostPerKm = model.FreightMinCostPerKm;
        _settings.FreightMaxCostPerKm = model.FreightMaxCostPerKm;
        _settings.FreightApiKey = model.FreightApiKey;
        _settings.FreightApiEndpoint = model.FreightApiEndpoint;

        _settings.CourierEnabled = model.CourierEnabled;
        _settings.CourierApiKey = model.CourierApiKey;
        _settings.CourierApiEndpoint = model.CourierApiEndpoint;

        await _settingService.SaveSettingAsync(_settings);

        ViewBag.SavedSuccessfully = true;
        return RedirectToAction(nameof(Configure));
    }

    #endregion

    #region City Mappings

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult CityMappings()
    {
        var searchModel = new CityMappingSearchModel
        {
            AvailableShippingTypes = BuildShippingTypeList()
        };
        searchModel.AvailableShippingTypes.Insert(0, new SelectListItem { Text = "-- All --", Value = "0" });
        searchModel.SetGridPageSize();
        return View("~/Plugins/Shipping.ConditionalMethods/Views/Admin/CityMappings.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> CityMappingListData(CityMappingSearchModel searchModel)
    {
        var shippingType = searchModel.ShippingTypeId > 0
            ? (ConditionalShippingType?)searchModel.ShippingTypeId
            : null;

        var mappings = await _conditionalShippingService.GetAllCityMappingsAsync(
            shippingType,
            searchModel.CityName,
            searchModel.Page - 1,
            searchModel.PageSize);

        var rows = new List<CityMappingModel>();
        foreach (var m in mappings)
        {
            var stateName = string.Empty;
            if (m.StateProvinceId > 0)
            {
                var sp = await _stateProvinceService.GetStateProvinceByIdAsync(m.StateProvinceId);
                stateName = sp?.Name ?? m.StateProvinceId.ToString();
            }

            rows.Add(new CityMappingModel
            {
                Id = m.Id,
                ShippingTypeId = m.ShippingTypeId,
                ShippingTypeName = ((ConditionalShippingType)m.ShippingTypeId).ToString(),
                CityName = m.CityName,
                StateProvinceId = m.StateProvinceId,
                StateProvinceName = stateName,
                IsActive = m.IsActive
            });
        }

        var listModel = new CityMappingListModel().PrepareToGrid(searchModel, mappings, () => rows);
        return Json(listModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> CreateCityMapping(CityMappingModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid data." });

        var mapping = new ShippingCityMapping
        {
            ShippingTypeId = model.ShippingTypeId,
            CityName = model.CityName?.Trim() ?? string.Empty,
            StateProvinceId = model.StateProvinceId,
            IsActive = model.IsActive
        };

        await _conditionalShippingService.InsertCityMappingAsync(mapping);
        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> EditCityMapping(CityMappingModel model)
    {
        var existing = await _conditionalShippingService.GetCityMappingByIdAsync(model.Id);
        if (existing == null)
            return Json(new { success = false, error = "Record not found." });

        existing.ShippingTypeId = model.ShippingTypeId;
        existing.CityName = model.CityName?.Trim() ?? string.Empty;
        existing.StateProvinceId = model.StateProvinceId;
        existing.IsActive = model.IsActive;

        await _conditionalShippingService.UpdateCityMappingAsync(existing);
        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> DeleteCityMapping(int id)
    {
        var mapping = await _conditionalShippingService.GetCityMappingByIdAsync(id);
        if (mapping == null)
            return Json(new { success = false, error = "Record not found." });

        await _conditionalShippingService.DeleteCityMappingAsync(mapping);
        return Json(new { success = true });
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> GetCityMappingCreateForm()
    {
        var model = new CityMappingModel
        {
            AvailableShippingTypes = BuildShippingTypeList(),
            AvailableStateProvinces = await BuildStateProvinceListAsync(),
            IsActive = true
        };
        return PartialView("~/Plugins/Shipping.ConditionalMethods/Views/Admin/_CityMappingForm.cshtml", model);
    }

    #endregion

    #region Product Mappings

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult ProductMappings()
    {
        var searchModel = new ProductMappingSearchModel
        {
            AvailableShippingTypes = BuildShippingTypeList()
        };
        searchModel.AvailableShippingTypes.Insert(0, new SelectListItem { Text = "-- All --", Value = "0" });
        searchModel.SetGridPageSize();
        return View("~/Plugins/Shipping.ConditionalMethods/Views/Admin/ProductMappings.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ProductMappingListData(ProductMappingSearchModel searchModel)
    {
        var shippingType = searchModel.ShippingTypeId > 0
            ? (ConditionalShippingType?)searchModel.ShippingTypeId
            : null;

        var mappings = await _conditionalShippingService.GetAllProductMappingsAsync(
            shippingType,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var rows = new List<ProductMappingModel>();
        foreach (var m in mappings)
        {
            var product = await _productService.GetProductByIdAsync(m.ProductId);
            rows.Add(new ProductMappingModel
            {
                Id = m.Id,
                ShippingTypeId = m.ShippingTypeId,
                ShippingTypeName = ((ConditionalShippingType)m.ShippingTypeId).ToString(),
                ProductId = m.ProductId,
                ProductName = product?.Name ?? m.ProductId.ToString()
            });
        }

        var listModel = new ProductMappingListModel().PrepareToGrid(searchModel, mappings, () => rows);
        return Json(listModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> CreateProductMapping(ProductMappingModel model)
    {
        if (!ModelState.IsValid || model.ProductId <= 0)
            return Json(new { success = false, error = "Invalid data. Product ID is required." });

        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return Json(new { success = false, error = "Product not found." });

        var mapping = new ShippingProductMapping
        {
            ShippingTypeId = model.ShippingTypeId,
            ProductId = model.ProductId
        };

        await _conditionalShippingService.InsertProductMappingAsync(mapping);
        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> DeleteProductMapping(int id)
    {
        var mapping = await _conditionalShippingService.GetProductMappingByIdAsync(id);
        if (mapping == null)
            return Json(new { success = false, error = "Record not found." });

        await _conditionalShippingService.DeleteProductMappingAsync(mapping);
        return Json(new { success = true });
    }

    #endregion

    #region Warehouse Mappings

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult WarehouseMappings()
    {
        var searchModel = new WarehouseMappingSearchModel
        {
            AvailableShippingTypes = BuildShippingTypeList()
        };
        searchModel.AvailableShippingTypes.Insert(0, new SelectListItem { Text = "-- All --", Value = "0" });
        searchModel.SetGridPageSize();
        return View("~/Plugins/Shipping.ConditionalMethods/Views/Admin/WarehouseMappings.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> WarehouseMappingListData(WarehouseMappingSearchModel searchModel)
    {
        var shippingType = searchModel.ShippingTypeId > 0
            ? (ConditionalShippingType?)searchModel.ShippingTypeId
            : null;

        var mappings = await _conditionalShippingService.GetAllWarehouseMappingsAsync(
            shippingType,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var warehouses = await _warehouseService.GetAllWarehousesAsync();
        var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);

        var rows = mappings.Select(m => new WarehouseMappingModel
        {
            Id = m.Id,
            ShippingTypeId = m.ShippingTypeId,
            ShippingTypeName = ((ConditionalShippingType)m.ShippingTypeId).ToString(),
            WarehouseId = m.WarehouseId,
            WarehouseName = warehouseDict.TryGetValue(m.WarehouseId, out var name) ? name : m.WarehouseId.ToString()
        });

        var listModel = new WarehouseMappingListModel().PrepareToGrid(searchModel, mappings, () => rows);
        return Json(listModel);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> CreateWarehouseMapping(WarehouseMappingModel model)
    {
        if (!ModelState.IsValid || model.WarehouseId <= 0)
            return Json(new { success = false, error = "Invalid data. Warehouse ID is required." });

        var mapping = new ShippingWarehouseMapping
        {
            ShippingTypeId = model.ShippingTypeId,
            WarehouseId = model.WarehouseId
        };

        await _conditionalShippingService.InsertWarehouseMappingAsync(mapping);
        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> DeleteWarehouseMapping(int id)
    {
        var mapping = await _conditionalShippingService.GetWarehouseMappingByIdAsync(id);
        if (mapping == null)
            return Json(new { success = false, error = "Record not found." });

        await _conditionalShippingService.DeleteWarehouseMappingAsync(mapping);
        return Json(new { success = true });
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> GetWarehouseMappingCreateForm()
    {
        var model = new WarehouseMappingModel
        {
            AvailableShippingTypes = BuildShippingTypeList(),
            AvailableWarehouses = await BuildWarehouseListAsync()
        };
        return PartialView("~/Plugins/Shipping.ConditionalMethods/Views/Admin/_WarehouseMappingForm.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult GetProductMappingCreateForm()
    {
        var model = new ProductMappingModel
        {
            AvailableShippingTypes = BuildShippingTypeList()
        };
        return PartialView("~/Plugins/Shipping.ConditionalMethods/Views/Admin/_ProductMappingForm.cshtml", model);
    }

    #endregion
}

using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class CustomerWalletAdminController : BasePluginController
{
    private readonly IRepository<CustomerWallet> _customerWalletRepository;
    private readonly IRepository<WalletTransaction> _walletTransactionRepository;
    private readonly ICustomerService _customerService;

    public CustomerWalletAdminController(
        IRepository<CustomerWallet> customerWalletRepository,
        IRepository<WalletTransaction> walletTransactionRepository,
        ICustomerService customerService)
    {
        _customerWalletRepository = customerWalletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _customerService = customerService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult List()
    {
        var model = new CustomerWalletSearchModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerWalletAdmin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ListData(CustomerWalletSearchModel searchModel)
    {
        var wallets = await _customerWalletRepository.GetAllPagedAsync(query =>
            query.OrderByDescending(cw => cw.UpdatedOnUtc),
            searchModel.Page - 1, searchModel.PageSize);

        var model = await new CustomerWalletAdminListModel().PrepareToGridAsync(searchModel, wallets, () =>
        {
            return wallets.SelectAwait(async wallet =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(wallet.CustomerId);
                return new CustomerWalletAdminModel
                {
                    Id = wallet.Id,
                    CustomerId = wallet.CustomerId,
                    CustomerEmail = customer?.Email ?? "Unknown",
                    WalletTypeName = wallet.WalletType.ToString(),
                    Balance = wallet.Balance,
                    UpdatedOnUtc = wallet.UpdatedOnUtc
                };
            });
        });

        return Json(model);
    }
}

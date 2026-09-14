using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.SellerMarketing.Models;

public record SellerBackupItemModel : BaseNopEntityModel
{
    public string FileName { get; set; }
    public string IncludedEntities { get; set; }
    public string FileSizeText { get; set; }
    public DateTime CreatedOn { get; set; }
}

public record SellerRestoreRequestModel : BaseNopEntityModel
{
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public bool IsExternalUpload { get; set; }
    public string FileHashSha256 { get; set; }
    public string StatusName { get; set; }
    public string AdminComment { get; set; }
    public DateTime RequestedOn { get; set; }
    public DateTime? ReviewedOn { get; set; }
}

public record SellerBackupSettingsModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowProducts")]
    public bool AllowBackupProducts { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowProductAttributes")]
    public bool AllowBackupProductAttributes { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowPictures")]
    public bool AllowBackupPictures { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowOrders")]
    public bool AllowBackupOrders { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowFinancialData")]
    public bool AllowBackupFinancialData { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.AllowExternalFileRestore")]
    public bool AllowExternalFileRestore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.RequireAdminApproval")]
    public bool RequireAdminApprovalForRestore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Backup.HmacSecretKey")]
    public string HmacSignatureSecretKey { get; set; }
}

public record SellerRestoreRequestSearchModel : BaseSearchModel
{
    public int? VendorId { get; set; }
    public int? StatusId { get; set; }
}

public record SellerRestoreRequestListModel : BasePagedListModel<SellerRestoreRequestModel>
{
}

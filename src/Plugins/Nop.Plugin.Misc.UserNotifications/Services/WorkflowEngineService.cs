using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.UserNotifications.Services;

public class WorkflowEngineService : IWorkflowEngineService
{
    private readonly IRepository<NotificationWorkflow> _workflowRepository;
    private readonly IRepository<NotificationWorkflowStep> _stepRepository;
    private readonly IRepository<NotificationQueueItem> _queueRepository;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IProductService _productService;
    private readonly IPictureService _pictureService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IOrderService _orderService;
    private readonly IDiscountService _discountService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly ISmsNotificationService _smsNotificationService;
    private readonly IUserInboxService _userInboxService;
    private readonly IPopupNotificationService _popupNotificationService;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly IStoreContext _storeContext;
    private readonly IWebHelper _webHelper;
    private readonly ILogger<WorkflowEngineService> _logger;

    public WorkflowEngineService(
        IRepository<NotificationWorkflow> workflowRepository,
        IRepository<NotificationWorkflowStep> stepRepository,
        IRepository<NotificationQueueItem> queueRepository,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        IProductService productService,
        IPictureService pictureService,
        IUrlRecordService urlRecordService,
        IOrderService orderService,
        IDiscountService discountService,
        IQueuedEmailService queuedEmailService,
        IEmailAccountService emailAccountService,
        ISmsNotificationService smsNotificationService,
        IUserInboxService userInboxService,
        IPopupNotificationService popupNotificationService,
        INotificationPreferenceService preferenceService,
        IStoreContext storeContext,
        IWebHelper webHelper,
        ILogger<WorkflowEngineService> logger)
    {
        _workflowRepository = workflowRepository;
        _stepRepository = stepRepository;
        _queueRepository = queueRepository;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _productService = productService;
        _pictureService = pictureService;
        _urlRecordService = urlRecordService;
        _orderService = orderService;
        _discountService = discountService;
        _queuedEmailService = queuedEmailService;
        _emailAccountService = emailAccountService;
        _smsNotificationService = smsNotificationService;
        _userInboxService = userInboxService;
        _popupNotificationService = popupNotificationService;
        _preferenceService = preferenceService;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _logger = logger;
    }

    public async Task TriggerWorkflowAsync(NotificationTriggerType triggerType, int customerId, int? productId = null, int? orderId = null)
    {
        var workflows = await _workflowRepository.GetAllAsync(query =>
        {
            return query.Where(w => w.IsActive && w.TriggerTypeId == (int)triggerType);
        });

        foreach (var workflow in workflows)
        {
            var steps = await _stepRepository.GetAllAsync(query =>
            {
                return query.Where(s => s.WorkflowId == workflow.Id && s.IsActive).OrderBy(s => s.StepOrder);
            });

            foreach (var step in steps)
            {
                var channels = new List<string>();
                if (step.SendEmail) channels.Add("Email");
                if (step.SendSms) channels.Add("Sms");
                if (step.SendPopUp) channels.Add("PopUp");
                if (step.SendInbox) channels.Add("Inbox");

                var scheduledTime = DateTime.UtcNow.AddMinutes(step.DelayMinutes);

                var queueItem = new NotificationQueueItem
                {
                    WorkflowStepId = step.Id,
                    CustomerId = customerId,
                    ProductId = productId,
                    OrderId = orderId,
                    ScheduledSendTimeUtc = scheduledTime,
                    Status = NotificationQueueStatus.Pending,
                    DeliveryChannels = string.Join(",", channels),
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _queueRepository.InsertAsync(queueItem);
            }
        }
    }

    public async Task ProcessPendingQueueItemsAsync()
    {
        var now = DateTime.UtcNow;
        var dueItems = await _queueRepository.GetAllAsync(query =>
        {
            return query.Where(q => q.StatusId == (int)NotificationQueueStatus.Pending && q.ScheduledSendTimeUtc <= now);
        });

        foreach (var item in dueItems)
        {
            try
            {
                item.Status = NotificationQueueStatus.Processing;
                await _queueRepository.UpdateAsync(item);

                var step = await _stepRepository.GetByIdAsync(item.WorkflowStepId);
                if (step == null || !step.IsActive)
                {
                    item.Status = NotificationQueueStatus.Failed;
                    item.ErrorLog = "Workflow step inactive or deleted.";
                    await _queueRepository.UpdateAsync(item);
                    continue;
                }

                var customer = await _customerService.GetCustomerByIdAsync(item.CustomerId);
                if (customer == null)
                {
                    item.Status = NotificationQueueStatus.Failed;
                    item.ErrorLog = "Customer not found.";
                    await _queueRepository.UpdateAsync(item);
                    continue;
                }

                var product = item.ProductId.HasValue ? await _productService.GetProductByIdAsync(item.ProductId.Value) : null;
                var order = item.OrderId.HasValue ? await _orderService.GetOrderByIdAsync(item.OrderId.Value) : null;

                // Category & Icon detection
                string category = "System";
                string icon = "fa-bell";
                if (order != null)
                {
                    category = "Order";
                    icon = "fa-box";
                }
                else if (step.GenerateDiscountCode || product != null)
                {
                    category = "Promotion";
                    icon = "fa-gift";
                }

                // Dynamic Discount Code generation if configured
                string discountCode = null;
                DateTime? discountExpiry = null;
                if (step.GenerateDiscountCode && step.DiscountPercentage > 0)
                {
                    discountCode = $"NOTIF-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                    discountExpiry = DateTime.UtcNow.AddDays(7);
                    var discount = new Discount
                    {
                        Name = $"Dynamic Notification Discount {discountCode}",
                        DiscountType = DiscountType.AssignedToOrderTotal,
                        UsePercentage = true,
                        DiscountPercentage = step.DiscountPercentage,
                        RequiresCouponCode = true,
                        CouponCode = discountCode,
                        IsActive = true,
                        StartDateUtc = DateTime.UtcNow,
                        EndDateUtc = discountExpiry,
                        LimitationTimes = 1
                    };
                    await _discountService.InsertDiscountAsync(discount);
                    item.GeneratedDiscountCode = discountCode;
                }

                var storeLocation = _webHelper.GetStoreLocation();
                var productSeName = product != null ? await _urlRecordService.GetSeNameAsync(product) : string.Empty;

                // Product image thumbnail
                string imageUrl = null;
                if (product != null)
                {
                    var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    if (picture != null)
                    {
                        var (url, _) = await _pictureService.GetPictureUrlAsync(picture, 150, true);
                        imageUrl = url;
                    }
                }

                // Render dynamic text templates
                var title = ReplaceTokens(step.SubjectTemplate, customer, product, productSeName, order, discountCode, storeLocation);
                var body = ReplaceTokens(step.BodyTemplate, customer, product, productSeName, order, discountCode, storeLocation);

                item.RenderedTitle = title;
                item.RenderedBody = body;

                var actionUrl = product != null ? $"{storeLocation}{productSeName}" : (order != null ? $"{storeLocation}orderdetails/{order.Id}" : storeLocation);

                // 1. Account Inbox
                if (step.SendInbox)
                {
                    await _userInboxService.AddInboxMessageAsync(
                        customer.Id,
                        title,
                        body,
                        actionUrl,
                        category,
                        icon,
                        imageUrl,
                        discountCode,
                        discountExpiry);
                }

                // 2. Storefront Popup Modal / Toast (Respecting user preferences)
                if (step.SendPopUp && await _preferenceService.IsNotificationAllowedAsync(customer.Id, "Toast", category))
                {
                    var popupType = !string.IsNullOrWhiteSpace(discountCode) ? "Celebration" : "Toast";
                    await _popupNotificationService.AddPopupAsync(
                        customer.Id,
                        title,
                        body,
                        actionUrl,
                        popupType,
                        category,
                        icon,
                        imageUrl,
                        discountCode,
                        discountExpiry);
                }

                // 3. FarazSMS Integration (Respecting user preferences)
                if (step.SendSms && await _preferenceService.IsNotificationAllowedAsync(customer.Id, "Sms", category))
                {
                    var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        var patternValues = new Dictionary<string, string>
                        {
                            { "title", title },
                            { "code", discountCode ?? "" },
                            { "customer", customer.FirstName ?? "Customer" }
                        };
                        await _smsNotificationService.SendSmsAsync(phone, body, step.SmsPatternCode, patternValues);
                    }
                }

                // 4. Transactional Email Queue (Respecting user preferences)
                if (step.SendEmail && !string.IsNullOrWhiteSpace(customer.Email) && await _preferenceService.IsNotificationAllowedAsync(customer.Id, "Email", category))
                {
                    var emailAccounts = await _emailAccountService.GetAllEmailAccountsAsync();
                    var emailAccount = emailAccounts.FirstOrDefault();
                    if (emailAccount != null)
                    {
                        var queuedEmail = new QueuedEmail
                        {
                            Priority = QueuedEmailPriority.High,
                            From = emailAccount.Email,
                            FromName = emailAccount.DisplayName,
                            To = customer.Email,
                            ToName = customer.FirstName ?? customer.Email,
                            Subject = title,
                            Body = body,
                            CreatedOnUtc = DateTime.UtcNow,
                            EmailAccountId = emailAccount.Id
                        };
                        await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);
                    }
                }

                item.Status = NotificationQueueStatus.Sent;
                item.SentOnUtc = DateTime.UtcNow;
                await _queueRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification queue item #{Id}", item.Id);
                item.Status = NotificationQueueStatus.Failed;
                item.ErrorLog = ex.ToString();
                await _queueRepository.UpdateAsync(item);
            }
        }
    }

    private static string ReplaceTokens(string template, Customer customer, Product product, string productSeName, Order order, string discountCode, string storeLocation)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        var result = template;
        result = result.Replace("%Customer.FullName%", $"{customer.FirstName} {customer.LastName}".Trim());
        result = result.Replace("%Customer.Email%", customer.Email ?? "");

        if (product != null)
        {
            result = result.Replace("%Product.Name%", product.Name ?? "");
            result = result.Replace("%Product.Price%", product.Price.ToString("F2"));
            result = result.Replace("%Product.Url%", $"{storeLocation}{productSeName}");
        }

        if (order != null)
        {
            result = result.Replace("%Order.Id%", order.Id.ToString());
            result = result.Replace("%Order.Total%", order.OrderTotal.ToString("F2"));
        }

        if (!string.IsNullOrWhiteSpace(discountCode))
        {
            result = result.Replace("%Discount.Code%", discountCode);
        }

        return result;
    }

    public async Task<IList<NotificationWorkflow>> GetAllWorkflowsAsync()
    {
        return await _workflowRepository.GetAllAsync(query =>
        {
            return query.OrderByDescending(w => w.CreatedOnUtc);
        });
    }

    public async Task<NotificationWorkflow> GetWorkflowByIdAsync(int workflowId)
    {
        return await _workflowRepository.GetByIdAsync(workflowId);
    }

    public async Task InsertWorkflowAsync(NotificationWorkflow workflow)
    {
        workflow.CreatedOnUtc = DateTime.UtcNow;
        await _workflowRepository.InsertAsync(workflow);
    }

    public async Task UpdateWorkflowAsync(NotificationWorkflow workflow)
    {
        await _workflowRepository.UpdateAsync(workflow);
    }

    public async Task DeleteWorkflowAsync(NotificationWorkflow workflow)
    {
        var steps = await GetWorkflowStepsAsync(workflow.Id);
        foreach (var s in steps)
        {
            await _stepRepository.DeleteAsync(s);
        }
        await _workflowRepository.DeleteAsync(workflow);
    }

    public async Task<IList<NotificationWorkflowStep>> GetWorkflowStepsAsync(int workflowId)
    {
        return await _stepRepository.GetAllAsync(query =>
        {
            return query.Where(s => s.WorkflowId == workflowId).OrderBy(s => s.StepOrder);
        });
    }

    public async Task<NotificationWorkflowStep> GetStepByIdAsync(int stepId)
    {
        return await _stepRepository.GetByIdAsync(stepId);
    }

    public async Task InsertStepAsync(NotificationWorkflowStep step)
    {
        await _stepRepository.InsertAsync(step);
    }

    public async Task UpdateStepAsync(NotificationWorkflowStep step)
    {
        await _stepRepository.UpdateAsync(step);
    }

    public async Task DeleteStepAsync(NotificationWorkflowStep step)
    {
        await _stepRepository.DeleteAsync(step);
    }

    public async Task<IList<NotificationQueueItem>> GetQueueItemsAsync(int pageIndex = 0, int pageSize = 50)
    {
        return await _queueRepository.GetAllAsync(query =>
        {
            return query.OrderByDescending(q => q.CreatedOnUtc);
        });
    }
}

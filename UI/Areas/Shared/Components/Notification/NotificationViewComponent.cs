using Application.ICurrentUserService;
using Application.IDataService;
using Domain.Common.Const;
using Domain.Common.Enum;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;


namespace UI.Areas.Shared.Components.Notification;

[Authorize]
public class NotificationViewComponent : ViewComponent
{
    private readonly IEventRequestService _eventRequestService;
    private readonly IMatchEventService _matchEventService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConferenceService _conferenceService;
    private readonly IAttendeeService _attendeeService;
    private readonly IMemoryCache _cache;

    public NotificationViewComponent(
        IEventRequestService eventRequestService,
        IMatchEventService matchEventService,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IConferenceService conferenceService,
        IAttendeeService attendeeService,
        IMemoryCache cache)
    {
        _eventRequestService = eventRequestService;
        _matchEventService = matchEventService;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _conferenceService = conferenceService;
        _attendeeService = attendeeService;
        _cache = cache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        //var cacheKey = $"Notification_{_currentUserService.UserId}";

        // if (!_cache.TryGetValue(cacheKey, out List<NotificationViewComponentVM>? notificationModels))
        // {
        var users = await GetUsersWithRoleAsync(nameof(RoleType.Executive));

        var notificationModels = await LoadNotificationModelsAsync(users);
        notificationModels.AddRange(await LoadConferenceNotificationModelsAsync(users));
        notificationModels.AddRange(await LoadAttendeeEventRequestNotificationModelsAsync(users));
        notificationModels.AddRange(await LoadAttendeeConferenceNotificationModelsAsync(users));
        var notifications = notificationModels.OrderByDescending(notification => notification.CreatedDate).Take(4).ToList();
        return View(notifications);

        //    var loadTasks = new List<Task<List<NotificationViewComponentVM>>>
        //    {
        //        LoadNotificationModelsAsync(users),
        //        LoadConferenceNotificationModelsAsync(users),
        //        LoadAttendeeEventRequestNotificationModelsAsync(users),
        //        LoadAttendeeConferenceNotificationModelsAsync(users)
        //    };

        //    var results = await Task.WhenAll(loadTasks);
        //    notificationModels = results.SelectMany(x => x).OrderByDescending(notification => notification.CreatedDate).Take(4).ToList();

        //    var cacheEntryOptions = new MemoryCacheEntryOptions
        //    {
        //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        //        SlidingExpiration = TimeSpan.FromMinutes(1)
        //    };

        //    _cache.Set(cacheKey, notificationModels, cacheEntryOptions);
        //}

        // return View(notificationModels);
    }

    private async Task<List<NotificationViewComponentVM>> LoadNotificationModelsAsync(IEnumerable<ApplicationUser> users)
    {
        DateTimeOffset oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var isAdmin = _currentUserService.IsAdmin;

        var eventRequests = isAdmin ? await _eventRequestService.GetAllAsync() : await _eventRequestService.GetByUserIdAsync(_currentUserService.UserId);
        var eventMatches = await _matchEventService.GetAllMatchEventAsync();

        //await Task.WhenAll(eventRequestsTask, eventMatchesTask);

        //var eventRequests = eventRequestsTask;
        //var eventMatches =  eventMatchesTask;

        return eventRequests
            .Where(request => FilterEventRequests(request, oneWeekAgo, isAdmin))
            .Join(eventMatches, request => request.EventId, match => match.EventId, (request, match) => new { request, match })
            .Join(users, combined => combined.request.CreatedBy, user => user.Id, (combined, user) => CreateNotificationEventRequestModel(combined.request, combined.match, user, isAdmin))
           // .OrderByDescending(notification => notification.CreatedDate)
            .ToList();
    }

    private bool FilterEventRequests(EventRequest request, DateTimeOffset oneWeekAgo, bool isAdmin)
    {
        return (isAdmin && (request.Status == nameof(StatusType.Pending) || request.LastModifiedOn >= oneWeekAgo)) ||
               (!isAdmin && (request.Status == nameof(StatusType.Rejected) || request.LastModifiedOn >= oneWeekAgo)) ||
               (!isAdmin && request.Status != nameof(StatusType.Approved) && (request.CreatedOn >= oneWeekAgo || request.LastModifiedOn >= oneWeekAgo));
    }

    private bool FilterConferenceRequests(Conference request, DateTimeOffset oneWeekAgo, bool isAdmin)
    {
        return (isAdmin && (request.Status == nameof(StatusType.Pending) || request.LastModifiedOn >= oneWeekAgo)) ||
               (!isAdmin && (request.Status == nameof(StatusType.Rejected) || request.LastModifiedOn >= oneWeekAgo)) ||
               (!isAdmin && request.Status != nameof(StatusType.Approved) && (request.CreatedOn >= oneWeekAgo || request.LastModifiedOn >= oneWeekAgo));
    }

    private NotificationViewComponentVM CreateNotificationEventRequestModel(EventRequest request, MatchEvent match, ApplicationUser user, bool isAdmin)
    {
        return new NotificationViewComponentVM
        {
            EventName = match.EventName,
            CreatedDate = request.LastModifiedOn ?? request.CreatedOn,
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            IsAdmin = isAdmin,
            PendingRequest = isAdmin ? request.Status == nameof(StatusType.Pending) ? 2 : 0 : 0,
            ApproveRequest = !isAdmin ? request.Status == nameof(StatusType.Approved) ? 1 : 0 : 0,
            RejectRequest = !isAdmin ? request.Status == nameof(StatusType.Rejected) ? 3 : 0 : 0,
            RequestId = request.RequestId,
            EventId = request.EventId,
            Status = request.Status,
            EventDate = match.EventDate,
            PackageSelected = request.PackageSelected,
            AddOns = request.AddonsSelected
        };
    }

    private async Task<List<NotificationViewComponentVM>> LoadConferenceNotificationModelsAsync(IEnumerable<ApplicationUser> users)
    {
        DateTimeOffset oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var isAdmin = _currentUserService.IsAdmin;

        var conferenceRequests = isAdmin ? await _conferenceService.GetAllAsync() : await _conferenceService.GetByUserIdAsync(_currentUserService.UserId);

        return conferenceRequests
            .Where(request => FilterConferenceRequests(request, oneWeekAgo, isAdmin))
            .Join(users, request => request.CreatedBy, user => user.Id, (request, user) => CreateNotificationConferenceModel(request, user, isAdmin))
           // .OrderByDescending(notification => notification.CreatedDate)
            .ToList();
    }

    private NotificationViewComponentVM CreateNotificationConferenceModel(Conference request, ApplicationUser user, bool isAdmin)
    {
        return new NotificationViewComponentVM
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            IsAdmin = isAdmin,
            CreatedDate = request.LastModifiedOn ?? request.CreatedOn,
            PendingRequest = isAdmin ? request.Status == nameof(StatusType.Pending) ? 2 : 0 : 0,
            ApproveRequest = !isAdmin ? request.Status == nameof(StatusType.Approved) ? 1 : 0 : 0,
            RejectRequest = !isAdmin ? request.Status == nameof(StatusType.Rejected) ? 3 : 0 : 0,
            ConferenceId = request.ConferenceId,
            ConferenceDate = request.ConferenceDate,
            Status = request.Status,
            SuiteType = request.SuiteType,
            PackageSelected = request.PackageSelected,
            AddOns = request.AddonsSelected
        };
    }

    //private async Task<List<NotificationViewComponentVM>> LoadAttendeeConferenceNotificationModelsAsync(IEnumerable<ApplicationUser> users)
    //{
    //    DateTimeOffset oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
    //    var isAdmin = _currentUserService.IsAdmin;

    //    var attendeeRequests = isAdmin ? await _attendeeService.GetAllAsync() : await _attendeeService.GetByUserIdAsync(_currentUserService.UserId);

    //    return attendeeRequests
    //        .Where(request => isAdmin || request.LastModifiedOn >= oneWeekAgo)
    //        .Join(users, request => request.CreatedBy, user => user.Id, (request, user) => await CreateNotificationAttendeeModel(request, user, isAdmin))
    //       // .OrderByDescending(notification => notification.CreatedDate)
    //        .ToList();
    //}

    private async Task<List<NotificationViewComponentVM>> LoadAttendeeConferenceNotificationModelsAsync(IEnumerable<ApplicationUser> users)
    {
        DateTimeOffset oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var isAdmin = _currentUserService.IsAdmin;

        var attendeeRequests = isAdmin ? await _attendeeService.GetAllAsync() : await _attendeeService.GetByUserIdAsync(_currentUserService.UserId);

        return attendeeRequests
            .Where(request => isAdmin || request.LastModifiedOn >= oneWeekAgo)
            .Join(users, request => request.CreatedBy, user => user.Id, (request, user) => CreateNotificationAttendeeModel(request, user, isAdmin).Result)
            .ToList();
    }


    private async Task<NotificationViewComponentVM> CreateNotificationAttendeeModel(Attendee request, ApplicationUser user, bool isAdmin)
    {
        var requestEvent = new EventRequest();
        var eventdate = new DateTimeOffset();
        var conferencedate = new DateTimeOffset();

        if (request.RequestId != 0)
        {
            requestEvent = await _eventRequestService.GetById(request.RequestId);
            var eventbyId = await _matchEventService.GetMatchEventById(requestEvent.EventId);
            eventdate = (DateTimeOffset)eventbyId.EventDate;
        }
        else
        {
            var conferenceById = await _conferenceService.GetById(request.ConferenceId);
            conferencedate = conferenceById.ConferenceDate;
        }

        return new NotificationViewComponentVM
        {
            ExecutiveName = $"{user.FirstName} {user.LastName}",
            IsAdmin = isAdmin,
            CreatedDate = request.LastModifiedOn ?? request.CreatedOn,
            PendingRequest = 0,
            ApproveRequest = 0,
            RejectRequest = 0,
            ConferenceId = request.ConferenceId ?? 0,
            AttendeeId = request.Id,
            EventDate = eventdate,
            ConferenceDate = conferencedate,
            RequestId = request.RequestId ?? 0,
            Status = requestEvent.Status == nameof(StatusType.Approved) ? requestEvent.Status : null
        };
    }

    private async Task<List<NotificationViewComponentVM>> LoadAttendeeEventRequestNotificationModelsAsync(IEnumerable<ApplicationUser> users)
    {
        DateTimeOffset oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var isAdmin = _currentUserService.IsAdmin;

        var attendeeRequests = isAdmin ? await _attendeeService.GetAllAsync() : await _attendeeService.GetByUserIdAsync(_currentUserService.UserId);

        return attendeeRequests
            .Where(request => isAdmin || request.LastModifiedOn >= oneWeekAgo)
            .Join(users, request => request.CreatedBy, user => user.Id, (request, user) => CreateNotificationAttendeeModel(request, user, isAdmin).Result)
           // .OrderByDescending(notification => notification.CreatedDate)
            .ToList();
    }

    private async Task<IEnumerable<ApplicationUser>> GetUsersWithRoleAsync(string roleName)
    {
        return await _userManager.GetUsersInRoleAsync(roleName);
    }
}





namespace UI.Areas;

public static class PageNames
{
    // Area: Site
    public const string Index = "/Index";

    // Area: Admin
    public const string DashboardAdmin = "/DashboardAdmin";
    public const string ManageRequests = "/ManageRequests";
    public const string ManageExecutives = "/ManageExecutives";
    public const string CreateorUpdateEvent = "/CreateorUpdateEvent";
    public const string MatchEvents = "/Events";

    // Area: Excecutive
    public const string CreateConferenceRequest = "/CreateConferenceRequest";
    public const string Conferences = "/Conferences";
    public const string EventRequest = "/CreateRequest";
    public const string ExecutiveDashboard = "/ExecutiveDashboard";
    public const string ExploreEvents = "/ExploreEvents";
    public const string MyRequests = "/MyRequests";
    public const string AttendeeDetails = "/CreateOrUpdateAttendee";
    public const string ContactUs = "/ContactUs";

    // Area: Account
    public const string ChangePassword = "/Account/Manage/ChangePassword";
    public const string ForgotPassword = "/Account/ForgotPassword";
    public const string Login = "/Account/Login";
    public const string RegisterConfirmation = "/Account/RegisterConfirmation";
}
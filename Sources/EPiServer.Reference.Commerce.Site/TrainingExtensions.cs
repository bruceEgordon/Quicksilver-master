using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site
{
    public class TrainingExtensions
    {
        public static IDictionary<string, string> TrackingDataInterceptorDescriptions = new Dictionary<string, string>
        {
            { "EPiServer.Tracking.Core.DefaultTrackingDataInterceptor", "Sets track event's Scope to the value of the episerver:profiles.Scope appSetting." },
            { "EPiServer.Tracking.Commerce.DefaultScopeTrackingDataInterceptor", "Sets track event's Scope to the current site's ID." },
            { "EPiServer.Tracking.Commerce.UserDataTrackingDataInterceptor", "Sets track event's User.Name, User.Email, User.Info to data from the registered IUserDataService that by default reads from CustomerContext.Current.CurrentContact including the first address found." },
            { "EPiServer.Personalization.Commerce.Tracking.PersonalizationTrackingDataInterceptor", "Sets track event's Cuid and Session from the registered IUserSessionService that by default reads from cookies named epi_RecommendationsTrackingUserId and epi_RecommendationsTrackingSessionId." },
        };
    }
}
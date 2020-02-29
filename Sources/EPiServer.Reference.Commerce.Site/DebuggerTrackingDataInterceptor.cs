using EPiServer.ServiceLocation;
using EPiServer.Tracking.Core;
using System.Diagnostics;

namespace EPiServer.Reference.Commerce.Site
{
    [ServiceConfiguration(ServiceType = typeof(ITrackingDataInterceptor), 
        Lifecycle = ServiceInstanceScope.Singleton)]
    public class DebuggerTrackingDataInterceptor : ITrackingDataInterceptor
    {
        public int SortOrder => int.MaxValue;

        public void Intercept<TPayload>(TrackingData<TPayload> trackingData)
        {
            if (Debugger.IsAttached)
            {
                string eventType = trackingData.EventType;
                UserData user = trackingData.User;
                TPayload payload = trackingData.Payload;

                //Debugger.Break();
            }
        }
    }
}
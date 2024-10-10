using Microsoft.Extensions.Configuration;
using QBSDKEVENTLib;
using QbSync.QbXml.Objects;
using System.Runtime.InteropServices;


namespace Application.Infrastructure.QuickBooksIntegration;

[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public class QBEventCallback : IQBEventCallback
{
    public void inform(string eventXML)
    {
        throw new NotImplementedException();
    }
}

public class QuickBooksEventService
{
    private readonly IConfiguration _config;
    public QuickBooksEventService(IConfiguration config)
    {
        _config = config; 
    }

    private void SampleQbRequest()
    {
        var eventSubscription = new DataEventSubscriptionAddRqType()
        {
            DataEventSubscriptionAdd = new DataEventSubscriptionAdd()
            {
                SubscriberID = Guid.NewGuid(), // Required field
                COMCallbackInfo = new COMCallbackInfo()
                {
                    AppName = _config.GetValue<string>("Qb:AppName"), // Required field
                    CLSID = Guid.NewGuid(), // One of CLSID or ProgID is required
                },
                DeliveryPolicy = DeliveryPolicy.DeliverAlways, // Required field
                TrackLostEvents = TrackLostEvents.All, // Optional, but set as "All" for tracking all lost events
                DeliverOwnEvents = true, // Optional, but set to true in this case
                ListEventSubscription = [
                    new ListEventSubscription()
                    {
                        ListEventType = [ListEventType.Customer], // Example for Customer event type
                        ListEventOperation = [ListEventOperation.Add, ListEventOperation.Modify]
                    }],
                TxnEventSubscription = [
                    new TxnEventSubscription()
                    {
                        TxnEventType = [TxnEventType.Invoice], // Transaction event type as Invoice
                        TxnEventOperation = [TxnEventOperation.Modify] // Event operation as Modify
                    }
                ]
            }
        };
    }
}

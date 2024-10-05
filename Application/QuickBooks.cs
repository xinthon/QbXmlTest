using Application.Commond.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using QbSync.QbXml;
using QbSync.QbXml.Objects;
using QBXMLRP2Lib;

namespace Application;

public class QuickBooks
{
    private Guid AppID = new Guid("1b31ca84-ed13-4458-92dc-67f9a742a94e");
    private string AppName = "FaceAzure";

    IRequestProcessor5 requestprocessor6 { get; set; }

    public QuickBooks()
    {
        requestprocessor6 = new RequestProcessor3();
    }

    public void XmlRequest()
    {
        var request = new QbXmlRequest();
        var innerRequest = new CustomerQueryRqType();

        // Add some filters here
        innerRequest.MaxReturned = "100";
        innerRequest.FromModifiedDate = new DATETIMETYPE(DateTime.Now);

        request.AddToSingle(innerRequest);

        // Get the XML
        var xml = request.GetRequest();
    }

    public void Connect()
    {
        requestprocessor6
            .OpenConnection(AppID.ToString(), AppName);

       var ticket = requestprocessor6
            .BeginSession("", 
                QBXMLRP2Lib.QBFileMode.qbFileOpenDoNotCare);

        var filePath = requestprocessor6
            .GetCurrentCompanyFileName(ticket);


        var request = new QbXmlRequest();
        var innerRequest = new CustomerQueryRqType();

        // Add some filters here
        innerRequest.MaxReturned = "100";

        request.AddToSingle(innerRequest);

        // Get the XML
        var xml = request.GetRequest();


        var response = requestprocessor6
            .ProcessRequest(ticket, xml);

        var auth = requestprocessor6
            .AuthPreferences;

        requestprocessor6.EndSession(ticket);
    }

    public void Close()
    {
        requestprocessor6.CloseConnection();
    }
}

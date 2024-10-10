using QbSync.QbXml;
using QbSync.QbXml.Objects;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Application.Infrastructure.QuickBooksIntegration;

public class QbXmlSubscriptionRequest
{
    public static readonly Version VERSION = new Version(13, 0);

    private readonly List<QBXMLSubscriptionMsgsRq> qbxmlMsgsRqList;

    public QbXmlSubscriptionRequest() 
    { 
        qbxmlMsgsRqList = new List<QBXMLSubscriptionMsgsRq>(); 
    }

    public void Add(params QBXMLSubscriptionMsgsRq[] messages) 
    { 
        qbxmlMsgsRqList.AddRange(messages); 
    }

    public void AddToSingle(params object[] requests) 
    { 
        Add(new QBXMLSubscriptionMsgsRq 
        { 
            Items = requests,
            
        }); 
    }

    public string GetRequest()
    {
        var qbXml = new QBXML
        {
            Items = qbxmlMsgsRqList.ToArray(),
            ItemsElementName = Enumerable.Repeat(
                ItemsChoiceType99.QBXMLMsgsRq, 
                qbxmlMsgsRqList.Count())
            .ToArray()
        };

        using var writer = new StringWriter();
        using XmlWriter xmlWriter = new QbXmlSubscriptionTextWriter(writer);
        xmlWriter.WriteProcessingInstruction(
            "xml", 
            "version=\"1.0\" encoding=\"utf-8\"");
        xmlWriter.WriteProcessingInstruction(
            "qbxml",
            string.Format("version=\"{0}.{1}\"", 
            VERSION.Major, 
            VERSION.Minor));

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        QbXmlSerializer.Instance
            .XmlSerializer
            .Serialize(xmlWriter, qbXml, ns);

        xmlWriter.Flush();
        return writer.ToString();
    }
}

public class QbXmlSubscriptionTextWriter : XmlTextWriter
{
    public QbXmlSubscriptionTextWriter(TextWriter textWriter) : base(textWriter)
    {
    }

    public override void WriteString(string? text)
    {
        if (text != null)
        {
            WriteRaw(HtmlEncodeSpecialCharacters(text));
        }
    }

    private string HtmlEncodeSpecialCharacters(string text)
    {
        text = HttpUtility.HtmlEncode(text);
        StringBuilder stringBuilder = new StringBuilder();
        string text2 = text;
        foreach (char c in text2)
        {
            if (c > '\u007f')
            {
                stringBuilder.Append($"&#{(int)c};");
            }
            else
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}

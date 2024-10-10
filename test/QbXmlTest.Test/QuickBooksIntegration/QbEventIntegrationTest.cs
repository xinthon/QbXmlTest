using Moq;
using NUnit.Framework;
using QBXMLRP2Lib;

namespace Application.Test.QuickBooksIntegration;

[TestFixture]
public class QbEventIntegrationTest
{
    private Mock<IRequestProcessor3> _requestProcessorMock;

    public QbEventIntegrationTest()
    {
        _requestProcessorMock = new Mock<IRequestProcessor3>();
    }

    [SetUp]
    public void SetUp()
    {
        // You can move Mock setup here if needed
    }

    [Test]
    public void TestEvent_ProcessSubscription_ReturnsExpectedResponse()
    {
        // Arrange
        var expectedResponse = "SomeResponse";  // Replace with an appropriate expected response
        _requestProcessorMock
            .Setup(rp => rp.ProcessSubscription(It.IsAny<string>()))
            .Returns(expectedResponse);

        var requestProcessor = _requestProcessorMock.Object;

        // Act
        var response = requestProcessor.ProcessSubscription("SomeInput");

        // Assert
        Assert.That(expectedResponse, Is.EqualTo(response));

        _requestProcessorMock.Verify(rp => rp.ProcessSubscription(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void TestEvent_ProcessSubscription_ThrowsException()
    {
        // Arrange
        _requestProcessorMock
            .Setup(rp => rp.ProcessSubscription(It.IsAny<string>()))
            .Throws(new Exception("Test Exception"));

        var requestProcessor = _requestProcessorMock.Object;

        // Act & Assert
        Assert.Throws<Exception>(() => requestProcessor.ProcessSubscription("SomeInput"));
    }
}


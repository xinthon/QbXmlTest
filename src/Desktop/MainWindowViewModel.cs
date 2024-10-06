using MediatR;
using QbSync.QbXml.Objects;
using System.ComponentModel;
using System.Windows.Input;
using Application.Features.Qb.Queries.GetQbLists;
using QbSync.QbXml;
using Application.Features.Qb.Queries;

namespace Desktop;

public class MainWindowViewModel 
{
    public ICommand ProcessCommand { get; }

    private readonly IMediator _sender;
    public MainWindowViewModel(IMediator sender)
    {
        _sender = sender;
        ProcessCommand = new RelayCommand(OnProcess);
    }

    private async void OnProcess()
    {
        var xmlRequest = new QbXmlRequest();
        xmlRequest.AddToSingle(new CustomerQueryRqType()
        {
            MaxReturned = "100",
            ActiveStatus = ActiveStatus.ActiveOnly,
        });

        var xmlResponse = await _sender.Send(new GetQbListQuery(xmlRequest));

        var customerQueryRsType = new QbXmlResponse()
            .GetSingleItemFromResponse<CustomerQueryRsType>(xmlResponse);

        if (customerQueryRsType is null)
            return;

        var number = 1;
        foreach (var customerRet in  customerQueryRsType.CustomerRet)
        {
            Console.WriteLine($"{number} - {customerRet.Name}");
            number++;
        }
    }
}

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private Action _action;

    public RelayCommand(Action action)
    {
        _action = action;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _action?.Invoke();
    }
}

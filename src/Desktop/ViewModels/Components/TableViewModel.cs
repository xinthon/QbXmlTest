using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Framework;
using QbSync.QbXml;
using QbSync.QbXml.Objects;
using System.Collections.ObjectModel;
using System;
using Application.Commond.Abstractions.Qb;
using System.CodeDom;
using MediatR;
using Application.Features.Qb.Queries.GetQbList;

namespace Desktop.ViewModels.Components;

public partial class NavBarItemViewModel : ViewModelBase
{
    [RelayCommand]
    public void Navigate() => _callBack?
        .Invoke(_value);

    private readonly Action<System.Type> _callBack;

    private readonly System.Type _value;

    public NavBarItemViewModel(string name, System.Type value, Action<System.Type> callBack)
    {
        _name = name;
        _value = value;
        _callBack = callBack; 
    }

    [ObservableProperty]
    private string _name;
}

public partial class QbListNavBarViewModel<TQbRequest, TQbResponse> : ViewModelBase
    where TQbRequest : IQbRequest where TQbResponse : IQbResponse
{
    private readonly Action<QbListNavBarViewModel<TQbRequest, TQbResponse>> _callBack;

    [ObservableProperty]
    private string _name;
}

public partial class TableViewModel : ViewModelBase
{
    private readonly ObservableCollection<object>  _itemCollection =
        new ObservableCollection<object>();

    private readonly ObservableCollection<NavBarItemViewModel> _menuCollection = 
        new ObservableCollection<NavBarItemViewModel>();

    private readonly IQbXmlRequestProcessor _requestProcessor;
    private readonly ISender _sender;
    public TableViewModel(IQbXmlRequestProcessor requestProcessor, ISender sender)
    {
        _requestProcessor = requestProcessor;
        _sender = sender;

        var types = typeof(QbXmlRequest)
            .Assembly
            .GetTypes();

        var requestTypes = types
            .Where(x => x.IsAssignableTo(typeof(IQbRequest)) && x.Name.Contains("Query"))
            .OrderBy(x => x.Name);

        var testing = typeof(QbListNavBarViewModel<,>)
            .MakeGenericType(typeof(CustomerQueryRqType), typeof(CustomerQueryRsType));

        foreach(var type in requestTypes)
        {
            _itemCollection.Add(type.Name);

            _menuCollection.Add(new NavBarItemViewModel(
                type.Name, 
                type, 
                MenuViewModelCallBack));
        }

        OnPropertyChanged(nameof(ItemCollection));
        OnPropertyChanged(nameof(MenuCollection));
    }

    public async void MenuViewModelCallBack(System.Type menu)
    {
        var response = await _sender.Send(new GetQbListQuery<CustomerQueryRqType, CustomerQueryRsType>(new CustomerQueryRqType()
        {
            MaxReturned = "100"
        }));

        _itemCollection.Clear();
        foreach(var item in response.GetRetResult())
        {
            _itemCollection.Add(item);
        }

        OnPropertyChanged(nameof(ItemCollection));
    }

    public List<object> ItemCollection => _itemCollection.ToList();

    public List<NavBarItemViewModel> MenuCollection => _menuCollection.ToList();
}

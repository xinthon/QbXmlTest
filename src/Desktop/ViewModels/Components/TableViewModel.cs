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

public partial class QbListNavBarViewModel<TQbRequest, TQbResponse> : ViewModelBase
    where TQbRequest : class, IQbRequest where TQbResponse : IQbResponse
{
    private readonly Action<QbListNavBarViewModel<TQbRequest, TQbResponse>> _callBack;
    public QbListNavBarViewModel(string name, Action<QbListNavBarViewModel<TQbRequest, TQbResponse>> callBack)
    {
        _name = name;
        _callBack = callBack;
    }

    [ObservableProperty]
    private string _name;

    [RelayCommand]
    public void Navigate() => _callBack?.Invoke(this);
}

public partial class QbListNavBarViewModel : ViewModelBase
{
    private readonly Action<object> _callBack;
    private readonly object _query;
    public QbListNavBarViewModel(string name, object query, Action<object> callBack)
    {
        _name = name;
        _query = query; 
        _callBack = callBack;
    }

    [ObservableProperty]
    private string _name;

    [RelayCommand]
    public void Navigate() => _callBack?.Invoke(_query);
}

public partial class TableViewModel : ViewModelBase
{
    private readonly ObservableCollection<object> _itemCollection =
        new ObservableCollection<object>();

    private readonly ObservableCollection<object> _menuCollection =
        new ObservableCollection<object>();

    private readonly IQbXmlRequestProcessor _requestProcessor;
    private readonly ISender _sender;
    public TableViewModel(IQbXmlRequestProcessor requestProcessor, ISender sender)
    {
        _requestProcessor = requestProcessor;
        _sender = sender;
        InitializeMenuBars();
    }

    private void InitializeMenuBars()
    {
        var assemblyTypes = typeof(QbXmlRequest)
            .Assembly
            .GetTypes();

        Func<System.Type, string> group = (type) =>
        {
            var typeName = type.FullName ?? type.Name;

            if (typeName.EndsWith("RqType"))
                return typeName.Replace("RqType", "");

            if (typeName.EndsWith("RsType"))
                return typeName.Replace("RsType", "");

            return typeName;
        };

        Func<System.Type, bool> isQueryPredicate = (type) =>
        {
            return (typeof(IQbRequest).IsAssignableFrom(type) ||
                typeof(IQbResponse).IsAssignableFrom(type)) &&
                type.Name.Contains("Query");
        };

        var qbRequestResponseGroups = assemblyTypes
            .Where(isQueryPredicate)
            .GroupBy(group)
            .ToList();

        foreach (var qbTypeGroup in qbRequestResponseGroups)
        {
            var qbRequestType = qbTypeGroup
                .FirstOrDefault(type => type.IsAssignableTo(typeof(IQbRequest)));
            var qbResponseType = qbTypeGroup
                .FirstOrDefault(type => type.IsAssignableTo(typeof(IQbResponse)));

            if (qbRequestType != null && qbResponseType != null)
            {
                var navBarType = typeof(GetQbListQuery<,>)
                    .MakeGenericType(qbRequestType, qbResponseType);

                var navBarInstance = Activator
                    .CreateInstance(navBarType, [Activator.CreateInstance(qbRequestType)]);

                var name = qbRequestType.Name;

                _menuCollection.Add(new QbListNavBarViewModel(
                    name.Replace("RqType", ""), 
                    navBarInstance, 
                    MenuViewModelCallBack));
            }
        }
        OnPropertyChanged(nameof(MenuCollection));
    }

    public async void MenuViewModelCallBack(object viewModel, object testing)
    {
        if (viewModel.GetType().IsGenericType &&
            viewModel.GetType().GetGenericTypeDefinition() == typeof(QbListNavBarViewModel<,>))
        {
            var genericArguments = viewModel
                .GetType()
                .GetGenericArguments();

            var qbRequestType = genericArguments[0];
            var qbResponseType = genericArguments[1]; 

            var qbRequestInstance = Activator
                .CreateInstance(qbRequestType);

            var maxReturnedProperty = qbRequestType
                .GetProperty("MaxReturned");
            if (maxReturnedProperty != null)
            {
                maxReturnedProperty.SetValue(qbRequestInstance, "100");
            }

            var queryType = typeof(GetQbListQuery<,>)
                .MakeGenericType(qbRequestType, qbResponseType);
            var queryInstance = Activator
                .CreateInstance(queryType, qbRequestInstance);

            if (queryInstance is not IRequest<IQbResponse<object[]>> request)
                return;

            var response = await _sender
                .Send(request);

            var resultItems = response
                .GetRetResult();

            _itemCollection.Clear();
            foreach (var item in resultItems)
            {
                _itemCollection.Add(item);
            }
            OnPropertyChanged(nameof(ItemCollection));
        }
    }

    public async void MenuViewModelCallBack(object viewModel)
    {
        var query = new GetQbListQuery<InvoiceQueryRqType, InvoiceQueryRsType>(new InvoiceQueryRqType()
        {
            MaxReturned = "100"
        });

        var response = await _sender.Send(viewModel);

        _itemCollection.Clear();
        if (response is IQbResponse<object[]> qbResponse)
        {
            var result = qbResponse.GetRetResult();
            if (result != null)
            {
                foreach (var item in result)
                {
                    _itemCollection.Add(item);
                }
            }
        }
        OnPropertyChanged(nameof(ItemCollection));
    }

    public List<object> ItemCollection => _itemCollection.ToList();

    public List<object> MenuCollection => _menuCollection.ToList();
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Framework;
using QbSync.QbXml;
using QbSync.QbXml.Objects;
using System.Collections.ObjectModel;
using MediatR;
using Application.Features.Qb.Queries.GetQbList;

namespace Desktop.ViewModels.Components;

/// <summary>
/// Navigation bar ViewModel for QuickBooks.
/// </summary>
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

/// <summary>
/// ViewModel for managing a table view and the corresponding menu options for QuickBooks data.
/// </summary>
public partial class TableViewModel : ViewModelBase
{
    private readonly ObservableCollection<object> _itemCollection =
        new ObservableCollection<object>();

    private readonly ObservableCollection<object> _menuCollection =
        new ObservableCollection<object>();

    [ObservableProperty]
    private bool _isLoading;

    private readonly ISender _sender;
    public TableViewModel(ISender sender)
    {
        _sender = sender;
        InitializeMenuBars();
    }

    /// <summary>
    /// Initializes the navigation bar menu options for QuickBooks requests.
    /// </summary>
    private void InitializeMenuBars()
    {
        var assemblyTypes = typeof(QbXmlRequest).Assembly.GetTypes();

        var qbRequestResponseGroups = assemblyTypes
            .Where(IsQueryType)
            .GroupBy(GetTypeGroupName)
            .ToList();

        foreach (var group in qbRequestResponseGroups)
        {
            var qbRequestType = group.FirstOrDefault(t => typeof(IQbRequest).IsAssignableFrom(t));
            var qbResponseType = group.FirstOrDefault(t => typeof(IQbResponse).IsAssignableFrom(t));

            if (qbRequestType != null && qbResponseType != null &&
                qbResponseType.IsAssignableTo(typeof(IQbResponse<object[]>)))
            {
                var navBarType = typeof(GetQbListQuery<,>).MakeGenericType(qbRequestType, qbResponseType);
                var navBarInstance = Activator.CreateInstance(navBarType, Activator.CreateInstance(qbRequestType));
                var displayName = qbRequestType.Name.Replace("QueryRqType", "");

                _menuCollection.Add(new QbListNavBarViewModel(displayName, navBarInstance!, MenuViewModelCallback));
            }
        }

        OnPropertyChanged(nameof(MenuCollection));
    }

    /// <summary>
    /// Executes the query and updates the item collection based on the response.
    /// </summary>
    /// <param name="query">The query object to send.</param>
    public async void MenuViewModelCallback(object query)
    {
        try
        {
            IsLoading = true;

            var response = await Task.Run(() => _sender.Send(query));

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
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Checks whether a type represents a QuickBooks query.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is a QuickBooks query, otherwise false.</returns>
    private static bool IsQueryType(System.Type type) =>
        (typeof(IQbRequest).IsAssignableFrom(type) || typeof(IQbResponse).IsAssignableFrom(type)) && type.Name.Contains("Query");

    /// <summary>
    /// Determines the group name for a type based on its suffix.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>A string representing the group name.</returns>
    private static string GetTypeGroupName(System.Type type)
    {
        var typeName = type.FullName ?? type.Name;
        if (typeName.EndsWith("RqType"))
            return typeName.Replace("RqType", "");
        
        if (typeName.EndsWith("RsType"))
            return typeName.Replace("RsType", "");

        return typeName;
    }

    public List<object> ItemCollection => _itemCollection.ToList();

    public List<object> MenuCollection => _menuCollection.ToList();
}

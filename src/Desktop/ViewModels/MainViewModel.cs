using Application.Infrastructure.Workers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Framework;
using Desktop.ViewModels.Components;
using Microsoft.Extensions.Hosting;
using System.Windows.Controls;

namespace Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Title { get; } = "QbXmlTest";

    private readonly List<Control> _viewHistory =
        new List<Control>();

    private readonly DialogManager _dialogManager;
    private readonly ViewModelManager _viewModelManager;
    private readonly ViewManager _viewManager;
    private readonly IHostedService _hostedService;
    public MainViewModel(
        DialogManager dialogManager, 
        ViewModelManager viewModelManager, 
        ViewManager viewManager, 
        IHostedService hostedService)
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        _viewManager = viewManager;
        _hostedService = hostedService;

        _viewModelManager.SubscribeOnViewModelChanged(OnViewModelChanged);
        _viewModelManager.NavigateTo<TableViewModel>();
    }

    [RelayCommand]
    private async Task StopService()
    {
        await _hostedService
            .StopAsync(CancellationToken.None);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartService()
    {
        await _hostedService
            .StartAsync(CancellationToken.None);
        await Task.CompletedTask;
    }

    [ObservableProperty]
    private Control? _activeView;

    private void OnViewModelChanged(ViewModelBase viewModel)
    {
        var view = _viewHistory
            .FirstOrDefault(x => x.DataContext == viewModel);

        if (view is null)
        {
            view = _viewManager.TryBindView(viewModel);
            if (view is not null)
            {
                _viewHistory.Add(view);
            }
        }

        ActiveView = view;
        OnPropertyChanged(nameof(ActiveView));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModelManager.UnsubscribeOnViewModelChanged(OnViewModelChanged);
        }
        base.Dispose(disposing);
    }
}


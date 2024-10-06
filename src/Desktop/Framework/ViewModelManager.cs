using Desktop.ViewModels;
using Desktop.ViewModels.Components;
using Desktop.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Desktop.Framework;

public class ViewModelManager
{
    private readonly IServiceProvider _serviveProvider;
    public ViewModelManager(IServiceProvider serviveProvider)
    {
        _serviveProvider = serviveProvider;
    }

    public ListViewModel CreateListViewModel()
    {
        return _serviveProvider
            .GetRequiredService<ListViewModel>();
    }

    public MainViewModel CreateMainViewModel()
    {
        return _serviveProvider
            .GetRequiredService<MainViewModel>();
    } 

    public MessageBoxViewModel CreateMessageBoxViewModel(
        string title,
        string message,
        string? okButtonText,
        string? cancelButtonText)
    {
        var viewModel = _serviveProvider
            .GetRequiredService<MessageBoxViewModel>();

        viewModel.Title = title;
        viewModel.Message = message;
        viewModel.DefaultButtonText = okButtonText;
        viewModel.CancelButtonText = cancelButtonText;

        return viewModel;
    }

    public MessageBoxViewModel CreateMessageBoxViewModel(string title, string message) =>
        CreateMessageBoxViewModel(title, message, "CLOSE", null);}

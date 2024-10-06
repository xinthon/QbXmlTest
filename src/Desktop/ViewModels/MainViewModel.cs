using CommunityToolkit.Mvvm.Input;
using Desktop.Framework;

namespace Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase 
{
    public string Title { get; } = "QbXmlTest";

    private readonly DialogManager _dialogManager;
    private readonly ViewModelManager _viewModelManager;

    public MainViewModel(DialogManager dialogManager, ViewModelManager viewModelManager)
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
    }

    [RelayCommand]
    private async Task ShowMessage()
    {
        var result = await _dialogManager.ShowDialogAsync(_viewModelManager
            .CreateMessageBoxViewModel("Testing Message", "Hello this is a testing message box!"));

        if(result is true)
        {
        }
    }
}


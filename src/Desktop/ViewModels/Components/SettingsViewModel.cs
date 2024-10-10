using Desktop.Framework;
using MediatR;

namespace Desktop.ViewModels.Components;

internal class SettingsViewModel : ViewModelBase
{
    private readonly ISender _sender;
    public SettingsViewModel(ISender sender)
    {
        _sender = sender; 
    }
}

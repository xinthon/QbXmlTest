using Desktop.ViewModels;
using Desktop.ViewModels.Components;
using Desktop.ViewModels.Dialogs;
using Desktop.Views;
using Desktop.Views.Components;
using Desktop.Views.Dialogs;
using System.Windows.Controls;

namespace Desktop.Framework;

public class ViewManager
{
    private Control? TryCreateView(ViewModelBase viewModel)
    {
        return viewModel switch
        {
            MainViewModel => new MainView(),
            QbListViewModel => new QbListView(),
            SettingsViewModel => new SettingsView(),
            MessageBoxViewModel => new MessageBoxView(),
            _ => null,
        };
    }

    public Control? TryBindView(ViewModelBase viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        view.DataContext ??= viewModel;

        return view;
    }
}
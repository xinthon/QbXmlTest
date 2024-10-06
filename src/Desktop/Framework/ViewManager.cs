using Desktop.ViewModels;
using Desktop.ViewModels.Dialogs;
using Desktop.Views;
using Desktop.Views.Dialogs;
using System.Windows.Controls;

namespace Desktop.Framework;

public class ViewManager
{
    private Control? TryCreateView(ViewModelBase viewModel) =>
        viewModel switch
        {
            MainViewModel => new MainView(),
            MessageBoxViewModel => new MessageBoxView(),
            _ => null,
        };

    public Control? TryBindView(ViewModelBase viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        view.DataContext ??= viewModel;

        return view;
    }
}
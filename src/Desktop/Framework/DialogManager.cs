using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Desktop.Framework;

public class DialogManager : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new SemaphoreSlim(1, 1);
    private readonly ViewManager _viewManger;
    public DialogManager(ViewManager viewManger)
    {
        _viewManger = viewManger; 
    }

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialogViewModel)
    {
        await _dialogLock.WaitAsync();
        try
        {
            var dialogView = _viewManger
                .TryBindView(dialogViewModel) ?? new UserControl();

            await DialogHost.Show(dialogView, async (object _, DialogOpenedEventArgs args) =>
            {
                await dialogViewModel.WaitForCloseAsync();
                args.Session.Close();
            });
            
            return dialogViewModel.DialogResult;
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    public void Dispose() => _dialogLock.Dispose();
}


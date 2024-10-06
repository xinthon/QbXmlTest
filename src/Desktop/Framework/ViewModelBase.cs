using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.Framework;

public class ViewModelBase : ObservableObject, IDisposable
{
    ~ViewModelBase() => Dispose(false);

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

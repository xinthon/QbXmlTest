using Application;
using MediatR;
using QbSync.QbXml.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Desktop;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ICommand OpenConnectionCommand { get; }

    public MainWindowViewModel()
    {
        OpenConnectionCommand = new RelayCommand<string>(
            OnOpenConnectionCommand, 
            CanOpenConnectionCommand);
    }

    QuickBooks q = new QuickBooks();
    private void OnOpenConnectionCommand(string? param)
    {
        q.Connect();
        q.Close();
    }

    private bool CanOpenConnectionCommand(string? param)
    {
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class RelayCommand<TParam> : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Action<TParam?>? _action;
    private Func<TParam?, bool>? _canAction;

    public RelayCommand(Action<TParam?>? action = null, Func<TParam?, bool>? canAction = null)
    {
        _action = action; 
        _canAction = canAction;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canAction is null || parameter is not TParam p)
            return true;

        return _canAction.Invoke(p);   
    }

    public void Execute(object? parameter)
    {
        _action?.Invoke((TParam)parameter);
    }
}

public class RelayCommand : RelayCommand<object> { }

using Microsoft.Extensions.Logging;

namespace Desktop.Framework;

public class ViewModelManager
{
    private readonly Stack<ViewModelBase> _navigationStack = new Stack<ViewModelBase>();
    private readonly ViewModelFactory _viewModelFactory;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<ViewModelManager> _logger;

    private event Action<ViewModelBase>? _onViewModelChanged;

    public ViewModelManager(ViewModelFactory viewModelFactory, ILogger<ViewModelManager> logger)
    {
        _viewModelFactory = viewModelFactory;
        _semaphore = new SemaphoreSlim(1, 1);
        _logger = logger;
    }

    public async void NavigateTo<TViewModel>(params object[] parameters)
        where TViewModel : ViewModelBase
    {
        if (_navigationStack.Any() && _navigationStack
            .Peek()
            .GetType() == typeof(TViewModel))
        {
            return;
        }

        _logger.LogInformation("Attempting to navigate to {ViewModelName} with parameters {Parameters}.",
            typeof(TViewModel).Name, parameters);

        await _semaphore.WaitAsync();
        try
        {

            var viewModel = _viewModelFactory.Create<TViewModel>(parameters);

            _navigationStack.Push(viewModel);

            _logger.LogInformation("{ViewModelName} created and added to the navigation stack.", typeof(TViewModel).Name);
            _onViewModelChanged?.Invoke(viewModel);
            _logger.LogInformation("Content changed to {ViewModelName}.", typeof(TViewModel).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to {ViewModelName}.", typeof(TViewModel).Name);
            throw;
        }
        finally
        {
            _semaphore.Release();
            _logger.LogInformation("Semaphore released after navigation.");
        }
    }

    public async void NavigateBack()
    {
        _logger.LogInformation("Attempting to navigate back.");

        await _semaphore.WaitAsync();
        try
        {
            if (_navigationStack.Count > 1)
            {
                _navigationStack.Pop();
                var previousViewModel = _navigationStack.Peek();
                _logger.LogInformation("Navigated back to {ViewModelName}.", previousViewModel.GetType().Name);
                _onViewModelChanged?.Invoke(previousViewModel);
            }
            else
            {
                _logger.LogWarning("Navigation stack has only one item. Cannot navigate back further.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while navigating back.");
            throw;
        }
        finally
        {
            _semaphore.Release();
            _logger.LogInformation("Semaphore released after navigating back.");
        }
    }

    public void SubscribeOnViewModelChanged(Action<ViewModelBase> viewModelChanged)
    {
        _logger.LogInformation("Subscribing to view model changes.");
        _onViewModelChanged += viewModelChanged;
    }

    public void UnsubscribeOnViewModelChanged(Action<ViewModelBase> viewModelChanged)
    {
        _logger.LogInformation("Unsubscribing from view model changes.");
        _onViewModelChanged -= viewModelChanged;
    }
}


using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CadEye_WebVersion.Commands.Helpers
{
    public class AsyncCommandT<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncCommandT(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object? parameter)
        {
            if (_isExecuting) return false;
            T paramValue = parameter == null ? default! : (T)parameter;
            return _canExecute(paramValue);
        }

        public async void Execute(object? parameter)
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                T paramValue = parameter == null ? default! : (T)parameter;
                await _execute(paramValue);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

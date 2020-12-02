using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GitCodeSearch.ViewModels
{
    public class RelayCommand : ICommand, INotifyPropertyChanged
    {
        private Func<CancellationToken, Task> action_;
        private bool running_;
        private CancellationTokenSource cts_ = new CancellationTokenSource();

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand(Action action)
        {
            action_ = _ =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        public RelayCommand(Action<CancellationToken> action)
        {
            action_ = token =>
            {
                action(token);
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                else
                    return Task.CompletedTask;
            };
            CancelCommand = new RelayCommand(Cancel);
        }

        public RelayCommand(Func<Task> action)
        {
            action_ = _ => action();
        }

        public RelayCommand(Func<CancellationToken, Task> action)
        {
            action_ = action;
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Cancel()
        {
            cts_.Cancel();
         
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool IsRunning => running_;

        public bool CanExecute(object? parameter)
        {
            return !running_;
        }

        public ICommand? CancelCommand { get; }

        public void Execute(object? parameter)
        {
            running_ = true;
            cts_.Dispose();
            cts_ = new CancellationTokenSource();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            CommandManager.InvalidateRequerySuggested();

            try
            {
                action_(cts_.Token).WaitAndDispatch();
            }
            finally
            {
                running_ = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
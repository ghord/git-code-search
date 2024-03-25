using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitCodeSearch.ViewModels
{
    public class RelayCommand : ICommand, INotifyPropertyChanged
    {
        private readonly Func<CancellationToken, Task> action_;
        private readonly Func<bool>? canExecute_;
        private bool running_;
        private CancellationTokenSource cts_ = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand(Action action, Func<bool>? canExecute = null)
        {
            action_ = _ =>
            {
                action();
                return Task.CompletedTask;
            };
            canExecute_ = canExecute;
        }

        public RelayCommand(Action<CancellationToken> action, Func<bool>? canExecute = null)
        {
            action_ = token =>
            {
                action(token);
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                else
                    return Task.CompletedTask;
            };
            canExecute_ = canExecute;
            CancelCommand = new RelayCommand(Cancel);

        }

        public RelayCommand(Func<Task> action, Func<bool>? canExecute = null)
        {
            action_ = _ => action();
            canExecute_ = canExecute;
        }

        public RelayCommand(Func<CancellationToken, Task> action, Func<bool>? canExecute = null)
        {
            action_ = action;
            canExecute_ = canExecute;
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

        public bool IsRunning
        {
            get => running_;
            set
            {
                running_ = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }

        public bool CanExecute(object? parameter)
        {
            return !IsRunning && canExecute_?.Invoke() != false;
        }

        public ICommand? CancelCommand { get; }

        public void Execute(object? parameter)
        {
            IsRunning = true;
            cts_.Dispose();
            cts_ = new CancellationTokenSource();

            CommandManager.InvalidateRequerySuggested();

            try
            {
                action_(cts_.Token).WaitAndDispatch();
            }
            finally
            {
                IsRunning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public class RelayCommand<T> : ICommand, INotifyPropertyChanged
    {
        private readonly Func<T, CancellationToken, Task> action_;
        private readonly Func<T, bool>? canExecute_;
        private bool running_;
        private CancellationTokenSource cts_ = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand(Action<T> action, Func<T, bool>? canExecute = null)
        {
            action_ = (param, _) =>
            {
                action(param);
                return Task.CompletedTask;
            };
            canExecute_ = canExecute;
        }

        public RelayCommand(Action<CancellationToken> action, Func<T, bool>? canExecute = null)
        {
            action_ = (param, token) =>
            {
                action(token);
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                else
                    return Task.CompletedTask;
            };
            canExecute_ = canExecute;
            CancelCommand = new RelayCommand(Cancel);
        }

        public RelayCommand(Func<T, Task> action, Func<T, bool>? canExecute = null)
        {
            action_ = (param, _) => action(param);
            canExecute_ = canExecute;
        }

        public RelayCommand(Func<T, CancellationToken, Task> action, Func<T, bool>? canExecute = null)
        {
            action_ = action;
            canExecute_ = canExecute;
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

        public bool IsRunning
        {
            get => running_;
            set
            {
                running_ = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T value)
            {
                return !IsRunning && canExecute_?.Invoke(value) != false;
            }
            else
            {
                return false;
            }
        }

        public ICommand? CancelCommand { get; }

        public void Execute(object? parameter)
        {
            if (parameter is not T value)
                return;

            IsRunning = true;
            cts_.Dispose();
            cts_ = new CancellationTokenSource();

            CommandManager.InvalidateRequerySuggested();

            try
            {
                action_(value, cts_.Token).WaitAndDispatch();
            }
            finally
            {
                IsRunning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
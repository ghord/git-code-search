using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GitCodeSearch.ViewModels
{
    public class RelayCommand : ICommand, INotifyPropertyChanged
    {
        private Func<Task> action_;
        private bool running_;

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand(Action action)
            : this(() => { action(); return Task.CompletedTask; })
        {
        }

        public RelayCommand(Func<Task> action)
        {
            action_ = action;
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

        public void Execute(object? parameter)
        {
            running_ = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            CommandManager.InvalidateRequerySuggested();

            try
            {
                action_().WaitAndDispatch();
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
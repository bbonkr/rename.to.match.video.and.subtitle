using System;
using System.Windows.Input;

namespace SubtitleFileRename.v2.App
{
    public class BindableCommand<T> : ICommand
    {
        public BindableCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            if (parameter is T p)
            {
                return canExecute(p);
            }
            try
            {
                return canExecute((T)parameter);
            }
            catch
            {
                return false;
            }
        }

        public void Execute(object parameter)
        {
            if (parameter is T p)
            {
                execute(p);
            }
            else
            {
                try
                {
                    execute((T)parameter);
                }
                catch
                {
                    throw new ArgumentException("Could not convert parameter", nameof(parameter));
                }
            }
        }

        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;
    }

    public class BindableCommand : BindableCommand<object>
    {

        public BindableCommand(Action<object> execute, Func<object, bool> canExecute = null)
            : base(execute, canExecute)
        {

        }

        public BindableCommand(Action execute, Func<bool> canExecute = null)
            : this(new Action<object>((obj) => execute()), new Func<object, bool>((obj) => canExecute == null ? true : canExecute()))
        {

        }
    }
}

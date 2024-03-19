// RelayCommand.cs fájl
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SocketChat
{
    // RelayCommand osztály az ICommand interfész implementálására
    public class RelayCommand : ICommand
    {
        // Az elvégzendő művelet
        protected readonly Action execute;

        // A művelet végrehajthatóságát ellenőrző delegált
        protected readonly Func<bool> canExecute;

        // RelayCommand példány létrehozása az elvégzendő művelettel és végrehajthatóságot ellenőrző delegálttal
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            this.execute = execute;
            this.canExecute = canExecute;
        }

        // RelayCommand példány létrehozása csak az elvégzendő művelettel
        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        // ICommand esemény, amely a végrehajthatóság változását jelzi
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        // Vizsgálja, hogy a művelet végrehajtható-e
        public virtual bool CanExecute(object parameter)
        {
            return this.canExecute == null ? true : this.canExecute();
        }

        // Végrehajtja a műveletet
        public virtual void Execute(object parameter)
        {
            this.execute();
        }
    }

    // Generikus RelayCommand osztály az ICommand interfész implementálására
    public class RelayCommand<T> : ICommand
    {
        // Az elvégzendő művelet generikus típusú paraméterrel
        protected readonly Action<T> execute;

        // A művelet végrehajthatóságát ellenőrző delegált generikus típusú paraméterrel
        protected readonly Func<T, bool> canExecute;

        // RelayCommand példány létrehozása az elvégzendő művelettel és végrehajthatóságot ellenőrző delegálttal
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        // RelayCommand példány létrehozása csak az elvégzendő művelettel
        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        // ICommand esemény, amely a végrehajthatóság változását jelzi
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        // Vizsgálja, hogy a művelet végrehajtható-e a generikus típusú paraméter alapján
        public virtual bool CanExecute(object parameter)
        {
            return this.canExecute == null ? true : this.canExecute((T)parameter);
        }

        // Végrehajtja a műveletet a generikus típusú paraméterrel
        public virtual void Execute(object parameter)
        {
            this.execute((T)parameter);
        }
    }

    // AsynchronousRelayCommand osztály, amely aszinkron műveleteket támogat
    public class AsynchronousRelayCommand : RelayCommand
    {
        // Az aszinkron művelet végrehajtásának állapotát nyomon követő események
        public event EventHandler Started;
        public event EventHandler Ended;

        // Az aszinkron művelet végrehajtásának állapotát jelző tulajdonság
        private bool isExecuting = false;
        public bool IsExecuting
        {
            get { return this.isExecuting; }
        }

        // AsynchronousRelayCommand példány létrehozása az elvégzendő művelettel és végrehajthatóságot ellenőrző delegálttal
        public AsynchronousRelayCommand(Action execute, Func<bool> canExecute)
            : base(execute, canExecute)
        {
        }

        // AsynchronousRelayCommand példány létrehozása csak az elvégzendő művelettel
        public AsynchronousRelayCommand(Action execute)
            : base(execute)
        {
        }

        // Vizsgálja, hogy az aszinkron művelet végrehajtható-e
        public override bool CanExecute(object parameter)
        {
            return ((base.CanExecute(parameter)) && (!this.isExecuting));
        }

        // Végrehajtja az aszinkron műveletet
        public override void Execute(object parameter)
        {
            try
            {
                this.isExecuting = true;
                if (this.Started != null)
                {
                    this.Started(this, EventArgs.Empty);
                }

                Task task = Task.Factory.StartNew(() =>
                {
                    this.execute();
                });
                task.ContinueWith(t =>
                {
                    this.OnRunWorkerCompleted(EventArgs.Empty);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                this.OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(null, ex, true));
            }
        }

        // Az aszinkron művelet végrehajtásának befejezésekor hívódik meg
        private void OnRunWorkerCompleted(EventArgs e)
        {
            this.isExecuting = false;
            if (this.Ended != null)
            {
                this.Ended(this, e);
            }
        }
    }
}

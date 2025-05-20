using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ClassLibrary;

namespace ViewModel
{
    public class MyViewModel : INotifyPropertyChanged
    {
        private DataCollection dataCollection = new DataCollection();
        private int timePause;
        private int newDataItemsCount;
        private CancellationTokenSource tokenSource;
        private string textInfo;
        private bool isStartEnabled;
        private bool isCancelEnabled;
        public ObservableCollection<DataItem> DataItems => dataCollection.Obs;
        public ICommand StartCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ShowDataCommand { get; }
        public MyViewModel()
        {
            TimePause = 2000;
            NewDataItemsCount = 5;
            isStartEnabled = true;
            isCancelEnabled = false;
            StartCommand = new RelayCommand(async _ => await StartAsync());
            CancelCommand = new RelayCommand(_ => Cancel());
            ShowDataCommand = new RelayCommand(_ => ShowData());
        }
        public int TimePause
        {
            get => timePause;
            set
            {
                timePause = value;
                OnPropertyChanged(nameof(TimePause));
            }
        }
        public int NewDataItemsCount
        {
            get => newDataItemsCount;
            set
            {
                newDataItemsCount = value;
                OnPropertyChanged(nameof(NewDataItemsCount));
            }
        }
        public string TextInfo
        {
            get => textInfo;
            set
            {
                textInfo = value;
                OnPropertyChanged(nameof(TextInfo));
            }
        }
        public bool IsStartEnabled
        {
            get => isStartEnabled;
            set
            {
                isStartEnabled = value;
                OnPropertyChanged(nameof(IsStartEnabled));
            }
        }
        public bool IsCancelEnabled
        {
            get => isCancelEnabled;
            set
            {
                isCancelEnabled = value;
                OnPropertyChanged(nameof(IsCancelEnabled));
            }
        }
        private async Task StartAsync()
        {
            IsStartEnabled = false;
            IsCancelEnabled = true;
            TextInfo = "";
            tokenSource = new CancellationTokenSource();
            int current = 0;
            try
            {
                while (current < NewDataItemsCount)
                {
                    DataItem result = await Task.Run(() => DataItem.CreateLongTimeDataItem(current++, TimePause));
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        IsCancelEnabled = false;
                        TextInfo = "Operation cancelled";
                        tokenSource.Token.ThrowIfCancellationRequested();
                    }
                    dataCollection.Add(result);
                }
                TextInfo = "Operation completed";
            }
            catch (OperationCanceledException)
            {
                TextInfo = "Operation cancelled";
            }
            catch (Exception ex)
            {
                TextInfo = ex.Message;
            }
            IsStartEnabled = true;
            IsCancelEnabled = false;
        }
        public event Action CancelRequested;
        private void Cancel()
        {
            tokenSource?.Cancel();
            CancelRequested?.Invoke();
        }
        public event Action<string> ShowDataRequested;
        private void ShowData()
        {
            ShowDataRequested?.Invoke(dataCollection.ToString());
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;
        private event EventHandler canExecuteChanged;
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }
        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter);
        }
        public void Execute(object? parameter)
        {
            execute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add => canExecuteChanged += value;
            remove => canExecuteChanged -= value;
        }
        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
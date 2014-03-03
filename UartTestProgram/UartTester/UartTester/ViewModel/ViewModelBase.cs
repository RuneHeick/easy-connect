using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;

namespace UartTester.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {

        public bool HasValidationErrors
        {
            get;
            private set;
        }

        protected ViewModelBase()
        {
        }

        public event EventHandler RequestClose;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        protected void OnRequestClose()
        {
            var handler = RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

        public void SetHasErrors(bool hasErrors)
        {
            HasValidationErrors = hasErrors;
        }

        public Window ContainingWindow { get; set; }
    }
}

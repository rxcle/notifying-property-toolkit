// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System.ComponentModel;

namespace NotifyingPropertyToolkit.Examples.Wpf
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected NotifyingContext NotifyingContext { get; }

        public ViewModelBase()
        {
            NotifyingContext = new NotifyingContext(ni =>
            {
                if (ni is INotifyingProperty np)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(np.Name));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

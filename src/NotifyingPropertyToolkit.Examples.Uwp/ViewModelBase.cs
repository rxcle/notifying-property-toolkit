// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System.ComponentModel;

namespace NotifyingPropertyToolkit.Examples.Uwp
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected NotifyingContext NotifyingContext { get; }

        public ViewModelBase()
        {
            NotifyingContext = new NotifyingContext(np => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(np.Name)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

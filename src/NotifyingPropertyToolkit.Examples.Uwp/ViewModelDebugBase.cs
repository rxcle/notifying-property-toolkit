// Copyright (c) 2021 Rxcle. Rxcle licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NotifyingPropertyToolkit.Examples.Uwp
{
    public class ViewModelDebugBase : INotifyPropertyChanged
    {
        protected NotifyingContext NotifyingContext { get; }

        public ObservableCollection<NotifyingItemInfo> ItemInfos { get; } = new ObservableCollection<NotifyingItemInfo>();

        public ViewModelDebugBase()
        {
            NotifyingContext = new NotifyingContext(ni =>
            {
                var itemInfo = ItemInfos.FirstOrDefault(ii => ii.Name == ni.Name);
                if (itemInfo is null)
                {
                    itemInfo = new NotifyingItemInfo();
                    itemInfo.Name.Value = ni.Name;
                    ItemInfos.Add(itemInfo);
                }
                itemInfo.Updates.Value = itemInfo.Updates.Value + 1;
                if (ni is INotifyingProperty np)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(np.Name));
            },
            commandRequeryHook: (add, eventHandler) =>
            {/*
                if (add)
                {
                    CommandManager.RequerySuggested += eventHandler;
                }
                else
                {
                    CommandManager.RequerySuggested -= eventHandler;
                }
                */
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class NotifyingItemInfo : ViewModelBase
    {
        public NotifyingItemInfo()
        {
            Name = NotifyingContext.CreateWritableProperty<string>(nameof(Name));
            Updates = NotifyingContext.CreateWritableProperty<int>(nameof(Updates));
        }

        public NotifyingWritableProperty<string> Name { get; }

        public NotifyingWritableProperty<int> Updates { get; }
    }
}
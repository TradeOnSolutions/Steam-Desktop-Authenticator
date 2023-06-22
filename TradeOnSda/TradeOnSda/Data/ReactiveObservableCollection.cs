using System;
using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;

namespace TradeOnSda.Data;

public class ReactiveObservableCollection<T> 
    where T : class
{
    // ReSharper disable once InconsistentNaming
    protected ObservableCollectionExtended<T> _items { get; init; }

    public ReadOnlyObservableCollection<T> Items { get; }

    public IObservable<IChangeSet<T>> ItemsConnection =>
        _items.ToObservableChangeSet();
    
    public ReactiveObservableCollection()
    {
        _items = new ObservableCollectionExtended<T>();
        Items = new ReadOnlyObservableCollection<T>(_items);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
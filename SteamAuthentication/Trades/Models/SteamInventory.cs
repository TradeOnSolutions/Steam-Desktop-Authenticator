using System.Collections;
using SteamAuthentication.Trades.Responses;

namespace SteamAuthentication.Trades.Models;

public class SteamInventory : IEnumerable<ItemProxy>
{
    private readonly Dictionary<ItemId, ItemDescription> _itemDescriptions;

    public IReadOnlyDictionary<ItemId, ItemDescription> ItemDescriptions => _itemDescriptions.AsReadOnly();

    private readonly Dictionary<long, InventoryItem> _items;

    public IReadOnlyDictionary<long, InventoryItem> InventoryItems => _items.AsReadOnly();

    internal SteamInventory(Dictionary<long, InventoryItem> items,
        Dictionary<ItemId, ItemDescription> itemDescriptions)
    {
        _itemDescriptions = itemDescriptions;
        _items = items;
    }

    public IEnumerator<ItemProxy> GetEnumerator() => new ItemProxyEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class ItemProxyEnumerator : IEnumerator<ItemProxy>
{
    private readonly IEnumerator<KeyValuePair<long, InventoryItem>> _itemsEnumerator;

    private readonly SteamInventory _inventory;

    public ItemProxyEnumerator(SteamInventory inventory)
    {
        _inventory = inventory;
        _itemsEnumerator = inventory.InventoryItems.GetEnumerator();
    }

    public bool MoveNext() => _itemsEnumerator.MoveNext();

    public void Reset() => _itemsEnumerator.Reset();

    public ItemProxy Current
    {
        get
        {
            var (_, value) = _itemsEnumerator.Current;

            ItemDescription? description = null;

            var itemId = value.GetItemId();
            
            if (_inventory.ItemDescriptions.ContainsKey(itemId))
                description = _inventory.ItemDescriptions[itemId];

            return new ItemProxy(value, description);
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose() => _itemsEnumerator.Dispose();
}

public readonly struct ItemProxy
{
    public InventoryItem Item { get; }

    public ItemDescription? Description { get; }

    public ItemProxy(InventoryItem item, ItemDescription? description)
    {
        Item = item;
        Description = description;
    }

    public void Deconstruct(out InventoryItem item, out ItemDescription? description)
    {
        item = Item;
        description = Description;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int quantity;

    public InventorySlot(ItemSO item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

[Serializable]
public class Inventory
{
    [SerializeField] private List<InventorySlot> slots = new();

    public int MaxSlots { get; private set; }
    public int UsedSlots => slots.Count;
    public IReadOnlyList<InventorySlot> Slots => slots;

    public Inventory(int initialMaxSlots)
    {
        MaxSlots = initialMaxSlots;
    }

    public bool TryAdd(ItemSO item, int quantity = 1)
    {
        if (item.stackable)
        {
            var existing = slots.Find(s => s.item == item);
            if (existing != null)
            {
                existing.quantity += quantity;
                return true;
            }
        }

        if (UsedSlots >= MaxSlots) return false;

        slots.Add(new InventorySlot(item, quantity));
        return true;
    }

    public bool TryRemove(ItemSO item, int quantity = 1)
    {
        var slot = slots.Find(s => s.item == item);
        if (slot == null || slot.quantity < quantity) return false;

        slot.quantity -= quantity;
        if (slot.quantity <= 0)
            slots.Remove(slot);

        return true;
    }

    public bool HasItem(ItemSO item, int quantity = 1)
    {
        var slot = slots.Find(s => s.item == item);
        return slot != null && slot.quantity >= quantity;
    }

    public void ExpandCapacity(int extraSlots)
    {
        MaxSlots += extraSlots;
    }
}
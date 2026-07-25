***English*** | [简体中文](../../zh-CN/script-api/inventory.md)

# InventoryUtil + ItemUtil — Inventory & Items

InventoryUtil queries your backpack. ItemUtil repairs gear, adjusts durability, and destroys items.

## Hand Item

```js
// What's in your hand?
var handItem = InventoryUtil.GetItemIdInHand();   // returns item id, empty hand returns ''
var empty = InventoryUtil.IsHandEmpty();           // nothing in hand?

// Check for a specific item
if (InventoryUtil.HasItemInHand('rifle')) {
    Log.Info('Holding a rifle');
}

// Check by tag/category
if (InventoryUtil.HasItemInHandByTag('weapon')) {
    Log.Info('Holding a weapon');
}
if (InventoryUtil.HasItemInHandByCategory('Weapon')) {
    Log.Info('Holding a weapon-class item');
}
```

| Hand Query                     | Description                    |
|--------------------------------|--------------------------------|
| `GetItemIdInHand()`            | Item id in hand, `''` if empty |
| `IsHandEmpty()`                | Nothing in hand                |
| `HasItemInHand(id)`            | Holding a specific item        |
| `HasItemInHandByTag(tag)`      | Item has a specific tag        |
| `HasItemInHandByCategory(cat)` | Item is of a specific category |

## Inventory Queries

```js
// Slot info
var slotCount = InventoryUtil.GetSlotCount();       // total slots
var emptySlot = InventoryUtil.FindFirstEmptySlot(); // first empty slot, -1 = full

// Item presence
if (InventoryUtil.HasItem('medkit')) {
    Log.Info('Got a medkit');
}

// Item count
var count = InventoryUtil.CountItem('ammo_rifle');  // how many stacks?

// Check any of a list
if (InventoryUtil.HasAnyItem(['medkit', 'bandage', 'splint'])) {
    Log.Info('At least one medical item');
}

// By tag/category
if (InventoryUtil.HasItemByTag('food')) {
    Log.Info('Got food');
}
if (InventoryUtil.HasItemByCategory('Medical')) {
    Log.Info('Got medical supplies');
}
```

| Method                   | Description                 |
|--------------------------|-----------------------------|
| `GetSlotCount()`         | Inventory slot count        |
| `IsSlotEmpty(slot)`      | Slot is empty               |
| `IsSlotOccupied(slot)`   | Slot has something          |
| `GetItemId(slot)`        | Item id in a slot           |
| `FindFirstEmptySlot()`   | First empty slot, -1 = full |
| `HasItem(id)`            | Has item in backpack        |
| `HasAnyItem([ids])`      | Has any of the listed items |
| `CountItem(id)`          | How many stacks of an item  |
| `HasItemByTag(tag)`      | Has item with tag           |
| `HasItemByCategory(cat)` | Has item of category        |

## Deep Search

`Thorough` suffix methods search inside containers (bags within bags).

```js
if (InventoryUtil.HasItemThorough('key_golden')) {
    Log.Info('Found the golden key (maybe inside a container)');
}
```

## Iterating All Items

```js
// List all item ids (array)
var allIds = InventoryUtil.GetAllItemIds();
Log.Info('Inventory: ' + allIds.join(', '));

// Filter by tag
var weapons = InventoryUtil.GetItemIdsByTag('weapon');
Log.Info('Weapons: ' + weapons.join(', '));

// Filter by category
var medicals = InventoryUtil.GetItemIdsByCategory('Medical');
Log.Info('Medical: ' + medicals.join(', '));

// Everything: worn gear + hands + backpack + containers (deduplicated)
var everything = InventoryUtil.GetAllItemIdsAll();

// Worn gear
var worn = InventoryUtil.GetWearableItemIds();
if (InventoryUtil.HasWearableItem()) {
    Log.Info('Wearing equipment');
}
```

## ItemUtil — Equipment Maintenance

```js
// Repair item (durability to full)
ItemUtil.Repair('rifle');

// Set durability (0-1)
ItemUtil.SetCondition('sword', 0.8);

// Set/clear favorite
ItemUtil.SetFavourited('medkit', true);

// Destroy item
ItemUtil.Destroy('rotten_food');
```

| Method                        | Description          |
|-------------------------------|----------------------|
| `Repair(itemId)`              | Full durability to 1 |
| `SetCondition(itemId, float)` | Set durability 0-1   |
| `SetFavourited(itemId, bool)` | Mark as favorite     |
| `Destroy(itemId)`             | Destroy the item     |

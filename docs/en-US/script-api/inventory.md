***English*** | [简体中文](../../zh-CN/script-api/inventory.md)

# Inventory + Item — Inventory & Items

Inventory queries your backpack. Item repairs gear, adjusts durability, and destroys items.

## Hand Item

```js
// What's in your hand?
var handItem = Inventory.GetItemIdInHand();   // returns item id, empty hand returns ''
var empty = Inventory.IsHandEmpty();           // nothing in hand?

// Check for a specific item
if (Inventory.HasItemInHand('rifle')) {
    Log.Info('Holding a rifle');
}

// Check by tag/category
if (Inventory.HasItemInHandByTag('weapon')) {
    Log.Info('Holding a weapon');
}
if (Inventory.HasItemInHandByCategory('Weapon')) {
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
var slotCount = Inventory.GetSlotCount();       // total slots
var emptySlot = Inventory.FindFirstEmptySlot(); // first empty slot, -1 = full

// Item presence
if (Inventory.HasItem('medkit')) {
    Log.Info('Got a medkit');
}

// Item count
var count = Inventory.CountItem('ammo_rifle');  // how many stacks?

// Check any of a list
if (Inventory.HasAnyItem(['medkit', 'bandage', 'splint'])) {
    Log.Info('At least one medical item');
}

// By tag/category
if (Inventory.HasItemByTag('food')) {
    Log.Info('Got food');
}
if (Inventory.HasItemByCategory('Medical')) {
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
if (Inventory.HasItemThorough('key_golden')) {
    Log.Info('Found the golden key (maybe inside a container)');
}
```

## Iterating All Items

```js
// List all item ids (array)
var allIds = Inventory.GetAllItemIds();
Log.Info('Inventory: ' + allIds.join(', '));

// Filter by tag
var weapons = Inventory.GetItemIdsByTag('weapon');
Log.Info('Weapons: ' + weapons.join(', '));

// Filter by category
var medicals = Inventory.GetItemIdsByCategory('Medical');
Log.Info('Medical: ' + medicals.join(', '));

// Everything: worn gear + hands + backpack + containers (deduplicated)
var everything = Inventory.GetAllItemIdsAll();

// Worn gear
var worn = Inventory.GetWearableItemIds();
if (Inventory.HasWearableItem()) {
    Log.Info('Wearing equipment');
}
```

## Item — Equipment Maintenance

```js
// Repair item (durability to full)
Item.Repair('rifle');

// Set durability (0-1)
Item.SetCondition('sword', 0.8);

// Set/clear favorite
Item.SetFavourited('medkit', true);

// Destroy item
Item.Destroy('rotten_food');
```

| Method                        | Description          |
|-------------------------------|----------------------|
| `Repair(itemId)`              | Full durability to 1 |
| `SetCondition(itemId, float)` | Set durability 0-1   |
| `SetFavourited(itemId, bool)` | Mark as favorite     |
| `Destroy(itemId)`             | Destroy the item     |

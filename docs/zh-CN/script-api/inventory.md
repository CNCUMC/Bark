[English](../../en-US/script-api/inventory.md) | ***简体中文***

# Inventory + Item — 背包与物品

Inventory 查背包里的东西，Item 修装备、改耐久、销毁物品。

## 手上物品

```js
// 手上拿的是什么
var handItem = Inventory.GetItemIdInHand();   // 返回物品 id，空手返回 ''
var empty = Inventory.IsHandEmpty();           // 手上没东西？

// 检查手上是特定物品
if (Inventory.HasItemInHand('rifle')) {
    Log.Info('拿着步枪');
}

// 按标签/类别查
if (Inventory.HasItemInHandByTag('weapon')) {
    Log.Info('拿着武器');
}
if (Inventory.HasItemInHandByCategory('Weapon')) {
    Log.Info('拿着武器类物品');
}
```

| 手上查询                       | 说明                       |
|--------------------------------|----------------------------|
| `GetItemIdInHand()`            | 手上物品 id，空手返回 `''` |
| `IsHandEmpty()`                | 手上没东西                 |
| `HasItemInHand(id)`            | 手上是特定物品             |
| `HasItemInHandByTag(tag)`      | 手上物品带特定标签         |
| `HasItemInHandByCategory(cat)` | 手上物品属于某类别         |

## 背包查询

```js
// 槽位信息
var slotCount = Inventory.GetSlotCount();     // 背包槽位总数
var emptySlot = Inventory.FindFirstEmptySlot(); // 第一个空槽，-1 表示满了

// 物品存在性
if (Inventory.HasItem('medkit')) {
    Log.Info('有医疗包');
}

// 物品数量
var count = Inventory.CountItem('ammo_rifle');  // 背包里有几组步枪弹

// 查任意一个
if (Inventory.HasAnyItem(['medkit', 'bandage', 'splint'])) {
    Log.Info('至少有一样医疗用品');
}

// 按标签/类别查
if (Inventory.HasItemByTag('food')) {
    Log.Info('有食物');
}
if (Inventory.HasItemByCategory('Medical')) {
    Log.Info('有医疗用品');
}
```

| 方法                     | 说明                  |
|--------------------------|-----------------------|
| `GetSlotCount()`         | 背包槽位数            |
| `IsSlotEmpty(slot)`      | 某槽位是否为空        |
| `IsSlotOccupied(slot)`   | 某槽位有东西          |
| `GetItemId(slot)`        | 某槽位的物品 id       |
| `FindFirstEmptySlot()`   | 第一个空槽，-1 = 满了 |
| `HasItem(id)`            | 背包有某物品          |
| `HasAnyItem([ids])`      | 有其中任意一个        |
| `CountItem(id)`          | 某物品有几组          |
| `HasItemByTag(tag)`      | 有带某标签的物品      |
| `HasItemByCategory(cat)` | 有某类别的物品        |

## 深度搜索

`Thorough` 后缀的方法会搜索容器内的物品（包里套包的情况）。

```js
if (Inventory.HasItemThorough('key_golden')) {
    Log.Info('找到金钥匙了（可能在某个包里）');
}
```

## 遍历全部物品

```js
// 列出所有物品 id（数组）
var allIds = Inventory.GetAllItemIds();
Log.Info('背包物品: ' + allIds.join(', '));

// 按标签筛选
var weapons = Inventory.GetItemIdsByTag('weapon');
Log.Info('武器: ' + weapons.join(', '));

// 按类别筛选
var medicals = Inventory.GetItemIdsByCategory('Medical');
Log.Info('医疗: ' + medicals.join(', '));

// 包括穿戴装备 + 手拿 + 背包 + 容器内（去重）
var everything = Inventory.GetAllItemIdsAll();

// 穿戴装备
var worn = Inventory.GetWearableItemIds();
if (Inventory.HasWearableItem()) {
    Log.Info('穿着装备');
}
```

## Item — 装备维护

```js
// 修理物品（耐久回满）
Item.Repair('rifle');

// 按指定量修复（累加到当前耐久）
Item.Repair('rifle', 0.3);    // 修复 30%

// 扣耐久（负值降低耐久）
Item.Repair('battery', -0.1); // 放电 10%

// 直接设置耐久（0-1）
Item.SetCondition('sword', 0.8);

// 标记/取消收藏
Item.SetFavourited('medkit', true);

// 销毁物品
Item.Destroy('rotten_food');
```

| 方法                          | 说明                                        |
|-------------------------------|---------------------------------------------|
| `Repair(itemId, amount? = 1)` | 按 amount 修复（默认 1 = 回满），负值扣耐久 |
| `SetCondition(itemId, float)` | 直接设置耐久 0-1                            |
| `SetFavourited(itemId, bool)` | 标记收藏                                    |
| `Destroy(itemId)`             | 销毁物品                                    |

[English](../../en-US/script-mod/recipe.md) | ***简体中文***

# 自定义合成表

在脚本模组目录下创建 `Recipe/` 文件夹，放入 JSON 文件即可注册自定义合成表。Bark 会在脚本模组加载时自动扫描并注册到
CUCoreLib 的
`RecipeRegistry`。

## 目录结构

```
ScriptMod/Mods/
  MyMod/
    Recipe/
      bandage123.json       ← 每个 JSON 一个合成表配方
      antidote.json
```

## JSON 格式

一个完整的合成表 JSON：

```json
{
  "id": "my_custom_item",
  "int": 5,
  "category": "Medicine",
  "amount": 3,
  "is_liquid": false,
  "result_condition": 1.0,
  "is_repair": false,
  "dont_drain_result_liquid": false,
  "items": [
    {
      "specific": false,
      "specific_id": "",
      "quality": "foliage",
      "quality_condition": 1.0,
      "minimum_condition": 0.5,
      "destroy_item": true,
      "is_liquid": false,
      "ignored_id": ""
    },
    {
      "specific": true,
      "specific_id": "bandage",
      "minimum_condition": 0.9,
      "destroy_item": true
    }
  ]
}
```

## 字段说明

### 配方产物

| 字段                       | 类型   | 默认值        | 说明                                                            |
|----------------------------|--------|---------------|-----------------------------------------------------------------|
| `id`                       | string | (必填)        | 产物物品 ID                                                     |
| `int`                      | int    | 0             | 制作所需智力                                                    |
| `category`                 | string | `"Materials"` | 配方分类：`Materials`, `Tools`, `Medicine`, `Utilities`, `Food` |
| `amount`                   | int    | 1             | 每次合成产物数量                                                |
| `is_liquid`                | bool   | false         | 产物是否为液体                                                  |
| `result_condition`         | float  | 1.0           | 产物默认耐久度（1.0 = 100%）                                    |
| `is_repair`                | bool   | false         | 是否为修复配方（修复物品耐久）                                  |
| `dont_drain_result_liquid` | bool   | false         | 不消耗原料液体                                                  |
| `replace_original_recipe`  | bool   | false         | 是否替换原版同名合成表                                          |

### 材料 (`items[i]`)

| 字段                | 类型   | 默认值 | 说明                                                      |
|---------------------|--------|--------|-----------------------------------------------------------|
| `specific`          | bool   | false  | `true` = 精确匹配物品 ID，`false` = 按 `quality` 特性匹配 |
| `specific_id`       | string | `""`   | `specific=true` 时的目标物品 ID                           |
| `quality`           | string | `""`   | 制作特性关键字，拥有该特性的物品都可作为材料              |
| `quality_condition` | float  | 1.0    | 特性消耗量                                                |
| `minimum_condition` | float  | 0.9    | 材料物品最小耐久度                                        |
| `destroy_item`      | bool   | true   | 合成后是否消耗材料                                        |
| `is_liquid`         | bool   | false  | 材料是否为液体                                            |
| `ignored_id`        | string | `""`   | 排除的特定物品 ID                                         |

### 可用制作特性

`quality` 字段支持以下特性，与游戏原生合成系统一致：

`foliage` `cutting` `rippable` `dressing` `disinfectant` `water`
`blood` `nails` `fat` `opiate` `heatsource` `firestarter` `flammable`
`flour` `produce` `condiment` `hammering`

## C# API

`RecipeLoader` 提供以下公共接口：

```csharp
// 获取已加载的配方列表：Dictionary<modId, List<RecipeEntry>>
var entries = RecipeLoader.LoadedRecipes;

foreach (var entry in entries["my_mod"])
{
    Console.WriteLine($"{entry.Id} from {entry.FileName}");
}
```

`RecipeEntry` 字段：

- `Id`：配方产物 ID
- `FileName`：来源 JSON 文件名（如 `"bandage123.json"`）

## 热重载

开发时无需重启游戏，指令 `script reload`/`rs` 会重载合成表定义。`RecipeLoader` 会先清除该脚本模组的旧配方再注册新的，避免新旧配方共存。

## 与物品系统联动

自定义合成表的 `id` 可以是原版物品，也可以是 [`Assets/Item/` 目录下定义的自定义物品](item.md)。两者配合可构建完整的自定义物品生态系统。

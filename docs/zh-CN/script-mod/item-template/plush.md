[English](../../../en-US/script-mod/item-template/plush.md) | ***简体中文***

← [返回模板总览](index.md)

# 玩偶模板

`"type": "plush"` — 会吱吱叫的玩具玩偶。模板预设 `plushie` 预制体及其全部默认属性（重量、价值、识别、标签等），
使用时会发出吱吱声。**声音属性复用 Bark 的 Audio 属性**：可通过 `squeak_sound` 配置模组自定义吱吱音效，否则用游戏默认音效。

## 参数速览

```json
{
  "template": {
    "type": "plush"
  }
}
```

仅此一行即可获得完整的 plushie 等价物。

## 玩偶专属参数（template 内）

```json
{
  "template": {
    "type": "plush",
    "plush": true,
    "squeak_sound": "Assets/Audio/plush_squeak.wav"
  }
}
```

| 参数           | 类型   | 默认值 | 说明                                                                                                      |
|----------------|--------|--------|-----------------------------------------------------------------------------------------------------------|
| `plush`        | bool   | `true` | 内部标记，**不要删除**                                                                                    |
| `squeak_sound` | string | `""`   | 自定义吱吱音效（**Bark Audio 属性**）。相对模组目录，纯文件名自动补全 `Assets/Audio/`。空则用游戏默认音效 |

配置了 `squeak_sound` 时，Bark 用 `AudioManager` 加载并播放自定义音效（支持 `.wav`/`.mp3`/`.aif` 等，同其他音效约定），
替代游戏默认的 `PlushScript` 吱吱声。

## 可覆盖的通用字段（顶层）

玩偶模板预设了以下 `ItemInfo` 字段，可在物品 JSON 顶层直接覆盖：

```json
{
  "category": "utility",
  "weight": 0.15,
  "value": 5,
  "recognition": 6,
  "tags": "belttool",
  "destroy_at_zero_condition": true,
  "sprite": {
    "slot_rotation": 0
  }
}
```

| 字段                          | 类型   | 默认值     | 说明                  |
|-------------------------------|--------|------------|-----------------------|
| `category`                    | string | `utility`  | 分类                  |
| `weight`                      | float  | `0.15`     | 物品重量              |
| `value`                       | int    | `5`        | 物品价值              |
| `recognition`                 | int    | `6`        | 识别等级              |
| `tags`                        | string | `belttool` | 标签，逗号分隔        |
| `destroy_at_zero_condition`   | bool   | `true`     | 耐久归零时销毁        |
| `sprite.slot_rotation`        | float  | `0`        | 物品栏旋转角度（度）  |

## 使用示例

### 最简单的玩偶

```json
{
  "full_name": "小熊玩偶",
  "description": "一只软乎乎的棕色小熊，捏一下会吱吱叫。",
  "category": "玩具",
  "template": { "type": "plush" }
}
```

使用时会播放游戏默认吱吱声。

### 自定义吱吱音效

```json
{
  "full_name": "小黄鸭",
  "description": "一捏就会嘎嘎叫的橡胶鸭。",
  "category": "玩具",
  "weight": 0.1,
  "template": {
    "type": "plush",
    "squeak_sound": "duck_squeak.wav"
  }
}
```

把 `duck_squeak.wav` 放到模组的 `Assets/Audio/` 下。使用时 Bark 用 `AudioManager` 播放它，替代默认吱吱声。

## 脚本集成

玩偶的吱吱声由模板自动处理，无需脚本即可工作。如需在使用时添加额外逻辑（如 buff、触发其他事件），可监听全局事件
[`onPlushSqueak`](../../script-events.md) 或定义 `use` 脚本。

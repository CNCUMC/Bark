using System;
using Bark.Audio;
using Bark.Items.Templates;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

// Partial：枪械实例初始化、UI 守卫、模板创建、枪管偏移
public static partial class GunRuntimeManager
{
    // ============================================================
    // OnGunStartPostfix：模板 GunData 写入 GunScript 的主编排方法
    // ============================================================
    // 职责在 GunScript.Start 之后，将模板中的弹道 / 音效 / 供弹 / 精灵等数据写入
    // GunScript 实例，确保每把模板枪械的行为完全由 JSON 模板数据控制。
    //
    // GunScript.Fire 内部使用 conditionLossPerShot * 0.01f 计算耐久损耗。
    // 模板的 ConditionLossPerShot 是直接的耐久扣除量，因此需要反向换算：
    //   conditionLossPerShot = ConditionLossPerShot / 0.01f

    private static void OnGunStartPostfix(GunScript __instance)
    {
        var item = __instance.GetComponent<Item>();
        if (item == null) return;
        if (!GunTemplate.IsGun(item.id)) return;

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData == null) return;

        ApplyFiringConfig(__instance, gunData);
        MapAmmoType(__instance, gunData);
        ApplyBallisticsData(__instance, gunData);
        ApplySpriteFields(__instance, item);
        ApplySoundFields(__instance, gunData);
        InitMagazineState(__instance, item, gunData);
        ApplyChamberInitState(__instance, gunData);
        EnsureMuzzleParticle(__instance);
    }

    // 射击模式 + 供弹方式
    private static void ApplyFiringConfig(GunScript gun, GunData gunData)
    {
        gun.firingMode = gunData.FiringMode switch
        {
            "auto" => GunScript.FiringMode.Auto,
            "pump" => GunScript.FiringMode.Pump,
            _ => GunScript.FiringMode.SemiAuto
        };

        // 供弹方式：优先使用新字段 feed_type，兼容旧 Direct 布尔字段
        gun.feedType = gunData.FeedType switch
        {
            "direct" => GunScript.FeedType.Direct,
            "revolver" => (GunScript.FeedType)2,
            _ => gunData.Direct ? GunScript.FeedType.Direct : GunScript.FeedType.Mag
        };
    }

    // 弹药类型枚举：从模板 string 标签映射到游戏枚举。
    // 尝试 string → enum 直接映射（如 "Rifle" "Shotgun" "Pistol"），
    // 失败时按口径前缀映射（7_62*/5_56* → Rifle, 12gauge → Shotgun）。
    private static void MapAmmoType(GunScript gun, GunData gunData)
    {
        if (Enum.TryParse<GunScript.AmmoType>(gunData.AmmoType, true, out var parsedAmmoType))
        {
            gun.ammoType = parsedAmmoType;
        }
        else if (gunData.AmmoType.StartsWith("12"))
        {
            gun.ammoType = GunScript.AmmoType.Shotgun;
        }
        else if (gunData.AmmoType.StartsWith("7_") || gunData.AmmoType.StartsWith("5_"))
        {
            gun.ammoType = GunScript.AmmoType.Rifle;
        }
        else
        {
            gun.ammoType = GunScript.AmmoType.Pistol;
        }
    }

    // 弹道 / 伤害 / 音效数值 + 管容量（直装枪）
    private static void ApplyBallisticsData(GunScript gun, GunData gunData)
    {
        gun.knockBack = gunData.Knockback;
        gun.structureDamage = gunData.StructureDamage;
        gun.animalDamage = gunData.AnimalDamage;
        gun.loudness = gunData.Loudness;
        gun.shotsPerFire = gunData.ShotsPerFire;
        gun.verticalSpread = gunData.VerticalSpread;

        // 耐久损耗（反向换算以适配 GunScript.Fire 的 *0.01f 公式）
        gun.conditionLossPerShot = gunData.ConditionLossPerShot / 0.01f;

        // 直装枪械的管容量
        if (gunData is { Direct: true, Capacity: > 0 })
        {
            gun.magCapacity = gunData.Capacity;
        }
    }

    // 精灵字段：normalSprite / rackedSprite / ...NoMag 四字段填充 + barrel 创建 + 兜底精灵
    private static void ApplySpriteFields(GunScript gun, Item item)
    {
        gun.racked = true;

        if (gun.barrel == null)
        {
            var (bx, by) = GetBarrelOffset(gun);
            CreateBarrelChild(gun.gameObject, gun, bx, by);
        }

        // GunScript.Update() 每帧用这四字段覆盖 SpriteRenderer.sprite，
        // 模板物品使用通用预制体，需要从当前的 sprite 拷贝。
        var sr = gun.GetComponent<SpriteRenderer>();
        if (sr?.sprite != null)
        {
            gun.normalSprite = sr.sprite;
            gun.rackedSprite = sr.sprite;
            gun.normalSpriteNoMag = sr.sprite;
            gun.rackedSpriteNoMag = sr.sprite;
        }
        else
        {
            // SpriteRenderer 无可渲染精灵时，从枪械预制体获取默认精灵作为回退。
            var fallbackSprite = LoadDefaultGunSprite(gun.ammoType);
            if (fallbackSprite != null)
            {
                gun.normalSprite = fallbackSprite;
                gun.rackedSprite = fallbackSprite;
                gun.normalSpriteNoMag = fallbackSprite;
                gun.rackedSpriteNoMag = fallbackSprite;
            }
        }

        // 最终兜底：使用 Unity 内置 4x4 白色纹理创建精灵，防止空引用崩溃。
        if (gun.normalSprite == null)
        {
            LogUtil.Warning("gun_runtime.gun_init_no_sprite", item.id);
            var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false) { hideFlags = HideFlags.DontSave };
            var pixels = new Color32[16];
            for (var i = 0; i < 16; i++) pixels[i] = new Color32(64, 64, 64, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            gun.normalSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            gun.rackedSprite = gun.normalSprite;
            gun.normalSpriteNoMag = gun.normalSprite;
            gun.rackedSpriteNoMag = gun.normalSprite;
        }
    }

    // 音效字段：fireSound / customRack / customUnrack
    // 枪声：音效档案优先 → 模板路径 → ammoType 默认回退
    private static void ApplySoundFields(GunScript gun, GunData gunData)
    {
        // fireSound
        if (gunData.SoundProfile?.Fire is { Count: > 0 })
        {
            // profile 接管开火音效 → 设为静音，实际播放由 OnFirePostfix 处理
            gun.fireSound = GetSilentClip();
        }
        else if (!string.IsNullOrEmpty(gunData.FireSound))
        {
            gun.fireSound = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
        }
        if (gun.fireSound == null)
        {
            gun.fireSound = gun.ammoType switch
            {
                GunScript.AmmoType.Shotgun => Resources.Load<AudioClip>("sounds/shotgunshot"),
                GunScript.AmmoType.Rifle => Resources.Load<AudioClip>("sounds/rifleshot")
                    ?? Resources.Load<AudioClip>("sounds/shotgunshot"),
                _ => Resources.Load<AudioClip>("sounds/pistolshot"),
            };
        }

        // customRack：音效档案优先 → 模板路径 → 游戏默认 "gunrack"
        if (gunData.SoundProfile?.Rack is { Count: > 0 })
        {
            gun.customRack = gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Rack);
        }
        else if (!string.IsNullOrEmpty(gunData.RackSound))
        {
            gun.customRack = AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);
        }

        // customUnrack：音效档案优先 → 模板路径 → customRack 兜底
        if (gunData.SoundProfile?.Unrack is { Count: > 0 })
        {
            gun.customUnrack = gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Unrack);
        }
        else if (!string.IsNullOrEmpty(gunData.UnrackSound))
        {
            gun.customUnrack = AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
        }
        else if (gun.customRack != null)
        {
            gun.customUnrack = gun.customRack;
        }
    }

    // 弹匣状态初始化：弹匣供弹 / 转轮 / 直装三分支
    private static void InitMagazineState(GunScript gun, Item item, GunData gunData)
    {
        if (!gunData.Direct && gunData.FeedType != "revolver")
        {
            // 弹匣供弹枪：出厂预装满弹匣
            var cap = gunData.Capacity > 0 ? gunData.Capacity : DefaultDirectCapacity;
            if (gunData.Capacity <= 0)
                LogUtil.Warning("gun_runtime.gun_init_capacity_zero", item.id, gunData.FeedType, DefaultDirectCapacity);

            gun.magCapacity = cap;
            gun.hasMag = true;
            gun.roundsInMag = cap;

            var state = GunMagTracker.GetOrCreate(item);
            state.RoundsInMag = cap;
            // 记录对应的弹匣物品 ID，确保卸弹时能正确生成弹匣物品。
            // 按 mag_type → ammo_type 顺序查找已注册的弹匣模板。
            var defaultMagIds = MagTemplate.FindMagsByType(gunData.MagType);
            if (defaultMagIds.Count == 0)
            {
                LogUtil.Warning("gun_runtime.gun_init_no_mag_by_type", item.id, gunData.MagType, gunData.AmmoType);
                defaultMagIds = MagTemplate.FindMagsByAmmoType(gunData.AmmoType);
            }
            if (defaultMagIds.Count > 0)
            {
                var foundMagData = MagTemplate.GetMagData(defaultMagIds[0]);
                if (foundMagData != null && foundMagData.MagType != gunData.MagType)
                    LogUtil.Warning("gun_runtime.gun_init_mag_type_mismatch", item.id, gunData.MagType,
                        defaultMagIds[0], foundMagData.MagType, gunData.MagType);
            }
            state.MagItemId = defaultMagIds.Count > 0 ? defaultMagIds[0] : null;
        }
        else if (gunData.FeedType == "revolver")
        {
            // 转轮枪：弹容量用 Capacity，出厂满弹
            var cap = gunData.Capacity > 0 ? gunData.Capacity : 6;
            gun.magCapacity = cap;
            gun.hasMag = false;
            gun.roundsInMag = cap;
        }
        else
        {
            // 直装枪：仅设置管容量，出厂空管
            var cap = gunData.Capacity > 0 ? gunData.Capacity : DefaultDirectCapacity;
            gun.magCapacity = cap;
        }
    }

    // 膛内状态初始化
    private static void ApplyChamberInitState(GunScript gun, GunData gunData)
    {
        // 膛内状态：GunScript.RoundInChamber 只有 Round / None 两个值
        gun.roundInChamber = gunData.StartChambered
            ? GunScript.RoundInChamber.Round
            : GunScript.RoundInChamber.None;

        // 保险状态：按模板配置，默认 false 允许直接射击
        gun.safe = gunData.StartSafe;

        // 拉膛/退膛状态标记
        gun.lastRacked = false;

        // 半自动/全自动枪机循环延迟（pump 模式忽略此字段）
        gun.desiredGasTime = gunData.DesiredGasTime;
    }

    // 枪口粒子：从游戏预制体克隆，防止空 ParticleSystem 渲染紫色方块
    private static void EnsureMuzzleParticle(GunScript gun)
    {
        if (gun.muzzleParticle == null)
        {
            gun.muzzleParticle = CloneMuzzleFromPrefab(gun.ammoType, gun.transform);
        }
    }

    // 从游戏 pistol/shotgun 预制体克隆枪口粒子。
    // 参考 NewGun.CreateMuzzle：无弹药类型的完整映射。
    private static ParticleSystem CloneMuzzleFromPrefab(GunScript.AmmoType ammoType, Transform parent)
    {
        try
        {
            var prefabName = ammoType switch
            {
                GunScript.AmmoType.Shotgun => "shotgun",
                GunScript.AmmoType.Rifle => "rifle",
                _ => "pistol",
            };
            var prefab = Resources.Load(prefabName) as GameObject;
            if (prefab == null) goto fallback;

            var childIdx = ammoType == GunScript.AmmoType.Shotgun ? 1 : 2;
            if (prefab.transform.childCount <= childIdx) goto fallback;

            var src = prefab.transform.GetChild(childIdx).gameObject;
            var clone = Object.Instantiate(src, parent, false);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;

            var ps = clone.GetComponent<ParticleSystem>();
            if (ps != null) return ps;
        }
        catch
        {
            // ignored
        }

        fallback:
        // 回退：创建空占位防止 NRE，禁用发射避免紫色方块
        var fallbackPs = parent.gameObject.AddComponent<ParticleSystem>();
        fallbackPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var emission = fallbackPs.emission;
        emission.enabled = false;
        return fallbackPs;
    }

    // 从枪械预制体获取默认精灵，用于 GunScript 精灵字段的回退。
    // 当 CCL 自定义 PNG 精灵尚未就绪时，防止 HandleGunMenu 空引用崩溃。
    private static Sprite? LoadDefaultGunSprite(GunScript.AmmoType ammoType)
    {
        var prefabName = ammoType switch
        {
            GunScript.AmmoType.Shotgun => "shotgun",
            GunScript.AmmoType.Rifle => "rifle",
            _ => "pistol",
        };
        var prefab = Resources.Load(prefabName) as GameObject;
        return prefab?.GetComponent<SpriteRenderer>()?.sprite;
    }

    // ============================================================
    // HandleGunMenu Prefix：守卫自定义枪械的所有潜在 null 字段
    // ============================================================
    //
    // HandleGunMenu 在 PlayerCamera.LateUpdate 中每帧调用，直接读取：
    //   a) PlayerCamera 的 UI 精灵字段（gunNormalSprite / gunRackedSprite 等）
    //   b) GunScript 的非序列化字段（barrel / roundInChamber 等）
    //
    // 自定义枪械通过 geofruit 预制体 + 运行时 AddComponent<GunScript>() 创建，
    // OnGunStartPostfix 已补齐 GunScript 自身的字段，但 PlayerCamera 的 UI 字段
    // 在首次持握自定义枪械时可能尚未从 GunScript 同步，导致 NullReferenceException。
    //
    // 此 Prefix 在原始 HandleGunMenu 方法体执行前统一检查所有潜在 null 路径，
    // 发现 null 则记录日志并跳过本帧（返回 false），避免整个游戏崩溃。

    private static bool OnHandleGunMenuPrefix(PlayerCamera __instance)
    {
        if (__instance == null) return false;

        try
        {
            var pc = Traverse.Create(__instance);

            // 1. 检查 body
            var body = pc.Field("body").GetValue();
            if (body == null) return false;

            var bodyTr = Traverse.Create(body);

            // 2. 检查是否持枪（复制 HandleGunMenu:2461 的条件链）
            var handSlot = bodyTr.Field("handSlot").GetValue<int>();
            if (!bodyTr.Method("HoldingItem", handSlot).GetValue<bool>())
                return true;

            var item = bodyTr.Method("GetItem", handSlot).GetValue();
            if (item == null) return false;

            // 3. 检查 Stats.HasTag("gun")
            var stats = Traverse.Create(item).Property("Stats").GetValue();
            if (stats == null) return false;
            if (!Traverse.Create(stats).Method("HasTag", "gun").GetValue<bool>())
                return true;

            // 4. 检查 GetComponent<GunScript>()
            var gun = ((Component)item).GetComponent<GunScript>();
            if (gun == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_gunscript");
                return false;
            }

            // 5. GunScript.barrel — HandleGunMenu:2470 访问 barrel.transform.position
            if (gun.barrel == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_barrel");
                return false;
            }

            // 6. PlayerCamera UI 字段（HandleGunMenu:2465-2469）
            return CheckPlayerCameraUIFields(pc);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("gun_runtime.handle_gun_menu_prefix_error", ex.GetType().Name, ex.Message);
            return false;
        }
    }

    // 检查 HandleGunMenu 中使用的 PlayerCamera UI 字段。
    // 这些是 PlayerCamera 的序列化字段，正常应由场景预制体赋值。
    // 如果为 null 说明 UI 未完全初始化（自定义枪械首次持握时的时序问题）。
    // 返回 true 表示全部有效，false 表示有 null 字段。
    private static bool CheckPlayerCameraUIFields(Traverse pc)
    {
        // HandleGunMenu:2465 gunRackImage.sprite = component.racked ? gunRackedSprite : gunNormalSprite
        if (pc.Field("gunRackImage").GetValue() == null) { Log("gunRackImage"); return false; }
        if (pc.Field("gunRackedSprite").GetValue() == null) { Log("gunRackedSprite"); return false; }
        if (pc.Field("gunNormalSprite").GetValue() == null) { Log("gunNormalSprite"); return false; }

        // HandleGunMenu:2466 gunMagButton.interactable = ...
        if (pc.Field("gunMagButton").GetValue() == null) { Log("gunMagButton"); return false; }

        // HandleGunMenu:2467 gunSafeImage.sprite = component.safe ? gunSafeSprite : gunUnsafeSprite
        if (pc.Field("gunSafeImage").GetValue() == null) { Log("gunSafeImage"); return false; }
        if (pc.Field("gunSafeSprite").GetValue() == null) { Log("gunSafeSprite"); return false; }
        if (pc.Field("gunUnsafeSprite").GetValue() == null) { Log("gunUnsafeSprite"); return false; }

        // HandleGunMenu:2468 gunBulletImage.sprite = gunBulletSprites[...]
        if (pc.Field("gunBulletImage").GetValue() == null) { Log("gunBulletImage"); return false; }
        if (pc.Field("gunBulletSprites").GetValue() == null) { Log("gunBulletSprites"); return false; }

        // HandleGunMenu:2469 gunCrosshair.gameObject.SetActive(!component.safe)
        if (pc.Field("gunCrosshair").GetValue() != null) return true;
        Log("gunCrosshair"); return false;

        static void Log(string field) =>
            LogUtil.Warning("gun_runtime.handle_gun_menu_null_pc_field", field);
    }

    // ============================================================
    // OnTemplateCreatedPostfix：模板创建时补加组件
    // ============================================================
    // CCL 创建模板物品后，按 Bark 模板类型动态补加 GunScript / AmmoScript。
    // CCL 的 CreateTemplate 已为 Wearable/BatteryItem 做了同样的 AddComponent 模式。
    // 模板对象 SetActive(false) 缓存，后续 InstantiateReturn 克隆自动继承这些组件。
    //
    // 只影响 Bark.RegisterFromMod 注册的模板物品（GunTemplate/MagTemplate/AmmunitionTemplate），
    // 不会触及原版物品的初始化流程。
    private static void OnTemplateCreatedPostfix(GameObject __result, string id)
    {
        if (__result == null)
            return;

        // 从模板身上的 Item 组件读取 CCL 已标准化后的物品 ID，
        // 优先使用组件上的 id（CreateTemplate.component1.id）确保与模板缓存精确一致。
        var item = __result.GetComponent<Item>();
        var itemId = item is not null ? item.id : id;
        if (string.IsNullOrEmpty(itemId))
            return;

        // 枪械模板：补加 GunScript + 枪管子对象
        // 原版枪械预制体自带名为 "barrel" 的子 Transform，GunScript.barrel 指向它。
        // 通用预制体（geofruit/rifle）没有此子对象，导致：
        //   1. barrel 为 null → HandleGunMenu NRE
        //   2. 用 transform 替代 → 子弹从枪中心射出，偏移异常
        // 在模板身上创建 barrel 子对象后，Unity 序列化系统会在 clone 时
        // 自动重映射引用，clone 的 GunScript.barrel 正确指向 clone 的 barrel 子对象。
        if (GunTemplate.IsGun(itemId) && __result.GetComponent<GunScript>() == null)
        {
            var gun = __result.AddComponent<GunScript>();
            var (bx, by) = GetBarrelOffset(gun);
            CreateBarrelChild(__result, gun, bx, by);
        }

        // 弹匣模板：补加 AmmoScript（itemType=Magazine）
        if (MagTemplate.IsMag(itemId) && __result.GetComponent<AmmoScript>() == null)
        {
            var magData = MagTemplate.GetMagData(itemId);
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Magazine;
            if (magData != null)
            {
                am.maxRounds = magData.Capacity;
                am.rounds = magData.Capacity; // 出厂满弹匣
                am.ammoType = Enum.TryParse<GunScript.AmmoType>(magData.AmmoType, true, out var parsedMagAmmo)
                    ? parsedMagAmmo
                    : GunScript.AmmoType.Pistol;
            }
        }

        // 弹药模板：补加 AmmoScript（itemType=Round）
        if (AmmunitionTemplate.IsAmmo(itemId) && __result.GetComponent<AmmoScript>() == null)
        {
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Round;
        }
    }

    // 为枪械创建 barrel 子 Transform 作为枪口引用点。
    // 原版枪械预制体自带此子对象，GunScript.barrel 指向它；
    // GunScript.Fire 用 barrel.position 作为子弹生成位置，用 transform.right 作为射击方向。
    // HandleGunMenu 用 barrel.position + transform.right * distance 放置准星。
    // 两者在同一射线上，barrel 只影响生成点/准星位置，不影响方向。
    // geofruit/rifle 通用预制体无此子对象，使用时 barrel=transform 会导致：
    //   - 子弹从枪中心而非枪口射出 → 弹道偏移
    //   - 如果 barrel 不是序列化字段 → clone 后为 null
    //
    // 偏移量由枪械模板的 barrel_offset.{x,y} 控制，不同枪型可自行配置。
    private static void CreateBarrelChild(GameObject gunObj, GunScript gun, float offsetX, float offsetY)
    {
        // 查找已有的 barrel 子对象（部分预制体可能自带）
        var existing = gunObj.transform.Find("barrel");
        if (existing != null)
        {
            gun.barrel = existing;
            return;
        }

        var barrelChild = new GameObject("barrel");
        barrelChild.transform.SetParent(gunObj.transform);

        barrelChild.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
        barrelChild.transform.localRotation = Quaternion.identity;
        barrelChild.transform.localScale = Vector3.one;

        gun.barrel = barrelChild.transform;
    }

    // 从 Item 组件获取物品 ID，查找 GunData 中的 barrel_offset，
    // 返回枪口偏移量。若无法获取则返回默认步枪偏移 (0.5, 0)。
    private static (float x, float y) GetBarrelOffset(Component? component)
    {
        var item = component?.GetComponent<Item>();
        if (item is null) return (0.5f, 0f);

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData is null) return (0.5f, 0f);

        return (gunData.BarrelOffsetX, gunData.BarrelOffsetY);
    }
}

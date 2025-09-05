

namespace Dynastio.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class DynastioItemsRoot
    {
        [JsonPropertyName("items")]
        public Dictionary<string, DynastioItem> Items { get; set; } = new();
    }

    public sealed class DynastioItem
    {
        [JsonPropertyName("item_type")]
        public string? ItemType { get; set; }

        [JsonPropertyName("durability")]
        public int? Durability { get; set; }

        [JsonPropertyName("grade")]
        public int? Grade { get; set; }

        [JsonPropertyName("is_tool")]
        public bool? IsTool { get; set; }

        [JsonPropertyName("is_potion")]
        public bool? IsPotion { get; set; }

        [JsonPropertyName("personal")]
        public bool? Personal { get; set; }

        [JsonPropertyName("repair_price")]
        public int? RepairPrice { get; set; }

        [JsonPropertyName("stack")]
        public int? Stack { get; set; }

        // Some weights come as numbers, some as strings ("0.6")
        [JsonPropertyName("weight")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Weight { get; set; }

        // Present on some items
        [JsonPropertyName("custom_effect_type")]
        public string? CustomEffectType { get; set; }

        // Present on some items
        [JsonPropertyName("tag_mul")]
        public Dictionary<string, double>? TagMul { get; set; }

        [JsonPropertyName("item_action")]
        public DynastioItemAction? ItemAction { get; set; }
    }

    public sealed class DynastioItemAction
    {
        // Generic discriminator sometimes present
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        // Common combat/action numbers (some appear as quoted strings)
        [JsonPropertyName("ai_distance_mul")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? AiDistanceMul { get; set; }

        [JsonPropertyName("ai_use_predicted_transform")]
        public bool? AiUsePredictedTransform { get; set; }

        [JsonPropertyName("allow_redirect")]
        public bool? AllowRedirect { get; set; }

        [JsonPropertyName("redirect")]
        public bool? Redirect { get; set; }

        [JsonPropertyName("animation_length")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? AnimationLength { get; set; }

        [JsonPropertyName("attack_angle")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? AttackAngle { get; set; }

        [JsonPropertyName("block_mirror")]
        public bool? BlockMirror { get; set; }

        [JsonPropertyName("bullet_entity_type")]
        public string? BulletEntityType { get; set; }

        [JsonPropertyName("bullet_item_type")]
        public string? BulletItemType { get; set; }

        [JsonPropertyName("bullet_offset")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? BulletOffset { get; set; }

        [JsonPropertyName("bullet_speed")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? BulletSpeed { get; set; }

        [JsonPropertyName("critical_chance")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? CriticalChance { get; set; }

        [JsonPropertyName("damage")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Damage { get; set; }

        [JsonPropertyName("distance")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Distance { get; set; }

        [JsonPropertyName("dodge")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Dodge { get; set; }

        [JsonPropertyName("hand_type")]
        public string? HandType { get; set; }

        [JsonPropertyName("health")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Health { get; set; }

        // Some items use ignore_shield true/false, some use ignore_shield_chance as number/bool
        [JsonPropertyName("ignore_shield")]
        public bool? IgnoreShield { get; set; }

        [JsonPropertyName("ignore_shield_chance")]
        [JsonConverter(typeof(DynastioItemDoubleOrBoolAsDoubleConverter))]
        public double? IgnoreShieldChance { get; set; }

        [JsonPropertyName("ignore_types")]
        public List<string>? IgnoreTypes { get; set; }

        [JsonPropertyName("friend_ignore_tags")]
        public List<string>? FriendIgnoreTags { get; set; }

        [JsonPropertyName("ignore_tags")]
        public List<string>? IgnoreTags { get; set; }

        [JsonPropertyName("passthrough_tags")]
        public List<string>? PassthroughTags { get; set; }

        [JsonPropertyName("joint_power")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? JointPower { get; set; }

        [JsonPropertyName("max_joint_acceleration")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? MaxJointAcceleration { get; set; }

        [JsonPropertyName("min_joint_distance")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? MinJointDistance { get; set; }

        [JsonPropertyName("break_joint_distance")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? BreakJointDistance { get; set; }

        [JsonPropertyName("make_fire")]
        public bool? MakeFire { get; set; }

        [JsonPropertyName("player")]
        public int? Player { get; set; }

        [JsonPropertyName("post_timeout")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? PostTimeout { get; set; }

        [JsonPropertyName("pre_timeout")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? PreTimeout { get; set; }

        [JsonPropertyName("power")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Power { get; set; }

        [JsonPropertyName("push")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Push { get; set; }

        [JsonPropertyName("stamina")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Stamina { get; set; }

        [JsonPropertyName("stamina_pulse_timeout")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? StaminaPulseTimeout { get; set; }

        [JsonPropertyName("stamina_pulse_value")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? StaminaPulseValue { get; set; }

        [JsonPropertyName("tag")]
        public string? Tag { get; set; }

        [JsonPropertyName("weight")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Weight { get; set; }

        // Nested objects used by various action types
        [JsonPropertyName("armor")]
        public DynastioItemArmor? Armor { get; set; }

        [JsonPropertyName("gathering")]
        public DynastioItemGathering? Gathering { get; set; }


        // Action payloads vary a lot: could be "heal", "buff", "random_buff", "trap", etc.
        // Some actions also have a nested "action" object with chance/args.
        [JsonPropertyName("action")]
        public JsonElement Action { get; set; }

        // Args themselves vary widely; keep them flexible
        [JsonPropertyName("args")]
        public JsonElement? Args { get; set; }

        // For spawn/open kinds
        [JsonPropertyName("entity_type")]
        public string? EntityType { get; set; }

        [JsonPropertyName("offset")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Offset { get; set; }

        [JsonPropertyName("key_type")]
        public string? KeyType { get; set; }

        // Multipliers per tag (e.g., buildings, defense, incrasedtowerdamage)
        [JsonPropertyName("tag_mul")]
        public Dictionary<string, double>? TagMul { get; set; }
    }

    public sealed class DynastioItemArmor
    {
        [JsonPropertyName("absorb")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Absorb { get; set; }

        [JsonPropertyName("resist")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Resist { get; set; }

        // e.g., { "fire": 0.8, "range": 0.9 }
        [JsonPropertyName("custom_resist")]
        public Dictionary<string, double>? CustomResist { get; set; }
    }

    public sealed class DynastioItemGathering
    {
        [JsonPropertyName("axeable")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Axeable { get; set; }

        [JsonPropertyName("pickaxeable")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Pickaxeable { get; set; }

        [JsonPropertyName("treasurechest")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? TreasureChest { get; set; }

        [JsonPropertyName("buildings")]
        [JsonConverter(typeof(DynastioItemDoubleFlexibleConverter))]
        public double? Buildings { get; set; }
    }



    // Converts number-or-string to double
    public sealed class DynastioItemDoubleFlexibleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDouble(out var d)) return d;
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return d;
                return null;
            }

            // Fallback: try to parse whatever is there into a string then double
            try
            {
                var je = JsonElement.ParseValue(ref reader);
                if (je.ValueKind == JsonValueKind.String &&
                    double.TryParse(je.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return d;
            }
            catch { }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }
            writer.WriteNumberValue(value.Value);
        }
    }

    // Some fields (e.g., ignore_shield_chance) sometimes show up as false/true or as numbers
    public sealed class DynastioItemDoubleOrBoolAsDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.True) return 1.0;
            if (reader.TokenType == JsonTokenType.False) return 0.0;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDouble(out var d)) return d;
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (bool.TryParse(s, out var b)) return b ? 1.0 : 0.0;
                if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return d;
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }
            writer.WriteNumberValue(value.Value);
        }
    }




}

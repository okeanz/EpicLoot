﻿using System;
using JetBrains.Annotations;

namespace EpicLoot.BaseEL.Data
{
    public enum ConfigType
    {
        Synced,
        Nonsynced
    }

    public abstract class ConfigValueBase
    {
        public readonly string Identifier;
        public readonly Type Type;
        #nullable enable
        private object? _boxedValue;
        #nullable disable

        [CanBeNull] public event Action ValueChanged;
        #nullable enable
        public object? BoxedValue
        #nullable disable
        {
            get => _boxedValue;
            set
            {
                _boxedValue = value;
                var valueChanged = ValueChanged;
                if (valueChanged == null)
                    return;
                valueChanged();
            }
        }
        protected ConfigValueBase(string identifier, Type type)
        {
            Identifier = identifier;
            Type = type;
        }
    }

    public sealed class ConfigValue<T> : ConfigValueBase
    {
        public T Value
        {
            get => (T)BoxedValue;
            set => BoxedValue = value;
        }

        public ConfigValue(string identifier, T value = default)
            : base(identifier, typeof(T))
        {
            Value = value;
        }

        public void AssignValue(T value)
        {
            Value = value;
        }
    }

}

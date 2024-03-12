using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LethalCompanyMenu.MainMenu.Patch
{
    public class ReflectionUtil
    {
        private const BindingFlags privateInst = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags privateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        private const BindingFlags privateField = privateInst | BindingFlags.GetField;
        private const BindingFlags privateProp = privateInst | BindingFlags.GetProperty;
        private const BindingFlags privateMethod = privateInst | BindingFlags.InvokeMethod;
        private const BindingFlags staticField = privateStatic | BindingFlags.GetField;
        private const BindingFlags staticProp = privateStatic | BindingFlags.GetProperty;
        private const BindingFlags staticMethod = privateStatic | BindingFlags.InvokeMethod;

        private object @object { get; }
        private Type type { get; }

        ReflectionUtil(object obj)
        {
            @object = obj;
            type = obj.GetType();
        }

        private T? GetValue<T>(string variableName, BindingFlags flags)
        {
            try { return (T)type.GetField(variableName, flags).GetValue(@object); } catch (InvalidCastException) { return default; }
        }
        private T? GetProperty<T>(string propertyName, BindingFlags flags)
        {
            try { return (T)type.GetProperty(propertyName, flags).GetValue(@object); } catch (InvalidCastException) { return default; }
        }

        private ReflectionUtil SetValue(string variableName, object value, BindingFlags flags)
        {
            try { type.GetField(variableName, flags).SetValue(@object, value); return this; } catch (Exception) { return null; }
        }
        private ReflectionUtil SetProperty(string propertyName, object value, BindingFlags flags)
        {
            try { type.GetProperty(propertyName, flags).SetValue(@object, value); return this; } catch (Exception) { return null; }
        }

        private T? Invoke<T>(string methodName, BindingFlags flags, params object[] args)
        {
            try { return (T)type.GetMethod(methodName, flags).Invoke(@object, args); } catch (InvalidCastException) { return default; }
        }


        private T? GetValue<T>(string fieldName, bool isStatic = false, bool isProperty = false)
        {
            BindingFlags flags = isProperty ? isStatic ? staticProp : privateProp : isStatic ? staticField : privateField;
            return isProperty ? GetProperty<T>(fieldName, flags) : GetValue<T>(fieldName, flags);
        }
        public ReflectionUtil SetValue(string fieldName, object value, bool isStatic = false, bool isProperty = false)
        {
            BindingFlags flags = isProperty ? isStatic ? staticProp : privateProp : isStatic ? staticField : privateField;
            return isProperty ? SetProperty(fieldName, value, flags) : SetValue(fieldName, value, flags);
        }
        private T? Invoke<T>(string methodName, bool isStatic = false, params object[] args) => Invoke<T>(methodName, isStatic ? staticMethod : privateMethod, args);

        public object GetValue(string fieldName, bool isStatic = false, bool isProperty = false) => GetValue<object>(fieldName, isStatic, isProperty);
        public ReflectionUtil Invoke(string methodName, bool isStatic = false, params object[] args) => Invoke<object>(methodName, isStatic, args)?.Reflect();
        public ReflectionUtil Invoke(string methodName, params object[] args) => Invoke<object>(methodName, args: args)?.Reflect();


        public static ReflectionUtil GetReflection(object obj) => new(obj);
    }
    public static class ReflectorExtensions
    {
        public static ReflectionUtil Reflect(this object obj) => ReflectionUtil.GetReflection(obj);
    }
}

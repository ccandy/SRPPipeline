using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
    public static class ReflectTools
    {
        public static object GetObjectField(object Obj, string FieldName)
        {
            if (Obj == null)
                return null;

            var StateField = Obj.GetType().GetField(FieldName);

            return StateField.GetValue(Obj);
        }

        public static void SetObjectField(object Obj, string FieldName, object Value)
        {
            var StateField = Obj.GetType().GetField(FieldName);

            StateField.SetValue(Obj, Value);
        }

        public static object InvokeMethod(object Obj, string MethodName, params object[] Params)
        {
            var Method = Obj.GetType().GetMethod(MethodName);

            if (Method == null)
                return null;

            return Method.Invoke(Obj, Params);
        }

        public static object InvokeStaticMethod(Type type, string MethodName, params object[] Params)
        {
            var Method = type.GetMethod(MethodName);

            if (Method == null)
                return null;

            return Method.Invoke( null, Params);
        }

        public static T GetPrivateField<T>(this object instance, string fieldname)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flag);
            return (T)field.GetValue(instance);
        }

        public static T GetPrivateField<T>(Type type, string fieldname)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            
            FieldInfo field = type.GetField(fieldname, flag);
            return (T)field.GetValue(null);
        }

		public static T GetField<T>(Type type, string fieldname)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			FieldInfo field = type.GetField(fieldname, flag);
			return (T)field.GetValue(null);
		}

		public static T GetStaticField<T>(Type type, string fieldname)
		{
			BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

			FieldInfo field = type.GetField(fieldname, flag);
			return (T)field.GetValue(null);
		}

		public static bool IsSubClassOfRawGeneric(Type type, Type generic)
		{
			if (type == null)
				return false;
			if (generic == null)
				return false;

			while (type != null && type != typeof(object))
			{
				bool isTheRawGenericType = IsTheRawGenericType(type);
				if (isTheRawGenericType)
					return true;
				type = type.BaseType;
			}

			return false;

			bool IsTheRawGenericType(Type test)
			=> generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
		}
	}
}

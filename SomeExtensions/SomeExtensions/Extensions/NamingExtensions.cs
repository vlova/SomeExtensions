using System;

namespace SomeExtensions.Extensions {
    public static class NamingExtensions {
        public static string ToFieldName(this string propertyName) {
            return "_" + (propertyName[0].ToString().ToLower()) + propertyName.Substring(1);
        }

        public static string ToParameterName(this string name) {
            return name
				.If(f => f.StartsWith("_"), f => f.Substring("_".Length))
				.If(f => char.IsUpper(f[0]), f => char.ToLower(f[0]) + f.Substring(1));
		}

		public static string UppercaseFirst(this string name) {
			if (name?.Length > 0 && char.IsLower(name[0])) {
				return char.ToUpper(name[0]) + name.Substring(1);
			} else {
				return name;
			}
		}

		public static string BoolParameterToMethodName(this string name) {
			if (name.StartsWith("is", StringComparison.OrdinalIgnoreCase)) {
				if (name.Length > 2 && char.IsUpper(name[2])) {
					name = name.Substring("Is".Length);
				}
			}

			return "Is" + name.UppercaseFirst();
		}
	}
}

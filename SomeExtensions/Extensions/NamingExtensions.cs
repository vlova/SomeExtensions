using static System.StringComparison;

namespace SomeExtensions.Extensions {
	public static class NamingExtensions {
		public static string ToFieldName(this string propertyName) {
			return "_" + propertyName.LowercaseFirst();
		}

		public static string BoolParameterToMethodName(this string name) {
			if (name.StartsWith("is", OrdinalIgnoreCase)) {
				if (name.Length > 2 && char.IsUpper(name[2])) {
					name = name.Substring("Is".Length);
				}
			}

			return "Is" + name.UppercaseFirst();
		}

		public static string WithoutUnderscore(this string name) {
			return name.StartsWith("_")
				? name.Substring("_".Length)
				: name;
		}

		public static string ToParameterName(this string name) {
			return name.WithoutUnderscore().LowercaseFirst();
		}

		public static string LowercaseFirst(this string name) {
			if (name?.Length > 0 && char.IsUpper(name[0])) {
				return char.ToLower(name[0]) + name.Substring(1);
			}
			else {
				return name;
			}
		}

		public static string UppercaseFirst(this string name) {
			if (name?.Length > 0 && char.IsLower(name[0])) {
				return char.ToUpper(name[0]) + name.Substring(1);
			} else {
				return name;
			}
		}
	}
}

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
    }
}

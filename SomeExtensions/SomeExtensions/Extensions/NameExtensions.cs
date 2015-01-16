namespace SomeExtensions.Extensions {
    public static class NameExtensions {
        public static string ToFieldName(this string propertyName) {
            return "_" + (propertyName[0].ToString().ToLower()) + propertyName.Substring(1);
        }

        public static string ToParameterName(this string fieldName) {
            return fieldName.If(f => f.StartsWith("_"), f => f.Substring("_".Length));
        }
    }
}

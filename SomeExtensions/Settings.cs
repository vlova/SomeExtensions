namespace SomeExtensions {
	public class Settings {
		public static Settings Instance { get; } = new Settings();

		public bool CanThrow { get; set; }

		protected Settings() {
		}
	}
}

namespace System.Runtime.CompilerServices {
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class InternalsVisibleToAttribute : Attribute {
		private readonly string assemblyName;
		private bool all_visible = true;

		public InternalsVisibleToAttribute(string assemblyName) {
			this.assemblyName = assemblyName;
		}

		public string AssemblyName {
			get {
				return assemblyName;
			}
		}

		public bool AllInternalsVisible {
			get {
				return all_visible;
			}
			set {
				all_visible = value;
			}
		}
	}
}

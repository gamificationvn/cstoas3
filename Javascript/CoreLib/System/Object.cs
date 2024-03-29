﻿namespace System {
	using Collections;
	using Collections.Generic;

	using ComponentModel;
	using Javascript;

	[Name("delete", "delete ({0})", "")]
	public class Object {
		sealed class ObjectEnumerator : IEnumerator<string>{
			object IEnumerator.Current {
				get {
					return Current;
				}
			}

			public string Current {
				get {
					return "";
				}
			}

			public bool MoveNext() {
				return true;
			}

			public void Reset() {}
		}

		public static bool delete(object pObject) {
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public IEnumerator<string> GetEnumerator() {
			return new ObjectEnumerator();
		}

		/// <summary>
		/// Indicates whether an object has a specified property defined.
		/// </summary>
		/// <param name="pName">The property of the object.</param>
		/// <returns>If the target object has the property specified by the name parameter this value is <c>true</c>, otherwise <c>false</c></returns>
		public bool hasOwnProperty(string pName) {
			return false;
		}

		/// <summary>
		/// Indicates whether an instance of the Object class is in the prototype chain of the object specified as the parameter.
		/// </summary>
		/// <param name="pTheClass">The class to which the specified object may refer.</param>
		/// <returns>If the object is in the prototype chain of the object specified by the theClass parameter this value is <c>true</c>, otherwise <c>false</c>.</returns>
		public bool isPrototypeOf(object pTheClass) {
			return false;
		}

		/// <summary>
		/// Indicates whether the specified property exists and is enumerable.
		/// </summary>
		/// <param name="pName">The property of the object.</param>
		/// <returns>If the property specified by the name parameter is enumerable this value is <c>true</c>, otherwise <c>false</c>.</returns>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public bool propertyIsEnumerable(string pName) {
			return false;
		}

		/// <summary>
		/// Sets the availability of a dynamic property for loop operations. The property must exist on the target object because this method does not check the target object's prototype chain.
		/// </summary>
		/// <param name="pName">The property of the object.</param>
		/// <param name="pIsEnum">If set to <c>false</c>, the dynamic property does not show up in for..in loops, and the method <see cref="propertyIsEnumerable"/>() returns <c>false</c></param>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public void setPropertyIsEnumerable(string pName, bool pIsEnum) {
			return;
		}

		/// <summary>
		/// Returns the string representation of the specified object.
		/// </summary>
		/// <returns></returns>
		public string toString() {
			return "";
		}

		public object this[string pName] {
			get {
				return null;
			}

			set {
				value = pName;
				return;
			}
		}

		[Obsolete("Just for compatibility with C# compiler. DO NOT USE")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public bool Equals(object value) {
			return false;
		}

		[Obsolete("Just for compatibility with C# compiler. DO NOT USE")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public int GetHashCode() {
			return 0;
		}

		[Obsolete("Just for compatibility with C# compiler. DO NOT USE")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public virtual string ToString() {
			return "";
		}
	}
}

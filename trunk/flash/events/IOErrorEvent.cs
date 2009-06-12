﻿namespace flash.events {
	public class IOErrorEvent : ErrorEvent {
		/// <summary>
		/// Defines the value of the type property of an ioError event object.
		/// </summary>
		public static readonly string IO_ERROR;

		/// <summary>
		/// Creates a copy of the IOErrorEvent object and sets the value of each property to match that of the original.
		/// </summary>
		public Event clone() {
			return default(Event);
		}

		/// <summary>
		/// Creates an Event object that contains specific information about ioError events.
		/// </summary>
		public IOErrorEvent(string type, bool bubbles, bool cancelable, string text, int id)
			: base(type, bubbles, cancelable, text) {}

		/// <summary>
		/// Creates an Event object that contains specific information about ioError events.
		/// </summary>
		public IOErrorEvent(string type, bool bubbles, bool cancelable, string text) : base(type, bubbles, cancelable, text) {}

		/// <summary>
		/// Creates an Event object that contains specific information about ioError events.
		/// </summary>
		public IOErrorEvent(string type, bool bubbles, bool cancelable) : base(type, bubbles, cancelable) {}

		/// <summary>
		/// Creates an Event object that contains specific information about ioError events.
		/// </summary>
		public IOErrorEvent(string type, bool bubbles) : base(type, bubbles) {}

		/// <summary>
		/// Creates an Event object that contains specific information about ioError events.
		/// </summary>
		public IOErrorEvent(string type) : base(type) {}
	}
}
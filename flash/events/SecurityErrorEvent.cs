﻿namespace flash.events {
	public class SecurityErrorEvent : ErrorEvent {
		/// <summary>
		/// The <see cref="SecurityErrorEvent"/>.SECURITY_ERROR constant defines the value of the type property of a securityError event object.
		/// </summary>
		public const string SECURITY_ERROR = "securityError";

		/// <summary>
		/// Creates an Event object that contains information about security error events.
		/// </summary>
		public SecurityErrorEvent(string type, bool bubbles, bool cancelable, string text, int id)
			: base(type, bubbles, cancelable, text, id) {}

		/// <summary>
		/// Creates an Event object that contains information about security error events.
		/// </summary>
		public SecurityErrorEvent(string type, bool bubbles, bool cancelable, string text)
			: base(type, bubbles, cancelable, text) {}

		/// <summary>
		/// Creates an Event object that contains information about security error events.
		/// </summary>
		public SecurityErrorEvent(string type, bool bubbles, bool cancelable) : base(type, bubbles, cancelable) {}

		/// <summary>
		/// Creates an Event object that contains information about security error events.
		/// </summary>
		public SecurityErrorEvent(string type, bool bubbles) : base(type, bubbles) {}

		/// <summary>
		/// Creates an Event object that contains information about security error events.
		/// </summary>
		public SecurityErrorEvent(string type) : base(type) {}
	}
}
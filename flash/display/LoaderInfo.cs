﻿namespace flash.display {
	using System;

	using events;

	using system;

	using utils;

	public class LoaderInfo : EventDispatcher {
		public readonly ActionScriptVersion actionScriptVersion;
		public readonly ApplicationDomain applicationDomain;

		public readonly ByteArray bytes;

		public readonly uint bytesLoaded;
		public readonly uint bytesTotal;

		public readonly bool childAllowsParent;
		
		public object childSandboxBridge;

		public readonly DisplayObject content;
		public readonly string contentType;
		public readonly float frameRate;
		public readonly int height;
		public readonly Loader loader;
		public readonly string loaderURL;
		public readonly object parameters;
		public readonly bool parentAllowsChild;
		public object parentSandboxBridge;
		public readonly bool sameDomain;
		public readonly EventDispatcher sharedEvents;
		public readonly uint swfVersion;
		public readonly string url;
		public readonly int width;

		public LoaderInfo getLoaderInfoByDefinition(object obj) {
			return null;
		}

		/// <summary>
		/// Dispatched when data has loaded successfully.
		/// </summary>
		[EventAttribute("Event.COMPLETE")]
		public event Action<Event> complete;

		/// <summary>
		/// Dispatched when a network request is made over HTTP and an HTTP status code can be detected.
		/// </summary>
		[EventAttribute("HTTPStatusEvent.HTTP_STATUS")]
		public event Action<HTTPStatusEvent> httpStatus;

		/// <summary>
		/// Dispatched when the properties and methods of a loaded SWF file are accessible.
		/// </summary>
		[EventAttribute("Event.INIT")]
		public event Action<Event> init;

		/// <summary>
		/// Dispatched when an input or output error occurs that causes a load operation to fail.
		/// </summary>
		[EventAttribute("IOErrorEvent.IO_ERROR")]
		public event Action<IOErrorEvent> ioError;

		/// <summary>
		/// Dispatched when a load operation starts.
		/// </summary>
		[EventAttribute("Event.OPEN")]
		public event Action<Event> open;

		/// <summary>
		/// Dispatched when data is received as the download operation progresses.
		/// </summary>
		[EventAttribute("ProgressEvent.PROGRESS")]
		public event Action<ProgressEvent> progress;

		/// <summary>
		/// Dispatched by a <see cref="LoaderInfo"/> object whenever a loaded object is removed by using the unload() method of the Loader object, or when a second load is performed by the same Loader object and the original content is removed prior to the load beginning.
		/// </summary>
		[EventAttribute("Event.UNLOAD")]
		public event Action<Event> unload;
	}
}

﻿namespace CsCompiler.AS3Writer {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using Tools;

	internal sealed class FlexCompilerShell : MarshalByRefObject {
		private static Process _process;
		private static string _workingDir;

		private static Thread _errorThread;
		private static List<Error> _errorList;
		private static volatile bool _foundErrors;

		private static string _lastArguments;
		private static int _lastCompileID;

		private static string initialize(string pJvmarg, string pProjectPath) {
			_errorList = new List<Error>();

			if (pJvmarg == null) {
				//  || !File.Exists(fcshPath)
				// removed! how can i guess file existence using jvm arguments?
				_process = null;
				return "Failed, no compiler configured";
			}

			_workingDir = pProjectPath;
			_process = new Process {
				StartInfo = {
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					StandardOutputEncoding = Encoding.Default,
					StandardErrorEncoding = Encoding.Default,
					CreateNoWindow = true,
					FileName = @"java.exe",
					Arguments = pJvmarg,
					WorkingDirectory = _workingDir
				}
			};

			try {
				_process.Start();

			} catch (Exception ex) {
				_process = null;
				_errorList.Add(new Error {
					Message = @"Unable to start java.exe: {0}",
					AdditionalInfo = ex.Message,
					ErrorType = ErrorType.Error
				});

				return "Failed, unable to run compiler";
			}

			_errorThread = new Thread(readErrors);
			_errorThread.Start();

			return readUntilPrompt();
		}

		public void Compile(string pProjectPath,
		                    bool pConfigChanged,
		                    string pArguments,
		                    out string pOutput,
							out ICollection<Error> pErrors,
		                    string pJvmarg) {
			StringBuilder o = new StringBuilder();

			// shut down fcsh if our working path has changed
			if (pProjectPath != _workingDir) {
				cleanup();
			}

			// start up fcsh if necessary
			if (_process == null || _process.HasExited) {
				o.AppendLine("INITIALIZING: " + initialize(pJvmarg, pProjectPath));

			} else {
				_errorList.Clear();
			}

			// success?
			if (_process == null) {
				pOutput = o.ToString();
				_errorList.Add(new Error {
					Message = @"Could not compile because the fcsh process could not be started.",
					ErrorType = ErrorType.Error
				});

				pErrors = _errorList.ToArray();
				return;
			}

			if (pArguments != _lastArguments) {
				if (_lastCompileID != 0) {
					clearOldCompile();
				}

				o.AppendLine("Starting new compile.");
				_process.StandardInput.WriteLine(@"mxmlc " + pArguments);

				// remember this for next time
				_lastCompileID = readCompileID();
				_lastArguments = pArguments;

			} else {
				// incrementally build the last compiled ID
				o.AppendLine("Incremental compile of " + _lastCompileID);
				_process.StandardInput.WriteLine("compile " + _lastCompileID);
			}

			o.Append(readUntilPrompt());

			// this is hacky.  allow some time for errors to accumulate in our separate thread.
			do {
				_foundErrors = false;
				Thread.Sleep(100);
			} while (_foundErrors);

			pOutput = o.ToString();

			if (Regex.IsMatch(pOutput, string.Format("fcsh: Target {0} not found", _lastCompileID))) {
				// force a fresh compile
				_lastCompileID = 0;
				_lastArguments = null;
				Compile(pProjectPath, true, pArguments, out pOutput, out pErrors, pJvmarg);
				return;
			}

			lock (_errorList)
				pErrors = _errorList.ToArray();
		}

		private static void clearOldCompile() {
			_process.StandardInput.WriteLine("clear " + _lastCompileID);
			readUntilPrompt();
			_lastCompileID = 0;
			_lastArguments = null;
		}

		// Run in a separate thread to read errors as they accumulate
		static readonly Regex _errorMessage = new Regex(@"(.*?)\((\d+)\)\:\W+col\:\W+(\d+)\W+(\w+):\W+(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static void readErrors() {
			while (_process != null && !_process.StandardError.EndOfStream) {
				string line = _process.StandardError.ReadLine();
				lock (_errorList) {
					if (string.IsNullOrEmpty(line)) {
						continue;
					}

					Error e = new Error();

					Match m = _errorMessage.Match(line);
					if (m.Success) {//parse error message
						//capture 1: row
						//capture 2: col
						//capture 3: type (Warning/Error)
						//capture 4: Message

						e.File = m.Groups[1].Value;

						int i;
						int.TryParse(m.Groups[2].Value, out i);
						e.Line = i;

						int.TryParse(m.Groups[3].Value, out i);
						e.Column = i;

						switch (m.Groups[4].Value.ToLowerInvariant()) {
							case "warning":
								e.ErrorType = ErrorType.Warning;
								break;

							case "error":
								e.ErrorType = ErrorType.Error;
								break;

							default:
								e.ErrorType = ErrorType.Message;
								break;
						}

						e.Message = m.Groups[5].Value;
						_errorList.Add(e);

					} else {
						if (line.Trim().Equals("^", StringComparison.Ordinal)) {
							_errorList[_errorList.Count - 2].AdditionalInfo = _errorList[_errorList.Count - 1].Message;
							_errorList.RemoveAt(_errorList.Count - 1);
							continue;
						}

						e.ErrorType = ErrorType.Message;
						e.Message = line.Trim();
						_errorList.Add(e);
					}
					
					_foundErrors = true;
				}
			}
		}

		private static void cleanup() {
			_lastCompileID = 0;
			_lastArguments = null;
			// this will free up our error-reading thread as well.
			try {
				if (_process != null && !_process.HasExited) {
					_process.Kill();
				}
			} catch {}
		}

		#region FCSH Output Parsing
		/// <summary>
		/// 	Read the compile id fsch returns
		/// </summary>
		/// <returns></returns>
		private static int readCompileID() {
			string line;
			lock (typeof (FlexCompilerShell))
				line = _process.StandardOutput.ReadLine();

			if (line == null) {
				return 0;
			}

			// loop through all lines, regex matching phrase
			Match m = Regex.Match(line, "Assigned ([0-9]+) as the compile target id");
			while (!m.Success) {
				lock (typeof (FlexCompilerShell))
					line = _process.StandardOutput.ReadLine();

				if (line == null) {
					return 0;
				}

				m = Regex.Match(line, "Assigned ([0-9]+) as the compile target id");
			}

			if (m.Groups.Count == 2) {
				return int.Parse(m.Groups[1].Value);
			}

			throw new Exception("Couldn't find the compile ID assigned by fcsh!");
		}

		/// <summary>
		/// 	Read until fcsh is in idle state, displaying its (fcsh) prompt
		/// </summary>
		/// <returns></returns>
		private static string readUntilPrompt() {
			return readUntilToken("(fcsh)");
		}

		private static string readUntilToken(string pToken) {
			StringBuilder output = new StringBuilder();
			Queue<char> queue = new Queue<char>();

			lock (typeof (FlexCompilerShell)) {
				bool keepProcessing = true;
				while (keepProcessing) {
					if (_process.HasExited) {
						keepProcessing = false;
					} else {
						char c = (char)_process.StandardOutput.Read();
						output.Append(c);

						queue.Enqueue(c);
						if (queue.Count > pToken.Length) {
							queue.Dequeue();
						}

						if (new string(queue.ToArray()).Equals(pToken)) {
							keepProcessing = false;
						}
					}
				}
			}
			return output.ToString();
		}
		#endregion
	}
}

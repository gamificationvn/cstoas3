﻿namespace CsCompilerTask {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using CsCompiler.AS3Writer;
	using CsCompiler.JsWriter;
	using CsCompiler.Tools;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public sealed class CsCompilerTask : Task {
		public CsCompilerTask() {
			ConverterFactory.AddParser(new As3NamespaceParser(), "as3");
			ConverterFactory.AddParser(new JsNamespaceParser(), "js");
		}

		[Required]
		public string ProjectPath { get; set; }

		[Required]
		public string RootPath { get; set; }

		[Required]
		public string OutputPath { get; set; }

		[Required]
		public string FlexSdkPath { get; set; }

		public ITaskItem[] References { get; set; }

		public ITaskItem[] Resources { get; set; }

		public string AdditionalConfig { get; set; }

		[Required]
		public string Language { get; set; }
		
		public string Configuration { get; set; }

		public ITaskItem DocumentationFile { get; set; }

		public override bool Execute() {
			if (!ConverterFactory.HasConverter(Language)) {
				Log.LogError("The specified language [{0}] does not has a parser associated.", Language);
				return false;
			}

			string[] sourceFiles = Project.GetSourceFiles(ProjectPath);
			if (sourceFiles == null || sourceFiles.Length == 0) {
				Log.LogError("Source files were not found at the specified location.");
				return false;
			}

			try {
				Dictionary<string, string> args = Project.GetArguments(AdditionalConfig);
				args.Add("FlexSdkPath", FlexSdkPath);

				//Resources.Select(pTaskItem => pTaskItem.ItemSpec).ToList()
				ICollection<Error> errors = Project.Parse(
					sourceFiles,
					Language,
					OutputPath,
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					Configuration.Equals("debug", StringComparison.OrdinalIgnoreCase),
					args,
					RootPath
				);

				foreach (Error error in errors) {
					string errorMessage = error.Message;
					if (!string.IsNullOrEmpty(error.AdditionalInfo)) {
						errorMessage += " [{0}]";
					}

					switch (error.ErrorType) {
						case ErrorType.Error:
							Log.LogError(null, null, null, error.File, error.Line, error.Column, 0, 0, errorMessage, error.AdditionalInfo);
							break;

						case ErrorType.Warning:
							Log.LogWarning(null, null, null, error.File, error.Line, error.Column, 0, 0, errorMessage, error.AdditionalInfo);
							break;

						default:
							Log.LogMessage(errorMessage, error.AdditionalInfo);
							break;
					}
				}

				if (errors.Count > 0) {
					return false;
				}

			} catch (Exception ex) {
				Log.LogErrorFromException(ex);
				return false;
			}

			return true;
		}
	}
}

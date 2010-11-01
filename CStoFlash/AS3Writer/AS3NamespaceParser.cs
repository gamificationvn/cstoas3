﻿namespace CStoFlash.AS3Writer {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using CsParser;
	using Expressions;
	using Metaspec;
	using Tools;
	using VsProjectParser;

	public class As3NamespaceParser : INamespaceParser {
		private string _outputFolder;

		static As3NamespaceParser() {
			FactoryExpressionCreator.AddParser(typeof (CsBinaryExpression), new BinaryExpression());
			FactoryExpressionCreator.AddParser(typeof (CsArrayInitializer), new ArrayInitializer());
			FactoryExpressionCreator.AddParser(typeof (CsAsIsExpression), new AsIsExpression());
			FactoryExpressionCreator.AddParser(typeof (CsAssignmentExpression), new AssignmentExpression());
			FactoryExpressionCreator.AddParser(typeof (CsConditionalExpression), new ConditionalExpression());
			FactoryExpressionCreator.AddParser(typeof (CsLambdaExpression), new LambdaExpression());

			FactoryExpressionCreator.AddParser(typeof (CsTypeofExpression), new TypeofExpression());
			FactoryExpressionCreator.AddParser(typeof (CsThisAccess), new ThisAccess());
			FactoryExpressionCreator.AddParser(typeof (CsSizeofExpression), new SizeofExpression());
			FactoryExpressionCreator.AddParser(typeof (CsSimpleName), new SimpleName());
			FactoryExpressionCreator.AddParser(typeof (CsRefValueExpression), new RefTypeExpression());
			FactoryExpressionCreator.AddParser(typeof (CsRefTypeExpression), new RefTypeExpression());
			FactoryExpressionCreator.AddParser(typeof (CsQueryExpression), new QueryExpression());
			FactoryExpressionCreator.AddParser(typeof (CsQualifiedAliasMemberAccess), new QualifiedAliasMemberAccess());
			FactoryExpressionCreator.AddParser(typeof (CsPrimaryExpressionMemberAccess), new PrimaryExpressionMemberAccess());
			FactoryExpressionCreator.AddParser(typeof (CsPredefinedTypeMemberAccess), new PredefinedTypeMemberAccess());
			FactoryExpressionCreator.AddParser(typeof (CsPostIncrementDecrementExpression),
			                                   new PostIncrementDecrementExpression());
			FactoryExpressionCreator.AddParser(typeof (CsPointerMemberAccess), new PointerMemberAccess());
			FactoryExpressionCreator.AddParser(typeof (CsParenthesizedExpression), new ParenthesizedExpression());
			FactoryExpressionCreator.AddParser(typeof (CsNewObjectExpression), new NewObjectExpression());
			FactoryExpressionCreator.AddParser(typeof (CsNewArrayExpression), new NewArrayExpression());
			FactoryExpressionCreator.AddParser(typeof (CsMakeRefExpression), new MakeRefExpression());
			FactoryExpressionCreator.AddParser(typeof (CsLiteral), new Literal());
			FactoryExpressionCreator.AddParser(typeof (CsInvocationExpression), new InvocationExpression());
			FactoryExpressionCreator.AddParser(typeof (CsElementAccess), new ElementAccess());
			FactoryExpressionCreator.AddParser(typeof (CsDefaultValueExpression), new DefaultValueExpression());
			FactoryExpressionCreator.AddParser(typeof (CsCheckedExpression), new CheckedExpression());
			FactoryExpressionCreator.AddParser(typeof (CsBaseMemberAccess), new BaseMemberAccess());
			FactoryExpressionCreator.AddParser(typeof (CsBaseIndexerAccess), new BaseIndexerAccess());
			FactoryExpressionCreator.AddParser(typeof (CsArgListExpression), new ArgListExpression());
			FactoryExpressionCreator.AddParser(typeof (CsAnonymousObjectCreationExpression),
			                                   new AnonymousObjectCreationExpression());
			FactoryExpressionCreator.AddParser(typeof (CsAnonymousMethodExpression), new AnonymousMethodExpression());
			FactoryExpressionCreator.AddParser(typeof (CsCastUnaryExpression), new CastUnaryExpression());
			FactoryExpressionCreator.AddParser(typeof (CsOperatorUnaryExpression), new OperatorUnaryExpression());
			FactoryExpressionCreator.AddParser(typeof (CsPreIncrementDecrementExpression), new PreIncrementDecrementExpression());
		}

		public static string Using { get; private set; }

		#region INamespaceParser Members
		public void PreBuildEvents(ICsProject pProject, bool pDebug) {
			string flashLibrary = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			byte[] assemblyBuffer = File.ReadAllBytes(flashLibrary + @"\\flash\\flash.dll");
			IExternalAssemblyModule module = IExternalAssemblyModuleFactory.create(assemblyBuffer, flashLibrary);
			pProject.addExternalAssemblyModules(new[] { module }, false, null);
		}

		public void Parse(CsNamespace pNameSpace, IEnumerable<CsUsingDirective> pUsing, string pOutputFolder) {
			if (pNameSpace == null) {
				return;
			}

			_outputFolder = pOutputFolder;
			string name = getNamespace(pNameSpace);
			string packDir = pOutputFolder + name.Replace('.', '\\');
			Directory.CreateDirectory(packDir);
			As3Builder builder = new As3Builder("\t");

			foreach (CsNode cn in pNameSpace.member_declarations) {
				builder.Append("package ");

				builder.Append(name);
				builder.AppendLineAndIndent(" {");

				StringBuilder usings = new StringBuilder();

				parseUsing(pUsing, usings);
				parseUsing(pNameSpace.using_directives, usings);
				Using = usings.ToString();

				builder.Append(usings);

				builder.AppendLine(ClassParser.IMPORT_MARKER);
				ImportStatementList.Init();

				CsClass csClass = cn as CsClass;

				if (csClass != null) {
//it's a class....
					ClassParser.Parse(csClass, builder);

					File.WriteAllText(packDir + "\\" + Helpers.GetRealName(csClass, csClass.identifier.identifier) + ".as",
					                  builder.ToString());
					builder.Length = 0;
				}

				if (csClass == null) {
					throw new Exception("Unknown type");
				}
			}
		}

		public static string MainClassName { get; internal set; }

		public void PostBuildEvents(bool pDebug) {
			string path = Path.Combine(As3Configuration.Instance.FlexSdkPath, @"bin\mxmlc.exe");
			ExecuteProcess process = new ExecuteProcess(path);

			process.AddArgument(Path.Combine(_outputFolder, MainClassName.Replace(".", "\\")+".as"));
			process.AddArgument("source-path", _outputFolder);

			foreach (FlexOption option in As3Configuration.Instance.FlexOptions) {
				process.AddArgument(option.Name, option.Value);
			}

			if (pDebug) {
				process.AddArgument(@"debug", "true");
				process.AddArgument(@"verbose-stacktraces", "true");
			}

			foreach (var argument in Program.Arguments) {
				process.AddArgument(argument.Key, argument.Value);	
			}

			process.AddArgument("o", Path.Combine(_outputFolder, @"..\swf\file.swf"));

			process.Execute(_outputFolder);

			Console.WriteLine(process.Output);
			Console.WriteLine(process.Error);
			//process.AddArgument("-library-path","%2libraries");
		}

		public void HandleAdditionalProjectFiles(VsProject pProject) {
			foreach (var projectItem in pProject.Items) {
				if (projectItem.ItemType != VsItemType.EmbeddedResource) continue;
				//TODO: use swfmill to compile resources.
				//check type of resource by extension: xml, font, images (png, jpg), sounds (mp3)

			}
		}
		#endregion

		private static string getNamespace(CsNamespace pNamespace) {
			string name = pNamespace.qualified_identifier.Aggregate(string.Empty,
			                                                        (pCurrent, pIdentifier) =>
			                                                        pCurrent + (pIdentifier.identifier.identifier + "."));
			return name.Substring(0, name.Length - 1);
		}

		private static void parseUsing(IEnumerable<CsUsingDirective> pNn, StringBuilder pStrb) {
			if (pNn == null) {
				return;
			}

			foreach (CsUsingDirective directive in pNn) {
				if (directive is CsUsingNamespaceDirective) {
					string name = As3Helpers.Convert(Helpers.GetType(directive));
					if (name.StartsWith("flash.Global", StringComparison.Ordinal) ||
						//name.StartsWith("System", StringComparison.Ordinal) || 
						name.Equals("flash.", StringComparison.Ordinal)) {
						continue;
					}

					pStrb.AppendFormat("import {0}*;", name);
					pStrb.AppendLine();
					continue;
				}

				if (directive is CsUsingAliasDirective) {
					string name = As3Helpers.Convert(Helpers.GetType(directive));
					if (name.StartsWith("flash.Global", StringComparison.Ordinal) || name.Equals("flash.", StringComparison.Ordinal)) {
						continue;
					}

					pStrb.AppendFormat("import {0}*;", name);
					pStrb.AppendLine();
					continue;
				}

				throw new Exception(@"Unhandled using type");
			}
		}
	}
}
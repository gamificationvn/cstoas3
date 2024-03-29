﻿namespace CsCompiler.CsParser {
	using System.Collections.Generic;
	using Interfaces;
	using Metaspec;
	using Tools;

	public sealed class TheConstant : BaseNode {
		public TheConstant(CsConstantDeclaration pCsConstantDeclaration, TheClass pTheClass, FactoryExpressionCreator pCreator) {
			Modifiers.AddRange(Helpers.GetModifiers(pCsConstantDeclaration.modifiers));

			foreach (CsConstantDeclarator declarator in pCsConstantDeclaration.declarators) {
				Constant v = new Constant {
					//RealName = declarator.identifier.identifier,
					//Name = Helpers.GetRealName(declarator, declarator.identifier.identifier),
					Name = declarator.identifier.identifier,
					Initializer = pCreator.Parse(declarator.expression),
					ReturnType = Helpers.GetType(declarator.entity.type)
				};

				v.Modifiers.AddRange(Modifiers);
				Constants.Add(v);
			}
		}

		public readonly List<Constant> Constants = new List<Constant>();
	}

	public sealed class Constant : BaseNode, ICsHasReturnType {
		public Expression Initializer;

		public string ReturnType {
			get;
			internal set;
		}
	}
}

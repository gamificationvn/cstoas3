﻿namespace CsCompiler.AS3Writer.Expressions {
	using System;
	using Metaspec;
	using Tools;

	public class TypeofExpression : IExpressionParser {
		public Expression Parse(CsExpression pStatement) {
			CsTypeofExpression ex = (CsTypeofExpression)pStatement;
			//"typeof" "(" type ")"

			CsNamespaceOrTypeName isClass = ex.type.type_name as CsNamespaceOrTypeName;
			if (isClass != null) {
				return new Expression(isClass.identifier.identifier, ex.type.entity_typeref);
			}

			throw new NotImplementedException();
		}
	}
}
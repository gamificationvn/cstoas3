﻿namespace CsCompiler.JsWriter.Expressions {
	using Metaspec;
	using Tools;

	public sealed class CastUnaryExpression : IExpressionParser {
		public Expression Parse(CsExpression pStatement, FactoryExpressionCreator pCreator) {
			CsCastUnaryExpression ex = (CsCastUnaryExpression)pStatement;

			//Do not cast anything. This is here just to support the automatic casting done by the flash player.
			return new Expression(
				//JsHelpers.Convert(Helpers.GetType(ex.type)) + "(" +
				pCreator.Parse(ex.unary_expression).Value
				//+ ")"
				, ex.type.entity_typeref
			);
		}
	}
}

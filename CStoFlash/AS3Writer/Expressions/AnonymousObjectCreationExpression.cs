﻿namespace CStoFlash.AS3Writer.Expressions {
	using System;

	using Metaspec;

	using Utils;

	public class AnonymousObjectCreationExpression : IExpressionParser {
		public Expression Parse(CsExpression pStatement) {
			//"new" anonymous-object-initializer
			// "{" member-declarator-list? "}"
			//"{" member-declarator-list "," "}"
			//
			throw new NotImplementedException();
		}
	}
}
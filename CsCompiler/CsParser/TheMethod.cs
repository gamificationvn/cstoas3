﻿namespace CsCompiler.CsParser {
	using Interfaces;
	using Metaspec;
	using Tools;

	public sealed class TheMethod : BaseMethod, ICsMethod {
		private readonly string _name;
		private readonly string _realName;

		internal TheMethod(CsEntityMethod pCsMethod, TheClass pMyClass, FactoryExpressionCreator pCreator, bool pIsEvent = false, bool pIsAddEvent = false) {
			MyClass = pMyClass;
			//Modifiers.AddRange(Helpers.GetModifiers(pCsMethod.access));
			Arguments = getArguments(pCsMethod.parameters, pCreator);
			Signature = getSignature(Arguments);
			
			//_name = Helpers.GetRealName(pCsMethod, pIsEvent ? 
			//    pIsAddEvent ? "add" : "remove" : 
			//    pCsMethod.name);
			
			_name = pIsEvent ? pIsAddEvent ? "add" : "remove" : pCsMethod.name;
			_realName = pCsMethod.name;

			//FullRealName = MyClass.FullRealName + "." + RealName;

			ReturnType = Helpers.GetType(pCsMethod.specifier.return_type);
			IsExtensionMethod = pCsMethod.isExtensionMethod();
		}

		public override string FullName {
			get {
				return MyClass.FullName + "." + Name;
			}

			internal set {
				base.FullName = value;
			}
		}

		private string _lazyName;
		public override string Name {
			get {
				if (_lazyName == null) {
					_lazyName = _isUnique ? _name : _name + _index;

					if (MyClass.Base != null) {
						TheClass c = MyClass.Base;
						TheMethod m = c.FindMethod(_realName, Signature);
						if (m != null)
							_lazyName = m.Name;		
					}
				}

				return _lazyName;
			}
		}

		internal TheMethod(CsMethod pCsMethod, TheClass pMyClass, FactoryExpressionCreator pCreator) {
			MyClass = pMyClass;
			Modifiers.AddRange(Helpers.GetModifiers(pCsMethod.modifiers));
			Arguments = getArguments(pCsMethod.parameters.parameters, pCreator);
			Signature = getSignature(Arguments);
			CodeBlock = pCsMethod.definition;

			//_sig = Signature.Replace(',', '_').Replace("<", "").Replace(">", "");
			//_name = Helpers.GetRealName(pCsMethod, pCsMethod.identifier.identifier);
			_realName = _name = pCsMethod.identifier.identifier;
			//FullRealName = MyClass.FullRealName + "." + RealName;

			ReturnType = Helpers.GetType(pCsMethod.return_type);
			IsExtensionMethod = pCsMethod.entity.isExtensionMethod();
		}

		public string ReturnType {get; private set;}
		public bool IsExtensionMethod {
			get; private set; }
	}
}

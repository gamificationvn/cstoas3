﻿namespace CsCompiler.CsParser {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Metaspec;

	public sealed class TheClass : BaseNode {
		private readonly Dictionary<CsMethod, TheMethod> _methods = new Dictionary<CsMethod, TheMethod>();
		private readonly Dictionary<CsEntityMethod, TheMethod> _entityMethods = new Dictionary<CsEntityMethod, TheMethod>();
		private readonly Dictionary<CsConstructor, TheConstructor> _constructors = new Dictionary<CsConstructor, TheConstructor>();
		private readonly Dictionary<CsIndexer, TheIndexer> _indexers = new Dictionary<CsIndexer, TheIndexer>();
		private readonly Dictionary<CsVariableDeclaration, TheVariable> _variables = new Dictionary<CsVariableDeclaration, TheVariable>();
		private readonly Dictionary<CsConstantDeclaration, TheConstant> _constants = new Dictionary<CsConstantDeclaration, TheConstant>();
		private readonly Dictionary<CsProperty, TheProperty> _properties = new Dictionary<CsProperty, TheProperty>();
		private readonly Dictionary<CsDelegate, TheDelegate> _delegates = new Dictionary<CsDelegate, TheDelegate>();
		private readonly Dictionary<string, TheEvent> _events = new Dictionary<string, TheEvent>();

		readonly List<string> _extends = new List<string>();
		private readonly List<string> _implements = new List<string>();

		public TheClass(CsClassStruct pCsClass) {
			CsNamespace csNamespace;
			List<string> name = new List<string>();
			if (pCsClass.parent is CsClass) {
				IsPrivate = true;
				csNamespace = (CsNamespace)pCsClass.parent.parent;
				

			} else {
				csNamespace = (CsNamespace)pCsClass.parent;
			}

			CsQualifiedIdentifier list = csNamespace.qualified_identifier;
			name.AddRange(list.Select(pIdentifier => pIdentifier.identifier.identifier));	
			
			if (IsPrivate)
				name.Add(((CsClass)pCsClass.parent).identifier.identifier);

			NameSpace = string.Join(".", name.ToArray());
			RealName = pCsClass.identifier.identifier;
			Name = Helpers.GetRealName(pCsClass, RealName);
			
			FullName = NameSpace + "." + Name;
			FullRealName = NameSpace + "." + RealName;

			if (pCsClass.type_base != null && pCsClass.type_base.base_list.Count != 0) {
				foreach (CsTypeRef typeRef in pCsClass.type_base.base_list) {
					if (typeRef.entity_typeref.u is CsEntityClass) {
						Extends.Add(Helpers.GetType(typeRef.type_name));
						_baseTyperef = typeRef;

					} else if (typeRef.entity_typeref.u is CsEntityInterface) {
						Implements.Add(Helpers.GetType(typeRef.type_name));

					} else if (typeRef.entity_typeref.u is CsEntityInstanceSpecifier) {
						Implements.Add(Helpers.GetType(typeRef.type_name));

						//CsEntityInstanceSpecifier specifier = (CsEntityInstanceSpecifier)typeRef.entity_typeref.u;

					} else {
						throw new NotSupportedException();
					}
				}
			}

			Dictionary<string, int> methodNames = new Dictionary<string, int>();
			bool constructorsDone = false;
			bool methodsDone = false;

			if (pCsClass.member_declarations != null) {
				foreach (CsNode memberDeclaration in pCsClass.member_declarations) {
					CsConstructor c = memberDeclaration as CsConstructor;
					if (c != null) {
						TheConstructor tm = new TheConstructor(c, this);

						if (methodNames.ContainsKey(tm.RealName)) {
							methodNames[tm.RealName]++;
							int index = tm._index = methodNames[tm.RealName];

							if (!constructorsDone) {
								constructorsDone = true;
								foreach (KeyValuePair<CsConstructor, TheConstructor> constructor in _constructors) {
									constructor.Value._isUnique = false;
									constructor.Value._index = --index;
								}
							}

							tm._isUnique = false;

						} else {
							methodNames[tm.RealName] = tm._index = 1;
						}

						_constructors.Add(c, tm);
						continue;
					}

					CsMethod m = memberDeclaration as CsMethod;
					if (m != null) {
						if (m.interface_type != null) continue;

						TheMethod tm = new TheMethod(m, this);
						if (methodNames.ContainsKey(tm.RealName)) {
							methodNames[tm.RealName]++;
							int index = tm._index = methodNames[tm.RealName];

							if (!methodsDone) {
								methodsDone = true;
								foreach (KeyValuePair<CsMethod, TheMethod> method in _methods) {
									method.Value._isUnique = false;
									method.Value._index = --index;
								}
							}

							tm._isUnique = false;

						} else {
							methodNames[tm.RealName] = tm._index = 1;
						}

						_methods.Add(m, tm);
						continue;
					}

					CsIndexer i = memberDeclaration as CsIndexer;
					if (i != null) {
						_indexers.Add(i, new TheIndexer(i, this));
						continue;
					}

					CsVariableDeclaration v = memberDeclaration as CsVariableDeclaration;
					if (v != null) {
						_variables.Add(v, new TheVariable(v, this));
						continue;
					}

					CsConstantDeclaration k = memberDeclaration as CsConstantDeclaration;
					if (k != null) {
						_constants.Add(k, new TheConstant(k, this));
						continue;
					}

					CsProperty p = memberDeclaration as CsProperty;
					if (p != null) {
						if (p.interface_type == null)
							_properties.Add(p, new TheProperty(p, this));
						continue;
					}

					CsDelegate d = memberDeclaration as CsDelegate;
					if (d != null) {
						_delegates.Add(d, new TheDelegate(d, this));
						continue;
					}

					CsEvent e = memberDeclaration as CsEvent;
					if (e != null) {
						TheEvent theEvent = new TheEvent(e, this);
						_events.Add(theEvent.RealName, theEvent);
						continue;
					}

					CsClass csClass = memberDeclaration as CsClass;
					if (csClass != null) {

						continue;
					}

					throw new NotImplementedException("Unknown type not implemented");
				}
			}

			Modifiers.AddRange(Helpers.GetModifiers(pCsClass.modifiers));
		}

		public bool IsPrivate {
			get; private set; }

		public TheClass(CsEntity pCsEntity) {
			IsEntity = true;
			List<string> name = new List<string>();
			CsEntity parent = pCsEntity.parent;

			while (parent != null && parent is CsEntityNamespace) {
				if (!string.IsNullOrEmpty(parent.name))
					name.Add(parent.name);
				parent = parent.parent;
			}

			name.Reverse();

			NameSpace = string.Join(".", name.ToArray());
			RealName = pCsEntity.name;
			Name = Helpers.GetRealName(pCsEntity, RealName);

			FullName = NameSpace + "." + Name;
			FullRealName = NameSpace + "." + RealName;

			CsEntityClass klass = pCsEntity as CsEntityClass;
			if (klass == null)
				throw new NotImplementedException();

			_baseEntityTyperef = klass.base_type;

			if (klass.base_type.type != cs_entity_type.et_object)
				Extends.Add(Helpers.GetType(klass.base_type));

			if (klass.interfaces != null) {
				foreach (CsEntityTypeRef @interface in klass.interfaces) {
					Implements.Add(Helpers.GetType(@interface));
				}	
			}

			Dictionary<string, int> methodNames = new Dictionary<string, int>();
			//bool constructorsDone = false;
			bool methodsDone = false;
			if (klass.method_implementations == null) {
				return;
			}

			foreach (CsEntityMethodImplementation methodImplementation in klass.method_implementations) {
				CsEntityMethod m = methodImplementation.implementation_method;
				TheMethod tm = new TheMethod(m, this);
				if (methodNames.ContainsKey(tm.RealName)) {
					methodNames[tm.RealName]++;
					int index = tm._index = methodNames[tm.RealName];

					if (!methodsDone) {
						methodsDone = true;
						foreach (KeyValuePair<CsMethod, TheMethod> method in _methods) {
							method.Value._isUnique = false;
							method.Value._index = --index;
						}
					}

					tm._isUnique = false;

				} else {
					methodNames[tm.RealName] = tm._index = 1;
				}

				_entityMethods.Add(m, tm);
			}
		}

		private readonly CsTypeRef _baseTyperef;
		private readonly CsEntityTypeRef _baseEntityTyperef;
		public TheClass Base {
			get {
				if (_baseEntityTyperef == null) {
					return _baseTyperef == null ? null : TheClassFactory.Get(_baseTyperef);	
				}

				return TheClassFactory.Get(_baseEntityTyperef);
			}
		}

		public string NameSpace {
			get;
			private set;
		}

		public List<string> Implements {
			get {
				return _implements;
			}
		}

		public List<string> Extends {
			get {
				return _extends;
			}
		}

		public TheMethod GetMethod(CsMethod pMethod) {
			TheMethod c;
			if (_methods.TryGetValue(pMethod, out c)) {
				return c;
			}

			return _entityMethods.TryGetValue(pMethod.entity, out c) ? c : null;
		}

		public TheMethod GetMethod(CsEntityMethod pMethod) {
			if (pMethod.decl != null)
				return GetMethod((CsMethod)pMethod.decl);

			TheMethod c;
			if (_entityMethods.TryGetValue(pMethod, out c))
				return c;

			c = new TheMethod(pMethod, this);
			Dictionary<string, bool> methodNames = new Dictionary<string, bool>();

			foreach (KeyValuePair<CsEntityMethod, TheMethod> entityMethod in _entityMethods) {
				if (!methodNames.ContainsKey(entityMethod.Value.Name)) {
					continue;
				}
				entityMethod.Value._isUnique = false;
				c._isUnique = false;
			}

			_entityMethods.Add(pMethod, c);
			return c;
		}

		public bool IsEntity { get; private set; }

		public TheConstructor GetConstructor(CsConstructor pMethod) {
			if (pMethod == null) {//no constructor...
				return null;
			}

			TheConstructor c;
			return _constructors.TryGetValue(pMethod, out c) ? c : null;
		}

		public TheConstructor GetConstructor(CsExpression pMethod) {
			return GetConstructor((CsConstructor)((CsEntityMethod)pMethod.entity).decl);
		}

		public TheIndexer GetIndexer(CsIndexer pMemberDeclaration) {
			TheIndexer c;
			return _indexers.TryGetValue(pMemberDeclaration, out c) ? c : null;
		}

		public TheVariable GetVariable(CsVariableDeclaration pMemberDeclaration) {
			TheVariable c;
			return _variables.TryGetValue(pMemberDeclaration, out c) ? c : null;
		}

		public TheConstant GetConstant(CsConstantDeclaration pConstantDeclaration) {
			TheConstant c;
			return _constants.TryGetValue(pConstantDeclaration, out c) ? c : null;
		}

		public TheDelegate GetDelegate(CsDelegate pMemberDeclaration) {
			TheDelegate c;
			return _delegates.TryGetValue(pMemberDeclaration, out c) ? c : null;
		}

		public TheEvent GetEvent(string pEventName) {
			TheEvent c;
			return _events.TryGetValue(pEventName, out c) ? c : null;
		}

		public TheProperty GetProperty(CsProperty pMemberDeclaration) {
			TheProperty c;
			return _properties.TryGetValue(pMemberDeclaration, out c) ? c : null;
		}

		public TheIndexer GetIndexer(CsExpression pIndexer) {
			CsBaseIndexerAccess csbia = pIndexer as CsBaseIndexerAccess;
			CsIndexer i;
			CsEntityProperty p;
			if (csbia != null) {
				p = (CsEntityProperty)csbia.entity;
				if (p == null)
					return null;

				i = (CsIndexer)p.decl;
				if (i == null)
					return null;

				return _indexers.ContainsKey(i) ? _indexers[i] : null;
			}

			CsElementAccess csea = pIndexer as CsElementAccess;

			if (csea != null) {
				p = (CsEntityProperty)csea.entity;
				if (p == null)
					return null;

				i = (CsIndexer)p.decl;
				if (i == null)
					return null;

				return _indexers.ContainsKey(i) ? _indexers[i] : null;
			}

			throw new NotImplementedException();
		}

		public TheConstructor GetDefaultConstructor() {
			return (from theConstructor in _constructors where theConstructor.Value.IsDefaultConstructor select theConstructor.Value).FirstOrDefault();
		}

		public TheMethod FindMethod(string pRealName, string pSignature) {
			foreach (var theMethod in
				_methods.Where(pTheMethod => pRealName.Equals(pTheMethod.Key.identifier.identifier, StringComparison.Ordinal) && pSignature.Equals(pTheMethod.Value.Signature, StringComparison.Ordinal))) {
				return theMethod.Value;
			}

			return (from theMethod in _entityMethods
			        where pRealName.Equals(theMethod.Key.name, StringComparison.Ordinal) && pSignature.Equals(theMethod.Value.Signature, StringComparison.Ordinal)
			        select theMethod.Value).FirstOrDefault();
		}

		public TheConstructor FindConstructor(string pSignature) {
			return (from theMethod in _constructors
			        where pSignature.Equals(theMethod.Value.Signature, StringComparison.Ordinal)
			        select theMethod.Value).FirstOrDefault();
		}
	}
}

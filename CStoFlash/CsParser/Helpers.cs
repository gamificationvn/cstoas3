﻿namespace CStoFlash.CsParser {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Metaspec;
	using Tools;

	internal static class Helpers {
		private const string AS3_AS_OBJECT = "As3AsObject";
		private const string AS3_EVENT_ATTRIBUTE = "As3EventAttribute";
		private const string AS3_NAME_ATTRIBUTE = "As3NameAttribute";
		private const string AS3_NAMESPACE_ATTRIBUTE = "As3NamespaceAttribute";
		private static readonly Dictionary<cs_entity_type, string> _entityTypeRef = new Dictionary<cs_entity_type, string>();
		private static readonly Dictionary<CsTokenType, string> _typeRef = new Dictionary<CsTokenType, string>();
		private static readonly char[] _paramTrim = new[] {',', ' '};

		static Helpers() {
			_typeRef.Add(CsTokenType.tkASSIGN, "=");
			_typeRef.Add(CsTokenType.tkSTAR, "*");
			_typeRef.Add(CsTokenType.tkMUL_EQ, "*=");
			_typeRef.Add(CsTokenType.tkDIV, "/");
			_typeRef.Add(CsTokenType.tkDIV_EQ, "/=");
			_typeRef.Add(CsTokenType.tkMOD, "%");
			_typeRef.Add(CsTokenType.tkMOD_EQ, "%=");
			_typeRef.Add(CsTokenType.tkPLUS, "+");
			//_typeRef.Add(CsTokenType.tkINC, "++");
			_typeRef.Add(CsTokenType.tkDEC, "--");
			_typeRef.Add(CsTokenType.tkPLUS_EQ, "+=");
			_typeRef.Add(CsTokenType.tkMINUS, "-");
			_typeRef.Add(CsTokenType.tkMINUS_EQ, "-=");
			_typeRef.Add(CsTokenType.tkSHIFT_LEFT, "<<");
			_typeRef.Add(CsTokenType.tkSHIFT_RIGHT, ">>");
			_typeRef.Add(CsTokenType.tkLESS, "<");
			_typeRef.Add(CsTokenType.tkGREATER, ">");
			_typeRef.Add(CsTokenType.tkLESS_OR_EQUAL, "<=");
			_typeRef.Add(CsTokenType.tkGREATER_OR_EQUAL, ">=");
			_typeRef.Add(CsTokenType.tkEQUAL, "==");
			_typeRef.Add(CsTokenType.tkNOT_EQ, "!=");
			_typeRef.Add(CsTokenType.tkBITAND, "&");
			_typeRef.Add(CsTokenType.tkXOR, "^");
			_typeRef.Add(CsTokenType.tkBITOR, "|");
			_typeRef.Add(CsTokenType.tkAND, "&&");
			_typeRef.Add(CsTokenType.tkOR, "||");
			_typeRef.Add(CsTokenType.tkQUESTION2, "??");
			_typeRef.Add(CsTokenType.tkCOMPL, "~");
			_typeRef.Add(CsTokenType.tkNOT, "!");

			_typeRef.Add(CsTokenType.tkAS, "as");
			_typeRef.Add(CsTokenType.tkIS, "is");

			_typeRef.Add(CsTokenType.tkVOID, "void");
			_typeRef.Add(CsTokenType.tkOBJECT, "object");
			_typeRef.Add(CsTokenType.tkSTRING, "string");
			_typeRef.Add(CsTokenType.tkBOOL, @"bool");

			_typeRef.Add(CsTokenType.tkINT, "int");
			_typeRef.Add(CsTokenType.tkSHORT, "short");

			_typeRef.Add(CsTokenType.tkUINT, @"uint");
			_typeRef.Add(CsTokenType.tkUSHORT, @"ushort");

			_typeRef.Add(CsTokenType.tkULONG, @"ulong");
			_typeRef.Add(CsTokenType.tkLONG, "long");
			_typeRef.Add(CsTokenType.tkFLOAT, "float");
			_typeRef.Add(CsTokenType.tkDOUBLE, "double");
			_typeRef.Add(CsTokenType.tkDECIMAL, "decimal");

			//_entityTypeRef.Add(cs_entity_type.et_array, "array");
			_entityTypeRef.Add(cs_entity_type.et_string, "string");
			_entityTypeRef.Add(cs_entity_type.et_boolean, @"bool");
			_entityTypeRef.Add(cs_entity_type.et_void, "void");
			_entityTypeRef.Add(cs_entity_type.et_object, "object");

			_entityTypeRef.Add(cs_entity_type.et_uint8, @"uint");
			_entityTypeRef.Add(cs_entity_type.et_uint16, @"uint");
			_entityTypeRef.Add(cs_entity_type.et_uint32, @"uint");
			_entityTypeRef.Add(cs_entity_type.et_uint64, @"uint");

			_entityTypeRef.Add(cs_entity_type.et_int8, "int");
			_entityTypeRef.Add(cs_entity_type.et_int16, "int");
			_entityTypeRef.Add(cs_entity_type.et_int32, "int");
			_entityTypeRef.Add(cs_entity_type.et_int64, "int");

			_entityTypeRef.Add(cs_entity_type.et_float32, "float");
			_entityTypeRef.Add(cs_entity_type.et_float64, "float");
			_entityTypeRef.Add(cs_entity_type.et_decimal, "decimal");

			_entityTypeRef.Add(cs_entity_type.et_unknown, "*UnknownEntityRef*");
		}

		public static List<Expression> GetCallingArguments(CsArgumentList pCsEntityFormalParameter) {
			List<Expression> arguments = new List<Expression>();

			if (pCsEntityFormalParameter == null || pCsEntityFormalParameter.list == null) {
				return arguments;
			}

			arguments.AddRange(
			                   pCsEntityFormalParameter.list.Select(
			                                                        pCsArgument =>
			                                                        FactoryExpressionCreator.Parse(pCsArgument.expression)));

			return arguments;
		}

		public static string Join(this IEnumerable<string> pEnumerable, string pSeparator) {
			if (pEnumerable == null) {
				return string.Empty;
			}

			string[] a = pEnumerable.ToArray();
			return a.Length == 0 ? string.Empty : string.Join(pSeparator, a);
		}

		public static string GetTokenType(CsTokenType pTokenType) {
			if (_typeRef.ContainsKey(pTokenType)) {
				return _typeRef[pTokenType];
			}

			throw new Exception(@"Unknown Typeref: " + pTokenType);
		}

		public static string GetType(CsEntityTypeRef pDirective) {
			if (pDirective == null) {
				return null;
			}

			if (_entityTypeRef.ContainsKey(pDirective.type)) {
				return _entityTypeRef[pDirective.type];
			}

			switch (pDirective.type) {
				case cs_entity_type.et_array:
					return GetType(((CsEntityArraySpecifier)pDirective.u).type) + "[]";

				case cs_entity_type.et_enum:
					return ((CsEntityEnum)pDirective.u).name;

				case cs_entity_type.et_generic_param:
					//CsEntityGenericParam egp = pDirective.u as CsEntityGenericParam;
					//TODO: check generics parameters
					return "*";
					//return egp.name;

				case cs_entity_type.et_genericinst:
					CsEntityInstanceSpecifier eis = pDirective.u as CsEntityInstanceSpecifier;
					if (eis != null) {
						string ret = GetType(eis.type);

						if (eis.arguments != null) {
							List<string> val = new List<string> {"<"};

							foreach (CsEntityTypeRef argument in eis.arguments) {
								val.Add(GetType(argument));
								val.Add(", ");
							}
							val.RemoveAt(val.Count - 1);
							val.Add(">");
							ret += string.Join("", val.ToArray());
						}

						return ret;
					}

					throw new Exception();

				case cs_entity_type.et_class:
					CsEntityInterface itf = pDirective.u as CsEntityInterface;
					if (itf != null) {
						if (itf.interfaces != null && itf.interfaces.Count > 0) {
							List<string> val = new List<string>(itf.interfaces.Count);
							foreach (CsEntityTypeRef typeRef in itf.interfaces) {
								val.Add(GetType(typeRef));
							}

							return string.Join(", ", val.ToArray());
						}

						if (!string.IsNullOrEmpty(itf.name)) {
							return itf.name;
						}
					}

					CsEntityClass cls = pDirective.u as CsEntityClass;
					if (cls != null) {
						if (cls.indexers != null) {
							foreach (CsEntityProperty indexer in cls.indexers) {
								return GetType(indexer.type);
							}
						}

						return cls.name;
					}

					CsEntityDelegate entityDelegate = pDirective.u as CsEntityDelegate;
					if (entityDelegate != null) {
						return "function";
					}

					return "IHaveNOIdeaWhatShouldBeHere";
			}

			throw new Exception("Unknown EntityTypeRef: " + pDirective.type);
		}

		public static string GetType(CsTypeRef pDirective) {
			switch (pDirective.entity_typeref.type) {
				case cs_entity_type.et_array:
					return GetTokenType(pDirective.predefined_type) + "[]";

				case cs_entity_type.et_genericinst:
					return GetType(pDirective.type_name);

				default:
					return pDirective.predefined_type == CsTokenType.tkEOF
					       	? GetType(pDirective.type_name)
					       	: GetTokenType(pDirective.predefined_type);
			}
		}

		public static string GetType(CsUsingNamespaceDirective pDirective) {
			if (pDirective.namespace_or_type_name == null) {
				return "";
			}

			//pDirective.namespace_or_type_entity.name

			string ret = GetType(pDirective.namespace_or_type_name);
			if (!string.IsNullOrEmpty(ret)) {
				ret += ".";
			}

			return ret;
			//+pDirective.namespace_or_type_entity.name;
			//pDirective.namespace_or_type_name.identifier.identifier;
		}

		public static string GetType(CsUsingAliasDirective pDirective) {
			if (pDirective.namespace_or_type_name == null) {
				return "";
			}


			string ret = GetType(pDirective.namespace_or_type_name);
			if (!string.IsNullOrEmpty(ret)) {
				ret += ".";
			}

			return ret + pDirective.namespace_or_type_entity.name;
		}


		public static string GetType(CsNamespaceOrTypeName pDirective) {
			string g = "";
			//generics
			if (pDirective.type_argument_list != null && pDirective.type_argument_list.list.Count != 0) {
				g += "<";
				foreach (CsTypeRef typeRef in pDirective.type_argument_list.list) {
					g += GetType(typeRef);
					g += ", ";
				}

				g = g.TrimEnd(_paramTrim) + ">";
			}

			if (pDirective.namespace_or_type_name == null) {
				if (pDirective.parent is CsTypeRef) {
					CsTypeRef parent = ((CsTypeRef)pDirective.parent);

					if (parent.entity_typeref.u is CsEntityClass) {
						if (IsClassDefinedAsObject(((CsEntityClass)parent.entity_typeref.u).attributes)) {
							return "object";
						}

						return ((CsEntityClass)parent.entity_typeref.u).name;
					}

					if (parent.entity_typeref.type == cs_entity_type.et_generic_param) {
						return "*";
					}

					if (parent.entity_typeref.u is CsEntityInstanceSpecifier) {
						//if (IsClassDefinedAsObject(((CsEntityInstanceSpecifier)parent.entity_typeref.u)))
						//return "Object";
					}
				}

				return pDirective.identifier.identifier + g;
			}


			string ret = GetType(pDirective.namespace_or_type_name);
			if (!string.IsNullOrEmpty(ret)) {
				ret += ".";
			}

			return ret + pDirective.identifier.identifier + g;
		}

		public static bool IsClassDefinedAsObject(CsAttributes pList) {
			return HasAttribute(pList, AS3_AS_OBJECT);
		}

		public static bool IsClassDefinedAsObject(IEnumerable<CsEntityAttribute> pList) {
			return HasAttribute(pList, AS3_AS_OBJECT);
		}

		private static void addImports(IEnumerable<CsEntityAttribute> pList) {
			if (pList == null) {
				return;
			}

			List<object> vals = GetAttributeValue(pList, AS3_NAMESPACE_ATTRIBUTE);
			if (vals.Count == 0) {
				return;
			}

			ImportStatementList.AddImport((string)vals[0]);
		}

		private static void addImports(CsAttributes pList) {
			if (pList == null) {
				return;
			}

			List<object> vals = GetAttributeValue(pList, AS3_NAMESPACE_ATTRIBUTE);
			if (vals.Count == 0) {
				return;
			}

			ImportStatementList.AddImport((string)vals[0]);
		}

		public static List<object> GetAttributeValue(CsAttributes pList, string pAttrName) {
			if (pList == null) {
				return new List<object>();
			}

			if (pList.sections == null || pList.sections.Count == 0) {
				return new List<object>();
			}

			return
				pList.sections.SelectMany(
				                          pSection =>
				                          pSection.attribute_list.Select(
				                                                         pAttribute =>
				                                                         pAttribute.entities == null
				                                                         	? getAttributeValue(pAttribute, pAttrName)
				                                                         	: GetAttributeValue(pAttribute.entities, pAttrName)).Where
				                          	(pVal => pVal != null)).FirstOrDefault();
		}

		private static List<object> getAttributeValue(CsAttribute pAttribute, string pAttrName) {
			string s;
			List<object> values = new List<object>();

			if (pAttribute.attribute_name != null) {
				CsNamespaceOrTypeName n = (CsNamespaceOrTypeName)pAttribute.attribute_name;
				s = n.identifier.original_text;
			} else {
				throw new Exception();
				//s = attribute.type.parent.name;
			}

			if (s.Equals(pAttrName, StringComparison.Ordinal) || (s + "Attribute").Equals(pAttrName, StringComparison.Ordinal)) {
				foreach (var argument in pAttribute.positional_argument_list.list) {
					values.Add(((CsLiteral)argument).literal);
				}
			}

			return values;
		}

		public static List<object> GetAttributeValue(IEnumerable<CsEntityAttribute> pList, string pAttrName) {
			List<object> values = new List<object>();

			if (pList == null) {
				return values;
			}

			foreach (CsEntityAttribute attribute in pList) {
				string s;
				if (attribute.decl != null) {
					CsNamespaceOrTypeName n = (CsNamespaceOrTypeName)attribute.decl.attribute_name;
					s = n.identifier.original_text;
				} else if (attribute.type.parent != null && attribute.type.parent is CsEntityClass) {
					s = attribute.type.parent.name;
				} else {
					continue;
				}

				if (!s.Equals(pAttrName, StringComparison.Ordinal) && !(s + "Attribute").Equals(pAttrName, StringComparison.Ordinal)) {
					continue;
				}

				values.AddRange(attribute.fixed_arguments.Select(pArgument => pArgument.value));
				return values;
			}

			return values;
		}

		public static bool HasAttribute(CsAttributes pList, string pAttrName) {
			if (pList == null) {
				return false;
			}

			if (pList.sections == null || pList.sections.Count == 0) {
				return false;
			}

			return (from section in pList.sections
			        from attribute in section.attribute_list
			        select HasAttribute(attribute.entities, pAttrName)).Any(pVal => pVal);
		}

		public static string EscapeString(string pIn) {
			return "\"" + pIn.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
		}


		public static string GetRealName(CsExpression pExpression, string pName) {
			return getRealName(pExpression, pName);
		}

		public static string GetRealName(CsClassStruct pExpression, string pName) {
			return getRealName(pExpression.entity, pName);
		}

		public static string GetRealName(CsMethod pExpression, string pName) {
			return getRealName(pExpression.attributes, pName);
		}

		public static string GetRealName(CsIndexer pIndexer, string pName) {
			return getRealName(pIndexer.attributes, pName);
		}

		public static string GetRealName(CsVariableDeclarator pExpression, string pIdentifier) {
			return getRealName(pExpression.entity, pIdentifier);
		}

		public static string GetRealName(CsConstantDeclarator pExpression, string pIdentifier) {
			return getRealName(pExpression.entity, pIdentifier);
		}

		public static string GetRealName(CsProperty pCsProperty, string pIdentifier) {
			return getRealName(pCsProperty.entity, pIdentifier);
		}

		public static string GetRealName(CsEntity pCsProperty, string pRealName) {
			return getRealName(pCsProperty.e, pRealName);
		}

		private static string getRealName(object pEntity, string pName) {
			if (pName.Equals("ToString", StringComparison.Ordinal)) {
				pName = "toString";
			}

			if (pEntity is CsAttributes) {
				string n = (string)GetAttributeValue((CsAttributes)pEntity, AS3_NAME_ATTRIBUTE)[0];
				return string.IsNullOrEmpty(n) ? pName : n;
			}

			IEnumerable<CsEntityAttribute> m;

			if (pEntity is CsEntityClass) {
				return getRealNameFromAttr(((CsEntityClass)pEntity).attributes, pName);
			}

			if (pEntity is CsEntityVariable) {
				m = ((CsEntityVariable)pEntity).attributes;
				addImports(m);
				return getRealNameFromAttr(m, pName);
			}

			if (pEntity is CsEntityEnum) {
				return getRealNameFromAttr(((CsEntityEnum)pEntity).attributes, pName);
			}

			if (pEntity is CsEntityStruct) {
				return getRealNameFromAttr(((CsEntityStruct)pEntity).attributes, pName);
			}

			if (pEntity is CsEntityConstant) {
				return getRealNameFromAttr(((CsEntityConstant)pEntity).attributes, pName);
			}

			if (pEntity is CsEntityMethod) {
				m = ((CsEntityMethod)pEntity).attributes;
				addImports(m);
				return getRealNameFromAttr(m, pName);
			}

			return pName;
		}

		private static string getRealNameFromAttr(IEnumerable<CsEntityAttribute> pList, string pName) {
			if (pList == null) {
				return pName;
			}

			List<object> vals = GetAttributeValue(pList, AS3_NAME_ATTRIBUTE);
			if (vals.Count == 0) {
				return pName;
			}

			string n = (string)vals[0];
			return string.IsNullOrEmpty(n) ? pName : n;
		}

		public static string GetEventFromAttr(CsAttributes pList) {
			addImports(pList);

			List<object> vals = GetAttributeValue(pList, AS3_EVENT_ATTRIBUTE);
			return vals.Count == 0 ? null : (string)vals[0];
		}

		public static string GetEventFromAttr(IEnumerable<CsEntityAttribute> pList) {
			addImports(pList);

			List<object> vals = GetAttributeValue(pList, AS3_EVENT_ATTRIBUTE);
			return vals.Count == 0 ? null : (string)vals[0];
		}

		public static bool HasAttribute(IEnumerable<CsEntityAttribute> pList, string pAttrName) {
			return pList != null &&
			       pList.Any(pAttribute => pAttribute.type.parent.name.Equals(pAttrName, StringComparison.Ordinal));
		}

		public static string GetType(CsNode pDirective) {
			if (pDirective == null) {
				return "";
			}

			if (pDirective is CsNamespaceOrTypeName) {
				return GetType((CsNamespaceOrTypeName)pDirective);
			}

			if (pDirective is CsUsingNamespaceDirective) {
				return GetType((CsUsingNamespaceDirective)pDirective);
			}

			if (pDirective is CsTypeRef) {
				return GetType((CsTypeRef)pDirective);
			}

			if (pDirective is CsUsingAliasDirective) {
				return GetType((CsUsingAliasDirective)pDirective);
			}

			throw new Exception("Unsupported node type");
		}

		public static List<string> GetModifiers(CsModifiers pModifiers) {
			List<string> mods = new List<string>();

			if ((pModifiers.flags & (uint)CsModifierEnum.mABSTRACT) != 0) {
				mods.Add("abstract");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mEXTERN) != 0) {
				mods.Add("extern");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mINTERNAL) != 0) {
				mods.Add("internal");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mNEW) != 0) {
				mods.Add("new");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mOVERRIDE) != 0) {
				mods.Add("override");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mPARTIAL) != 0) {
				mods.Add("partial");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mPRIVATE) != 0) {
				mods.Add("private");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mPROTECTED) != 0) {
				mods.Add("protected");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mPUBLIC) != 0) {
				mods.Add("public");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mREADONLY) != 0) {
				mods.Add(@"readonly");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mSEALED) != 0) {
				mods.Add("sealed");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mSTATIC) != 0) {
				mods.Add("static");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mUNSAFE) != 0) {
				mods.Add(@"unsafe");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mVIRTUAL) != 0) {
				mods.Add(@"virtual");
			}
			if ((pModifiers.flags & (uint)CsModifierEnum.mVOLATILE) != 0) {
				mods.Add(@"volatile");
			}

			return mods;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text;

namespace TableGenerator
{
	public class cLexem
	{
		public static readonly string cc_Epsilon = "ε";
		public static readonly string cc_Stop = "$";
		public static readonly cLexem cc_EpsilonLexem = new cLexem(cc_Epsilon, eLexType.Epsilon);
		public static readonly cLexem cc_StopLexem = new cLexem(cc_Stop, eLexType.Stop);

		public static readonly Dictionary<string, cLexem> cf_LexemDic = new Dictionary<string, cLexem>();

		public readonly string cf_Name;
		eLexType cf_type;

		public int cf_EpsilonCount;

		List<cProduction> cf_listProducts = new List<cProduction>();

		cSet<cLexem> cf_firstCache;

		public cSet<cLexem> cp_FirstCache
		{
			get
			{
				if (cf_firstCache == null)
				{
					cm_CacheFirst();
				}
				return cf_firstCache;
			}
		}

		private cLexem(string a_name, eLexType a_type)
		{
			cf_firstCache = null;
			cf_EpsilonCount = 0;
			cf_Name = a_name;
			cf_type = a_type;
			if (a_name == cc_Epsilon)
				cf_type = eLexType.Epsilon;
		}

		public static cLexem cm_ExtendGrammatic(cLexem a_root)
		{
			cLexem _newRoot = cLexem.cm_GetLexem(" E'");
			_newRoot.cf_type = eLexType.NonTerminal;
			_newRoot.cm_AddChildLexem(a_root, true);
			//_newRoot.cm_AddChildLexem(cLexem.cc_StopLexem, false);
			return _newRoot;
		}

		public eLexType cp_Type
		{
			get
			{
				return cf_type;
			}
			set
			{
				if (cf_type == eLexType.Terminal)
				{
					cf_type = value;
				}
			}
		}

		public List<cProduction> cp_ListProducts
		{
			get
			{
				return cf_listProducts;
			}
		}

		public static cLexem cm_GetLexem(string a_name)
		{
			if (a_name == cc_Epsilon)
			{
				return cc_EpsilonLexem;
			}
			if (a_name == cc_Stop)
			{
				return cc_StopLexem;
			}
			cLexem _retLex = null;
			if (cf_LexemDic.ContainsKey(a_name))
			{
				_retLex = cf_LexemDic[a_name];
			}
			else
			{
				_retLex = new cLexem(a_name, eLexType.Terminal);
				cf_LexemDic.Add(a_name, _retLex);
			}
			return _retLex;
		}

		public void cm_AddChildLexem(cLexem a_lexem, bool a_newProduct)
		{
			if (a_lexem.cp_Type == eLexType.Action)
			{
				throw new Exception("Попытка добавить к продукции лексемы " + cf_Name + " лексему типа " + a_lexem.cp_Type.ToString());
			}
			if (a_newProduct)
			{
				cProduction _production = new cProduction(this);
				_production.cm_Add(a_lexem);
				cf_listProducts.Add(_production);
			}
			else
			{
				cProduction _production = cf_listProducts[cf_listProducts.Count - 1];
				_production.cm_Add(a_lexem);
			}
		}

		public void cm_AddAction(cLexem a_action)
		{
			if (a_action.cp_Type != eLexType.Action)
			{
				throw new Exception("Попытка добавить к лексеме " + cf_Name + " в качестве действия лексему типа " + cp_Type.ToString());
			}
			cProduction _production = cf_listProducts[cf_listProducts.Count - 1];
			_production.cm_AddAction(a_action);
		}

		public bool cp_HasEpsilonProduct
		{
			get
			{
				return cf_EpsilonCount > 0;
			}
		}

		public override string ToString()
		{
			return cf_Name;
		}

		//private static cSet<cLexem> cm_FirstInList(List<cLexem> a_listLexem)
		//{
		//    cSet<cLexem> _retSet = new cSet<cLexem>();
		//    if (a_listLexem.Count > 0)
		//    {
		//        int _pos = 0;
		//        cLexem _currLexem = a_listLexem[_pos];
		//        do
		//        {
		//            _retSet.Remove(cLexem.cc_EpsilonLexem);
		//            _currLexem = a_listLexem[_pos++];
		//            _currLexem.cm_First(_retSet);
		//        } while (_retSet.Contains(cLexem.cc_EpsilonLexem) && (_pos < a_listLexem.Count));
		//    }
		//    return _retSet;
		//}

		//public cSet<cLexem> cm_First(cSet<cLexem> a_set)
		//{
		//    if (cf_firstCache == null)
		//    {
		//        cSet<cLexem> _retSet = a_set ?? new cSet<cLexem>();
		//        switch (cf_type)
		//        {
		//            case (eLexType.Epsilon):
		//            case (eLexType.Stop):
		//            case (eLexType.Terminal):
		//                _retSet.Add(this);
		//                break;
		//            case (eLexType.NonTerminal):
		//                if (cf_EpsilonCount > 0)
		//                {
		//                    _retSet.Add(cc_EpsilonLexem);
		//                }
		//                bool _added = false;
		//                do
		//                {
		//                    _added = false;
		//                    foreach (cProduction _production in cf_listProducts)
		//                    {
		//                        if (_production.cp_RightPart.Count > 0)
		//                        {
		//                            int _pos = 0;
		//                            cLexem _currLexem = _production.cp_RightPart[_pos];
		//                            do
		//                            {
		//                                _retSet.Remove(cLexem.cc_EpsilonLexem);
		//                                _currLexem = _production.cp_RightPart[_pos++];
		//                                if (_currLexem != this)
		//                                {
		//                                    _currLexem.cm_First(_retSet);
		//                                }
		//                                else
		//                                {
		//                                    break;
		//                                    //if (_retSet.Contains(cLexem.cc_EpsilonLexem))
		//                                    //{
		//                                    //    //    int _countBefore=_retSet.Count;
		//                                    //    //    for
		//                                    //}
		//                                    //else
		//                                    //{
		//                                    //    break;
		//                                    //}
		//                                }
		//                            } while (_retSet.Contains(cLexem.cc_EpsilonLexem) && (_pos < _production.cp_RightPart.Count));
		//                        }
		//                    }
		//                } while (_added);
		//                break;
		//            default:
		//                throw new Exception("Передан некорректный тип в cm_First: " + cf_type.ToString());
		//        }
		//        cf_firstCache = _retSet.Clone() as cSet<cLexem>;
		//        return _retSet;
		//    }
		//    else
		//    {
		//        if (a_set == null)
		//        {
		//            return cf_firstCache.Clone() as cSet<cLexem>;
		//        }
		//        else
		//        {
		//            a_set.AddRange(cf_firstCache);
		//            return a_set;
		//        }
		//    }
		//}

		public static void cm_CacheFirst()
		{
			Dictionary<cLexem, cSet<cLexem>> _firstPerLexem = new Dictionary<cLexem, cSet<cLexem>>();

			foreach (cLexem _lexem in cf_LexemDic.Values)
			{
				_firstPerLexem.Add(_lexem, new cSet<cLexem>());
			}
			_firstPerLexem.Add(cLexem.cc_EpsilonLexem, new cSet<cLexem>());
			_firstPerLexem.Add(cLexem.cc_StopLexem, new cSet<cLexem>());

			bool _added = false;
			do
			{
				_added = false;
				foreach (cLexem _lexem in _firstPerLexem.Keys)
				{
					switch (_lexem.cf_type)
					{
						case eLexType.Epsilon:
						case eLexType.Stop:
						case eLexType.Terminal:
							_added |= _firstPerLexem[_lexem].Add(_lexem);
							break;
						case eLexType.NonTerminal:
							foreach (cProduction _production in _lexem.cf_listProducts)
							{
								if (_production.cp_RightPart.Count > 0)
								{
									int _pos = 0;
									cLexem _currLexem = _production.cp_RightPart[_pos];
									bool _hasEpsilon = false;
									do
									{
										_currLexem = _production.cp_RightPart[_pos];
										_hasEpsilon = false;
										cSet<cLexem> _currentSet = _firstPerLexem[_currLexem];
										_hasEpsilon |= _currentSet.Contains(cLexem.cc_EpsilonLexem);
										if (_hasEpsilon)
										{
											_currentSet = _currentSet.Clone() as cSet<cLexem>;
											_currentSet.Remove(cLexem.cc_EpsilonLexem);
										}
										_added |= _firstPerLexem[_lexem].AddRange(_currentSet);
										_pos++;
									} while (_hasEpsilon && (_pos < _production.cp_RightPart.Count));
									if (_hasEpsilon)
									{
										_added |= _firstPerLexem[_lexem].Add(cLexem.cc_EpsilonLexem);
									}
								}
							}
							break;
						case eLexType.Action:
							break;
						default:
							throw new Exception("Некорректный тип в cm_CacheFirst: " + _lexem.cf_type.ToString());
					}
				}
			} while (_added);

			foreach (cLexem _lexem in _firstPerLexem.Keys)
			{
				_lexem.cf_firstCache = _firstPerLexem[_lexem];
			}
		}
	}

    public enum eLexType
    {
        Terminal,
        NonTerminal,
        Epsilon,
        Action,
		Stop
    }
}

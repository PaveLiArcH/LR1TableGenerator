using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TableGenerator
{
	public class cLRTableGenerator
	{
		cLexem cf_root = null;
		bool cf_valid = false;
		Dictionary<cLexem, cSet<cLexem>> cf_follow = null;

		List<cSet<cConfiguration>> cf_listItems = null;
		Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>> cf_gotoResults = null;

		public cLexem cp_Root
		{
			get
			{
				return cf_root;
			}
		}

		public ICollection<cLexem> cp_Lexems
		{
			get
			{
				return cLexem.cf_LexemDic.Values;
			}
		}

		public void cm_Init(cParser a_parser)
		{
			cf_valid = false;
			cf_root = cLexem.cm_ExtendGrammatic(a_parser.cp_Root);

			// Заполнение множества FOLLOW
			cf_follow = cm_Follow();

			//cLexem _lexem = cLexem.cm_GetLexem("Es");
			//cLexem _lexem2 = cLexem.cm_GetLexem("E");
			//cLexem _lexem3 = cLexem.cm_GetLexem("+");
			//cSet<cConfiguration> _test = cConfiguration.cm_Goto(new cSet<cConfiguration>() { cConfiguration.cm_GetConfiguration(_lexem.cp_ListProducts[0], 1), cConfiguration.cm_GetConfiguration(_lexem2.cp_ListProducts[0], 1) }, _lexem3);
			//int i;
			//i = 1;
			KeyValuePair<List<cSet<cConfiguration>>, Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>>> _pair = cm_Items(cf_root);
			cf_listItems = _pair.Key;
			cf_gotoResults = _pair.Value;

			cf_valid = true;
		}

		private cSet<cLexem> cm_First(List<cLexem> a_listLexem)
		{
			cSet<cLexem> _retSet = new cSet<cLexem>();
			if (a_listLexem.Count > 0)
			{
				int _pos = a_listLexem.Count-1;
				cLexem _currLexem = a_listLexem[_pos];
				do
				{
					_retSet.Remove(cLexem.cc_EpsilonLexem);
					_currLexem = a_listLexem[_pos--];
					_currLexem.cm_First(_retSet);
				} while (_retSet.Contains(cLexem.cc_EpsilonLexem) && (_pos > 0));
			}
			return _retSet;
		}

		private bool cm_dataTableAdd(DataTable a_dataTable, int a_rowNum, string a_column, string a_value)
		{
			bool _retVal = true;
			if (!a_dataTable.Columns.Contains(a_column))
			{
				DataColumn _column = a_dataTable.Columns.Add(a_column);
			}
			if (a_dataTable.Rows[a_rowNum][a_column] is System.DBNull)
			{
				a_dataTable.Rows[a_rowNum][a_column] = a_value;
			}
			else
			{
				_retVal = false;
				throw new Exception("Не удалось сгенерировать SLR-таблицу ввиду неразрешимых конфликтов");
			}
			return _retVal;
		}

		public DataTable cm_GenerateTable()
		{
			DataTable _retDataTable = new DataTable();

			Dictionary<cSet<cConfiguration>, int> _jumpDictionary = new Dictionary<cSet<cConfiguration>, int>();
			int _i=0;
			string _States=" States";
			_retDataTable.Columns.Add(_States);
			foreach (cSet<cConfiguration> _item in cf_listItems)
			{
				_jumpDictionary.Add(_item, _i);
				_retDataTable.Rows.Add(new object[] { _i });
				_i++;
			}

			Dictionary<cProduction, int> _productsDictionary = new Dictionary<cProduction, int>();
			_i = 0;
			foreach (cLexem _lexem in cp_Lexems)
			{
				foreach (cProduction _production in _lexem.cp_ListProducts)
				{
					_productsDictionary.Add(_production, _i++);
				}
			}

			foreach (cSet<cConfiguration> _item in cf_listItems)
			{
				int _itemIndex=_jumpDictionary[_item];
				foreach (cConfiguration _configuration in _item)
				{
					if (_configuration.cf_Production.cp_RightPart.Count > _configuration.cf_Position)
					{
						cLexem _lexem = _configuration.cf_Production.cp_RightPart[_configuration.cf_Position];
						if (_lexem.cp_Type == eLexType.Terminal)
						{
							cm_dataTableAdd(_retDataTable, _itemIndex, _lexem.cf_Name, "S" + _jumpDictionary[cf_gotoResults[_item][_lexem]].ToString());
						}
					}
					else
					{
						if (_configuration.cf_Production.cp_Root != cf_root)
						{
							cSet<cLexem> _follow = cf_follow[_configuration.cf_Production.cp_Root];
							foreach (cLexem _lexem in _follow)
							{
								string _str = "R" + _productsDictionary[_configuration.cf_Production].ToString();
								foreach (cLexem _action in _configuration.cf_Production.cp_ActionList)
								{
									_str += " {" + _action.ToString() +"}";
								}
								cm_dataTableAdd(_retDataTable, _itemIndex, _lexem.cf_Name, _str);
							}
						}
						else
						{
							cm_dataTableAdd(_retDataTable, _itemIndex, cLexem.cc_StopLexem.cf_Name, "A");
						}
					}
				}
				if (cf_gotoResults.ContainsKey(_item))
				{
					foreach (cLexem _lexem in cf_gotoResults[_item].Keys)
					{
						if (_lexem.cp_Type==eLexType.NonTerminal)
						{
							cm_dataTableAdd(_retDataTable, _itemIndex, _lexem.cf_Name, _jumpDictionary[cf_gotoResults[_item][_lexem]].ToString());
						}
					}
				}
			}

			return _retDataTable;
		}

		private Dictionary<cLexem, cSet<cLexem>> cm_Follow()
		{
			Dictionary<cLexem, cSet<cLexem>> _retDic = new Dictionary<cLexem, cSet<cLexem>>();
			foreach (cLexem _nonTerminal in cp_Lexems)
			{
				if (_nonTerminal.cp_Type == eLexType.NonTerminal)
				{
					_retDic[_nonTerminal] = new cSet<cLexem>();
				}
			}

			_retDic[cf_root].Add(cLexem.cc_StopLexem);

			// 2
			foreach (cLexem _nonTerminal in cp_Lexems)
			{
				if (_nonTerminal.cp_Type == eLexType.NonTerminal)
				{
					foreach (cProduction _production in _nonTerminal.cp_ListProducts)
					{
						int _count = _production.cp_RightPart.Count;
						List<cLexem> _revListProduct = new List<cLexem>();
						for (int i = _count - 1; i >= 0; i--)
						{
							cLexem _lex = _production.cp_RightPart[i];
							switch (_lex.cp_Type)
							{
								case eLexType.NonTerminal:
									cSet<cLexem> _first = cm_First(_revListProduct);
									_first.Remove(cLexem.cc_EpsilonLexem);
									_retDic[_lex].AddRange(_first);
									break;
								default:
									break;
							}
							_revListProduct.Add(_lex);
						}
					}
				}
			}

			// 3
			bool _added = true;
			while (_added)
			{
				_added = false;
				foreach (cLexem _nonTerminal in cp_Lexems)
				{
					if (_nonTerminal.cp_Type == eLexType.NonTerminal)
					{
						foreach (cProduction _production in _nonTerminal.cp_ListProducts)
						{
							int _count = _production.cp_RightPart.Count;
							List<cLexem> _revListProduct = new List<cLexem>();
							bool _break = false;
							for (int i = _count - 1; i >= 0; i--)
							{
								cLexem _lex = _production.cp_RightPart[i];
								switch (_lex.cp_Type)
								{
									case eLexType.NonTerminal:
										cSet<cLexem> _first = cm_First(_revListProduct);
										if (_first.Contains(cLexem.cc_EpsilonLexem))
										{
											_production.cp_Root.cm_First(_first);
										}
										_added = _retDic[_lex].AddRange(_retDic[_nonTerminal]) | _added;
										break;
									case eLexType.Action:
										break;
									default:
										_break = true;
										break;
								}
								_revListProduct.Add(_lex);
								if (_break) break;
							}
						}
					}
				}
			}

			return _retDic;
		}

		private bool cm_addToGoto(Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>> a_goto, cSet<cConfiguration> a_item, cLexem a_lexem, cSet<cConfiguration> a_gotoItem)
		{
			bool _retVal;
			if (!a_goto.ContainsKey(a_item))
			{
				a_goto.Add(a_item, new Dictionary<cLexem, cSet<cConfiguration>>());
			}
			if (!a_goto[a_item].ContainsKey(a_lexem))
			{
				a_goto[a_item].Add(a_lexem, a_gotoItem);
				_retVal = true;
			}
			else
			{
				_retVal = false;
				throw new Exception("Попытка заменить значение в дереве Goto при построении списка пунктов");
			}
			return _retVal;
		}

		private bool cm_addItemGroup(List<cSet<cConfiguration>> a_items, cSet<cConfiguration> a_item, cLexem a_lexem, cSet<cConfiguration> a_set, Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>> a_goto)
		{
			bool _notSubset = true;
			foreach (cSet<cConfiguration> _set in a_items)
			{
				if (_set.IsSupersetOf(a_set))
				{
					_notSubset = false;
					cm_addToGoto(a_goto, a_item, a_lexem, _set);
					break;
				}
			}
			if (_notSubset)
			{
				a_items.Add(a_set);
				cm_addToGoto(a_goto, a_item, a_lexem, a_set);
			}
			return _notSubset;
		}

		private cSet<cLexem> cm_getAllPossibleGotoLexems(cSet<cConfiguration> a_set)
		{
			cSet<cLexem> _retSet=new cSet<cLexem>();
			foreach (cConfiguration _configuration in a_set)
			{
				if (_configuration.cf_Production.cp_RightPart.Count > _configuration.cf_Position)
				{
					_retSet.Add(_configuration.cf_Production.cp_RightPart[_configuration.cf_Position]);
				}
			}
			return _retSet;
		}

		private KeyValuePair<List<cSet<cConfiguration>>, Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>>> cm_Items(cLexem a_root)
		{
			List<cSet<cConfiguration>> _retList = new List<cSet<cConfiguration>>();

			cConfiguration _rootConfiguration=cConfiguration.cm_GetConfiguration(a_root.cp_ListProducts[0], 0);
			cSet<cConfiguration> _rootClosure = cConfiguration.cm_Closure(new cSet<cConfiguration>() { _rootConfiguration });

			Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>> _gotoResults = new Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>>();

			_retList.Add(_rootClosure);

			bool _added=false;
			int _lastChecked=0;
			do
			{
				_added = false;
				for (; _lastChecked < _retList.Count; _lastChecked++)
				{
					cSet<cConfiguration> _currentItem = _retList[_lastChecked];
					cSet<cLexem> _currentChecked = cm_getAllPossibleGotoLexems(_currentItem);
					foreach (cLexem _lexem in _currentChecked)
					{
						cSet<cConfiguration> _possibleItem = cConfiguration.cm_Goto(_currentItem, _lexem);
						if (cm_addItemGroup(_retList, _currentItem, _lexem, _possibleItem, _gotoResults))
						{
							_added = true;
						}
					}
				}

			} while (_added);

			return new KeyValuePair<List<cSet<cConfiguration>>, Dictionary<cSet<cConfiguration>, Dictionary<cLexem, cSet<cConfiguration>>>>(_retList, _gotoResults);
		}
	}
}

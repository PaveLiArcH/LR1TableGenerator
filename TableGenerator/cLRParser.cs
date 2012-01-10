using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace TableGenerator
{
	public class cLRParser : cParser
	{
		private cScanner cf_scanner;
		private DataTable cf_dataTable;

		private List<cToken> cf_lisTokens = new List<cToken>();

		private cLexem cf_root = null;
		private cLexem cf_leftLex = null;
		private cLexem cf_firstLex = null;


		public cLRParser(cScanner a_scanner, string a_filename)
		{
			cf_scanner = a_scanner;
			cf_dataTable = new DataTable("table");
			cf_dataTable.ReadXml(a_filename);
		}

		public override cLexem cp_Root
		{
			get { return cf_root; }
		}

		public override void cm_Parse()
		{
			cLexem.cf_LexemDic.Clear();

			Stack<object> _s = new Stack<object>();
			_s.Push(0);

			cToken _token = cf_scanner.cm_GetNextToken();
			cf_lisTokens.Add(_token);
			string _lexem = _token.cf_Type.ToString();

			bool _finished = false, _error = false;
			//while (!_finished & !_error & (_token != null))
			while (!_finished & !_error)
			{
				int _num = 0;
				try
				{
					_num = (int)_s.Peek();
				}
				catch
				{
					throw new cParserException(cf_lisTokens, new string[] { });
				}
				DataRow _row = cf_dataTable.Rows[_num];
				object _value = _row[_lexem];
				if (!(_value is System.DBNull))
				{
					string _val = _value as String;
					string[] _splitVal = _val.Split(new char[] { cLRTableGenerator.cc_Separator }, StringSplitOptions.RemoveEmptyEntries);
					if (_splitVal.Length > 0)
					{
						string _do = _splitVal[0];
						char _c = _do[0];
						switch (_c)
						{
							case 'S':
								_s.Push(_lexem);
								int _sNum = 0;
								if (!Int32.TryParse(_do.TrimStart(new char[] { 'S' }), out _sNum))
								{
									throw new cParserException(cf_lisTokens, new string[] { });
								}
								_s.Push(_sNum);
								_token = cf_scanner.cm_GetNextToken()??cToken.cc_NullToken;
								cf_lisTokens.Add(_token);
								_lexem = _token.cm_TypeToStr();
								break;
							case 'R':
								uint _rNum = 0;
								if (!UInt32.TryParse(_do.TrimStart(new char[] { 'R' }), out _rNum))
								{
									throw new cParserException(cf_lisTokens, new string[] { });
								}
								//_lexem=cm_doR(_rNum, _s);
								cm_stackPop(_s, _rNum * 2);
								_lexem = _splitVal[1];
								cm_doAction(_splitVal);
								break;
							case 'A':
								_finished = true;
								break;
							default:
								_s.Push(_lexem);
								int _numN = 0;
								if (!Int32.TryParse(_do, out _numN))
								{
									throw new cParserException(cf_lisTokens, new string[] { });
								}
								_s.Push(_numN);
								//_token = cf_scanner.cm_GetNextToken();
								//cf_lisTokens.Add(_token);
								_lexem = _token.cm_TypeToStr();
								break;
						}
					}
					else
					{
						throw new cParserException(cf_lisTokens, new string[] { });
					}
				}
				else
				{
					throw new cParserException(cf_lisTokens, new string[] { });
				}
			}
		}

		private void cm_stackPop(Stack<object> a_stack, uint a_count)
		{
			for (uint i = 0; i < a_count; i++)
			{
				try
				{
					a_stack.Pop();
				}
				catch
				{
					throw new Exception("Ошибка в действии R");
				}
			}
		}

		private string cm_doR(int a_rNum, Stack<object> a_stack)
		{
			string _retString;
			switch (a_rNum)
			{
				//<грамматика> -> <продукция> <список_продукций> 
				case 0:
					cm_stackPop(a_stack, 2 * 2);
					_retString = "грамматика";
					break;
				//<продукция> -> <левая_часть> <правая_часть> перевод_строки
				case 1:
					cm_stackPop(a_stack, 2 * 3);
					_retString = "продукция";
					break;
				//<список_продукций> -> <продукция> <список_продукций>
				case 2:
					cm_stackPop(a_stack, 2 * 2);
					_retString = "список_продукций";
					break;
				//<список_продукций> -> ε 
				case 3:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "список_продукций";
					break;
				//<левая_часть> -> лексема стрелка {A1}
				case 4:
					cm_stackPop(a_stack, 2 * 2);
					_retString = "левая_часть";
					break;
				//<правая_часть> -> <первая_лексема> <список_лексем> <список_действий>
				case 5:
					cm_stackPop(a_stack, 2 * 3);
					_retString = "правая_часть";
					break;
				//<правая_часть> -> пустая_лексема {A3}
				case 6:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "правая_часть";
					break;
				//<первая_лексема> -> лексема {A2}
				case 7:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "первая_лексема";
					break;
				//<список_лексем> -> <последующая_лексема> <список_лексем>
				case 8:
					cm_stackPop(a_stack, 2 * 2);
					_retString = "список_лексем";
					break;
				//<список_лексем> -> ε
				case 9:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "список_лексем";
					break;
				//<список_действий> -> <очередное_действие> <список_действий>
				case 10:
					cm_stackPop(a_stack, 2 * 2);
					_retString = "список_действий";
					break;
				//<список_действий> -> ε
				case 11:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "список_действий";
					break;
				//<последующая_лексема> -> лексема {A4}
				case 12:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "последующая_лексема";
					break;
				//<очередное_действие> -> действие {A5}
				case 13:
					cm_stackPop(a_stack, 2 * 1);
					_retString = "очередное_действие";
					break;
				////< E'> -> <грамматика> 
				default:
					throw new Exception("Неизвестный номер продукции в действии R");
			}
			return _retString;
		}

		private void cm_doAction(string[] a_action)
		{
			for (int i = 2; i < a_action.Length; i++)
			{
				switch (a_action[i])
				{
					case "A1":
						cm_doA1();
						break;
					case "A2":
						cm_doA2();
						break;
					case "A3":
						cm_doA3();
						break;
					case "A4":
						cm_doA4();
						break;
					case "A5":
						cm_doA5();
						break;
					default:
						throw new Exception("Неизвестное действие в таблице синтаксического анализа.");
				}
			}
		}

		private void cm_doA1()
		{
			cf_leftLex = cf_lisTokens[cf_lisTokens.Count - 3].cf_Value as cLexem;
			cf_leftLex.cp_Type = eLexType.NonTerminal;
			if (cf_root == null)
			{
				cf_root = cf_leftLex;
			}
		}

		private void cm_doA2()
		{
			cLexem _lexem = cf_lisTokens[cf_lisTokens.Count - 2].cf_Value as cLexem;
			cf_leftLex.cm_AddChildLexem(_lexem as cLexem, true);
			cf_firstLex = _lexem;
		}

		private void cm_doA3()
		{
			cf_leftLex.cm_AddChildLexem(cLexem.cc_EpsilonLexem, true);
		}

		private void cm_doA4()
		{
			cLexem _lexem = cf_lisTokens[cf_lisTokens.Count - 2].cf_Value as cLexem;
			cf_leftLex.cm_AddChildLexem(_lexem as cLexem, false);
		}
		private void cm_doA5()
		{
			cLexem _lexem = cf_lisTokens[cf_lisTokens.Count - 2].cf_Value as cLexem;
			cf_leftLex.cm_AddAction(_lexem as cLexem);
		}
	}
}

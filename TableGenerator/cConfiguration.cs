using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGenerator
{
	public class cConfiguration
	{
		static Dictionary<cProduction, Dictionary<int, Dictionary<cLexem, cConfiguration>>> cf_configurationsCache = new Dictionary<cProduction, Dictionary<int, Dictionary<cLexem, cConfiguration>>>();

		public readonly cProduction cf_Production;
		public readonly int cf_Position;
		public readonly cLexem cf_Terminal;

		private cConfiguration(cProduction a_production, int a_position, cLexem a_terminal)
		{
			cf_Production = a_production;
			cf_Position = a_position;
			cf_Terminal = a_terminal;
		}

		public static cConfiguration cm_GetConfiguration(cProduction a_production, int a_position, cLexem a_terminal)
		{
			if (!cf_configurationsCache.ContainsKey(a_production))
			{
				cf_configurationsCache.Add(a_production, new Dictionary<int,Dictionary<cLexem,cConfiguration>>());
			}
			if (!cf_configurationsCache[a_production].ContainsKey(a_position))
			{
				cf_configurationsCache[a_production].Add(a_position,new Dictionary<cLexem, cConfiguration>());
			}
			if (!cf_configurationsCache[a_production][a_position].ContainsKey(a_terminal))
			{
				if (a_production.cp_EpsilonProduct & (a_position != 0))
				{
					throw new Exception("Попытка создать некорректную конфигурацию из эпсилон-продукции");
				}
				cf_configurationsCache[a_production][a_position].Add(a_terminal, new cConfiguration(a_production, a_position, a_terminal));
			}
			return cf_configurationsCache[a_production][a_position][a_terminal];
		}

		public static cSet<cLexem> cm_FirstTerminals(List<cLexem> a_listLexem)
		{
			cSet<cLexem> _tempSet = new cSet<cLexem>();
			if (a_listLexem.Count > 0)
			{
				int _pos = 0;
				cLexem _currLexem = a_listLexem[_pos];
				do
				{
					_tempSet.Remove(cLexem.cc_EpsilonLexem);
					_currLexem = a_listLexem[_pos];
					_tempSet.UnionWith(_currLexem.cp_FirstCache);
					_pos++;
				} while (_tempSet.Contains(cLexem.cc_EpsilonLexem) && (_pos < a_listLexem.Count));
			}
			cSet<cLexem> _retSet = new cSet<cLexem>();
			foreach (cLexem _lexem in _tempSet)
			{
				if (_lexem.cp_Type != eLexType.NonTerminal)
				{
					_retSet.Add(_lexem);
				}
			}
			return _retSet;
		}

		public static cSet<cConfiguration> cm_Closure(cSet<cConfiguration> a_listConfiguration)
		{
			bool _added=false;
			if (a_listConfiguration != null)
			{
				cSet<cConfiguration> _retList = a_listConfiguration.Clone() as cSet<cConfiguration>;
				cSet<cConfiguration> _addRetList = new cSet<cConfiguration>();
				do
				{
					_addRetList.Clear();
					foreach (cConfiguration _configuration in _retList)
					{
						if (_configuration.cf_Production.cp_RightPart.Count > _configuration.cf_Position)
						{
							cLexem _lexem = _configuration.cf_Production.cp_RightPart[_configuration.cf_Position];
							if (_lexem.cp_Type == eLexType.NonTerminal)
							{
								List<cLexem> _listTerminals=new List<cLexem>();
								_listTerminals.AddRange(_configuration.cf_Production.cp_RightPart.GetRange(_configuration.cf_Position+1, _configuration.cf_Production.cp_RightPart.Count-_configuration.cf_Position-1));
								_listTerminals.Add(_configuration.cf_Terminal);
								cSet<cLexem> _firstTerminals = cm_FirstTerminals(_listTerminals);
								foreach (cProduction _production in _lexem.cp_ListProducts)
								{
									foreach (cLexem _terminal in _firstTerminals)
									{
										cConfiguration _newConfiguration = cConfiguration.cm_GetConfiguration(_production, 0, _terminal);
										_addRetList.Add(_newConfiguration);
									}
								}
							}
						}
					}
					_added = _retList.AddRange(_addRetList);
				} while (_added == true);
				return _retList;
			}
			else
			{
				return new cSet<cConfiguration>();
			}
		}

		public static cSet<cConfiguration> cm_Goto(cSet<cConfiguration> a_listConfiguration, cLexem a_lexem)
		{
			cSet<cConfiguration> _validConfigurations = new cSet<cConfiguration>();
			foreach (cConfiguration _configuration in a_listConfiguration)
			{
				if (_configuration.cf_Production.cp_RightPart.Count > _configuration.cf_Position)
				{
					if (_configuration.cf_Production.cp_RightPart[_configuration.cf_Position] == a_lexem)
					{
						cConfiguration _newConfiguration = cConfiguration.cm_GetConfiguration(_configuration.cf_Production, _configuration.cf_Position + 1, _configuration.cf_Terminal);
						_validConfigurations.Add(_newConfiguration);
					}
				}
			}
			return cm_Closure(_validConfigurations);
		}
	}
}

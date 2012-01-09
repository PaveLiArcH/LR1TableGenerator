using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGenerator
{
	public class cConfiguration
	{
		static Dictionary<cProduction, Dictionary<int, cConfiguration>> cf_configurationsCache=new Dictionary<cProduction,Dictionary<int,cConfiguration>>();

		public readonly cProduction cf_Production;
		public readonly int cf_Position;

		private cConfiguration(cProduction a_production, int a_position)
		{
			cf_Production = a_production;
			cf_Position = a_position;
		}

		public static cConfiguration cm_GetConfiguration(cProduction a_production, int a_position)
		{
			if (!cf_configurationsCache.ContainsKey(a_production))
			{
				if (a_production.cp_EpsilonProduct & (a_position != 0))
				{
					throw new Exception("Попытка создать некорректную конфигурацию из эпсилон-продукции");
				}
				cf_configurationsCache.Add(a_production, new Dictionary<int, cConfiguration>());
			}
			if (!cf_configurationsCache[a_production].ContainsKey(a_position))
			{
				cf_configurationsCache[a_production].Add(a_position, new cConfiguration(a_production, a_position));
			}
			return cf_configurationsCache[a_production][a_position];
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
								foreach (cProduction _production in _lexem.cp_ListProducts)
								{
									cConfiguration _newConfiguration = cConfiguration.cm_GetConfiguration(_production, 0);
									_addRetList.Add(_newConfiguration);
								}
							}
						}
					}
					int _countBefore = _retList.Count;
					_retList.UnionWith(_addRetList);
					_added = _countBefore != _retList.Count;
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
						cConfiguration _newConfiguration = cConfiguration.cm_GetConfiguration(_configuration.cf_Production, _configuration.cf_Position + 1);
						_validConfigurations.Add(_newConfiguration);
					}
				}
			}
			return cm_Closure(_validConfigurations);
		}
	}
}

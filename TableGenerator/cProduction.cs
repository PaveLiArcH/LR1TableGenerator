using System;
using System.Collections.Generic;
using System.Text;

namespace TableGenerator
{
	public class cProduction
	{
		cLexem cf_root;
		List<cLexem> cf_rightPart;
		List<cLexem> cf_actionList = new List<cLexem>();

		bool cf_epsilonProduct;

		public cLexem cp_Root
		{
			get
			{
				return cf_root;
			}
		}

		public List<cLexem> cp_RightPart
		{
			get
			{
				return cf_rightPart;
			}
		}

		public List<cLexem> cp_ActionList
		{
			get
			{
				return cf_actionList;
			}
		}

		public bool cp_EpsilonProduct
		{
			get
			{
				return cf_epsilonProduct;
			}
		}

		public cProduction(cLexem a_root)
		{
			cf_root = a_root;
			cf_rightPart = new List<cLexem>();
			cf_actionList = new List<cLexem>();
			cf_epsilonProduct = false;
		}

		public void cm_Add(cLexem a_lexem)
		{
			if ((cf_rightPart.Count == 0) & (a_lexem == cLexem.cc_EpsilonLexem))
			{
				cf_root.cf_EpsilonCount++;
				cf_epsilonProduct = true;
			}
			if (cf_epsilonProduct & (a_lexem != cLexem.cc_EpsilonLexem))
			{
				cf_epsilonProduct = false;
				cf_root.cf_EpsilonCount--;
			}
			cf_rightPart.Add(a_lexem);
		}

		public void cm_AddAction(cLexem a_action)
		{
			if (a_action.cp_Type != eLexType.Action)
			{
				throw new Exception("Попытка добавить к продукции лексемы " + cf_root.cf_Name + " в качестве действия лексему типа " + a_action.cp_Type.ToString());
			}
			cf_actionList.Add(a_action);
		}
	}
}

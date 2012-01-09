using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableGenerator
{
	public abstract class cParser
	{
		virtual public cLexem cp_Root
		{
			get { return cLexem.cc_EpsilonLexem; }
		}

		public abstract void cm_Parse();
	}
}

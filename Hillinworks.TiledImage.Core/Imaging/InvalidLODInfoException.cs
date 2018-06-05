using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hillinworks.TiledImage.Imaging
{
	public class InvalidLODInfoException : Exception
	{
		public InvalidLODInfoException(string message)
			: base(message)
		{
			
		}
	}
}

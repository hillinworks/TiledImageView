using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hillinworks.TiledImage.Imaging
{
	public class InvalidDimensionsException : Exception
	{
		public InvalidDimensionsException(string message)
			: base(message)
		{
			
		}
	}
}

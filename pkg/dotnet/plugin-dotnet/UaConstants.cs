using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	class UaConstants
	{
		private static readonly uint _nodeClassAll = 255u;
		private static readonly string _hasInterface = "i=17603";
		private static readonly string _definedByEquipmentClass = "{\"namespaceUrl\":\"http://www.OPCFoundation.org/UA/2013/01/ISA95\",\"id\":\"i=4919\"}";

		public static uint NodeClassAll { get { return _nodeClassAll; } }

		public static string HasInterface { get { return _hasInterface; } }

		public static string DefinedByEquipmentClass { get { return _definedByEquipmentClass; } }
	}
}

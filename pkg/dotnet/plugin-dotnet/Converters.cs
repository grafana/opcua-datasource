using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public static class Converter
	{
		public static BrowseResultsEntry ConvertToBrowseResult(ReferenceDescription referenceDescription)
		{
			return new BrowseResultsEntry(
				referenceDescription.DisplayName.ToString(),
				referenceDescription.BrowseName.ToString(),
				referenceDescription.NodeId.ToString(),
				referenceDescription.TypeId,
				referenceDescription.IsForward,
				Convert.ToUInt32(referenceDescription.NodeClass));
		}
	}
}

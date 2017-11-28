
using System.IO;
using System.Collections.Generic;
using Core;
using Core.Serializer;


/// <summary>
/// Auto generate code, not need modify.
/// </summary>
namespace Model
{
    public static class CommonSerializer
    {
		public static void Serialize(this BinaryWriter rWriter, List<Model.UnitInfo> value)
		{
			var bValid = (null != value);
			rWriter.Serialize(bValid);
			if (!bValid) return;

			rWriter.Serialize(value.Count);
			for (int nIndex = 0; nIndex < value.Count; ++ nIndex)
				rWriter.Serialize(value[nIndex]);
		}
		public static List<Model.UnitInfo> Deserialize(this BinaryReader rReader, List<Model.UnitInfo> value)
		{
			var bValid = rReader.Deserialize(default(bool));
			if (!bValid) return null;

			var nCount  = rReader.Deserialize(default(int));
            var rResult = new List<Model.UnitInfo>(nCount);
			for (int nIndex = 0; nIndex < nCount; nIndex++)
                rResult.Add(rReader.Deserialize(default(Model.UnitInfo)));
			return rResult;
		}
		public static void Serialize(this BinaryWriter rWriter, List<Model.AFrameMessage> value)
		{
			var bValid = (null != value);
			rWriter.Serialize(bValid);
			if (!bValid) return;

			rWriter.Serialize(value.Count);
			for (int nIndex = 0; nIndex < value.Count; ++ nIndex)
				rWriter.Serialize(value[nIndex]);
		}
		public static List<Model.AFrameMessage> Deserialize(this BinaryReader rReader, List<Model.AFrameMessage> value)
		{
			var bValid = rReader.Deserialize(default(bool));
			if (!bValid) return null;

			var nCount  = rReader.Deserialize(default(int));
            var rResult = new List<Model.AFrameMessage>(nCount);
			for (int nIndex = 0; nIndex < nCount; nIndex++)
                rResult.Add(rReader.Deserialize(default(Model.AFrameMessage)));
			return rResult;
		}
	}
}

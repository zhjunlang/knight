
using System.IO;
using Core;
using Core.Serializer;

/// <summary>
/// Auto generate code, not need modify.
/// </summary>
namespace Model
{
	public partial class AMessage
	{
        public override void Serialize(BinaryWriter rWriter)
	    {
            base.Serialize(rWriter);
		}
		public override void Deserialize(BinaryReader rReader)
	    {
		    base.Deserialize(rReader);
		}
    }
}



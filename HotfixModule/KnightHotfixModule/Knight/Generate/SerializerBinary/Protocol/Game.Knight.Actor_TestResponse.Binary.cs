using System.IO;
using Core;
using WindHotfix.Core;


/// <summary>
/// 文件自动生成无需又该！如果出现编译错误，删除文件后会自动生成
/// </summary>
namespace Game.Knight
{
public partial class Actor_TestResponse
{
	public override void Serialize(BinaryWriter rWriter)
	{
		base.Serialize(rWriter);
		rWriter.Serialize(this.response);
	}
	public override void Deserialize(BinaryReader rReader)
	{
		base.Deserialize(rReader);
		this.response = rReader.Deserialize(this.response);
	}
}
}

﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;
using System.IO;
using Core.WindJson;
using WindHotfix.Core;
using UnityEngine.AssetBundles;
using System.Threading.Tasks;

namespace Game.Knight
{
    public enum GPCSymbolType
    {
        Unknown = 0,    // 未知
        ObjStart,       // '{'
        ObjEnd,         // '}'
        ArgsStart,      // '('
        ArgsEnd,        // ')'
        ArgsSplit,      // ','
        ElementSplit,   // ';'
        Identifer,      // 标识符
        Arg,            // 值类型: 整数、实数、字符串、true、false、null
    }
    
    [HotfixSBGroup("GamePlayConfig")]
    public partial class GPCSymbolItem : HotfixSerializerBinary
    {
        public string           Value;
        public GPCSymbolType    Type;

        public override string ToString()
        {
            return string.Format("({0}, {1})", Type, Value);
        }
    }

    [HotfixSBGroup("GamePlayConfig")]
    public partial class GPCSymbolElement : HotfixSerializerBinary
    {
        public GPCSymbolItem       Identifer;
        public List<GPCSymbolItem> Args;

        public GPCSymbolElement()
        {
        }

        public GPCSymbolElement(GPCSymbolItem rIdentifer, List<GPCSymbolItem> rArgs)
        {
            this.Identifer = rIdentifer;
            this.Args = new List<GPCSymbolItem>(rArgs);
        }

        public override string ToString()
        {
            string rResult = this.Identifer.ToString();
            for (int i = 0; i < this.Args.Count; i++)
            {
                rResult += ", " + this.Args[i].ToString();
            }
            return rResult;
        }

        public List<string> ToArgs()
        {
            var rArgs = new List<string>();
            for (int i = 0; i < this.Args.Count; i++)
            {
                rArgs.Add(this.Args[i].Value);
            }
            return rArgs;
        }
    }

    [HotfixSBGroup("GamePlayConfig")]
    public partial class GPCSymbolObject : HotfixSerializerBinary
    {
        public GPCSymbolElement        Head;
        public List<GPCSymbolElement>  Bodies;
    }

    [HotfixSBGroup("GamePlayConfig")]
    public partial class GPCSkillConfig : HotfixSerializerBinary
    {
        [HotfixSBIgnore]
        public static GPCSkillConfig Instance { get { return HotfixSingleton<GPCSkillConfig>.GetInstance(); } }

        public Dict<int, List<GPCSymbolObject>> ActorSkills;

        #region Loading...
        public static void Load_Local(string rLocalAssetPath)
        {
            GPCSkillConfig.Instance.LoadLocal(rLocalAssetPath);
        }

        public void LoadLocal(string rLocalAssetPath)
        {
            string rSkillListPath = rLocalAssetPath + "/SkillList.txt";
            string rSkillListConfig = File.ReadAllText(rSkillListPath);

            this.ActorSkills = new Dict<int, List<GPCSymbolObject>>();

            JsonNode rJsonArray = JsonParser.Parse(rSkillListConfig);
            for (int i = 0; i < rJsonArray.Count; i++)
            {
                int nSkillID = int.Parse(rJsonArray[i].Value);

                string rSkillPath = rLocalAssetPath + "/" + nSkillID + ".txt";
                string rSkillConfig = File.ReadAllText(rSkillPath);

                var rParser = new GamePlayComponentParser(rSkillConfig);
                var rSymbolObjs = rParser.Parser();

                this.ActorSkills.Add(nSkillID, rSymbolObjs);
            }

            string rBinaryPath = UtilTool.PathCombine(rLocalAssetPath.Replace("Text", "Binary"), "SkillConfig.bytes");
            using (var fs = new FileStream(rBinaryPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var br = new BinaryWriter(fs))
                {
                    this.Serialize(br);
                }
            }
        }

        /// <summary>
        /// 异步加载技能配置
        /// </summary>
        public async Task Load(string rConfigABPath, string rConfigName)
        {
            var rAssetRequesst = await ABLoader.Instance.LoadAsset(rConfigABPath, rConfigName, ABPlatform.Instance.IsSumilateMode_Config());
            if (rAssetRequesst.Asset == null) return;

            TextAsset rConfigAsset = rAssetRequesst.Asset as TextAsset;
            if (rConfigAsset == null) return;

            using (var ms = new MemoryStream(rConfigAsset.bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    this.Deserialize(br);
                }
            }
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        public void Unload(string rConfigABPath)
        {
            ABLoader.Instance.UnloadAsset(rConfigABPath);
        }
        #endregion

        public List<GPCSymbolObject> GetActorSkill(int nSkillID)
        {
            List<GPCSymbolObject> rSymbolObjs = null;
            this.ActorSkills.TryGetValue(nSkillID, out rSymbolObjs);
            return rSymbolObjs;
        }
    }
}
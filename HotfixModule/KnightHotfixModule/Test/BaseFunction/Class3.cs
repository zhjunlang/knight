﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using System.Collections.Generic;
using UnityEngine;
using WindHotfix.Test;
using Core.WindJson;
using WindHotfix.Core;
using Core;

namespace WindHotfix.Test
{
    public partial class A1
    {
        public double C;
    }

    public partial class A1
    {
        public List<float> B;
    }

    public class TClass3<T> where T : class
    {
        public virtual void Test()
        {
            Debug.LogError("TClass3 test..");

            Vector3 v = Vector3.one * 4;
            Debug.LogError(v);

            //Dict<int, int> a = new Dict<int, int>();
            //for (int i = 0; i < 10; i++)
            //{
            //    a.Add(i, i);
            //}
            //foreach (var rItem in a)
            //{
            //    Debug.LogError(rItem.Key + ", " + rItem.Value);
            //}

            // Test JsonNode.ToObject
            //JsonNode rNode = JsonParser.Parse("{\"C\":2.345, \"B\":[2.1, 2.2] }");
            //Debug.LogError(rNode.ToString());
            //var A1 = HotfixJsonParser.ToObject<A1>(rNode);
            //Debug.LogError(A1.B[0]);
            //var rCovNode = HotfixJsonParser.ToJsonNode(A1);
            //Debug.LogError(rCovNode);

            // Test JsonNode.ToList
            //JsonNode rNode = JsonParser.Parse("[{\"C\":2.345, \"B\":[2.1, 2.2] }, {\"C\":2.346, \"B\":[2.2, 2.3] }]");
            //Debug.LogError(rNode.ToString());
            ////var A1 = JsonParser.ToList<List<A1>, A1>(rNode);
            //var A1 = HotfixJsonParser.ToArray<A1>(rNode);
            //Debug.LogError(A1[1].B[0]);
            //var rCovNode = HotfixJsonParser.ToJsonNode(A1);
            //Debug.LogError(rCovNode);

            // Test JsonNode.ToDict
            //JsonNode rNode = JsonParser.Parse("{\"1\": {\"C\":2.345, \"B\":[2.1, 2.2] }, \"2\":{\"C\":2.346, \"B\":[2.2, 2.3] } }");
            //Debug.LogError(rNode.ToString());
            //var A1 = HotfixJsonParser.ToDict<int, A1>(rNode);
            //Debug.LogError(A1[2].B[1]);
            //Debug.LogError(A1[1].C);
            //var rCovNode = HotfixJsonParser.ToJsonNode(A1);
            //Debug.LogError(rCovNode);

            //Debug.LogError("2222");
            //var A1 = JsonMapper.ToObject<A1>(new JsonReader("{\"C\":2.345 }")) as A1;
            //Debug.LogError(A1.C);

            //Debug.LogError(typeof(List<A1>).FullName);

            CoroutineManager.Instance.StartHandler(Test_Async());
        }


        System.Collections.IEnumerator Test_Async()
        {
            yield return new WaitForSeconds(1.0f);

            GameObject rGo = new GameObject("11");
            rGo.transform.localScale = Vector3.one * 3.0f + Vector3.one + Quaternion.AngleAxis(36.0f, Vector3.up) * Quaternion.identity * Vector3.one * 2.0f;

            Debug.LogError(rGo.transform.localScale);
        }
    }
}

namespace WindHotfix.Test
{
    public class Class3 : TClass3<Class3>
    {
        //public override void Test()
        //{
        //    base.Test();
        //}

        public void Test1()
        {
            Class3 c = new Class3();
            c.Test();
        }
    }
}


using Colin.CollectionRef;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace Colin
{
    /// <summary>
    /// 编辑器程序
    /// 在编辑模式执行协程Coroutine
    /// </summary>
    public static class CEditorCoroutine
    {
        private static CCollection<VCoroutineStruct> m_CollectionNull;

        public static void StartCoroutine(IEnumerator aCoroutine)
        {
            if (m_CollectionNull == null)
            {
                m_CollectionNull =new CCollection<VCoroutineStruct>(4)
                {
                    new VCoroutineStruct() { m_Iter = aCoroutine }
                };
                EditorApplication.update += OnUpdate;
            }
            else
                m_CollectionNull.Add(new VCoroutineStruct() { m_Iter = aCoroutine });

        }

        public static void StopCoroutine(IEnumerator aCoroutine)
        {
            if (m_CollectionNull == null)
                return;

            for (int i = m_CollectionNull.Count - 1; i > -1; --i)
            {
                if (m_CollectionNull[i].m_Iter == aCoroutine)
                {
                    m_CollectionNull.RemoveAt(i);
                    return;
                }
            }
        }

        public static void StopAllCoroutine()
        {
            EditorApplication.update -= OnUpdate;
            m_CollectionNull = null;
        }

        private static void OnUpdate()
        {

            for (int i = m_CollectionNull.Count - 1; i > -1; --i)
            {
                ref var aValue = ref m_CollectionNull.GetValueRefAt(i);
                if (!DoCoroutine(ref aValue , EditorApplication.timeSinceStartup))
                    m_CollectionNull.RemoveAt(i);
            }

            if (m_CollectionNull.Count == 0)
                StopAllCoroutine();
        }

        private static bool DoCoroutine(ref VCoroutineStruct aStruct, double aCurrentTime)
        {
            switch (aStruct.m_Flag)
            {
                default:
                case EStatusFlag.Default:
                    var aIter = aStruct.m_Iter;
                    object aData;
                    if (aIter.MoveNext())
                    {
                        aData = aIter.Current;
                        if (aData is WaitForSeconds)
                        {
                            var aType = typeof(WaitForSeconds);
                            var aField = aType.GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);

                            aStruct.m_Flag = EStatusFlag.Delay;
                            aStruct.m_Result = aCurrentTime + (float)aField.GetValue(aData);
                        }
                        else if (aData is AsyncOperation)
                        {
                            aStruct.m_Flag = EStatusFlag.Async;
                            aStruct.m_Result = aData;

                        }
                        else if (aData is CustomYieldInstruction)
                        {
                            aStruct.m_Flag = EStatusFlag.Custom;
                            aStruct.m_Result = aData;
                        }

                        return true;
                    }
                    else
                        return false;
                case EStatusFlag.Delay:
                    if ((double)aStruct.m_Result < aCurrentTime)
                    {
                        aStruct.m_Flag = EStatusFlag.Default;
                        aStruct.m_Result = null;
                    }
                    return true;
                    
                case EStatusFlag.Custom:
                    if (!((CustomYieldInstruction)aStruct.m_Result).MoveNext())
                    {
                        aStruct.m_Flag = EStatusFlag.Default;
                        aStruct.m_Result = null;
                    }
                   return true;
        
                case EStatusFlag.Async:
                    if (((AsyncOperation)aStruct.m_Result).isDone)
                    {
                        aStruct.m_Flag = EStatusFlag.Default;
                        aStruct.m_Result = null;
                    }
                    return true;
            }
        }

        struct VCoroutineStruct
        {
            public IEnumerator m_Iter;
            public EStatusFlag m_Flag;
            public object m_Result;
        }
        enum EStatusFlag :byte { Default = 0, Delay, Async, Custom }
    }

}
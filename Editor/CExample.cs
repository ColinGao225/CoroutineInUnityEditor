 
using UnityEngine;
using UnityEditor;
using Colin;
using System.Collections;

public class CTest : MonoBehaviour
{

    [MenuItem("Editor/TestCoroutine")]
    public  static unsafe void MDone()
    {
        CEditorCoroutine.StartCoroutine(DoCouroutine());
        CEditorCoroutine.StartCoroutine(DoCouroutine());
    }
        
    public static IEnumerator DoCouroutine()
    {
        Debug.Log("Start");
        yield return new WaitForSeconds(5);
        Debug.Log("OK1");
        yield return new WaitForSecondsRealtime(5);
        Debug.Log("OK2");
    }
}
 
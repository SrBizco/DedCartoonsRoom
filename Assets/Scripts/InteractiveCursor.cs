using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InteractiveCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SetBrowserCursor(string cursorStyle);
#endif

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetCursorStyle("pointer");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetCursorStyle("default");
    }

    private void OnDisable()
    {
        SetCursorStyle("default");
    }

    private static void SetCursorStyle(string cursorStyle)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetBrowserCursor(cursorStyle);
#endif
    }
}

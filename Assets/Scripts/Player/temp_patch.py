content = open("Assets/Scripts/Player/Controller.cs", "r", encoding="utf-8").read()

# Add DllImport after class opening brace
dllimport = """#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int IsMobileBrowser();
#endif"""

old_class_open = "public class Controller : MonoBehaviour`n{`n    [Header"
new_class_open = "public class Controller : MonoBehaviour`n{`n" + dllimport + "`n    [Header"
content = content.replace(old_class_open, new_class_open)


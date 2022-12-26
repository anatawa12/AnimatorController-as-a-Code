using CustomLocalization4EditorExtension;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    internal static class I18N
    {
        private static readonly Localization Localization = new Localization("c4b95afbb9f043329d6854fb73331892", "en");
        public static string Tr(string key) => Localization.Tr(key);
        public static void DrawLanguagePicker() => Localization.DrawLanguagePicker();
    }
}

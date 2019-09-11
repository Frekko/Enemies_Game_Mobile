namespace System.Scripts.Utils
{
    public class CommandLineParser
    {
        public static string GetParam( string name, string defaultVal = null)
        {
            string[] args = System.Environment.GetCommandLineArgs ();
            string input = "";
            for (int i = 0; i < args.Length; i++) 
            {
                if (args [i] == $"-{name}") {
                    input = args [i + 1];
                }
            }

            if (string.IsNullOrEmpty(input))
            {
                return defaultVal;
            }

            UnityEngine.Debug.Log($"Used commandline param {name} -> {input}");
            return input;
        }
    }
}
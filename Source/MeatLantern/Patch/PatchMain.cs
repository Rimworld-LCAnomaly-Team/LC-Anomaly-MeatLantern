using HarmonyLib;
using System.Reflection;
using Verse;

namespace MeatLantern.Patch
{
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        public static Harmony instance;

        static PatchMain()
        {
            //对所有特性标签的方法进行patch
            instance = new Harmony("MeatLantern.Patch");
            instance.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("LC MeatLantern Patched");
        }
    }
}

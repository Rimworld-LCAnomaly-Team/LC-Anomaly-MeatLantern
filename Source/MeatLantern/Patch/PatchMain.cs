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
            instance = new Harmony("MeatLantern.Patch");
            //对所有特性标签的方法进行patch
            instance.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("LC MeatLantern Patched");
        }
    }
}

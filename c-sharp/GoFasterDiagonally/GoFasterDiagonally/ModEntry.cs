using HarmonyLib;
using StardewModdingAPI;
using StardewValley;


namespace GoFasterDiagonally;

internal sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        Harmony harmony = new Harmony("meow.GoFasterDiagonally.patch");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(Farmer))]
    [HarmonyPatch(nameof(Farmer.getMovementSpeed))]
    class Patch01
    {
        static void Postfix(ref float __result)
        {
            if (Game1.player.movementDirections.Count > 1 && __result != 0f && 
                (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
            {
                __result /= 0.707f;
            }
        }
    }
}
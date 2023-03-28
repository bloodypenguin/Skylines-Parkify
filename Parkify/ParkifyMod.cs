using CitiesHarmony.API;
using ICities;
using Parkify.OptionsFramework.Extensions;

namespace Parkify
{
    public class ParkifyMod : IUserMod
    {
        public string Name => "Parkify r1.1.0";
        public string Description => "Allows to use the whole wealth of your park buildings as park modules";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddOptionsGroup<Options>();
        }
        
        public void OnEnabled()
        {
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}

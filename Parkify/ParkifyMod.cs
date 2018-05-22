using ICities;
using Parkify.OptionsFramework.Extensions;

namespace Parkify
{
    public class ParkifyMod : IUserMod
    {
        public string Name => "Parkify";
        public string Description => "Allows to use the whole wealth of your park buildings as park modules";

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddOptionsGroup<Options>();
        }
    }
}

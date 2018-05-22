using System.Xml.Serialization;
using Parkify.OptionsFramework.Attibutes;

namespace Parkify
{
    [Options("CubemapReplacer")]
    public class Options
    {

        public Options()
        {
            PatchVanillaUniqueBuildings = false;
            PatchFishingTours = true;
            PatchMarina = true;
        }

        [XmlElement("patch")]
        [Checkbox("Convert some vanilla unique buildings into park assets")]
        public bool PatchVanillaUniqueBuildings { set; get; }

        [XmlElement("patchFishingTours")]
        [Checkbox("Add missing boat to fishing tours asset", "Vanilla assets fixes")]
        public bool PatchFishingTours { set; get; }

        [XmlElement("patchMarina")]
        [Checkbox("Add missing boat to marina asset", "Vanilla assets fixes")]
        public bool PatchMarina { set; get; }

    }
}
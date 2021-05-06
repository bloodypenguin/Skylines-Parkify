using System.Xml.Serialization;
using Parkify.OptionsFramework.Attibutes;

namespace Parkify
{
    [Options("Parkify")]
    public class Options
    {

        public Options()
        {
            PatchVanillaUniqueBuildings = false;
            PatchFishingTours = true;
            PatchMarina = true;
            PatchBeachVolley = true;
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
        
        [XmlElement("patchBeachvolley")]
        [Checkbox("Remove parking lots and concrete", "Vanilla assets fixes")]
        public bool PatchBeachVolley { set; get; }

    }
}
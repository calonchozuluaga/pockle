using System;
using System.Collections.Generic;
namespace Pockle.Core
{
    /// <summary>Owner-uploaded Christmas characters; art availability never approves rewards.</summary>
    public static class HolidayCatalog
    {
        public static bool IsCharacter(string id) => id == "firb" || id == "coco" || id == "snowy" || id == "ginger" || IsWinterTrio(id);
        public static bool IsWinterTrio(string id) => id == "reiny" || id == "guin" || id == "bundy";
        public static bool HasArtSet(string id) => IsCharacter(id);
        public static string Title(string id) => id == "firb" ? "Firb" : id == "coco" ? "Coco" : id == "snowy" ? "Snowy" : id == "ginger" ? "Ginger" : id == "reiny" ? "Reiny" : id == "guin" ? "Guin" : id == "bundy" ? "Bundy" : "";
        public static IReadOnlyList<CollectionWorldDefinition> Worlds(string id)
        {
            switch(id)
            {
                case "firb": return Make(id, "evergreen-clearing|Evergreen Clearing", "cranberry-grove|Cranberry Grove", "frosted-trail|Frosted Trail", "golden-wood|Golden Wood", "starfall-forest|Starfall Forest");
                case "coco": return Make(id, "cocoa-nook|Cocoa Nook", "marshmallow-clouds|Marshmallow Clouds", "peppermint-cafe|Peppermint Cafe", "caramel-kitchen|Caramel Kitchen", "ember-lodge|Ember Lodge");
                case "snowy": return Make(id, "snowdrop-meadow|Snowdrop Meadow", "mint-frost|Mint Frost", "pearl-lake|Pearl Lake", "peach-dawn|Peach Dawn", "aurora-valley|Aurora Valley");
                case "ginger": return Make(id, "icing-village|Icing Village", "cinnamon-bakery|Cinnamon Bakery", "mint-courtyard|Mint Courtyard", "honey-market|Honey Market", "sugarplum-night|Sugarplum Night");
                case "reiny": return Make(id, "birch-clearing|Birch Clearing", "cranberry-trail|Cranberry Trail", "frosted-meadow|Frosted Meadow", "lantern-lodge|Lantern Lodge", "solstice-woods|Solstice Woods");
                case "guin": return Make(id, "mint-ice-cove|Mint Ice Cove", "pearl-snowfield|Pearl Snowfield", "coral-harbor|Coral Harbor", "glacier-grotto|Glacier Grotto", "polar-lights|Polar Lights");
                case "bundy": return Make(id, "vanilla-kitchen|Vanilla Kitchen", "holly-garden|Holly Garden", "caramel-pantry|Caramel Pantry", "sugarplum-table|Sugarplum Table", "midnight-feast|Midnight Feast");
                default: return Array.AsReadOnly(Array.Empty<CollectionWorldDefinition>());
            }
        }
        private static IReadOnlyList<CollectionWorldDefinition> Make(string id, params string[] entries)
        {
            var result = new CollectionWorldDefinition[entries.Length];
            for(int i=0;i<entries.Length;i++){var parts=entries[i].Split('|');result[i]=new CollectionWorldDefinition(id+"-"+parts[0],parts[1]);}
            return Array.AsReadOnly(result);
        }
        public static CatalogColor Accent(string id) => new CatalogColor(id == "reiny" ? 0xc39365u : id == "guin" ? 0x7397b5u : id == "bundy" ? 0x99674du : id == "firb" ? 0x548b66u : id == "coco" ? 0xbd7456u : id == "snowy" ? 0x7abbbbu : 0xd69b64u);
        public static string SpecialLabel(string id) => id == "reiny" ? "Solstice Glow" : id == "guin" ? "Polar Glow" : id == "bundy" ? "Holly Glow" : id == "firb" ? "Starfall Glow" : id == "coco" ? "Ember Glow" : id == "snowy" ? "Aurora Glow" : "Sugarplum Glow";
    }
}

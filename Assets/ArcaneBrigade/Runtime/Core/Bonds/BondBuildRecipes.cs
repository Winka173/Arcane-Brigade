namespace ArcaneBrigade
{
    public readonly struct BondBuildRecipe
    {
        public readonly string Name, Majors, Minors, Behavior;
        public BondBuildRecipe(string name, string majors, string minors, string behavior) { Name = name; Majors = majors; Minors = minors; Behavior = behavior; }
    }
    public static class BondBuildRecipes
    {
        public static readonly BondBuildRecipe[] All = {
            new BondBuildRecipe("Burning Court","Ember 8 + Carnival 4","Desperado, Bladenado","stacked burns burst, dice bonuses chain"),
            new BondBuildRecipe("Frozen Shore","Frost 8 + Tide 6","Gauntlet, Cloud","chilled, soaked enemies shatter in waves"),
            new BondBuildRecipe("Static Gale","Storm 6 + Windborne 8","Sniper, Rapidfire","lightning chains over long volleys"),
            new BondBuildRecipe("Poison Grove","Garden 8 + Venom 6","Root, Spiky Shell","growing allies and spreading poison"),
            new BondBuildRecipe("Night Library","Archive 6 + Dusk 6","Runes, Bludgeoner","marked species and executions"),
            new BondBuildRecipe("Iron Fortress","Stone 8 + Bulwark 6","Bastion, Monolith","walls, reflect and last stands"),
            new BondBuildRecipe("Ghost Choir","Spirit 8 + Melody 4","Echo, Mend","fallen friends rise and sing"),
            new BondBuildRecipe("Black Hole Clock","Gravity 8 + Clockwork 6","Singularity, Gears","holes pull enemies into clumps and bars fill fast"),
            new BondBuildRecipe("Lamp Court","Lantern 6 + Court 6","Hats, Masks","light, decrees and crowns"),
            new BondBuildRecipe("Feast Fire","Hearth 8 + Ember 6","Sweet Tooth, Ball","stats from meals and bouncing fire"),
            new BondBuildRecipe("Cross build: Wet Thunder","Tide 6 + Storm 6","Rain Script, Demolitionist","soaked enemies conduct lightning"),
            new BondBuildRecipe("Cross build: Shadow Garden","Dusk 6 + Garden 6","Clover, Cloud","shades grow from poisoned fallen"),
        };
    }
}

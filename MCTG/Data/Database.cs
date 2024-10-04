using MCTG.Services;

namespace MCTG.Data;

public class Database
{
    private static List<User> Users { get; set; } = new List<User>();
    private static List<Card> CardRepository { get; set; } = new List<Card>();

    static Database()
    {
        InitializeCards();
    }

    private static void InitializeCards()
    {
        CardRepository.Add(new MonsterCard("Goblin", 40, ElementType.Water, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Wizard", 30, ElementType.Fire, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Dragon", 80, ElementType.Fire, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Knight", 60, ElementType.Normal, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Kraken", 70, ElementType.Water, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Fire Elf", 55, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("Orc", 50, ElementType.Normal, TypeOfMonster.Ork));
        CardRepository.Add(new MonsterCard("Fire Goblin", 45, ElementType.Fire, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Water Wizard", 35, ElementType.Water, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Ice Dragon", 75, ElementType.Water, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Knight of the Flame", 65, ElementType.Fire, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Shadow Kraken", 90, ElementType.Normal, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Forest Fire Elf", 60, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("Savage Orc", 55, ElementType.Normal, TypeOfMonster.Ork));
        CardRepository.Add(new MonsterCard("Stone Goblin", 40, ElementType.Normal, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Lightning Wizard", 50, ElementType.Fire, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Ancient Dragon", 100, ElementType.Fire, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Armored Knight", 70, ElementType.Normal, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Deep Sea Kraken", 85, ElementType.Water, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Firestorm Elf", 65, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("Battle Orc", 75, ElementType.Normal, TypeOfMonster.Ork));
        CardRepository.Add(new MonsterCard("Mystic Goblin", 45, ElementType.Normal, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Frost Wizard", 55, ElementType.Water, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Dragon Rider", 90, ElementType.Fire, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Holy Knight", 80, ElementType.Normal, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Storm Kraken", 95, ElementType.Water, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Inferno Fire Elf", 75, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("Brutal Orc", 65, ElementType.Normal, TypeOfMonster.Ork));
        CardRepository.Add(new MonsterCard("Goblin Mage", 50, ElementType.Normal, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Arcane Wizard", 70, ElementType.Fire, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Firebreath Dragon", 95, ElementType.Fire, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Shadow Knight", 85, ElementType.Normal, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Chained Kraken", 100, ElementType.Water, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Sun Elf", 80, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("War Orc", 70, ElementType.Normal, TypeOfMonster.Ork));
        CardRepository.Add(new MonsterCard("Goblin Assassin", 55, ElementType.Normal, TypeOfMonster.Goblin));
        CardRepository.Add(new MonsterCard("Enchanted Wizard", 65, ElementType.Water, TypeOfMonster.Wizzard));
        CardRepository.Add(new MonsterCard("Ice Dragon", 90, ElementType.Water, TypeOfMonster.Dragon));
        CardRepository.Add(new MonsterCard("Cursed Knight", 75, ElementType.Normal, TypeOfMonster.Knight));
        CardRepository.Add(new MonsterCard("Kraken of the Abyss", 100, ElementType.Water, TypeOfMonster.Krake));
        CardRepository.Add(new MonsterCard("Elder Fire Elf", 85, ElementType.Fire, TypeOfMonster.FireElve));
        CardRepository.Add(new MonsterCard("Dread Orc", 75, ElementType.Normal, TypeOfMonster.Ork));

        // Adding Spell Cards
        CardRepository.Add(new SpellCard("Fireball", 10, ElementType.Fire));
        CardRepository.Add(new SpellCard("Ice Shard", 15, ElementType.Water));
        CardRepository.Add(new SpellCard("Lightning Bolt", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Healing Light", 5, ElementType.Normal));
        CardRepository.Add(new SpellCard("Meteor Shower", 25, ElementType.Fire));
        CardRepository.Add(new SpellCard("Water Wave", 18, ElementType.Water));
        CardRepository.Add(new SpellCard("Stone Skin", 12, ElementType.Normal));
        CardRepository.Add(new SpellCard("Firestorm", 30, ElementType.Fire));
        CardRepository.Add(new SpellCard("Tsunami", 22, ElementType.Water));
        CardRepository.Add(new SpellCard("Frostbite", 15, ElementType.Water));
        CardRepository.Add(new SpellCard("Shadow Strike", 25, ElementType.Normal));
        CardRepository.Add(new SpellCard("Whirlwind", 18, ElementType.Normal));
        CardRepository.Add(new SpellCard("Meteor Strike", 30, ElementType.Fire));
        CardRepository.Add(new SpellCard("Torrent", 20, ElementType.Water));
        CardRepository.Add(new SpellCard("Flame Wave", 24, ElementType.Fire));
        CardRepository.Add(new SpellCard("Normal Spell", 10, ElementType.Normal));
        CardRepository.Add(new SpellCard("Holy Light", 15, ElementType.Normal));
        CardRepository.Add(new SpellCard("Ice Shield", 12, ElementType.Water));
        CardRepository.Add(new SpellCard("Wind Blast", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Earthquake", 30, ElementType.Normal));
        CardRepository.Add(new SpellCard("Steam Blast", 22, ElementType.Water));
        CardRepository.Add(new SpellCard("Fire Shield", 15, ElementType.Fire));
        CardRepository.Add(new SpellCard("Water Shield", 18, ElementType.Water));
        CardRepository.Add(new SpellCard("Burning Hands", 10, ElementType.Fire));
        CardRepository.Add(new SpellCard("Water Whip", 15, ElementType.Water));
        CardRepository.Add(new SpellCard("Sandstorm", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Inferno", 35, ElementType.Fire));
        CardRepository.Add(new SpellCard("Deluge", 25, ElementType.Water));
        CardRepository.Add(new SpellCard("Meteor", 30, ElementType.Fire));
        CardRepository.Add(new SpellCard("Shadow Bolt", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Frost Nova", 25, ElementType.Water));
        CardRepository.Add(new SpellCard("Fire Rain", 28, ElementType.Fire));
        CardRepository.Add(new SpellCard("Water Spout", 30, ElementType.Water));
        CardRepository.Add(new SpellCard("Soul Drain", 12, ElementType.Normal));
        CardRepository.Add(new SpellCard("Healing Rain", 15, ElementType.Water));
        CardRepository.Add(new SpellCard("Vortex", 25, ElementType.Normal));
        CardRepository.Add(new SpellCard("Fire Wave", 20, ElementType.Fire));
        CardRepository.Add(new SpellCard("Icicle", 18, ElementType.Water));
        CardRepository.Add(new SpellCard("Scorch", 12, ElementType.Fire));
        CardRepository.Add(new SpellCard("Mend", 15, ElementType.Normal));
        CardRepository.Add(new SpellCard("Gale Force", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Fire Burst", 30, ElementType.Fire));
        CardRepository.Add(new SpellCard("Water Surge", 28, ElementType.Water));
        CardRepository.Add(new SpellCard("Rock Slide", 20, ElementType.Normal));
        CardRepository.Add(new SpellCard("Wildfire", 35, ElementType.Fire));
        CardRepository.Add(new SpellCard("Torrent Wave", 30, ElementType.Water));
    }
    public static bool UserExists(string username)
    {
        return Users.Any(u => u.UserName == username);
    }

    public static void AddUser(User user)
    {
        Users.Add(user);
    }

    public static User getUser(string username)
    {
        return Users.FirstOrDefault(u => u.UserName == username);
    }

    public static Card GetRandomCard()
    {
        Random rnd = new Random();
        int randomIndex = rnd.Next(CardRepository.Count);
        return CardRepository[randomIndex];
    }
}
namespace MCTG;

public enum TypeOfMonster
{
    Goblin,
    Wizzard,
    Dragon,
    Knight,
    Krake,
    FireElve,
    Ork
}
public class MonsterCard : Card
{
    public TypeOfMonster Monster { get; private set;}
    public MonsterCard(string name, int damage, ElementType elementType, TypeOfMonster monster) : base(name, damage, elementType)
    {
        Monster = monster;
    }
}
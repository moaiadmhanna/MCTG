namespace MCTG;

public abstract class Card
{
    public string Name { get; protected set; }
    public int Damage  {get;}
    public ElementType Element { get;}

    protected Card(string name, int damage, ElementType element)
    {
        Name = name;
        Damage = damage;
        Element = element;
    }
}
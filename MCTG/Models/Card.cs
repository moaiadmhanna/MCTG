namespace MCTG;

public abstract class Card
{
    public string Name { get; protected set; }
    protected int Damage {get;}
    protected ElementType Element { get; set; }

    protected Card(string name, int damage, ElementType element)
    {
        Name = name;
        Damage = damage;
        Element = element;
    }
     public abstract void attack();
}
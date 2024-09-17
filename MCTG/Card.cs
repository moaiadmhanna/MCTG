namespace MCTG;

abstract class Card
{
    protected string name { get; set; }
    protected int damage {get;}
    protected ElementType element { get; set; }

    protected Card(string name, int damage, ElementType element)
    {
        this.name = name;
        this.damage = damage;
        this.element = element;
    }
     public abstract void attack();
}
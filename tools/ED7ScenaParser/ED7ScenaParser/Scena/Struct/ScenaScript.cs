using System.Text;

namespace ED7ScenaParser.Scena.Struct;

public class ScenaScript :IScenaOut
{
    public ScenaHeader Header = new();
    public ScenaEntry Entry = new();
    public uint[] Chips = [];
    public ScenaNpc[] Npcs = [];
    public ScenaMonster[] Monsters = [];
    public ScenaTrigger[] Triggers = [];
    public ScenaLookPoint[] LookPoints = [];
    public ScenaAnimation[] Animations = [];

}
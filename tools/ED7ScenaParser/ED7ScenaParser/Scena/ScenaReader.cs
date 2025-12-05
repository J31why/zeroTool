using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;
using ED7ScenaParser.Scena.Struct;

namespace ED7ScenaParser.Scena;

public class ScenaReader(Stream input) : ReflectionReader(input)
{
    public ScenaScript Script { get; } = new();

    public void Parse()
    {
        var strings = new Queue<string>();
        BaseStream.Seek(0, SeekOrigin.Begin);
        // base
        ParseStructure(ref Script.Header);
        ParseStructure(ref Script.Entry);
        // strings
        BaseStream.Seek(Script.Header.pStrings, SeekOrigin.Begin);
        while (BaseStream.Position < BaseStream.Length)
        {
            var s = ReadCString();
            if (s is null)
                break;
            strings.Enqueue(s);
        }
        Script.Header.StringName = strings.Dequeue();
        // chips
        BaseStream.Seek(Script.Header.pChips, SeekOrigin.Begin);
        Script.Chips = new uint[Script.Header.nChips];
        for (var i = 0; i < Script.Chips.Length; i++)
            Script.Chips[i] =  ReadUInt32();
        // npcs
        BaseStream.Seek(Script.Header.pNpcs, SeekOrigin.Begin);
        Script.Npcs = new ScenaNpc[Script.Header.nNpcs];
        for (var i = 0; i < Script.Npcs.Length; i++)
        {
            Script.Npcs[i].Name = strings.Dequeue();
            ParseStructure(ref Script.Npcs[i]);
        }
        // monsters
        BaseStream.Seek(Script.Header.pMonsters, SeekOrigin.Begin);
        Script.Monsters = new ScenaMonster[Script.Header.nMonsters];
        for (var i = 0; i < Script.Monsters.Length; i++)
            ParseStructure(ref Script.Monsters[i]);
        // triggers
        BaseStream.Seek(Script.Header.pTriggers, SeekOrigin.Begin);
        Script.Triggers = new ScenaTrigger[Script.Header.nTriggers];
        for (var i = 0; i < Script.Triggers.Length; i++)
            ParseStructure(ref Script.Triggers[i]);
        // look points
        BaseStream.Seek(Script.Header.pLookPoints, SeekOrigin.Begin);
        Script.LookPoints = new ScenaLookPoint[Script.Header.nLookPoints];
        for (var i = 0; i < Script.LookPoints.Length; i++)
            ParseStructure(ref Script.LookPoints[i]);
        // animations
        var count = (Script.Header.pFuncTable - Script.Header.pAnimations) / 12;
        BaseStream.Seek(Script.Header.pAnimations, SeekOrigin.Begin);
        Script.Animations = new ScenaAnimation[count];
        for (var i = 0; i < Script.Animations.Length; i++)
        {
            ParseStructure(ref Script.Animations[i]);
            if(Script.Animations[i].CheckByte !=0 || Script.Animations[i].Count>8)
                throw new DataException($"Animation reader failed: {i}");
        }
        
        
    }

}
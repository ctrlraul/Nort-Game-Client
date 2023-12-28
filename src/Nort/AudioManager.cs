using System;
using System.Linq;
using CtrlRaul;
using Godot;

namespace Nort;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }
    
    private readonly Random random = new();


    private readonly AudioStream[] bulletFiredSounds =
    {
        GD.Load<AudioStream>("res://Sounds/BulletFired1.wav"),
    };

    private readonly AudioStream[] beamFiredSounds =
    {
        GD.Load<AudioStream>("res://Sounds/BeamFired1.wav"),
        GD.Load<AudioStream>("res://Sounds/BeamFired2.wav"),
    };

    private readonly AudioStream[] partDetachedSounds =
    {
        GD.Load<AudioStream>("res://Sounds/CraftPartDetached1.wav"),
        GD.Load<AudioStream>("res://Sounds/CraftPartDetached2.wav"),
        GD.Load<AudioStream>("res://Sounds/CraftPartDetached3.wav"),
        GD.Load<AudioStream>("res://Sounds/CraftPartDetached4.wav"),
    };


    private bool mute;
    public bool Mute
    {
        get => mute;
        set
        {
            mute = value;

            foreach (AudioStreamPlayer2D audioStreamPlayer2D in GetChildren().Cast<AudioStreamPlayer2D>())
            {
                audioStreamPlayer2D.Stop(); // Needed?
                audioStreamPlayer2D.QueueFree();
            }
        }
    }
    
    
    private AudioManager()
    {
        Instance = this;
    }
    
    
    public AudioStreamPlayer2D PlayBulletFired(Vector2 position)
    {
        if (Mute)
            return null;
        
        AudioStreamPlayer2D audioStreamPlayer2D = new();

        audioStreamPlayer2D.Position = position;
        audioStreamPlayer2D.Autoplay = true;
        audioStreamPlayer2D.PitchScale = 0.5f + random.NextSingle();
        audioStreamPlayer2D.Stream = bulletFiredSounds[random.Next() % bulletFiredSounds.Length];
        
        AddChild(audioStreamPlayer2D);
        audioStreamPlayer2D.Finished += audioStreamPlayer2D.QueueFree;

        return audioStreamPlayer2D;
    }
    
    public AudioStreamPlayer2D PlayBeamFired(Vector2 position)
    {
        if (Mute)
            return null;
        
        AudioStreamPlayer2D audioStreamPlayer2D = new();

        audioStreamPlayer2D.Position = position;
        audioStreamPlayer2D.Autoplay = true;
        audioStreamPlayer2D.PitchScale = 0.5f + random.NextSingle();
        audioStreamPlayer2D.Stream = beamFiredSounds[random.Next() % beamFiredSounds.Length];
        
        AddChild(audioStreamPlayer2D);
        audioStreamPlayer2D.Finished += audioStreamPlayer2D.QueueFree;

        return audioStreamPlayer2D;
    }
    
    public AudioStreamPlayer2D PlayPartDetached(Vector2 position)
    {
        if (Mute)
            return null;
        
        AudioStreamPlayer2D audioStreamPlayer2D = new();

        audioStreamPlayer2D.Position = position;
        audioStreamPlayer2D.Autoplay = true;
        audioStreamPlayer2D.PitchScale = 0.75f + random.NextSingle() * 0.5f;
        audioStreamPlayer2D.Stream = partDetachedSounds[random.Next() % partDetachedSounds.Length];
        
        AddChild(audioStreamPlayer2D);
        audioStreamPlayer2D.Finished += audioStreamPlayer2D.QueueFree;

        return audioStreamPlayer2D;
    }
}
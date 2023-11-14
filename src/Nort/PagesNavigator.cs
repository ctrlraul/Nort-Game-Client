using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CtrlRaul;
using Godot;
using Shouldly;

namespace Nort;

public partial class PagesNavigator : Node
{
    public static PagesNavigator Instance { get; private set; }
    public delegate Task Middleware(Node node);

    public bool Canceled { get; private set; }
    
    private readonly Logger logger = new(nameof(PagesNavigator));
    private readonly List<Middleware> middlewares = new();
    private readonly List<(PackedScene, object)> history = new();
    private readonly Dictionary<string, PackedScene> scenesCache = new();
    private SceneTree tree;
    private ulong firstSceneInstanceId;
    private PackedScene defaultScene;
    
    public object NavigationData { get; private set; }

    public PagesNavigator()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        
        tree = GetTree();
        firstSceneInstanceId = tree.CurrentScene.GetInstanceId();
        
        if (tree.CurrentScene.IsNodeReady())
        {
            _ = RunMiddlewares(tree.CurrentScene);
        }
        else
        {
            tree.CurrentScene.Ready += async () => await RunMiddlewares(tree.CurrentScene);
        }
    }

    public void SetDefaultScene(PackedScene scene)
    {
        defaultScene = scene;
    }

    public void SetDefaultScene(string sceneFilePath)
    {
        defaultScene = GD.Load<PackedScene>(sceneFilePath);
    }
    
    public async Task GoTo(PackedScene scene, object data = null)
    {
        await ChangePage(scene, data);
        history.Add((scene, data));
    }

    public async Task GoTo(string sceneFilePath, object data = null)
    {
        if (!scenesCache.ContainsKey(sceneFilePath))
        {
            scenesCache[sceneFilePath] = GD.Load<PackedScene>(sceneFilePath);
        }
        PackedScene scene = scenesCache[sceneFilePath];
        await GoTo(scene, data);
    }

    public async Task GoBack(object dataOverride = default)
    {
        defaultScene.ShouldNotBeNull();
        
        PackedScene scene;
        object data = dataOverride;

        switch (history.Count)
        {
            case 0:
                scene = defaultScene;
                break;

            case 1:
                history.RemoveAt(0);
                scene = defaultScene;
                break;

            default:
                history.RemoveAt(history.Count - 1);
                (PackedScene, object) entry = history.Last();
                scene = entry.Item1;
                if (dataOverride == default)
                {
                    data = entry.Item2;
                }

                break;
        }

        await ChangePage(scene, data);
    }


    public void AddMiddleware(Middleware middleware)
    {
        middlewares.Add(middleware);
    }

    public void Cancel()
    {
        // Edge case: Allowed to cancel the first scene, shouldn't.
        Canceled = true;
    }
    
    private async Task ChangePage(PackedScene scene, object data)
    {
        NavigationData = data;
        
        Node current = tree.CurrentScene;
        Node node = scene.Instantiate();
        
        tree.Root.AddChild(node);

        await RunMiddlewares(node);
        
        tree.CurrentScene = node;

        current.QueueFree();
    }

    private async Task RunMiddlewares(Node node)
    {
        GD.Print($"RunMiddlewares: {node.Name}");
        
        foreach (Middleware middleware in middlewares)
        {
            try
            {
                await middleware.Invoke(node);
            }
            catch (Exception exception)
            {
                logger.Error("Middleware exception");
                logger.Error(exception);
            }

            if (Canceled)
            {
                node.QueueFree();
                return;
            }
        }
    }

    public bool IsFirstScene(Node node)
    {
        return node.GetInstanceId() == firstSceneInstanceId;
    }
}
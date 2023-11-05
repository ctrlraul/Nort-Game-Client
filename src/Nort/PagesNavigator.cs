using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Godot;
using CtrlRaul;
using Nort.Page;

namespace Nort;

public class PagesNavigator : Singleton<PagesNavigator>
{
    public event Action<PageScene> PageDataLoading;
    public event Action<PageScene> PageDataLoaded;
    public event Action<PageScene, string> PageDataLoadError;
    public event Action<PackedScene, string> PageChangeError;

    private readonly List<(PackedScene, object)> history = new();
    private readonly Dictionary<string, PackedScene> scenesCache = new();
    private readonly SceneTree tree;
    private PackedScene defaultScene;

    public PagesNavigator()
    {
        tree = (Engine.GetMainLoop() as SceneTree)!;
        tree.CurrentScene.Ready += () => _ = CallPostReady(tree.CurrentScene, null);
    }

    private async Task<bool> CallPostReady(Node node, object data)
    {
        if (node is PageScene page)
        {
            page.navigatorData = data;
            
            try
            {
                Task task = page.PostReady();

                if (task.IsCompleted)
                {
                    if (task.Exception != null)
                        throw task.Exception;
                }
                else
                {
                    PageDataLoading?.Invoke(page);
                    await task;
                    PageDataLoaded?.Invoke(page);
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception);
                PageDataLoadError?.Invoke(page, "Failed to load data!");
                return false;
            }
        }
        else
        {
            logger.Warn($"'{node.SceneFilePath}' does not extend {typeof(PageScene)}");
        }

        return true;
    }


    public void SetDefaultScene(PackedScene scene)
    {
        defaultScene = scene;
    }

    public void SetDefaultScene(string sceneFilePath)
    {
        defaultScene = GD.Load<PackedScene>(sceneFilePath);
    }
    
    public void GoTo(PackedScene scene, object data = default)
    {
        bool success = ChangePage(scene, data);
        if (success)
        {
            history.Add((scene, data));
        }
    }

    public void GoTo(string sceneFilePath, object data = default)
    {
        if (!scenesCache.ContainsKey(sceneFilePath))
        {
            scenesCache[sceneFilePath] = GD.Load<PackedScene>(sceneFilePath);
        }
        PackedScene scene = scenesCache[sceneFilePath];
        GoTo(scene, data);
    }

    public void GoBack(object dataOverride = default)
    {
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

        ChangePage(scene, data);
    }

    private bool ChangePage(PackedScene scene, object data)
    {
        Node current = tree.CurrentScene;

        try
        {
            Node node = scene.Instantiate();
            tree.Root.AddChild(node);
            tree.CurrentScene = node;
            logger.Log(tree.CurrentScene.Name);
        }
        catch (Exception exception)
        {
            logger.Error($"Failed to change page: {exception.Message}");
            PageChangeError?.Invoke(scene, exception.Message);
            return false;
        }

        current.QueueFree();

        _ = CallPostReady(tree.CurrentScene, data);

        return true;
    }
}
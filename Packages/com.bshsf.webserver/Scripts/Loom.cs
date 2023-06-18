// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.LowLevel;
namespace zFramework.Web
{
    /// <summary>
    /// 任务同步器：在主线程中执行 Action 委托
    /// <br>原名 TaskSync，但是觉得 Loom（织布机）更有意境</br>
    /// </summary>
    public static class Loom
    {
        public static int mainThreadId;
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;
        static readonly ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            #region 使用 PlayerLoop 在 Unity 主线程的 Update 中更新本任务同步器
            var playerloop = PlayerLoop.GetCurrentPlayerLoop();
            var loop = new PlayerLoopSystem
            {
                type = typeof(Loom),
                updateDelegate = Update
            };
            //1. 找到 Update Loop System
            int index = Array.FindIndex(playerloop.subSystemList, v => v.type == typeof(UnityEngine.PlayerLoop.Update));
            //2.  将咱们的 loop 插入到 Update loop 中
            var updateloop = playerloop.subSystemList[index];
            var temp = updateloop.subSystemList.ToList();
            temp.Add(loop);
            updateloop.subSystemList = temp.ToArray();
            playerloop.subSystemList[index] = updateloop;
            //3. 设置自定义的 Loop 到 Unity 引擎
            PlayerLoop.SetPlayerLoop(playerloop);
#if UNITY_EDITOR
            //4. 已知：编辑器停止 Play 我们自己插入的 loop 依旧会触发，进入或退出Play 模式先清空 tasks
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
            {
                if (obj == PlayModeStateChange.ExitingEditMode ||
                      obj == PlayModeStateChange.ExitingPlayMode)
                {
                    //清空任务列表
                    while (tasks.TryDequeue(out _)) { }
                }
            }
#endif
            #endregion
        }

#if UNITY_EDITOR
        //5. 确保编辑器下推送的事件也能被执行
        [InitializeOnLoadMethod]
        static void EditorForceUpdate()
        {
            Install();
            EditorApplication.update -= ForceEditorPlayerLoopUpdate;
            EditorApplication.update += ForceEditorPlayerLoopUpdate;
            void ForceEditorPlayerLoopUpdate()
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    // Not in Edit mode, don't interfere
                    return;
                }
                Update();
            }
        }
#endif

        /// <summary>
        ///  在主线程中执行
        /// </summary>
        /// <param name="task">要执行的委托</param>
        public static void Post(Action task)
        {
            if (IsMainThread)
            {
                task?.Invoke();
            }
            else
            {
                tasks.Enqueue(task);
            }
        }

        static void Update()
        {
            while (tasks.TryDequeue(out var task))
            {
                try
                {
                    task?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.Log($"{nameof(Loom)}:  封送的任务执行过程中发现异常，请确认: {e}");
                }
            }
        }
        /// <summary>
        /// 切换到主线程中执行
        /// </summary>
        public static SwitchToUnityThreadAwaitable ToMainThread => new SwitchToUnityThreadAwaitable();
        /// <summary>
        /// 切换到线程池中执行
        /// </summary>
        public static SwitchToThreadPoolAwaitable ToOtherThread => new SwitchToThreadPoolAwaitable();
    }
    public struct SwitchToUnityThreadAwaitable
    {
        public Awaiter GetAwaiter() => new();
        public struct Awaiter : INotifyCompletion
        {
            public bool IsCompleted => Loom.IsMainThread;
            public void GetResult() { }
            public void OnCompleted(Action continuation) => Loom.Post(continuation);
        }
    }
    public struct SwitchToThreadPoolAwaitable
    {
        public Awaiter GetAwaiter() => new();
        public struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => false;
            public void GetResult() { }
            public void OnCompleted(Action continuation) => ThreadPool.UnsafeQueueUserWorkItem(Callback, continuation);
            public void UnsafeOnCompleted(Action continuation) => ThreadPool.UnsafeQueueUserWorkItem(Callback, continuation);
            static void Callback(object state) => ((Action)state).Invoke();
        }
    }
}
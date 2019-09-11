using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace Game
{
    public static class GameFlow
    {
        public static void Init()
        {
            
        }

        public static async UniTaskVoid MainMenu()
        {
            Debug.Log("Main menu");
            await UIManager.FadeIn();
            UIManager.ShowScreenInst<UIMainMenu>();
            await UIManager.FadeOut();
        }
    }
}
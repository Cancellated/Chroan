using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MyGame.System;
using MyGame.Managers;
using Dialog.Config;
using MyGame.Data;


namespace Dialog
{
    /// <summary>
    /// 对话系统核心控制器，功能包括：
    /// 1. 管理对话序列的播放流程
    /// 2. 实现逐字显示的文字效果
    /// 3. 处理用户输入的中断逻辑
    /// 4. 与全局事件系统集成
    /// 5. 背景音乐控制
    /// </summary>
    public class DialogManager : Singleton<DialogManager>
    {
        #region 字段
        [Header("UI Components")]
        [Tooltip("对话弹窗的CanvasGroup组件")]
        [SerializeField] private CanvasGroup dialogPopup;
        [Tooltip("显示对话内容的Text组件")]
        [SerializeField] private Text dialogText;
        [Tooltip("显示当前说话角色名的Text组件")]
        [SerializeField] private Text characterName;

        [Header("音频组件")]
        [Tooltip("BGM音频源")]
        [SerializeField] private AudioSource bgmSource;

        [Header("图像组件")]
        [Tooltip("角色立绘Image组件")]
        [SerializeField] private Image characterImage;
        [Tooltip("背景图Image组件")] 
        [SerializeField] private Image backgroundImage;

        private Queue<DialogData> _currentDialogs = new();
        private Coroutine _typingCoroutine;
        private bool _isTypingComplete;
        private bool _shouldSkipCurrentText;
        private AudioClip _currentBGM;
        #endregion
        #region 生命周期
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject); // 保持对话系统常驻
            dialogPopup.alpha = 0; // 初始隐藏
            dialogPopup.blocksRaycasts = false;

            GameEvents.OnStoryEnter += HandleStoryEnter;
            GameEvents.OnStoryComplete += HandleStoryComplete;
            
        }

        private void OnDestroy()
        {
            GameEvents.OnStoryEnter -= HandleStoryEnter;
            GameEvents.OnStoryComplete -= HandleStoryComplete;
        }
        #endregion
        private void ShowDialogUI()
        {
            dialogPopup.alpha = 1;
            dialogPopup.blocksRaycasts = true;
        }

        private void HideDialogUI()
        {
            dialogPopup.alpha = 0;
            dialogPopup.blocksRaycasts = false;
        }

        /// <summary>
        /// 处理故事进入事件, 触发对话系统开始
        /// </summary>
        /// <param name="storyId"></param>
        private void HandleStoryEnter(int storyId)
        {
            Debug.Log($"收到故事进入事件，storyId: {storyId}");
            ShowDialogUI();
            var dialogs = DialogConfigManager.GetDialogsByStoryId(storyId);
            
            if(dialogs != null) 
            {
                Debug.Log($"成功加载对话配置，共{dialogs.Count}条对话");
                PlayBGM(dialogs[0].BGM);
                
                foreach(var dialog in dialogs)
                {
                    if(string.IsNullOrEmpty(dialog.Character) || string.IsNullOrEmpty(dialog.Content))
                    {
                        Debug.LogError($"发现空对话数据 - 角色: '{dialog.Character}', 内容: '{dialog.Content}'");
                    }
                }
                HandleDialogStart(dialogs);
            }
            else
            {
                Debug.LogError($"无法加载storyId={storyId}的对话配置");
            }
        }
        /// <summary>
        /// 处理故事完成事件
        /// </summary>
        /// <param name="storyId"></param>
        private void HandleStoryComplete(int storyId)
        {
            Debug.Log($"收到故事完成事件, storyId: {storyId}");
        }
        private void PlayBGM(string bgmName)
        {
            if (string.IsNullOrEmpty(bgmName) || bgmName == "None") 
            {
                // 如果配置为None则停止当前BGM
                if(bgmSource.isPlaying)
                {
                    bgmSource.Stop();
                    _currentBGM = null;
                }
                return;
            }

            var bgmClip = Resources.Load<AudioClip>($"Audios/对话/{bgmName}");
            if (bgmClip == null)
            {
                Debug.LogError($"无法加载BGM: {bgmName}");
                return;
            }

            if(bgmClip != _currentBGM || !bgmSource.isPlaying)
            {
                _currentBGM = bgmClip;
                bgmSource.clip = _currentBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        private void StopBGM()
        {
            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
            _currentBGM = null;
        }

        private void HandleDialogStart(List<DialogData> dialogs)
        {
            Debug.Log("开始处理对话队列");
            
            var sortedDialogs = new SortedDictionary<int, DialogData>();
            foreach(var dialog in dialogs)
            {
                if(int.TryParse(dialog.DialogID, out int id))
                {
                    sortedDialogs.Add(id, dialog);
                }
                else
                {
                    Debug.LogError($"无效的DialogID格式: {dialog.DialogID}");
                }
            }
            
            _currentDialogs = new Queue<DialogData>(sortedDialogs.Values);
            StartCoroutine(PlayDialogs());
        }

        private IEnumerator PlayDialogs()
        {
            Debug.Log("[对话系统] 开始播放对话序列");
            dialogPopup.alpha = 1;
            
            while (_currentDialogs.Count > 0)
            {
                var data = _currentDialogs.Peek();
                PlayBGM(data.BGM);
                ShowText(data.Content, data.Character, data.Name ,data.Background);
                
                yield return new WaitUntil(() => _isTypingComplete);
                
                bool inputDetected = false;
                while (!inputDetected)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        inputDetected = true;
                    }
                    yield return null;
                }
                
                _currentDialogs.Dequeue();
            }
            
            Debug.Log("[对话系统] 对话播放完成");
            dialogPopup.alpha = 0;
            StopBGM();
            
            if(_currentDialogs.Count == 0)
            {
                
            }
        }

        private IEnumerator TypeText(string content)
        {
            dialogText.text = "";
            _isTypingComplete = false;
            _shouldSkipCurrentText = false;
            
            foreach (char c in content)
            {
                if (_shouldSkipCurrentText)
                {
                    dialogText.text = content;
                    break;
                }
                
                dialogText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
        
            _isTypingComplete = true;
            _typingCoroutine = null;
        }

        private void ShowText(string content, string character, string name, string background)
        {
            // 处理背景图加载
            if(!string.IsNullOrEmpty(background))
            {
                string bgPath = $"Images/Dialog/Background/{background}";
                var bgSprite = Resources.Load<Sprite>(bgPath);
                if(bgSprite != null)
                {
                    backgroundImage.sprite = bgSprite;
                    backgroundImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"背景图加载失败，路径：Assets/Resources/{bgPath}");
                }
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }
        
            // 处理角色立绘
            if(character == "旁白")
            {
                characterImage.gameObject.SetActive(false);
            }
            else if(!string.IsNullOrEmpty(character)) 
            {
                string charPath = $"Images/Dialog/Character/{character}";
                var sprite = Resources.Load<Sprite>(charPath);
                
                if(sprite != null)
                {
                    characterImage.sprite = sprite;
                    characterImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"角色立绘加载失败：{charPath}");
                    characterImage.gameObject.SetActive(false);
                }
            }
            
            
            // 设置角色名显示
            if(name == "旁白")
            {
                characterName.text = "";
            }
            else characterName.text = name;

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            _typingCoroutine = StartCoroutine(TypeText(content));
        }


        private void HandleChapterComplete(int chapterId)
        {
            Debug.Log($"收到章节完成事件，chapterId: {chapterId}");
        }

    }
}
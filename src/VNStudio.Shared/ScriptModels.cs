using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VNStudio.Shared
{
    public class VisualNovelProject
    {
        public string Title { get; set; } = "Untitled Visual Novel";
        public string StartSceneId { get; set; } = "start";
        public Dictionary<string, Scene> Scenes { get; set; } = new Dictionary<string, Scene>();
    }

    public class Scene
    {
        public string BackgroundImage { get; set; } = "";
        public List<DialogueLine> Lines { get; set; } = new List<DialogueLine>();
        public string NextSceneId { get; set; } = ""; 
    }

    public class DialogueLine : INotifyPropertyChanged
    {
        private string _speaker = "";
        private string _dialogue = "";
        private string _fullScreenCg = "";
        private string _audioEffect = "";
        private string _bgm = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Speaker
        {
            get => _speaker;
            set
            {
                if (_speaker != value)
                {
                    _speaker = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Dialogue
        {
            get => _dialogue;
            set
            {
                if (_dialogue != value)
                {
                    _dialogue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullScreenCG
        {
            get => _fullScreenCg;
            set
            {
                if (_fullScreenCg != value)
                {
                    _fullScreenCg = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BGM
        {
            get => _bgm;
            set
            {
                if (_bgm != value)
                {
                    _bgm = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, string> ActiveSprites { get; set; } = new Dictionary<string, string>();

        public string AudioEffect
        {
            get => _audioEffect;
            set
            {
                if (_audioEffect != value)
                {
                    _audioEffect = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
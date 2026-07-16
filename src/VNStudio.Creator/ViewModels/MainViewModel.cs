using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using VNStudio.Shared;

namespace VNStudio.Creator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private VisualNovelProject _project;
        private string _selectedSceneId = "";
        private Scene _selectedScene = new Scene();
        private DialogueLine _selectedLine = new DialogueLine();
        private string _projectFilePath = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> SceneIds { get; } = new ObservableCollection<string>();
        public ObservableCollection<DialogueLine> CurrentDialogueLines { get; } = new ObservableCollection<DialogueLine>();
        public IStorageProvider? StorageProvider { get; set; }

        public ICommand AddSceneCommand { get; }
        public ICommand AddLineCommand { get; }
        public ICommand DeleteSceneCommand { get; }
        public ICommand DeleteLineCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand ExportJsonCommand { get; }

        public MainViewModel()
        {
            _project = new VisualNovelProject();
            
            NewProject(); // Initializes default start scene automatically

            AddSceneCommand = new RelayCommand(AddScene);
            AddLineCommand = new RelayCommand(AddLine);
            DeleteSceneCommand = new RelayCommand(DeleteScene);
            DeleteLineCommand = new RelayCommand(DeleteLine);
            NewProjectCommand = new RelayCommand(NewProject);
            SaveProjectCommand = new RelayCommand(async () => await SaveProjectAsync());
            OpenProjectCommand = new RelayCommand(async () => await OpenProjectAsync());
            ExportJsonCommand = new RelayCommand(async () => await ExportJsonAsync());
        }

        public string ProjectTitle
        {
            get => _project?.Title ?? "Untitled Visual Novel";
            set
            {
                if (_project != null && _project.Title != value)
                {
                    _project.Title = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public string WindowTitle
        {
            get => $"Kei - [{ProjectTitle}]";
        }
        public string SelectedSceneId
        {
            get => _selectedSceneId;
            set
            {
                if (_selectedSceneId != value)
                {
                    _selectedSceneId = value;
                    OnPropertyChanged();
                    LoadScene(value);
                }
            }
        }

        public Scene SelectedScene
        {
            get => _selectedScene;
            set
            {
                _selectedScene = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSceneBackground));
                OnPropertyChanged(nameof(CurrentNextSceneId));
            }
        }

        public DialogueLine SelectedLine
        {
            get => _selectedLine;
            set
            {
                if (_selectedLine != value && value != null)
                {
                    _selectedLine = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentSpeaker));
                    OnPropertyChanged(nameof(CurrentDialogue));
                    OnPropertyChanged(nameof(LeftSprite));
                    OnPropertyChanged(nameof(CenterSprite));
                    OnPropertyChanged(nameof(RightSprite));
                    OnPropertyChanged(nameof(CGImage));
                    OnPropertyChanged(nameof(CurrentBGM));
                    OnPropertyChanged(nameof(CurrentAudioEffect));
                }
            }
        }

        public string CurrentSpeaker
        {
            get => SelectedLine?.Speaker ?? "";
            set
            {
                if (SelectedLine != null && SelectedLine.Speaker != value)
                {
                    SelectedLine.Speaker = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentDialogue
        {
            get => SelectedLine?.Dialogue ?? "";
            set
            {
                if (SelectedLine != null && SelectedLine.Dialogue != value)
                {
                    SelectedLine.Dialogue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LeftSprite
        {
            get => GetSpriteAtPosition("Left");
            set => SetSpriteAtPosition("Left", value);
        }

        public string CenterSprite
        {
            get => GetSpriteAtPosition("Center");
            set => SetSpriteAtPosition("Center", value);
        }

        public string RightSprite
        {
            get => GetSpriteAtPosition("Right");
            set => SetSpriteAtPosition("Right", value);
        }

        public string CGImage
        {
            get => SelectedLine?.FullScreenCG ?? "";
            set
            {
                if (SelectedLine != null && SelectedLine.FullScreenCG != value)
                {
                    SelectedLine.FullScreenCG = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentBGM
        {
            get => SelectedLine?.BGM ?? "";
            set
            {
                if (SelectedLine != null && SelectedLine.BGM != value)
                {
                    SelectedLine.BGM = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentAudioEffect
        {
            get => SelectedLine?.AudioEffect ?? "";
            set
            {
                if (SelectedLine != null && SelectedLine.AudioEffect != value)
                {
                    SelectedLine.AudioEffect = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentSceneBackground
        {
            get => SelectedScene?.BackgroundImage ?? "";
            set
            {
                if (SelectedScene != null && SelectedScene.BackgroundImage != value)
                {
                    SelectedScene.BackgroundImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentNextSceneId
        {
            get => SelectedScene?.NextSceneId ?? "";
            set
            {
                if (SelectedScene != null && SelectedScene.NextSceneId != value)
                {
                    SelectedScene.NextSceneId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string GetSpriteAtPosition(string position)
        {
            if (SelectedLine != null && SelectedLine.ActiveSprites.TryGetValue(position, out var sprite))
            {
                return sprite;
            }
            return "";
        }

        private void SetSpriteAtPosition(string position, string value)
        {
            if (SelectedLine != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SelectedLine.ActiveSprites.Remove(position);
                }
                else
                {
                    SelectedLine.ActiveSprites[position] = value;
                }
                OnPropertyChanged(position + "Sprite");
            }
        }

        private void LoadScene(string sceneId)
        {
            if (sceneId != null && _project.Scenes.TryGetValue(sceneId, out var scene))
            {
                SelectedScene = scene;
                CurrentDialogueLines.Clear();
                foreach (var line in scene.Lines)
                {
                    CurrentDialogueLines.Add(line);
                }
                SelectedLine = CurrentDialogueLines.FirstOrDefault() ?? new DialogueLine();
            }
        }

        private void AddScene()
        {
            int nextNumber = _project.Scenes.Count + 1;
            string newId = $"scene_{nextNumber:D2}";
            
            var newScene = new Scene();
            newScene.Lines.Add(new DialogueLine { Speaker = "Narrator", Dialogue = "A new scene begins..." });
            
            _project.Scenes.Add(newId, newScene);
            SceneIds.Add(newId);
            SelectedSceneId = newId; 
        }

        private void AddLine()
        {
            if (SelectedScene != null)
            {
                var newLine = new DialogueLine { Speaker = "Speaker", Dialogue = "New dialogue..." };
                SelectedScene.Lines.Add(newLine);
                CurrentDialogueLines.Add(newLine);
                SelectedLine = newLine;
            }
        }

        private void DeleteScene()
        {
            if (SceneIds.Count <= 1 || string.IsNullOrEmpty(SelectedSceneId)) return;

            string sceneToDelete = SelectedSceneId;
            int currentIndex = SceneIds.IndexOf(sceneToDelete);
            int nextIndex = currentIndex == SceneIds.Count - 1 ? currentIndex - 1 : currentIndex + 1;
            string nextSceneId = SceneIds[nextIndex];

            _project.Scenes.Remove(sceneToDelete);
            SceneIds.Remove(sceneToDelete);

            SelectedSceneId = nextSceneId;
        }

        private void DeleteLine()
        {
            if (SelectedScene == null || SelectedLine == null || CurrentDialogueLines.Count <= 1) return;

            var lineToDelete = SelectedLine;
            int currentIndex = CurrentDialogueLines.IndexOf(lineToDelete);
            int nextIndex = currentIndex == CurrentDialogueLines.Count - 1 ? currentIndex - 1 : currentIndex + 1;
            var nextLine = CurrentDialogueLines[nextIndex];

            SelectedScene.Lines.Remove(lineToDelete);
            CurrentDialogueLines.Remove(lineToDelete);

            SelectedLine = nextLine;
        }

        private void NewProject()
        {
            _project = new VisualNovelProject();
            _projectFilePath = "";

            var startScene = new Scene { BackgroundImage = "classroom.png" };
            startScene.Lines.Add(new DialogueLine { Speaker = "System", Dialogue = "Welcome to your new story!" });
            _project.Scenes.Add("start", startScene);

            SceneIds.Clear();
            SceneIds.Add("start");
            SelectedSceneId = "start";

            OnPropertyChanged(nameof(ProjectTitle));
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveProjectAsync()
        {
            if (StorageProvider == null) return;

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Visual Novel Project",
                SuggestedFileName = "my_story.vnp",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Visual Novel Project (*.vnp)") { Patterns = new[] { "*.vnp" } }
                }
            });

            if (file != null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_project, options);

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(jsonString);

                _projectFilePath = file.Path.LocalPath;
            }
        }

        private async Task OpenProjectAsync()
        {
            if (StorageProvider == null) return;

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Visual Novel Project",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Visual Novel Project (*.vnp)") { Patterns = new[] { "*.vnp" } }
                }
            });

            if (files != null && files.Count > 0)
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string jsonString = await reader.ReadToEndAsync();

                var loadedProject = JsonSerializer.Deserialize<VisualNovelProject>(jsonString);
                if (loadedProject != null)
                {
                    _project = loadedProject;
                    OnPropertyChanged(nameof(ProjectTitle));
                    OnPropertyChanged(nameof(WindowTitle));
                    SceneIds.Clear();
                    foreach (var sceneId in _project.Scenes.Keys)
                    {
                        SceneIds.Add(sceneId);
                    }
                    SelectedSceneId = _project.StartSceneId;
                    _projectFilePath = file.Path.LocalPath;
                }
            }
        }

        private async Task ExportJsonAsync()
        {
            if (StorageProvider == null) return;

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Game Script",
                SuggestedFileName = "story_data.json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Game Script JSON (*.json)") { Patterns = new[] { "*.json" } }
                }
            });

            if (file != null)
            {
                // Create a clean, path-normalized copy of our project for Arisu
                var exportedProject = CloneAndPrepareForExport();

                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string jsonString = JsonSerializer.Serialize(exportedProject, options);

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(jsonString);

                Console.WriteLine("Game script exported with normalized assets paths!");
            }
        }

        // Deep copies the project and automatically prepends standard asset directories
        private VisualNovelProject CloneAndPrepareForExport()
        {
            var exportProject = new VisualNovelProject
            {
                Title = _project.Title,
                StartSceneId = _project.StartSceneId
            };

            foreach (var sceneEntry in _project.Scenes)
            {
                var rawScene = sceneEntry.Value;
                var exportScene = new Scene
                {
                    BackgroundImage = NormalizePath(rawScene.BackgroundImage, "assets/bgs/"),
                    NextSceneId = rawScene.NextSceneId
                };

                foreach (var rawLine in rawScene.Lines)
                {
                    var exportLine = new DialogueLine
                    {
                        Speaker = rawLine.Speaker,
                        Dialogue = rawLine.Dialogue,
                        FullScreenCG = NormalizePath(rawLine.FullScreenCG, "assets/cgs/"),
                        AudioEffect = NormalizePath(rawLine.AudioEffect, "assets/sfx/"),
                        BGM = rawLine.BGM == "stop" ? "stop" : NormalizePath(rawLine.BGM, "assets/bgm/")
                    };

                    foreach (var spriteEntry in rawLine.ActiveSprites)
                    {
                        exportLine.ActiveSprites[spriteEntry.Key] = NormalizePath(spriteEntry.Value, "assets/sprites/");
                    }

                    exportScene.Lines.Add(exportLine);
                }

                exportProject.Scenes.Add(sceneEntry.Key, exportScene);
            }

            return exportProject;
        }

        private string NormalizePath(string fileName, string targetFolder)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";

            if (fileName.StartsWith("assets/"))
            {
                return fileName;
            }

            return targetFolder + fileName;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
    }
}
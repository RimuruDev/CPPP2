


## Как добавить новую модель в данную систему?
1. Создать модель, например WorldProgress
2. Создать Proxy для WorldProgress -> WorldProgressProxy
3. Создать интерфейс для WorldProgressProxy -> IWorldProgressProxy
4. Добавить IWorldProgressProxy в интерфейс IProgressService как свойство { get; }
5. Добавить в Constants новую константу для сохранения WorldProgress
6. У имплементаций IProgressService, реализовать новый IWorldProgressProxy
7. Создать в DefaultProgressFactory дефлтныйе настройки для WorldProgress
8. В конструкторе зарегать новое свойство для idToActions -> Save/Load/Delete
9. Добавить в Dispose имплементации IProgressService IWorldProgressProxy.Dispose();
10. В методе LoadProgress имплементации IProgressService, указать IWorldProgressProxy
11. В IProgressValidator добавить IsValid и ValidateAndFix для WorldProgress
12. Реализовать методы  IsValid и ValidateAndFix у имплементации IProgressValidator
13. Готово! Остается только 1 раз сделать попытку на Load/LoadById


1.
```csharp
 [Serializable]
public class WorldProgress
{
    public Vector3Data CurrentWorldPosition;
    public Vector3Data CurrentWorldRotation;
    public float CurrentTime;
}

[Serializable]
public struct Vector3Data
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public Vector3Data(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 ToVector3() => 
        new(X, Y, Z);

    public Vector3Data ToVector3Data() => 
        new(X, Y, Z);
}
```

2.
```csharp
  public class WorldProgressProxy : IWorldProgressProxy
    {
        public WorldProgress Origin { get; private set; }
        public ReactiveProperty<Vector3Data> CurrentWorldPosition { get; private set; }
        public ReactiveProperty<Vector3Data> CurrentWorldRotation { get; private set; }
        public ReactiveProperty<float> CurrentTime { get; private set; }

        private readonly List<IDisposable> disposables = new();

        public WorldProgressProxy(WorldProgress origin)
        {
            Origin = origin;

            CurrentWorldPosition = new ReactiveProperty<Vector3Data>(origin.CurrentWorldPosition);
            CurrentWorldRotation = new ReactiveProperty<Vector3Data>(origin.CurrentWorldRotation);
            CurrentTime = new ReactiveProperty<float>(origin.CurrentTime);

            disposables.Add(CurrentWorldPosition.Subscribe(value => Origin.CurrentWorldPosition = value));
            disposables.Add(CurrentWorldRotation.Subscribe(value => Origin.CurrentWorldRotation = value));
            disposables.Add(CurrentTime.Subscribe(value => Origin.CurrentTime = value));
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();

            disposables?.Clear();

            CurrentWorldPosition = null;
            CurrentWorldRotation = null;
            CurrentTime = null;
        }
    }
```

3.
```csharp
 public interface IWorldProgressProxy : IDisposable
 {
     public WorldProgress Origin { get; }
     public ReactiveProperty<Vector3Data> CurrentWorldPosition { get; }
     public ReactiveProperty<Vector3Data> CurrentWorldRotation { get; }
     public ReactiveProperty<float> CurrentTime { get; }
 }
```
4.
```csharp
public interface IProgressService : IDisposable
{
    ...
    ...
    public IWorldProgressProxy WorldProgress { get; }
    ...
    ...
}
```
5.
```csharp
 public static class Constants
    {
        public const string ROOT_FOLDER_NAME = "Database";
        public const string USER_PROGRESS_FILE = "user_progress";
        public const string AUDIO_SETTINGS_FILE = "audio_settings";
        // Вот так! :3
        public const string WORLD_PROGRESS_FILE = "world_progress";
    }
```

6. f
```csharp
// Ну тут и так понятно, шлепнуть по ошибке у интерфейса IProgressService
 public IWorldProgressProxy WorldProgress { get; private set; }
```
7. в
```csharp
   public static class DefaultProgressFactory
    {
        public static UserProgress CreateDefaultProgress()
        {
            return new UserProgress
            {
                UserName = "Rimuru",
                Level = 1,
                HardCurrency = 0,
                SoftCurrency = 0,
            };
        }

        public static AudioSettings CreateDefaultAudioSettings()
        {
            return new AudioSettings
            {
                BackgroundMusicVolume = 1,
                SfxVolume = 1
            };
        }

        // Вот так :D
        public static WorldProgress CreateDefaultWorldProgress()
        {
            return new WorldProgress
            {
                CurrentWorldPosition = new Vector3Data(10, 10, 10),
                CurrentWorldRotation = new Vector3Data(0, 0, 0),
                CurrentTime = 30
            };
        }
    }
```
8. Тут по одной строчке у каждой мапы добавить нужно
```csharp
  public MobileProgressService(
            IFileFormatConfiguration fileFormatConfig,
            IDataStorage dataStorage,
            IEncryptionService encryptionService,
            IProgressValidator validator,
            IProgressMigrationService migrationService)
        {
            this.fileFormatConfig = fileFormatConfig;
            this.dataStorage = dataStorage;
            this.encryptionService = encryptionService;
            this.validator = validator;
            this.migrationService = migrationService;

            directoryPath = Path.Combine(Application.persistentDataPath, Constants.ROOT_FOLDER_NAME);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            const string userProgressFile = Constants.USER_PROGRESS_FILE;
            const string audioSettingsFile = Constants.AUDIO_SETTINGS_FILE;
            const string worldProgressFile = Constants.WORLD_PROGRESS_FILE;

            // Маппинг ID на действия //
            idToLoadAction = new Dictionary<string, Action>
            {
                {
                    userProgressFile,
                    () => UserProgress = new UserProgressProxy(LoadAllProgress(userProgressFile,
                        DefaultProgressFactory.CreateDefaultProgress))
                },
                {
                    audioSettingsFile,
                    () => AudioSettings = new AudioSettingsProxy(LoadAllProgress(audioSettingsFile,
                        DefaultProgressFactory.CreateDefaultAudioSettings))
                },
                {
                    worldProgressFile,
                    () => WorldProgress = new WorldProgressProxy(LoadAllProgress(worldProgressFile,
                        DefaultProgressFactory.CreateDefaultWorldProgress))
                }
            };

            // Маппинг ID на действия сохранения //
            idToSaveAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => SaveProgress(userProgressFile, UserProgress.Origin) },
                { audioSettingsFile, () => SaveProgress(audioSettingsFile, AudioSettings.Origin) },
                { worldProgressFile, () => SaveProgress(worldProgressFile, WorldProgress.Origin) }
            };

            // Маппинг ID на действия удаления //
            idToDeleteAction = new Dictionary<string, Action>
            {
                { userProgressFile, () => DeleteProgress(userProgressFile) },
                { audioSettingsFile, () => DeleteProgress(audioSettingsFile) },
                { worldProgressFile, () => DeleteProgress(worldProgressFile) }
            };
        }
```
9.
````csharp
  public void Dispose()
  {
      UserProgress?.Dispose();
      AudioSettings?.Dispose();
      WorldProgress?.Dispose();

      idToLoadAction?.Clear();
      idToSaveAction?.Clear();
      idToDeleteAction?.Clear();
  }
````
10. Тут все просто но надо не забывать
```csharp
Метод ->    private TData LoadAllProgress<TData>(string fileName, Func<TData> createDefault) where TData : class
    
      try
            {
                var progress = fileFormatConfig.CurrentFormatHandler.Deserialize<TData>(decryptedData);

                if (typeof(TData) == typeof(UserProgress) && !validator.IsValid((UserProgress)(object)progress))
                    throw new InvalidDataException("Invalid UserProgress data.");

                if (typeof(TData) == typeof(AudioSettings) && !validator.IsValid((AudioSettings)(object)progress))
                    throw new InvalidDataException("Invalid AudioSettings data.");

                // Вот так нужно сделать
                if (typeof(TData) == typeof(WorldProgress) && !validator.IsValid((WorldProgress)(object)progress))
                    throw new InvalidDataException("Invalid WorldProgress data.");


                return progress;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load progress: {e.Message}. Initializing default data.");
                return createDefault();
            }
```
11. Слой валидации
```csharp
 public interface IProgressValidator
    {
        public bool IsValid(UserProgress progress);
        public UserProgress ValidateAndFix(UserProgress progress);

        public bool IsValid(AudioSettings progress);
        public AudioSettings ValidateAndFix(AudioSettings progress);
        
        public bool IsValid(WorldProgress progress);
        public WorldProgress ValidateAndFix(WorldProgress progress);
    }
    
 ```
12.
```csharp
  public class ProgressValidationService : IProgressValidator
  {
      // Ну для простоты вот так пусть будет.
      // Так конечно тут логика по сложнее.
       public bool IsValid(WorldProgress progress)
        {
            if (progress == null)
                return false;

            return true;
        }

        public WorldProgress ValidateAndFix(WorldProgress progress)
        {
            if (progress == null)
            {
                Debug.LogWarning("Progress is null, initializing default progress.");
                return DefaultProgressFactory.CreateDefaultWorldProgress();
            }

            return progress;
        }
   }
```


Все! Теперь только дергуть метод   progressService.LoadAllProgress(); и файлик будет создан.

````csharp
    public class PMExample : MonoBehaviour
    {
        [SerializeField] private FileFormatType fileFormat;
        private MobileProgressService progressService;

        private void Awake()
        {
            var dataStorage = new FileDataStorage();
            var encryptionService = new SimpleEncryptionService();
            var validator = new ProgressValidationService();
            var jsonHandler = new JsonFileFormatHandler();
            var binaryHandler = new BinaryFileFormatHandler();

            var fileFormatConfig = fileFormat switch
            {
                FileFormatType.Json => new FileFormatConfiguration(currentHandler: jsonHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }),
                FileFormatType.Binary => new FileFormatConfiguration(currentHandler: binaryHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler }),
                _ => new FileFormatConfiguration(currentHandler: jsonHandler, supportedHandlers: new List<IFileFormatHandler> { jsonHandler, binaryHandler })
            };

            var migrationService = new ProgressMigrationService(dataStorage, encryptionService);

            progressService = new MobileProgressService(
                fileFormatConfig,
                dataStorage,
                encryptionService,
                validator,
                migrationService
            );

            // progressService.LoadProgressById(Constants.AUDIO_SETTINGS_FILE);
            // progressService.LoadProgressById(Constants.USER_PROGRESS_FILE);
            //
            // progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
            // progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);
            
            progressService.LoadAllProgress();
            progressService.SaveAllProgress();
        }


        [ContextMenu(nameof(TestSave))]
        public void TestSave()
        {
            progressService.UserProgress.SoftCurrency.Value += 100;
            progressService.AudioSettings.BackgroundMusicVolume.Value -= 0.22f;
            progressService.AudioSettings.SfxVolume.Value += GetRandom01();

            progressService.SaveProgressById(Constants.USER_PROGRESS_FILE);
            progressService.SaveProgressById(Constants.AUDIO_SETTINGS_FILE);
        }
}
````
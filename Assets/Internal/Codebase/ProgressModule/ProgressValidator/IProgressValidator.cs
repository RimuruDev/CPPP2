using Internal.Codebase.ProgressModule.Models.Gameplay;

namespace Internal
{
    public interface IProgressValidator
    {
        public bool IsValid(UserProgress progress);
        public UserProgress ValidateAndFix(UserProgress progress);

        public bool IsValid(AudioSettings progress);
        public AudioSettings ValidateAndFix(AudioSettings progress);
        
        public bool IsValid(WorldProgress progress);
        public WorldProgress ValidateAndFix(WorldProgress progress);
    }
}
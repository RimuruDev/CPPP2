namespace Internal
{
    public interface IProgressValidator
    {
        bool IsValid(UserProgress progress);
        bool IsValid(AudioSettings progress);

        public UserProgress ValidateAndFix(UserProgress progress);
        public AudioSettings ValidateAndFix(AudioSettings progress);
    }
}
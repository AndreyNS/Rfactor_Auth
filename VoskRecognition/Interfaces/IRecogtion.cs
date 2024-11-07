namespace VoskRecognition.Interfaces
{
    public interface IRecogtion
    {
        Task<string> RecognizeSpeech(Stream stream);
    }
}

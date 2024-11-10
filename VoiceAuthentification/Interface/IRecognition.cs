namespace VoiceAuthentification.Interface
{
    public interface IRecognition
    {
        Task<string> RecognizeSpeech(byte[] audioBytes);
    }
}

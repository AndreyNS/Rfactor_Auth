namespace VoiceAuthentification.Interface
{
    public interface IManager
    {
    }

    public interface IVoiceManager : IManager
    {
        Task SetStreamAudio(Stream stream);
        Task VoiceProcessAsync();
    }
}

namespace Rfactor_Auth.Server.Interfaces
{
    public interface IVoiceConverter
    {
        Task<Stream> Convert(Stream voice);
        void TurnSaver(bool isSave);
    }

    public abstract class VoiceConverterBase : IVoiceConverter
    {
        public virtual Task<Stream> Convert(Stream voice)
        {
            throw new NotImplementedException();
        }

        public virtual void TurnSaver(bool isSave)
        {
            throw new NotImplementedException();
        }

    }
}

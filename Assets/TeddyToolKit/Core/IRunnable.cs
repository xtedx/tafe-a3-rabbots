namespace TeddyToolKit.Core
{
    public interface IRunnable
    {
        bool Enabled { get; set; }
        void Setup(params object[] parameters);
        void Run(params object[] parameters);
    }
}
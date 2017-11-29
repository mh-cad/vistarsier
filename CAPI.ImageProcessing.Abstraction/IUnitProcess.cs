namespace CAPI.ImageProcessing.Abstraction
{
    public interface IUnitProcess
    {
        string[] Parameters { get; set; }
        void Run();
    }
}

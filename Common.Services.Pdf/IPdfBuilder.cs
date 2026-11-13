namespace Common.Services.Pdf
{
    public interface IPdfBuilder<T>
    {
        byte[] Build(T model);
    }
}

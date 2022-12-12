namespace Anatawa12.AnimatorControllerAsACode.Framework.Clip
{
    internal interface IFixedArray<T>
    {
        T this[int x] { get; set; }
        int Length { get; }
    }
}

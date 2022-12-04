namespace Anatawa12.AnimatorControllerAsACode.Examples
{

    public enum Hand
    {
        Left,
        Right,
    }

    public static class Utils
    {
        public static Hand Opposite(this Hand hand)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            return hand ^ (Hand)1;
        }
    }
}
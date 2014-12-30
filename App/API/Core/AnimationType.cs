namespace Newgen
{
    /// <summary>
    /// Animation type
    /// </summary>
    public enum AnimationType
    {
        /// <summary>
        /// Animation is controlled internally, i.e. default animation.
        /// </summary>
        Internal = 0,
        /// <summary>
        /// Animation not controlled internally, i.e. you'll have to implement your logic for animation.
        /// </summary>
        Custom = 1
    }
}
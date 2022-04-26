namespace TetraPak.XP.Diagnostics
{
    /// <summary>
    ///   Provides convenient extensions for working with <see cref="StateDump"/>s.
    /// </summary>
    public static class StateDumpHelper
    {
        /// <summary>
        ///   (fluid api)<br/>
        ///   Increases the <see cref="StateDumpContext.Indentation"/> value one level and returns the context.
        /// </summary>
        public static StateDumpContext Indent(this StateDumpContext context) =>
            context.WithIndentation(context.Indentation.Push());

        /// <summary>
        ///   (fluid api)<br/>
        ///   Decreases the <see cref="StateDumpContext.Indentation"/> value one level and returns the context.
        /// </summary>
        public static StateDumpContext Outdent(this StateDumpContext context) =>
            context.WithIndentation(context.Indentation.Pop());

    }
}
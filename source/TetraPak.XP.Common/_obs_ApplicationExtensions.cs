/* obsolete?
using System.Collections.Generic;
using Xamarin.Forms;

namespace TetraPak.Auth.Xamarin.common
{
    // todo Consider moving ApplicationExtensions to a common NuGet package to be referenced instead
    /// <summary>
    ///   Extends a Xamarin Forms <seealso cref="Application"/>.
    /// </summary>
    public static class ApplicationExtensions
    {
        static readonly Stack<Page> s_modalStack = new Stack<Page>();

        /// <summary>
        ///   Gets the current page of the app.
        /// </summary>
        /// <returns>
        ///   A <seealso cref="Page"/>.
        /// </returns>
        public static Page CurrentPage(this Application self)
        {
            if (s_modalStack.Count != 0)
                return s_modalStack.Peek();

            if (self.MainPage is NavigationPage navigationPage)
                return navigationPage.CurrentPage;

            if (self.MainPage is TabbedPage tabbedPage)
                return tabbedPage.CurrentPage;

            // todo Support more parent page types
            return self.MainPage;
        }

        /// <summary>
        ///   Used to initialize the extension.
        /// </summary>
        public static void InitializeExtensions(this Application self)
        {
            self.ModalPushed += (s, e) => s_modalStack.Push(e.Modal);
            self.ModalPopped += (s, e) =>
            {
#if DEBUG
                if (e.Modal != s_modalStack.Peek())
                    throw new Exception("Huh?");
#endif
                s_modalStack.Pop();
            };
        }
    }
}
*/
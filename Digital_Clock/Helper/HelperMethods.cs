using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Digital_Clock.Helper
{
    class HelperMethods
    {
        #region HelperMethods

        /// <summary>
        /// Get controls of type in children of control
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindWindowChildren<T>(DependencyObject dObj) where T : DependencyObject
        {
            if (dObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dObj); i++)
                {
                    DependencyObject ch = VisualTreeHelper.GetChild(dObj, i);
                    if (ch != null && ch is T)
                    {
                        yield return (T)ch;
                    }

                    foreach (T nestedChild in FindWindowChildren<T>(ch))
                    {
                        yield return nestedChild;
                    }
                }
            }
        }

        /// <summary>
        /// Convert number to enum of specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        public static T NumToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        /// <summary>
        ///  Checks if all character is numeral
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (c != '.')
                {
                    if (!Char.IsNumber(c))
                        return false;
                }
            }

            return true;
        }

        #endregion HelperMethods

    }
}

/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : Utilities.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/24/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Provides helper methods that can be used by Windows Forms applications.
 *    
 * 
 ********************************************************************************/

using System;
using System.Windows.Forms;

namespace AwesomiumSharp.Windows.Forms
{
    /// <summary>
    /// Utility class providing helper methods that can be used by Windows Forms applications.
    /// </summary>
    public static class Utilities
    {
        #region GetCursor
        /// <summary>
        /// Gets the Windows Forms <see cref="Cursor"/> equivalent of an Awesomium <see cref="CursorType"/>.
        /// </summary>
        /// <param name="cursor">
        /// The Awesomium cursor type. You can get this by handling the <see cref="WebView.CursorChanged"/> event.
        /// </param>
        /// <returns>
        /// A Windows Forms <see cref="Cursor"/>, or null (Nothing in VB) if <see cref="CursorType.None"/>
        /// is specified.
        /// </returns>
        public static Cursor GetCursor( CursorType cursor )
        {
            switch ( cursor )
            {
                case CursorType.ColumnResize:
                    return Cursors.SizeWE;

                case CursorType.Cross:
                    return Cursors.Cross;

                case CursorType.EastPanning:
                    return Cursors.PanEast;

                case CursorType.EastResize:
                    return Cursors.SizeWE;

                case CursorType.EastWestResize:
                    return Cursors.SizeWE;

                case CursorType.Hand:
                    return Cursors.Hand;

                case CursorType.Help:
                    return Cursors.Help;

                case CursorType.IBeam:
                    return Cursors.IBeam;

                case CursorType.MiddlePanning:
                    return Cursors.Cross;

                case CursorType.Move:
                    return Cursors.Cross;

                case CursorType.NoDrop:
                    return Cursors.No;

                case CursorType.None:
                    return null;

                case CursorType.NortheastPanning:
                    return Cursors.PanNE;

                case CursorType.NortheastResize:
                    return Cursors.SizeNESW;

                case CursorType.NortheastSouthwestResize:
                    return Cursors.SizeNESW;

                case CursorType.NorthPanning:
                    return Cursors.PanNorth;

                case CursorType.NorthResize:
                    return Cursors.SizeNS;

                case CursorType.NorthSouthResize:
                    return Cursors.SizeNS;

                case CursorType.NorthwestPanning:
                    return Cursors.PanNW;

                case CursorType.NorthwestResize:
                    return Cursors.SizeNWSE;

                case CursorType.NorthwestSoutheastResize:
                    return Cursors.SizeNWSE;

                case CursorType.NotAllowed:
                    return Cursors.No;

                case CursorType.Pointer:
                    return Cursors.Arrow;

                case CursorType.Progress:
                    return Cursors.WaitCursor;

                case CursorType.RowResize:
                    return Cursors.SizeNS;

                case CursorType.SoutheastPanning:
                    return Cursors.PanSE;

                case CursorType.SoutheastResize:
                    return Cursors.SizeNWSE;

                case CursorType.SouthPanning:
                    return Cursors.PanSouth;

                case CursorType.SouthResize:
                    return Cursors.SizeNS;

                case CursorType.SouthwestPanning:
                    return Cursors.PanSW;

                case CursorType.SouthwestResize:
                    return Cursors.SizeNESW;

                case CursorType.VerticalText:
                    return Cursors.UpArrow;

                case CursorType.Wait:
                    return Cursors.WaitCursor;

                case CursorType.WestPanning:
                    return Cursors.PanWest;

                case CursorType.WestResize:
                    return Cursors.SizeWE;

                default:
                    return Cursors.Arrow;
            }
        }
        #endregion

        #region GetKeyboardEvent
        /// <summary>
        /// Gets an Awesomium <see cref="WebKeyboardEvent"/> equivalent of a Windows Forms key-down or key-up event.
        /// </summary>
        /// <param name="eventType">
        /// Indicates if this is a key-down or key-up event.
        /// </param>
        /// <param name="e">
        /// The Windows Forms key-down or key-up event arguments.
        /// </param>
        /// <returns>
        /// An instance of a <see cref="WebKeyboardEvent"/> representing the Awesomium equivalent of a
        /// Windows Forms key-down or key-up event.
        /// </returns>
        public static WebKeyboardEvent GetKeyboardEvent( WebKeyType eventType, KeyEventArgs e )
        {
            WebKeyModifiers modifiers = 0;

            if ( e.Alt )
                modifiers |= WebKeyModifiers.AltKey;

            if ( e.Shift )
                modifiers |= WebKeyModifiers.ShiftKey;

            if ( e.Control )
                modifiers |= WebKeyModifiers.ControlKey;

            WebKeyboardEvent keyEvent = new WebKeyboardEvent()
            {
                Type = eventType,
                VirtualKeyCode = (VirtualKey)e.KeyCode,
                Modifiers = modifiers
            };

            return keyEvent;
        }

        /// <summary>
        /// Gets an Awesomium <see cref="WebKeyboardEvent"/> equivalent of a Windows Forms key-press event.
        /// </summary>
        /// <param name="e">
        /// The Windows Forms key-press event arguments.
        /// </param>
        /// <returns>
        /// An instance of a <see cref="WebKeyboardEvent"/> representing the Awesomium equivalent of a
        /// Windows Forms key-press event.
        /// </returns>
        public static WebKeyboardEvent GetKeyboardEvent( KeyPressEventArgs e )
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent { 
                Type = WebKeyType.Char, 
                Text = new ushort[] { e.KeyChar, 0, 0, 0 } };

            return keyEvent;
        }
        #endregion
    }
}

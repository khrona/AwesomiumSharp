/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : Utilities.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/06/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Includes WPF specific utilities such as the convertions of several
 *    Awesomium types to their WPF equivalent.
 *    
 * 
 ********************************************************************************/

using System;
using System.Windows.Input;

namespace AwesomiumSharp.Windows.Controls
{
    static class Utilities
    {
        public static Cursor GetCursor( CursorType cursor )
        {
            switch ( cursor )
            {
                case CursorType.ColumnResize:
                    return Cursors.SizeWE;

                case CursorType.Cross:
                    return Cursors.Cross;

                case CursorType.EastPanning:
                    return Cursors.ScrollE;

                case CursorType.EastResize:
                    return Cursors.SizeWE;

                case CursorType.EastWestResize:
                    return Cursors.SizeWE;

                case CursorType.Hand:
                    return Cursors.Hand;

                case CursorType.Help:
                    return Cursors.Help;

                case CursorType.Ibeam:
                    return Cursors.IBeam;

                case CursorType.MiddlePanning:
                    return Cursors.ScrollAll;

                case CursorType.Move:
                    return Cursors.ScrollAll;

                case CursorType.NoDrop:
                    return Cursors.No;

                case CursorType.None:
                    return Cursors.None;

                case CursorType.NortheastPanning:
                    return Cursors.ScrollNE;

                case CursorType.NortheastResize:
                    return Cursors.SizeNESW;

                case CursorType.NortheastSouthwestResize:
                    return Cursors.SizeNESW;

                case CursorType.NorthPanning:
                    return Cursors.ScrollN;

                case CursorType.NorthResize:
                    return Cursors.SizeNS;

                case CursorType.NorthSouthResize:
                    return Cursors.SizeNS;

                case CursorType.NorthwestPanning:
                    return Cursors.ScrollNW;

                case CursorType.NorthwestResize:
                    return Cursors.SizeNWSE;

                case CursorType.NorthwestSoutheastResize:
                    return Cursors.SizeNWSE;

                case CursorType.NotAllowed:
                    return Cursors.No;

                case CursorType.Pointer:
                    return Cursors.Arrow;

                case CursorType.Progress:
                    return Cursors.Wait;

                case CursorType.RowResize:
                    return Cursors.SizeNS;

                case CursorType.SoutheastPanning:
                    return Cursors.ScrollSE;

                case CursorType.SoutheastResize:
                    return Cursors.SizeNWSE;

                case CursorType.SouthPanning:
                    return Cursors.ScrollS;

                case CursorType.SouthResize:
                    return Cursors.SizeNS;

                case CursorType.SouthwestPanning:
                    return Cursors.ScrollSW;

                case CursorType.SouthwestResize:
                    return Cursors.SizeNESW;

                case CursorType.VerticalText:
                    return Cursors.UpArrow;

                case CursorType.Wait:
                    return Cursors.Wait;

                case CursorType.WestPanning:
                    return Cursors.ScrollW;

                case CursorType.WestResize:
                    return Cursors.SizeWE;

                default:
                    return Cursors.Arrow;
            }
        }
    }
}

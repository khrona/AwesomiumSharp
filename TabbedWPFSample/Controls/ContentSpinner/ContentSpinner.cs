//***************************************************************************
//    Project: TabbedWPFSample
//    File:    ContentSpinner.cs
//    Version: 1.0.0.0
//
//    Copyright ©2010 Perikles C. Stephanidis; All rights reserved.
//    This code is provided "AS IS" without warranty of any kind.
//***************************************************************************

#region Using
using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;
#endregion

namespace TabbedWPFSample
{
    internal enum ContentSpinnerCanvas : int
    {
        Custom,
        Rectangles,
        Doughnut
    }

    /// <summary>
    /// Simple control providing content spinning capability.
    /// </summary>
    class ContentSpinner : ContentControl
    {

        #region  Fields
        private const string ANIMATION = "AnimatedRotateTransform";
        private string m_AnimationName;

        private FrameworkElement _Presenter;
        private Storyboard _Storyboard;
        #endregion


        #region  Constructors
        static ContentSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ContentSpinner ), new FrameworkPropertyMetadata( typeof( ContentSpinner ) ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSpinner"/> class.
        /// </summary>
        public ContentSpinner()
        {
            m_AnimationName = string.Format( "Spinner{0}", DateTime.Now.Ticks );

            Loaded += ( o, args ) => StartAnimation();
            SizeChanged += ( o, args ) => RestartAnimation();
            Unloaded += ( o, args ) => StopAnimation();
        }
        #endregion


        #region  Methods
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Presenter = GetTemplateChild( "PART_Content" ) as FrameworkElement;
        }

        protected void StartAnimation()
        {
            if ( ( Presenter == null ) || ( !Animating ) )
            {
                return;
            }

            Timeline animation = GetAnimation();

            Presenter.LayoutTransform = GetContentLayoutTransform();
            Presenter.RenderTransform = GetContentRenderTransform();

            if ( Storyboard != null )
            {
                Storyboard.Remove( this );
                _Storyboard = null;
            }

            _Storyboard = new Storyboard();
            Storyboard.Children.Add( animation );

            Storyboard.Begin( this );
        }

        protected void StopAnimation()
        {
            if ( Storyboard != null )
            {
                Storyboard.Stop( this );
            }
        }

        protected void RestartAnimation()
        {
            StopAnimation();
            StartAnimation();
        }

        private Transform GetContentLayoutTransform()
        {
            return new ScaleTransform( ContentScale, ContentScale );
        }

        private Transform GetContentRenderTransform()
        {
            var rotateTransform = new RotateTransform( 0, Presenter.ActualWidth / 2 * ContentScale, Presenter.ActualHeight / 2 * ContentScale );
            RegisterName( string.Format( "{0}{1}", m_AnimationName, ContentSpinner.ANIMATION ), rotateTransform );

            return rotateTransform;
        }

        private Timeline GetAnimation()
        {
            NameScope.SetNameScope( this, new NameScope() );
            Timeline animation = null;

            if ( UseKeyFrames )
            {
                animation = new DoubleAnimationUsingKeyFrames() { RepeatBehavior = RepeatBehavior.Forever };

                for ( int i = 0; i < NumberOfFrames; i++ )
                {
                    var angle = i * 360.0 / (double)NumberOfFrames;
                    var time = KeyTime.FromPercent( ( Convert.ToDouble( i ) ) / NumberOfFrames );
                    DoubleKeyFrame frame = new DiscreteDoubleKeyFrame( angle, time );
                    ( (DoubleAnimationUsingKeyFrames)animation ).KeyFrames.Add( frame );
                }
            }
            else
            {
                animation = new DoubleAnimation( 0.0D, 360.0D, TimeSpan.FromSeconds( 1 / RevolutionsPerSecond ) ) { RepeatBehavior = RepeatBehavior.Forever };

                if ( NumberOfFrames != 16 )
                {
                    Storyboard.SetDesiredFrameRate( animation, NumberOfFrames );
                }
            }

            Storyboard.SetTargetName( animation, string.Format( "{0}{1}", m_AnimationName, ContentSpinner.ANIMATION ) );
            Storyboard.SetTargetProperty( animation, new PropertyPath( RotateTransform.AngleProperty ) );

            return animation;
        }
        #endregion

        #region  Properties
        protected Storyboard Storyboard
        {
            get
            {
                return _Storyboard;
            }
        }

        protected FrameworkElement Presenter
        {
            get
            {
                return _Presenter;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames per rotation.
        /// </summary>
        public int NumberOfFrames // Default is 0 which tells the system to control the frame rate.
        {
            get
            {
                return Convert.ToInt32( (int)( GetValue( NumberOfFramesProperty ) ) );
            }
            set
            {
                SetValue( NumberOfFramesProperty, value );
            }
        }

        public static DependencyProperty NumberOfFramesProperty = DependencyProperty.Register( "NumberOfFrames", typeof( int ), typeof( ContentSpinner ), new FrameworkPropertyMetadata( 16, OnPropertyChange ), ValidateNumberOfFrames );

        /// <summary>
        /// Gets or sets the number of revolutions per second.
        /// </summary>
        public double RevolutionsPerSecond
        {
            get
            {
                return Convert.ToDouble( GetValue( RevolutionsPerSecondProperty ) );
            }
            set
            {
                SetValue( RevolutionsPerSecondProperty, value );
            }
        }

        public static DependencyProperty RevolutionsPerSecondProperty = DependencyProperty.Register( "RevolutionsPerSecond", typeof( double ), typeof( ContentSpinner ), new PropertyMetadata( 1.0, OnPropertyChange ), ValidateRevolutionsPerSecond );

        /// <summary>
        /// Gets or sets the content scale.
        /// </summary>
        public double ContentScale
        {
            get
            {
                return Convert.ToDouble( GetValue( ContentScaleProperty ) );
            }
            set
            {
                SetValue( ContentScaleProperty, value );
            }
        }

        public static DependencyProperty ContentScaleProperty = DependencyProperty.Register( "ContentScale", typeof( double ), typeof( ContentSpinner ), new PropertyMetadata( 1.0, OnPropertyChange ), ValidateContentScale );

        /// <summary>
        /// Gets or sets if spinner animation is active.
        /// </summary>
        public bool Animating
        {
            get
            {
                return Convert.ToBoolean( this.GetValue( ContentSpinner.AnimatingProperty ) );
            }
            set
            {
                this.SetValue( ContentSpinner.AnimatingProperty, value );
            }
        }

        public static readonly DependencyProperty AnimatingProperty = DependencyProperty.Register( "Animating", typeof( bool ), typeof( ContentSpinner ), new FrameworkPropertyMetadata( true, AnimatingChanged ) );

        private static void AnimatingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ContentSpinner owner = (ContentSpinner)d;
            bool value = Convert.ToBoolean( e.NewValue );

            // Add handling.
            if ( value )
            {
                if ( owner.IsLoaded )
                {
                    owner.RestartAnimation();
                }
            }
            else
            {
                owner.StopAnimation();
            }
        }

        /// <summary>
        /// Select one of the predefined canvases as content or set a custom.
        /// The default is <see cref="ContentSpinnerCanvas.Rectangles" />.
        /// </summary>
        public ContentSpinnerCanvas Canvas
        {
            get
            {
                return (ContentSpinnerCanvas)( this.GetValue( ContentSpinner.CanvasProperty ) );
            }
            set
            {
                this.SetValue( ContentSpinner.CanvasProperty, value );
            }
        }

        public static readonly DependencyProperty CanvasProperty = DependencyProperty.Register( "Canvas", typeof( ContentSpinnerCanvas ), typeof( ContentSpinner ), new FrameworkPropertyMetadata( ContentSpinnerCanvas.Rectangles, OnPropertyChange ) );

        /// <summary>
        /// Gets or sets if key frames should be used, or a native animation. The default is true.
        /// </summary>
        /// <remarks>
        /// Using key frames allows more control through <see cref="NumberOfFrames" />.
        /// </remarks>
        public bool UseKeyFrames
        {
            get
            {
                return Convert.ToBoolean( this.GetValue( ContentSpinner.UseKeyFramesProperty ) );
            }
            set
            {
                this.SetValue( ContentSpinner.UseKeyFramesProperty, value );
            }
        }

        public static readonly DependencyProperty UseKeyFramesProperty = DependencyProperty.Register( "UseKeyFrames", typeof( bool ), typeof( ContentSpinner ), new FrameworkPropertyMetadata( true ) );

        #endregion

        #region  Event Handlers
        private static bool ValidateNumberOfFrames( object value )
        {
            int frames = Convert.ToInt32( value );
            return frames > 0;
        }

        private static bool ValidateContentScale( object value )
        {
            var scale = Convert.ToDouble( value );
            return scale > 0.0;
        }

        private static bool ValidateRevolutionsPerSecond( object value )
        {
            var rps = Convert.ToDouble( value );
            return rps > 0.0;
        }

        private static void OnPropertyChange( DependencyObject target, DependencyPropertyChangedEventArgs args )
        {
            var spinner = (ContentSpinner)target;
            spinner.RestartAnimation();
        }
        #endregion

    }
}

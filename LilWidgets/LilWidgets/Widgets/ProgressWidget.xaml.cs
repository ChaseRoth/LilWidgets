﻿using LilWidgets.Lang;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LilWidgets.Widgets
{
    /// <summary>
    /// The Progress Widget is designed to show a percentage of a goal or objective. 
    /// This can be anything from a quiz score where the user attained a 90%, or even a loading bar where the loading process’s progress can be given.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressWidget : ContentView
    {
        #region Constants
        /// <summary>
        /// The base sigma for the drop shadow used with the backing arc.
        /// </summary>
        const float BASE_SHADOW_SIGMA = 3f;
        /// <summary>
        /// The starting position of the arcs.
        /// </summary>
        const float SWEEP_START = -90;
        /// <summary>
        /// The default value for the duration of the progress animation.
        /// </summary>
        public const uint DEFAULT_ANIMATION_DURATION = 2000;
        /// <summary>
        /// The default frame-rate being targeted.
        /// </summary>
        public const float DEFAULT_FRAME_RATE = 60f;
        /// <summary>
        /// The default stroke width used for the arcs.
        /// </summary>
        public const float DEFAULT_STROKE_WIDTH = 10;
        /// <summary>
        /// The default spacing between the arc's inner lining and the text rectangle.
        /// </summary>
        public const float DEFAULT_ARC_TO_TEXT_SPACING = 10;
        /// <summary>
        /// The default shadow color used with the arcs.
        /// </summary>
        public static readonly Color defaultShadowColor = Color.FromHex("#5555");
        /// <summary>
        /// The default text color used.
        /// </summary>
        public static readonly Color defaultTextColor = Color.Black;
        #endregion Constants

        #region Bind-able Properties
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="PercentProgressValue"/> property.
        /// </summary>
        public static readonly BindableProperty PercentProgressValueProperty = BindableProperty.Create(nameof(PercentProgressValue), typeof(double), typeof(ProgressWidget), 0d, BindingMode.OneWay, ValidatePercentage, PercentValuePropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="BackArcColorProperty"/> property.
        /// </summary>
        public static readonly BindableProperty BackArcColorProperty = BindableProperty.Create(nameof(BackArcColor), typeof(Color), typeof(ProgressWidget), Color.Black, BindingMode.OneWay, null, BackgroundRingColorPropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="ProgressArcColor"/> property.
        /// </summary>
        public static readonly BindableProperty ProgressArcColorProperty = BindableProperty.Create(nameof(ProgressArcColor), typeof(Color), typeof(ProgressWidget), Color.White, BindingMode.OneWay, null, ProgressRingColorPropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="IsTextVisible"/> property.
        /// </summary>
        public static readonly BindableProperty IsTextVisibleProperty = BindableProperty.Create(nameof(IsTextVisible), typeof(bool), typeof(ProgressWidget), true, BindingMode.OneWay, null, IsTextVisiblePropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="Duration"/> property.
        /// </summary>
        public static readonly BindableProperty DurationProperty = BindableProperty.Create(nameof(Duration), typeof(uint), typeof(ProgressWidget), DEFAULT_ANIMATION_DURATION);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="StrokeWidth"/> property.
        /// </summary>
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(ProgressWidget), DEFAULT_STROKE_WIDTH, BindingMode.OneWay, null, StokeWidthPropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="TextMargin"/> property.
        /// </summary>
        public static readonly BindableProperty TextMarginProperty = BindableProperty.Create(nameof(TextMargin), typeof(float), typeof(ProgressWidget), DEFAULT_ARC_TO_TEXT_SPACING, BindingMode.OneWay, null, ArcToTextSpacingPropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="ShadowColor"/> property.
        /// </summary>
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(ProgressWidget), defaultShadowColor, BindingMode.OneWay, null, ShadowColorPropertyChanged);
        /// <summary>
        /// <see cref="BindableProperty"/> for the <see cref="TextColor"/> property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ProgressWidget), defaultTextColor, BindingMode.OneWay, null, TextColorPropertyChanged);
        #endregion Bind-able Properties

        #region Properties
        /// <summary>
        /// The progress to be displayed. This value should be a percentage value (0.0 to 1.0).
        /// </summary>
        public double PercentProgressValue
        {
            get => (double)GetValue(PercentProgressValueProperty);
            set => SetValue(PercentProgressValueProperty, value);
        }
        /// <summary>
        /// The color of the backing arc to the progress arc.
        /// </summary>
        public Color BackArcColor
        {
            get => (Color)GetValue(BackArcColorProperty);
            set => SetValue(BackArcColorProperty, value);
        }
        /// <summary>
        /// The color of the progress arc to be used.
        /// </summary>
        public Color ProgressArcColor
        {
            get => (Color)GetValue(ProgressArcColorProperty);
            set => SetValue(ProgressArcColorProperty, value);
        }
        /// <summary>
        /// The color of the shadow to be used with the arcs.
        /// </summary>
        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }
        /// <summary>
        /// Controls whether the percentage text is visible to the user.
        /// </summary>
        public bool IsTextVisible
        {
            get => (bool)GetValue(IsTextVisibleProperty);
            set => SetValue(IsTextVisibleProperty, value);
        }
        /// <summary>
        /// Is the time required to go from a <see cref="PercentProgressValue"/> of 0.0 to a <see cref="PercentProgressValue"/> of 1.0. 
        /// Therefore if <see cref="PercentProgressValue"/> is changing from 0.0 to 0.5 and the <see cref="Duration"/> is set to 2000 milliseconds; it will take
        /// one second to complete the animation. The equation looks like this (relativeDuration == milliseconds * difference).
        /// </summary>
        public uint Duration
        {
            get => (uint)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
        /// <summary>
        /// The target stroke width value to be used for all arcs that make up the widget.
        /// </summary>
        public float StrokeWidth
        {
            get => (float)GetValue(StrokeWidthProperty);
            set => SetValue(StrokeWidthProperty, value);
        }
        /// <summary>
        /// The color the text will use.
        /// </summary>
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        /// <summary>
        /// The target spacing / margin between the inner side of the arc and the edges of the text's rectangle. The size of the margin can only be a single value at the moment that is used
        /// for both the left and right hand sides. The text will always be centered both vertically and horizontally.
        /// </summary>
        public float TextMargin
        {
            get => (float)GetValue(TextMarginProperty);
            set => SetValue(TextMarginProperty, value);
        }               
        private bool animating;
        /// <summary>
        /// Indicates whether the progress widget is animating right now.
        /// </summary>
        public bool Animating
        {
            get => animating;
            private set
            {
                if (animating == value) return;
                animating = value;
#if DEBUG
                if (animating)
                    debugWatch.Restart();
                else
                {
                    debugWatch.Stop();
                    Debug.WriteLine($"Animation Finished | Target Duration: {millisecondDuration} (milliseconds) | Actual: {debugWatch.ElapsedMilliseconds} (milliseconds) | " +
                        $"Error Percentage: {Math.Abs(millisecondDuration - debugWatch.ElapsedMilliseconds) / millisecondDuration:P}");
                }
#endif
            }
        }
        #endregion Properties

        #region Fields
        /// <summary>
        /// Used to draw the progress arc.
        /// </summary>
        SKPaint progressPaint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };
        /// <summary>
        /// Used to draw the background arc.
        /// </summary>
        SKPaint backgroundPaint = new SKPaint
        {
            Color = SKColors.White,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateDropShadow(0, 0, BASE_SHADOW_SIGMA, BASE_SHADOW_SIGMA, defaultShadowColor.ToSKColor())
        };
        /// <summary>
        /// Used to draw percentage text.
        /// </summary>
        SKPaint textPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        /// <summary>
        /// Indicates whether the <see cref="strokeRatio"/> needs to be re-calculated.
        /// </summary>
        bool isStrokeRatioDirty = true;
        /// <summary>
        /// Indicates whether the <see cref="txtSpaceRatio"/> needs to be re-calculated.
        /// </summary>
        bool isArcToTextRatioDirty = true;
        /// <summary>
        /// Used to determine when the animation should stop
        /// </summary>
        Func<bool> comparer = null;
        /// <summary>
        /// The target duration of the progress animation in milliseconds.
        /// </summary>
        float millisecondDuration = DEFAULT_ANIMATION_DURATION; // 2 seconds
        /// <summary>
        /// The difference from newly set <see cref="PercentProgressValue"/> and the old. Used specifically within progress animation.
        /// </summary>
        double difference = 0;
        /// <summary>
        /// The result of <see cref="SKPaint.MeasureText(string, ref SKRect)"/>.
        /// Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        float textWidth = 1;
        /// <summary>
        /// The animation's current progress towards the <see cref="PercentProgressValue"/>.
        /// Updates come the <see cref="Device.StartTimer(TimeSpan, Func{bool})"/> when <see cref="PercentValuePropertyChanged(BindableObject, object, object)"/> is invoked.
        /// </summary>
        double currentPercentageValue = 0;
        /// <summary>
        /// The mid-x value of the <see cref="canvas"/>. Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        float midX = 0;
        /// <summary>
        /// The mid-y value of the <see cref="canvas"/>. Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        float midY = 0;
        /// <summary>
        /// Used in conjunction with the <see cref="Device.StartTimer(TimeSpan, Func{bool})"/>'s lambda inside the <see cref="PercentValuePropertyChanged(BindableObject, object, object)"/>
        /// to determine the amount of time passed from the last time being restarted. This is used as deltaTime to help smooth out animations and pull away from using frame-rate as a method of
        /// determining how much a target value should change over a given amount of time.
        /// </summary>
        Stopwatch stopwatch = null;
        /// <summary>
        /// The last determined value of <see cref="StrokeWidth"/> * <see cref="strokeRatio"/> / 2.
        /// Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        float halfOfRelativeStrokeWidth = 0;
        /// <summary>
        /// The bounds or frame of both arcs and is a factor when determining the text's size.
        /// Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        SKRect arcRect;
        /// <summary>
        /// The information about the <see cref="canvas"/> from the last use of <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/>.
        /// </summary>
        SKImageInfo info;
        /// <summary>
        /// The last determined value of the product of <see cref="StrokeWidth"/> * <see cref="strokeRatio"/>.
        /// This is used for resizing. Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        float relativeStrokeWidth = 1;
        /// <summary>
        /// The last percentage message displayed to the user if <see cref="IsTextVisible"/> is true.
        /// Updates come from the <see cref="canvas_PaintSurface(object, SKPaintSurfaceEventArgs)"/> method.
        /// </summary>
        string percentageMsg = string.Empty;
        /// <summary>
        /// The ratio of the canvas size and <see cref="StrokeWidth"/> to be used when resizing. 
        /// This value only updates when the <see cref="StrokeWidth"/> is changed.
        /// </summary>
        float strokeRatio = 0.15f;
        /// <summary>
        /// The ratio of the canvas size and the <see cref="TextMargin"/> to be used when resizing.
        /// This value only updates when the <see cref="TextMargin"/> is changed.
        /// </summary>
        float txtSpaceRatio = 1;
        /// <summary>
        /// Indicates whether the width of the <see cref="canvas"/> is greater than the height.
        /// </summary>
        bool isWidthGreaterThanHeight = false;
        /// <summary>
        /// The smallest span of the <see cref="canvas"/>. This can either be the span from the top to the bottom aka the height
        /// or the left to the right aka the width.
        /// </summary>
        float limitingSpan = 0;
        /// <summary>
        /// The shadow sigma used for the drop shadow which is relative to the <see cref="strokeRatio"/>. This helps the shadow resize correctly. Without this the
        /// shadow can be come obnoxious when scaling from a larger size to a smaller one.
        /// </summary>
        float relativeShadowSigma = 0;
        /// <summary>
        /// The color used for the drop shadow.
        /// </summary>
        SKColor shadowColor = defaultShadowColor.ToSKColor();
        /// <summary>
        /// The width of half the stroke of the shadow arc. This is used as a padding when determining <see cref="arcRect"/> because the shadow is the widest
        /// arc compared to the others.
        /// </summary>
        float halfShadowStrokeWidth = 0;
        /// <summary>
        /// The bounds or frame of the text being displayed in the center of view.
        /// </summary>
        SKRect textBounds = new SKRect();
        /// <summary>
        /// The path used to define the progress arc.
        /// </summary>
        SKPath progressPath = null;
        /// <summary>
        /// The path used to define the background arc.
        /// </summary>
        SKPath backgroundPath = null;
#if DEBUG
        /// <summary>
        /// Used for debugging.
        /// </summary>
        private Stopwatch debugWatch = new Stopwatch();
#endif
        #endregion Fields

        /// <summary>
        /// Primary Constructor.
        /// </summary>
        public ProgressWidget()
        {
            InitializeComponent();           
        }

        LimitingSpan span = new LimitingSpan(Util.DisplayUtil.DPI);

        /// <summary>
        /// Applies the desired graphics to the <see cref="canvas"/>.
        /// </summary>
        private void canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            info = e.Info;
           
            canvas.Clear();

            midX = info.Rect.MidX;
            midY = info.Rect.MidY;

            isWidthGreaterThanHeight = info.Width > info.Height;
            limitingSpan = (isWidthGreaterThanHeight ? info.Height : info.Width);            

            if (isStrokeRatioDirty) // Calculate the correct strokeRatio if needed
                UpdateStrokeRatio();

            relativeStrokeWidth = limitingSpan * strokeRatio;            
            halfOfRelativeStrokeWidth = relativeStrokeWidth / 2;
            relativeShadowSigma = BASE_SHADOW_SIGMA + BASE_SHADOW_SIGMA * strokeRatio;
            // Compensate for the shadow
            halfShadowStrokeWidth = halfOfRelativeStrokeWidth + relativeShadowSigma * 3f;                        

            // Determine top / bottom by finding the MidY then subtracting half the target width (get the radius of our circle) then subtract the half of stroke which acts as an offset
            if (isWidthGreaterThanHeight) // Canvas is wider than it is tall, hence computer for height
            {
                arcRect = new SKRect(midX - midY + halfShadowStrokeWidth, // left
                                     halfShadowStrokeWidth, // top
                                     midX + midY - halfShadowStrokeWidth, // right
                                     info.Height - halfShadowStrokeWidth); // bottom
            }
            else // Canvas is taller than it is wide so compute for width
            {                
                arcRect = new SKRect(halfShadowStrokeWidth, // left
                                     midY - midX + halfShadowStrokeWidth, // top
                                     info.Width - halfShadowStrokeWidth, // right
                                     midY + midX - halfShadowStrokeWidth); // bottom
            }

            if (arcRect.Width < 0 || arcRect.Height < 0) // return if the control is becoming negatively sized
                return;
            if (relativeStrokeWidth > arcRect.Width)
                return;
#if DEBUG            
                //throw new Exception($"Error. Invalid stroke width was given. The stroke width {StrokeWidth} is larger than the view can handle (strokeWidth * 2 > totalWidth == true).");
            
            Debug.WriteLine($"Stroke Width: {StrokeWidth} | Stroke Ratio: {strokeRatio} | Relative Stroke: {relativeStrokeWidth}");
#endif
            // Set the shadow for the background arc
            backgroundPaint.ImageFilter = SKImageFilter.CreateDropShadow(0,
                                                                         0,
                                                                         relativeShadowSigma,
                                                                         relativeShadowSigma,
                                                                         shadowColor);

            // Creating paths
            progressPath = new SKPath();
            progressPath.AddArc(arcRect, SWEEP_START, PercentageToSweepAngle(currentPercentageValue));
            backgroundPath = new SKPath();
            backgroundPath.AddArc(arcRect, SWEEP_START, 360f);
            // Applying path widths aka strike widths
            progressPaint.StrokeWidth = relativeStrokeWidth;
            backgroundPaint.StrokeWidth = relativeStrokeWidth;
            // Draw Calls
            canvas.DrawPath(backgroundPath, backgroundPaint); // Background Arc
            canvas.DrawPath(progressPath, progressPaint); // Progress Arc

            if (isArcToTextRatioDirty) // If the arc to text ratio dirty update its value
                UpdateArcToTextSpacingRatio();

            if (IsTextVisible) // Draw text only if enabled
            {                            
                percentageMsg = currentPercentageValue.ToString("P");
                // Adjust TextSize property so text is 75% of screen width
                textWidth = textPaint.MeasureText(percentageMsg);
                float width = arcRect.Width - relativeStrokeWidth;
                if (width - TextMargin > 1) // We don't want *lose* the text, the IsTextVisible property should be used for hiding the text
                {
                    textPaint.TextSize = (arcRect.Width - relativeStrokeWidth - TextMargin) * textPaint.TextSize / textWidth * txtSpaceRatio;
                }
                // Find the text bounds
                textPaint.MeasureText(percentageMsg, ref textBounds);
                // Draw text in the center of the control vertically and horizontally
                canvas.DrawText(percentageMsg, 
                                arcRect.MidX - textBounds.MidX, 
                                arcRect.MidY - textBounds.MidY, 
                                textPaint); // Progress Text
            }            
        }        

        /// <summary>
        /// Converts a percentage value (1.0 to 0.0) to a sweep angle (0 to 360).
        /// </summary>
        /// <param name="percentage">Percentage to convert from.</param>
        /// <returns>Sweep angle.</returns>
        private float PercentageToSweepAngle(double percentage)
            => (float)percentage * 100 / 100f * 360f;

        /// <summary>
        /// Validates the given value of the <see cref="PercentProgressValue"/> property.
        /// </summary>
        /// <param name="bindable">Instance of declaring type.</param>
        /// <param name="value">Value of <see cref="PercentProgressValue"/></param>
        /// <returns>Indication if successful validation.</returns>
        private static bool ValidatePercentage(BindableObject bindable, object value)
        {
            double? percentage = value as double?;

            if (percentage == null)
                return false;
            else if (percentage < 0 || percentage > 1)
                return false;

            return true;
        }


        private void PrepareAnimation(double oldValue, double newValue)
        {
            // Getting the difference that must be applied, also determines if this is a increasing or decreasing function
            difference = newValue - oldValue;

            if (difference < 0) // If decreasing
                comparer = () => currentPercentageValue > PercentProgressValue; // When decreasing, if the value is larger than the target keep decreasing.
            else // If increasing
                comparer = () => currentPercentageValue < PercentProgressValue; // When increasing, until the value is larger than the target keep executing.

            Animation anim = new Animation((value) =>
            {
                // Add the correct difference based with respects the desired duration then multiplied by the time passed 
                currentPercentageValue = value;
                // Informing the SKCanvasView that it must redraw itself
                canvas.InvalidateSurface();
            }, currentPercentageValue, PercentProgressValue);
            anim.Commit(this, 
                        "percentage", 
                        16, 
                        (uint)(Duration * Math.Abs(difference)), 
                        Easing.Linear, 
                        (v, b) => 
                        {
                            Animating = b;

                        }, 
                        comparer);
        }

        /// <summary>
        /// A callback function to the <see cref="PercentProgressValue"/> being changed. IMPORTANT: No variables that are used directly inside the <see cref="Device.StartTimer(TimeSpan, Func{bool})"/>'s lambda can be instantiated here.
        /// This is because the closure that is generated by the lambda must capture references to variables that can be modified from the owning class instance. This means the class can modify the closure's 
        /// reference variables and therefore alter its behavior which is required.
        /// </summary>
        /// <param name="bindable">Declaring type.</param>tiger
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">Newly assigned value.</param>
        private static void PercentValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((ProgressWidget)bindable).PrepareAnimation((double)oldValue, (double)newValue);

        #region Property Change Handlers
        private static void BackgroundRingColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ProgressWidget widget = (ProgressWidget)bindable;
            widget.backgroundPaint.Color = ((Color)newValue).ToSKColor();
            TryUpdate(widget);
        }
        private static void ProgressRingColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ProgressWidget widget = (ProgressWidget)bindable;
            widget.progressPaint.Color = ((Color)newValue).ToSKColor();
            TryUpdate(widget);
        }
        private static void IsTextVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => TryUpdate((ProgressWidget)bindable);      
        private static void StokeWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var widget = (ProgressWidget)bindable;
            widget.isStrokeRatioDirty = true;
            TryUpdate(widget);
        }
        private static void ArcToTextSpacingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var widget = (ProgressWidget)bindable;
            widget.isArcToTextRatioDirty = true;
            TryUpdate(widget);
        }
        private static void ShadowColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var widget = (ProgressWidget)bindable;
            widget.shadowColor = ((Color)newValue).ToSKColor();
            TryUpdate(widget);
        }
        private static void TextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var widget = (ProgressWidget)bindable;
            widget.textPaint.Color = ((Color)newValue).ToSKColor();
            TryUpdate(widget);
        }
        #endregion

        /// <summary>
        /// Calculates a new ratio for the <see cref="strokeRatio"/> based off the target <see cref="StrokeWidth"/> and the current <see cref="limitingSpan"/>.
        /// </summary>
        private void UpdateStrokeRatio()
        {
            strokeRatio = 1.0f - (limitingSpan - StrokeWidth) / limitingSpan;
            isStrokeRatioDirty = false;
        }

        /// <summary>
        /// Calculates a new ratio for the <see cref="txtSpaceRatio"/> based off the target <see cref="TextMargin"/> and the current <see cref="arcRect"/>.
        /// </summary>
        private void UpdateArcToTextSpacingRatio()
        {
            float temp = (arcRect.Width - TextMargin) / arcRect.Width;

            if (temp > 0.01f)            
                txtSpaceRatio = temp;            
            isArcToTextRatioDirty = false;
        }        

        /// <summary>
        /// Invalidates the <see cref="canvas"/> of the given instance only if the animating is not running. Otherwise invalidating is not needed
        /// because the animation will apply the changes next cycle.
        /// </summary>
        /// <param name="widget"></param>
        private static void TryUpdate(ProgressWidget widget)
        {            
            if (!widget.Animating) // Don't update if we are animating because the next frame will be updated anyway
                widget.canvas.InvalidateSurface();
        }
    }
}
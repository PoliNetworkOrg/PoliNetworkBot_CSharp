#region

using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

#endregion

namespace Minista
{
    public static class CompositionExtensions
    {
        private const string TRANSLATION = "Translation";

        public static Visual GetVisual(this UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            try
            {
                ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            }
            catch
            {
            }

            visual.Properties.InsertVector3(TRANSLATION, Vector3.Zero);
            return visual;
        }

        public static CompositionAnimationBuilder StartBuildAnimation(this Visual visual)
        {
            return new CompositionAnimationBuilder(visual);
        }

        public static void SetTranslation(this Visual set, Vector3 value)
        {
            set.Properties.InsertVector3(TRANSLATION, value);
        }

        public static Vector3 GetTranslation(this Visual visual)
        {
            visual.Properties.TryGetVector3(TRANSLATION, out var value);
            return value;
        }

        public static string GetTranslationPropertyName(this Visual visual)
        {
            return AnimateProperties.Translation.GetPropertyValue();
        }

        public static string GetTranslationXPropertyName(this Visual visual)
        {
            return AnimateProperties.TranslationX.GetPropertyValue();
        }

        public static string GetTranslationYPropertyName(this Visual visual)
        {
            return AnimateProperties.TranslationY.GetPropertyValue();
        }

        public static string GetPropertyValue(this AnimateProperties property)
        {
            switch (property)
            {
                case AnimateProperties.Translation:
                    return TRANSLATION;

                case AnimateProperties.TranslationX:
                    return $"{TRANSLATION}.X";

                case AnimateProperties.TranslationY:
                    return $"{TRANSLATION}.Y";

                case AnimateProperties.Opacity:
                    return "Opacity";

                case AnimateProperties.RotationAngleInDegrees:
                    return "RotationAngleInDegrees";

                default:
                    throw new ArgumentException("Unknown properties");
            }
        }
    }

    public class CompositionAnimationBuilder
    {
        private float _delayMillis;

        private float _durationMillis;
        private AnimateProperties _property;
        private float _scalarValue;
        private AnimationType _type;

        private Vector3 _vector3Value;
        private readonly Visual _visusal;

        public CompositionAnimationBuilder(Visual visual)
        {
            _visusal = visual;
        }

        public event TypedEventHandler<object, CompositionBatchCompletedEventArgs> OnCompleted;

        public CompositionAnimationBuilder Animate(AnimateProperties property)
        {
            _property = property;
            return this;
        }

        public CompositionAnimationBuilder To(Vector3 vector)
        {
            _type = AnimationType.Vector3;
            _vector3Value = vector;
            return this;
        }

        public CompositionAnimationBuilder To(float value)
        {
            _type = AnimationType.Scalar;
            _scalarValue = value;
            return this;
        }

        public CompositionAnimationBuilder Delay(float durationMillis)
        {
            _delayMillis = durationMillis;
            return this;
        }

        public CompositionAnimationBuilder Spend(int durationMillis)
        {
            _durationMillis = durationMillis;
            return this;
        }

        public CompositionAnimationBuilder Start()
        {
            var comp = _visusal.Compositor;
            KeyFrameAnimation animation;
            switch (_type)
            {
                case AnimationType.Scalar:
                    animation = comp.CreateScalarKeyFrameAnimation();
                    (animation as ScalarKeyFrameAnimation).InsertKeyFrame(1f, _scalarValue);
                    break;

                case AnimationType.Vector3:
                    animation = comp.CreateVector3KeyFrameAnimation();
                    (animation as Vector3KeyFrameAnimation).InsertKeyFrame(1f, _vector3Value);
                    break;

                default:
                    throw new ArgumentException("Unknown animation type");
            }

            animation.Duration = TimeSpan.FromMilliseconds(_durationMillis);
            animation.DelayTime = TimeSpan.FromMilliseconds(_delayMillis);

            var batch = comp.CreateScopedBatch(CompositionBatchTypes.Animation);
            _visusal.StartAnimation(_property.GetPropertyValue(), animation);
            batch.Completed += Batch_Completed;
            batch.End();

            return this;
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            OnCompleted?.Invoke(sender, args);
        }

        private enum AnimationType
        {
            Vector3,
            Scalar
        }
    }

    public enum AnimateProperties
    {
        Translation,
        TranslationY,
        TranslationX,
        Opacity,
        RotationAngleInDegrees
    }

    public static class ImplicitAnimationFactory
    {
        public static CompositionAnimationGroup CreateListOffsetAnimationGroup(Compositor compositor)
        {
            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);
            offsetAnimation.Target = "Offset";

            var animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            return animationGroup;
        }

        public static ImplicitAnimationCollection CreateListOffsetAnimationCollection(Compositor compositor)
        {
            var collection = compositor.CreateImplicitAnimationCollection();
            collection["Offset"] = CreateListOffsetAnimationGroup(compositor);
            ;
            return collection;
        }

        public static ImplicitAnimationCollection CreateCommonOpacityAnimationCollection(Compositor compositor)
        {
            var collection = compositor.CreateImplicitAnimationCollection();
            collection["Opacity"] = CreateOpacityAnimation(compositor);
            return collection;
        }

        private static ScalarKeyFrameAnimation CreateOpacityAnimation(Compositor compositor)
        {
            var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target = "Opacity";
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(300);

            return opacityAnimation;
        }
    }
}
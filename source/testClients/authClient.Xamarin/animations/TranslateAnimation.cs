using Xamarin.Forms;

namespace authClient.animations
{
    public class TranslateTo : AnimationBase<VisualElement>
    {
        public double X { get; set; }
        public double Y { get; set; }

        protected override void Invoke(VisualElement sender)
        {
            sender.TranslateTo(X, Y, Length, Easing);
        }
    }

    [ContentProperty(nameof(Opacity))]
    public class FadeTo : AnimationBase<VisualElement>
    {
        public double Opacity { get; set; }
        
        protected override void Invoke(VisualElement sender)
        {
            sender.FadeTo(Opacity, Length, Easing);
        }
    }

    public abstract class AnimationBase<T> : TriggerAction<T> where T : VisualElement
    {
        public uint Length { get; set; }

        public Easing Easing { get; set; }

        public AnimationBase()
        {
            Length = 250;
            Easing = Easing.Linear;
        }
    }
}
